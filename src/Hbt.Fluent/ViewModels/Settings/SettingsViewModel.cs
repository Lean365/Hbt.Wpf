//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : SettingsViewModel.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 设置页面视图模型
//===================================================================

using CommunityToolkit.Mvvm.ComponentModel;
using Hbt.Application.Dtos.Routine;
using Hbt.Application.Services.Routine;
using Hbt.Fluent.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Hbt.Fluent.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<SettingCategoryGroup> _settingCategories = new();

    [ObservableProperty]
    private SettingCategoryGroup? _selectedCategory;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    private readonly ISettingService? _settingService;
    private readonly Hbt.Fluent.Services.LanguageService? _languageService;
    private readonly ThemeService? _themeService;

    public SettingsViewModel(ISettingService? settingService = null, Hbt.Fluent.Services.LanguageService? languageService = null, ThemeService? themeService = null)
    {
        _settingService = settingService ?? App.Services?.GetService<ISettingService>();
        _languageService = languageService ?? App.Services?.GetService<Hbt.Fluent.Services.LanguageService>();
        _themeService = themeService ?? App.Services?.GetService<ThemeService>();
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (_settingService == null)
            {
                ErrorMessage = "设置服务未初始化";
                return;
            }

            // 获取所有设置（使用分页查询，获取大量数据）
            var result = await _settingService.GetListAsync(1, int.MaxValue);
            if (!result.Success || result.Data == null || result.Data.Items == null)
            {
                ErrorMessage = result.Message ?? "加载设置失败";
                return;
            }

            var allSettings = result.Data.Items;

            // 按 Category 分组
            var grouped = allSettings
                .Where(s => !string.IsNullOrEmpty(s.Category))
                .GroupBy(s => s.Category!)
                .Select(g => new SettingCategoryGroup
                {
                    Category = g.Key,
                    Settings = new ObservableCollection<SettingDto>(g.OrderBy(s => s.OrderNum))
                })
                .OrderBy(g => g.Category)
                .ToList();

            // 添加未分类的设置（如果有）
            var uncategorized = allSettings.Where(s => string.IsNullOrEmpty(s.Category)).ToList();
            if (uncategorized.Any())
            {
                grouped.Add(new SettingCategoryGroup
                {
                    Category = "未分类",
                    Settings = new ObservableCollection<SettingDto>(uncategorized.OrderBy(s => s.OrderNum))
                });
            }

            SettingCategories.Clear();
            foreach (var group in grouped)
            {
                SettingCategories.Add(group);
            }

            // 默认选中第一个分类
            if (SettingCategories.Any())
            {
                SelectedCategory = SettingCategories.First();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载设置失败：{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SaveSettingAsync(SettingDto setting)
    {
        if (_settingService == null || setting.IsEditable != 0) return;

        try
        {
            var updateDto = new SettingUpdateDto
            {
                Id = setting.Id,
                SettingKey = setting.SettingKey,
                SettingValue = setting.SettingValue,
                Category = setting.Category,
                OrderNum = setting.OrderNum,
                SettingDescription = setting.SettingDescription,
                SettingType = setting.SettingType
            };

            var result = await _settingService.UpdateAsync(updateDto);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? "保存设置失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存设置失败：{ex.Message}";
        }
    }
}

public partial class SettingCategoryGroup : ObservableObject
{
    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SettingDto> _settings = new();
}
