// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Generator.CodeGenComponent
// 文件名称：CodeGenForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表单窗口
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using Takt.Fluent.ViewModels.Generator;
using System.Linq;

namespace Takt.Fluent.Views.Generator.CodeGenComponent;

/// <summary>
/// 代码生成表单窗口
/// </summary>
public partial class CodeGenForm : Window
{
    public CodeGenFormViewModel ViewModel { get; }

    public CodeGenForm(CodeGenFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        Owner = System.Windows.Application.Current?.MainWindow;

        Loaded += (s, e) =>
        {
            // 居中窗口
            CenterWindow();
            // 初始化生成功能CheckBox状态
            InitializeGenFunctionCheckBoxes();
            // 设置 DataGrid ComboBox 列的 ItemsSource
            InitializeDataGridComboBoxColumns();
        };
    }

    /// <summary>
    /// 初始化 DataGrid ComboBox 列的 ItemsSource
    /// </summary>
    private void InitializeDataGridComboBoxColumns()
    {
        if (ViewModel == null) return;

        // 通过名称直接设置各列的 ItemsSource
        if (FindName("PropertyNameColumn") is DataGridComboBoxColumn propertyNameColumn)
        {
            propertyNameColumn.ItemsSource = ViewModel.PropertyNames;
        }

        if (FindName("DataTypeColumn") is DataGridComboBoxColumn dataTypeColumn)
        {
            dataTypeColumn.ItemsSource = ViewModel.DataTypes;
        }

        if (FindName("ColumnDataTypeColumn") is DataGridComboBoxColumn columnDataTypeColumn)
        {
            columnDataTypeColumn.ItemsSource = ViewModel.ColumnDataTypes;
        }

        if (FindName("QueryTypeColumn") is DataGridComboBoxColumn queryTypeColumn)
        {
            queryTypeColumn.ItemsSource = ViewModel.QueryTypes;
        }

        if (FindName("FormControlTypeColumn") is DataGridComboBoxColumn formControlTypeColumn)
        {
            formControlTypeColumn.ItemsSource = ViewModel.FormControlTypes;
        }

        if (FindName("DictTypeColumn") is DataGridComboBoxColumn dictTypeColumn)
        {
            dictTypeColumn.ItemsSource = ViewModel.DictTypes;
        }
    }

    /// <summary>
    /// 居中窗口
    /// </summary>
    private void CenterWindow()
    {
        if (Owner != null)
        {
            Left = Owner.Left + (Owner.ActualWidth - Width) / 2;
            Top = Owner.Top + (Owner.ActualHeight - Height) / 2;
        }
        else
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        var viewModel = DataContext as CodeGenFormViewModel;
        if (viewModel != null && !viewModel.IsUpdatingColumn)
        {
            e.Cancel = true;
        }
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // 行内编辑完成，数据已自动更新到绑定对象
        if (e.Row.DataContext is Takt.Application.Dtos.Generator.GenColumnDto column)
        {
            // 检查是否是 ColumnDataType 列被编辑
            // 通过检查列的绑定路径来判断
            if (e.Column is DataGridComboBoxColumn comboBoxColumn)
            {
                var binding = comboBoxColumn.SelectedItemBinding as System.Windows.Data.Binding;
                if (binding != null && binding.Path?.Path == "ColumnDataType")
                {
                    // 延迟执行，确保数据绑定已完成，然后调用 ViewModel 的联动方法
                    // 根据 ColumnDataType 自动更新 PropertyName 等字段
                    Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        ViewModel?.SyncColumnFieldsByDataType(column);
                    }), System.Windows.Threading.DispatcherPriority.DataBind);
                }
            }
        }
    }

    /// <summary>
    /// 初始化生成功能CheckBox状态
    /// </summary>
    private void InitializeGenFunctionCheckBoxes()
    {
        if (ViewModel?.GenFunctions == null) return;

        var selectedFunctions = ViewModel.GenFunctions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet();

        // 查找所有Tag为生成功能的CheckBox
        var validTags = new HashSet<string> { "List", "Query", "Detail", "Preview", "Print", "Create", "Update", "Delete", "View", "Import", "Export" };
        var checkBoxes = FindVisualChildren<CheckBox>(this)
            .Where(cb => cb.Tag is string tag && validTags.Contains(tag));

        foreach (var checkBox in checkBoxes)
        {
            if (checkBox.Tag is string tag)
            {
                checkBox.IsChecked = selectedFunctions.Contains(tag);
            }
        }
    }

    /// <summary>
    /// 生成功能CheckBox选中事件
    /// </summary>
    private void GenFunctionCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Tag is string functionName && ViewModel != null)
        {
            UpdateGenFunctions(functionName, true);
        }
    }

    /// <summary>
    /// 生成功能CheckBox取消选中事件
    /// </summary>
    private void GenFunctionCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Tag is string functionName && ViewModel != null)
        {
            UpdateGenFunctions(functionName, false);
        }
    }

    /// <summary>
    /// 更新生成功能字符串
    /// </summary>
    private void UpdateGenFunctions(string functionName, bool isChecked)
    {
        if (ViewModel == null) return;

        var functions = string.IsNullOrWhiteSpace(ViewModel.GenFunctions)
            ? new HashSet<string>()
            : ViewModel.GenFunctions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .ToHashSet();

        if (isChecked)
        {
            functions.Add(functionName);
        }
        else
        {
            functions.Remove(functionName);
        }

        ViewModel.GenFunctions = functions.Count > 0 ? string.Join(",", functions.OrderBy(f => f)) : null;
    }

    /// <summary>
    /// 查找所有子元素
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

            if (child is T t)
            {
                yield return t;
            }

            foreach (T childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }
}

