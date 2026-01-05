// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visits
// 文件名称：WelcomeSignViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：欢迎牌视图模型
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Takt.Application.Dtos.Logistics.Visits;
using Takt.Application.Services.Logistics.Visits;

namespace Takt.Fluent.ViewModels.Logistics.Visits;

/// <summary>
/// 欢迎牌视图模型
/// 用于显示欢迎牌或广告视频
/// </summary>
public partial class WelcomeSignViewModel : ObservableObject, IDisposable
{
    private readonly IVisitingCompanyService _visitingCompanyService;
    private readonly IVisitingEntourageService _visitingEntourageService;
    private readonly OperLogManager? _operLog;

    private Timer? _refreshTimer;
    private bool _disposed = false;
    private long? _currentVisitingCompanyId; // 当前显示的来访公司ID，用于避免重复加载
    private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1); // 防止并发加载来访公司信息

    // 当前显示的来访公司信息
    [ObservableProperty]
    private VisitingCompanyDto? _currentVisitingCompany;

    [ObservableProperty]
    private ObservableCollection<VisitingEntourageDto> _currentVisitingEntourages = [];

    // 处理后的显示项列表（用于欢迎牌显示）
    [ObservableProperty]
    private ObservableCollection<VisitingDisplayItem> _currentVisitingDisplayItems = [];

    // 是否显示来访公司信息（true=显示来访公司信息，false=显示广告视频）
    [ObservableProperty]
    private bool _showVisitingInfo;

    // 广告视频路径
    [ObservableProperty]
    private string? _adVideoPath;

    // 刷新间隔（秒）- 每半点刷新（30分钟）
    [ObservableProperty]
    private int _refreshInterval = 1800; // 默认30分钟（1800秒）刷新一次

    [ObservableProperty]
    private string? _errorMessage;

    // 是否处于编辑模式（用于拖拽和调整大小）
    [ObservableProperty]
    private bool _isEditMode;

    // 是否全屏
    [ObservableProperty]
    private bool _isFullScreen;

    public WelcomeSignViewModel(
        IVisitingCompanyService visitingCompanyService,
        IVisitingEntourageService visitingEntourageService,
        OperLogManager? operLog = null)
    {
        _visitingCompanyService = visitingCompanyService ?? throw new ArgumentNullException(nameof(visitingCompanyService));
        _visitingEntourageService = visitingEntourageService ?? throw new ArgumentNullException(nameof(visitingEntourageService));
        _operLog = operLog;

        // 设置默认广告视频路径（可以从配置中读取）
        AdVideoPath = "Assets/teac.mp4"; // 默认路径，可以后续从配置读取

        // 立即加载一次
        _ = LoadCurrentVisitingCompanyAsync();
    }

    /// <summary>
    /// 切换全屏命令
    /// </summary>
    public ICommand ToggleFullScreenCommand => new RelayCommand(ToggleFullScreen);

    /// <summary>
    /// 切换编辑模式命令
    /// </summary>
    public ICommand ToggleEditModeCommand => new RelayCommand(() =>
    {
        _operLog?.Information("[WelcomeSignViewModel] 🔧 ToggleEditModeCommand 开始执行 - 当前 IsEditMode: {IsEdit}", IsEditMode);
        IsEditMode = !IsEditMode;
        _operLog?.Information("[WelcomeSignViewModel] 🔧 ToggleEditModeCommand 执行完成 - 新 IsEditMode: {IsEdit}", IsEditMode);
    }, () => true); // 确保命令始终可以执行

    /// <summary>
    /// 切换全屏
    /// </summary>
    private void ToggleFullScreen()
    {
        IsFullScreen = !IsFullScreen;
        _operLog?.Information("[WelcomeSignView] 切换全屏: {IsFullScreen}", IsFullScreen);
    }


    /// <summary>
    /// 加载当前时间范围内的随行人员信息
    /// **修改逻辑：只验证结束时间，只要未结束都应该显示出来（不检查开始时间）**
    /// </summary>
    private async Task LoadCurrentVisitingCompanyAsync()
    {
        // 防止并发执行
        if (!await _loadLock.WaitAsync(0))
        {
            _operLog?.Information("[WelcomeSignView] ⏭️ LoadCurrentVisitingCompanyAsync 并发跳过");
            return;
        }

        try
        {
            var now = DateTime.Now;
            _operLog?.Information("[WelcomeSignView] 🔍 开始加载当前随行人员信息 - 当前时间: {Now}", now);

            // 查询随行人员信息
            var query = new VisitingCompanyQueryDto
            {
                VisitStartTimeFrom = now.AddDays(-30),
                VisitStartTimeTo = now.AddDays(30),
                PageIndex = 1,
                PageSize = 1000
            };

            var result = await _visitingCompanyService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                _operLog?.Warning("[WelcomeSignView] ❌ 查询随行人员列表失败 - Success: {Success}", result.Success);
                SetVisitingState(null);
                return;
            }

            _operLog?.Information("[WelcomeSignView] ✅ 查询随行人员列表成功，共 {Count} 条", result.Data.Items.Count);

            // **修改筛选逻辑：只验证结束时间，只要未结束都应该显示出来**
            // 找到所有结束时间还未到达的随行人员（不检查开始时间）
            var activeVisitingCompanies = result.Data.Items
                .Where(v => v.IsDeleted == 0 && now <= v.VisitEndTime)
                .OrderBy(v => v.VisitEndTime)
                .ToList();

            if (activeVisitingCompanies.Any())
            {
                _operLog?.Information("[WelcomeSignView] ✅ 找到 {Count} 个符合条件的来访公司", activeVisitingCompanies.Count);
                foreach (var visitingCompany in activeVisitingCompanies)
                {
                    _operLog?.Information("[WelcomeSignView]    - 来访公司 ID: {Id}, 公司: {Company}, 开始: {Start}, 结束: {End}",
                        visitingCompany.Id, visitingCompany.VisitingCompanyName, visitingCompany.VisitStartTime, visitingCompany.VisitEndTime);
                }

                // **修改：显示第一个来访公司作为主公司（用于显示公司名称），但加载所有来访公司的详情**
                var primaryVisitingCompany = activeVisitingCompanies.First();

                // 设置来访公司信息（使用第一个来访公司作为主显示）
                SetVisitingState(primaryVisitingCompany);
                _currentVisitingCompanyId = primaryVisitingCompany.Id;

                // **关键修改：加载所有符合条件的来访公司的详情，合并显示**
                _ = LoadAllVisitingEntouragesAsync(activeVisitingCompanies);

                // **修改逻辑：计算下一个切换时间（最早的随行人员结束时间）**
                // 因为只验证结束时间，所以只需要在结束时间切换
                var nextSwitchTime = activeVisitingCompanies.Min(v => v.VisitEndTime);

                _operLog?.Information("[WelcomeSignView] ⏰ 下一个切换时间（最早随行人员结束时间）: {NextTime}", nextSwitchTime);
                StartRefreshTimer(nextSwitchTime);
            }
            else
            {
                _operLog?.Information("[WelcomeSignView] ℹ️ 当前无随行人员，显示广告");

                // 没有随行人员，显示广告
                SetVisitingState(null);
                _currentVisitingCompanyId = null;

                // **修改逻辑：查找下一个结束时间还未到达的来访公司（不检查开始时间）**
                var nextVisitingCompany = result.Data.Items
                    .Where(v => v.IsDeleted == 0 && v.VisitEndTime > now)
                    .OrderBy(v => v.VisitEndTime)
                    .FirstOrDefault();

                if (nextVisitingCompany != null)
                {
                    // **修改逻辑：因为只验证结束时间，所以等待到来访公司结束时间时再刷新检查**
                    _operLog?.Information("[WelcomeSignView] ⏰ 找到下一个来访公司（结束时间: {EndTime}），将在结束时间刷新检查", nextVisitingCompany.VisitEndTime);
                    StartRefreshTimer(nextVisitingCompany.VisitEndTime);
                }
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] ❌ 加载随行人员信息异常");
            SetVisitingState(null);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    /// <summary>
    /// 设置随行人员显示状态
    /// </summary>
    private void SetVisitingState(VisitingCompanyDto? visitingCompany)
    {
        _operLog?.Information("[WelcomeSignView] 🔄 SetVisitingState 调用 - VisitingCompany: {VisitingCompany}, 公司: {Company}",
            visitingCompany != null ? visitingCompany.Id.ToString() : "null",
            visitingCompany?.VisitingCompanyName ?? "null");

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            // 先清空详情，避免显示旧的详情
            CurrentVisitingEntourages.Clear();
            CurrentVisitingDisplayItems.Clear();

            // 先设置 ShowVisitingInfo，确保 CurrentVisitingCompany 变化时状态已正确
            ShowVisitingInfo = visitingCompany != null;

            // 再设置 CurrentVisitingCompany，此时 ShowVisitingInfo 已经是正确的值
            CurrentVisitingCompany = visitingCompany;

            _operLog?.Information("[WelcomeSignView] ✅ SetVisitingState 完成 - ShowVisitingInfo: {ShowVisiting}, CurrentVisitingCompany: {Current}",
                ShowVisitingInfo, CurrentVisitingCompany != null ? CurrentVisitingCompany.VisitingCompanyName : "null");

            // 明确输出当前显示状态
            if (ShowVisitingInfo && CurrentVisitingCompany != null)
            {
                _operLog?.Information("[WelcomeSignView] 📺 当前显示：来访公司信息 - 公司: {Company}, 来访公司ID: {Id}",
                    CurrentVisitingCompany.VisitingCompanyName, CurrentVisitingCompany.Id);
            }
            else
            {
                _operLog?.Information("[WelcomeSignView] 📺 当前显示：广告视频");
            }
        });
    }

    /// <summary>
    /// 启动刷新定时器 - 在随行人员开始/结束时间点自动切换
    /// </summary>
    private void StartRefreshTimer(DateTime? switchTime)
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;

        if (!switchTime.HasValue || switchTime.Value <= DateTime.Now)
            return;

        var delay = switchTime.Value - DateTime.Now;
        var delayMs = (int)Math.Max(0, delay.TotalMilliseconds);
        _refreshTimer = new Timer(async _ => await LoadCurrentVisitingCompanyAsync(), null, delayMs, Timeout.Infinite);
    }

    /// <summary>
    /// 加载随行人员详情
    /// </summary>
    private async Task LoadVisitingEntouragesAsync(long visitingCompanyId)
    {
        try
        {
            _operLog?.Information("[WelcomeSignView] 开始加载来访成员详情，来访公司ID: {VisitingCompanyId}", visitingCompanyId);

            var query = new VisitingEntourageQueryDto
            {
                VisitingCompanyId = visitingCompanyId,
                PageIndex = 1,
                PageSize = 100 // 获取所有详情
            };

            var result = await _visitingEntourageService.GetListAsync(query);

            if (result.Success && result.Data != null && result.Data.Items.Count > 0)
            {
                _operLog?.Information("[WelcomeSignView] ✅ 查询随行人员详情成功，共 {Count} 条详情", result.Data.Items.Count);

                // 在 UI 线程上更新集合（不影响 ShowVisitingInfo，因为主表信息已经显示）
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentVisitingEntourages.Clear();
                    foreach (var detail in result.Data.Items)
                    {
                        CurrentVisitingEntourages.Add(detail);
                        _operLog?.Information("[WelcomeSignView]    - 详情: {Dept} / {Name} / {Post}",
                            detail.VisitDept, detail.VisitingMembers, detail.VisitPost);
                    }

                    // **处理显示项：按公司分组，没有部门的人员优先显示，相同部门合并为一条**
                    // 注意：LoadVisitingEntouragesAsync 是旧方法，只加载单个随行人员，所以只使用该随行人员的公司名称
                    var singleVisitingCompanyMap = new Dictionary<long, string> { { visitingCompanyId, CurrentVisitingCompany?.VisitingCompanyName ?? string.Empty } };
                    var displayItems = VisitingDisplayItem.CreateDisplayItems(result.Data.Items, singleVisitingCompanyMap);
                    CurrentVisitingDisplayItems.Clear();
                    foreach (var displayItem in displayItems)
                    {
                        CurrentVisitingDisplayItems.Add(displayItem);
                        _operLog?.Information("[WelcomeSignView]    - 显示项: 部门={Dept}, 职务={Post}, 人员={VisitingMembers}",
                            displayItem.VisitDept ?? "(无部门)", displayItem.VisitPost, displayItem.VisitingMembers);
                    }

                    // 不要再次设置 ShowVisitingInfo，避免触发不必要的 UpdateVideoPlayback
                    // 状态已经在 LoadCurrentVisitingCompanyAsync 中正确设置了
                });

                _operLog?.Information("[WelcomeSignView] ✅ 来访成员详情已更新 - 公司: {VisitingCompanyName}, 来访成员数量: {Count}, 来访公司ID: {VisitingCompanyId}",
                    CurrentVisitingCompany?.VisitingCompanyName ?? "未知",
                    CurrentVisitingEntourages.Count,
                    visitingCompanyId);
            }
            else
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ 随行人员详情查询失败或无数据 - 随行人员ID: {EntourageId}, Success: {Success}, Count: {Count}",
                    visitingCompanyId, result.Success, result.Data?.Items.Count ?? 0);

                // 没有详情数据，但仍然显示随行人员主表信息（公司名称等）
                // 详情列表为空，但不影响主表信息的显示
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentVisitingEntourages.Clear();
                    CurrentVisitingDisplayItems.Clear();
                    // 不要再次设置 ShowVisitingInfo，避免触发不必要的 UpdateVideoPlayback
                    // 状态已经在 LoadCurrentVisitingCompanyAsync 中正确设置了
                    if (CurrentVisitingCompany != null && ShowVisitingInfo)
                    {
                        _operLog?.Information("[WelcomeSignView] ✅ 显示随行人员主表信息（无详情数据）- 公司: {VisitingCompany}, 随行人员ID: {EntourageId}",
                            CurrentVisitingCompany.VisitingCompanyName, visitingCompanyId);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] ❌ 加载随行人员详情异常 - 随行人员ID: {EntourageId}", visitingCompanyId);
            // 异常时不影响主表信息的显示
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                CurrentVisitingEntourages.Clear();
                CurrentVisitingDisplayItems.Clear();
                // 不要再次设置 ShowVisitingInfo，避免触发不必要的 UpdateVideoPlayback
                // 状态已经在 LoadCurrentVisitingCompanyAsync 中正确设置了
                if (CurrentVisitingCompany != null && ShowVisitingInfo)
                {
                    _operLog?.Information("[WelcomeSignView] ✅ 显示随行人员主表信息（详情加载异常）- 公司: {VisitingCompany}, 随行人员ID: {EntourageId}",
                        CurrentVisitingCompany.VisitingCompanyName, visitingCompanyId);
                }
            });
        }
    }

    /// <summary>
    /// 加载所有随行人员的详情（合并显示）
    /// </summary>
    private async Task LoadAllVisitingEntouragesAsync(List<VisitingCompanyDto> activeVisitingCompanies)
    {
        try
        {
            var visitingCompanyIds = activeVisitingCompanies.Select(v => v.Id).ToList();
            _operLog?.Information("[WelcomeSignView] 开始加载所有随行人员详情，随行人员ID列表: {EntourageIds}", string.Join(", ", visitingCompanyIds));

            // 构建随行人员ID到公司名称的映射
            var visitingCompanyIdToCompanyMap = activeVisitingCompanies.ToDictionary(
                v => v.Id,
                v => v.VisitingCompanyName ?? string.Empty);

            var allDetails = new List<VisitingEntourageDto>();

            // 遍历加载所有随行人员的详情
            foreach (var visitingCompanyId in visitingCompanyIds)
            {
                try
                {
                    var query = new VisitingEntourageQueryDto
                    {
                        VisitingCompanyId = visitingCompanyId,
                        PageIndex = 1,
                        PageSize = 100 // 获取所有详情
                    };

                    var result = await _visitingEntourageService.GetListAsync(query);

                    if (result.Success && result.Data != null && result.Data.Items.Count > 0)
                    {
                        _operLog?.Information("[WelcomeSignView] ✅ 随行人员ID {EntourageId} 查询详情成功，共 {Count} 条详情", visitingCompanyId, result.Data.Items.Count);
                        allDetails.AddRange(result.Data.Items);
                    }
                    else
                    {
                        _operLog?.Warning("[WelcomeSignView] ⚠️ 随行人员ID {EntourageId} 详情查询失败或无数据", visitingCompanyId);
                    }
                }
                catch (Exception ex)
                {
                    _operLog?.Error(ex, "[WelcomeSignView] ❌ 加载随行人员ID {EntourageId} 详情异常", visitingCompanyId);
                }
            }

            if (allDetails.Any())
            {
                _operLog?.Information("[WelcomeSignView] ✅ 所有随行人员详情加载完成，共 {TotalCount} 条详情", allDetails.Count);

                // 在 UI 线程上更新集合
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentVisitingEntourages.Clear();
                    foreach (var detail in allDetails)
                    {
                        CurrentVisitingEntourages.Add(detail);
                        _operLog?.Information("[WelcomeSignView]    - 详情: {Dept} / {Name} / {Post}",
                            detail.VisitDept, detail.VisitingMembers, detail.VisitPost);
                    }

                    // **处理显示项：按公司分组，没有部门的人员优先显示，相同部门合并为一条**
                    // **调试：记录随行人员ID到公司名称的映射，确认数据源**
                    _operLog?.Information("[WelcomeSignView] 🔍 随行人员ID到公司名称映射（共{Count}条）:", visitingCompanyIdToCompanyMap.Count);
                    foreach (var kvp in visitingCompanyIdToCompanyMap.OrderBy(k => k.Value).ThenBy(k => k.Key))
                    {
                        _operLog?.Information("[WelcomeSignView]    - VisitingCompanyId={Id}, Company=\"{Company}\"", kvp.Key, kvp.Value);
                    }

                    var displayItems = VisitingDisplayItem.CreateDisplayItems(allDetails, visitingCompanyIdToCompanyMap);

                    // **调试：统计按公司名称分组的结果**
                    var companyCount = displayItems.Count(d => d.ShowCompany);
                    _operLog?.Information("[WelcomeSignView] 📊 分组结果统计: 共 {Total} 个显示项，其中 {CompanyCount} 个公司名称行", displayItems.Count, companyCount);

                    CurrentVisitingDisplayItems.Clear();
                    foreach (var displayItem in displayItems)
                    {
                        CurrentVisitingDisplayItems.Add(displayItem);
                        if (displayItem.ShowCompany)
                        {
                            _operLog?.Information("[WelcomeSignView]    - 显示项[公司名称行]: 公司=\"{Company}\", VisitingCompanyId={VisitingCompanyId}",
                                displayItem.VisitingCompany ?? "(空)", displayItem.VisitingCompanyId);
                        }
                        else
                        {
                            _operLog?.Information("[WelcomeSignView]    - 显示项: 公司=\"{Company}\", VisitingCompanyId={VisitingCompanyId}, 部门={Dept}, 职务={Post}, 人员={VisitingMembers}",
                                displayItem.VisitingCompany ?? "(无公司)", displayItem.VisitingCompanyId, displayItem.VisitDept ?? "(无部门)", displayItem.VisitPost, displayItem.VisitingMembers);
                        }
                    }
                });

                _operLog?.Information("[WelcomeSignView] ✅ 所有随行人员详情已更新 - 随行人员详情总数: {Count}, 显示项总数: {DisplayCount}",
                    CurrentVisitingEntourages.Count,
                    CurrentVisitingDisplayItems.Count);

                // **关键修复**：触发CurrentVisitingDisplayItems的PropertyChanged事件，让View知道需要更新字体大小
                OnPropertyChanged(nameof(CurrentVisitingDisplayItems));
            }
            else
            {
                _operLog?.Warning("[WelcomeSignView] ⚠️ 所有随行人员都没有详情数据");

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentVisitingEntourages.Clear();
                    CurrentVisitingDisplayItems.Clear();
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[WelcomeSignView] ❌ 加载所有随行人员详情异常");
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                CurrentVisitingEntourages.Clear();
                CurrentVisitingDisplayItems.Clear();
            });
        }
    }

    /// <summary>
    /// 手动刷新
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadCurrentVisitingCompanyAsync();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _refreshTimer?.Dispose();
        _refreshTimer = null;
        _loadLock?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

