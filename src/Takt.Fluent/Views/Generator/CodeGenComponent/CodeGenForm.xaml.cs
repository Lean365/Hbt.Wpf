// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Generator.CodeGenComponent
// 文件名称：CodeGenForm.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：代码生成表单窗口
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
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

        // PropertyNameColumn 和 DataTypeColumn 现在是 DataGridTemplateColumn，ItemsSource 在 XAML 中直接绑定，无需在此设置
        // ColumnDataTypeColumn 现在是 DataGridTemplateColumn，ItemsSource 在 XAML 中直接绑定，无需在此设置

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
        if (viewModel != null && e.Row.DataContext is Takt.Application.Dtos.Generator.GenColumnDto column)
        {
            // 只有当前行是正在编辑的行时，才允许编辑
            if (viewModel.EditingColumn != column)
            {
                e.Cancel = true;
                return;
            }
            
            // 当开始编辑 DataType 列时，根据 ColumnDataType 更新选项
            // 检查是否是 DataType 列（第5列，索引4）
            var columnIndex = e.Column?.DisplayIndex ?? -1;
            if (columnIndex == 4) // DataType 列
            {
                // 延迟执行，确保编辑模板已创建
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    UpdatePropertyNameAndDataTypeOptions(column);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // 如果编辑被取消，不执行任何操作
        if (e.EditAction == System.Windows.Controls.DataGridEditAction.Cancel)
        {
            return;
        }

        // 检查是否是需要联动的列被编辑
        if (sender is DataGrid dataGrid && e.Row.DataContext is Takt.Application.Dtos.Generator.GenColumnDto column)
        {
            // 检查是否是 ColumnName 列被编辑（第1列，索引0）
            var columnIndex = e.Column?.DisplayIndex ?? -1;
            if (columnIndex == 0 && ViewModel != null)
            {
                // 从编辑元素中获取最新的 ColumnName 值
                string? newColumnName = null;
                if (e.EditingElement is TextBox textBox)
                {
                    newColumnName = textBox.Text;
                }
                
                // 如果无法从编辑元素获取，则使用 column.ColumnName（可能在编辑事务提交后已更新）
                if (string.IsNullOrWhiteSpace(newColumnName))
                {
                    newColumnName = column.ColumnName;
                }
                
                if (!string.IsNullOrWhiteSpace(newColumnName))
                {
                    // 根据 ColumnName 自动生成 PropertyName
                    var newPropertyName = CodeGenFormViewModel.ConvertToPropertyName(newColumnName);
                    column.PropertyName = newPropertyName;
                    
                    // 由于 GenColumnDto 没有实现 INotifyPropertyChanged，需要手动更新 UI
                    // 延迟执行，确保编辑事务已完成
                    Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        UpdatePropertyNameCell(dataGrid, column, newPropertyName);
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
    }

    /// <summary>
    /// 更新 PropertyName 列的显示
    /// </summary>
    private void UpdatePropertyNameCell(DataGrid dataGrid, Takt.Application.Dtos.Generator.GenColumnDto column, string newPropertyName)
    {
        if (dataGrid == null || column == null) return;

        // 找到对应的行
        var row = dataGrid.ItemContainerGenerator.ContainerFromItem(column) as DataGridRow;
        if (row != null)
        {
            // PropertyName 是第4列（索引3）
            var propertyNameCell = GetCell(dataGrid, row, 3);
            if (propertyNameCell != null)
            {
                // 查找 TextBlock（显示模式）或 TextBox（编辑模式）
                var textBlock = FindVisualChild<TextBlock>(propertyNameCell);
                var textBox = FindVisualChild<TextBox>(propertyNameCell);
                
                if (textBlock != null)
                {
                    textBlock.Text = newPropertyName;
                }
                else if (textBox != null)
                {
                    textBox.Text = newPropertyName;
                }
            }
        }
    }

    /// <summary>
    /// ColumnDataType ComboBox 选择改变事件
    /// 当用户选择 ColumnDataType 时，自动联动更新 PropertyName 和 DataType 的选项，并设置对应的值
    /// </summary>
    private void ColumnDataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.DataContext is Takt.Application.Dtos.Generator.GenColumnDto column)
        {
            // 获取选中的值（优先使用 SelectedItem，如果为空则使用 SelectedValue）
            var selectedValue = comboBox.SelectedItem?.ToString() ?? comboBox.SelectedValue?.ToString();
            
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                // 立即更新 ColumnDataType 属性，确保绑定值正确
                column.ColumnDataType = selectedValue;
                
                // 立即调用联动方法更新所有相关字段（包括 PropertyName 和 DataType）
                if (ViewModel != null && column != null)
                {
                    // 先调用联动方法，设置正确的 PropertyName 和 DataType 值
                    ViewModel.SyncColumnFieldsByDataType(column);
                    
                    // 延迟执行，确保数据绑定已完成，然后更新 PropertyName 和 DataType 的选项列表
                    Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        UpdatePropertyNameAndDataTypeOptions(column);
                    }), System.Windows.Threading.DispatcherPriority.DataBind);
                }
            }
        }
    }


    /// <summary>
    /// DataType ComboBox 选择改变事件
    /// DataType 只负责显示数据类型，不参与联动
    /// </summary>
    private void DataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.DataContext is Takt.Application.Dtos.Generator.GenColumnDto column)
        {
            // 获取选中的值
            var selectedValue = comboBox.SelectedItem?.ToString() ?? comboBox.SelectedValue?.ToString();
            
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                // 只更新 DataType 属性，不进行联动
                column.DataType = selectedValue;
            }
        }
    }

    /// <summary>
    /// 更新 DataType 的选项列表
    /// 根据 ColumnDataType 过滤并设置正确的选项
    /// </summary>
    private void UpdatePropertyNameAndDataTypeOptions(Takt.Application.Dtos.Generator.GenColumnDto column)
    {
        if (ViewModel == null || column == null) return;

        // 延迟执行，确保数据绑定已完成
        Dispatcher.BeginInvoke(new System.Action(() =>
        {
            // 获取根据 ColumnDataType 过滤后的 DataType 选项
            var dataTypeOptions = ViewModel.GetDataTypeOptionsByColumnDataType(column.ColumnDataType);

            // 找到 DataGrid
            var dataGrid = FindName("ColumnsDataGrid") as DataGrid;
            if (dataGrid != null)
            {
                // 找到对应的行
                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(column) as DataGridRow;
                if (row != null)
                {
                    // DataType 是第5列（索引4）
                    // 更新 DataType ComboBox 的 ItemsSource
                    var dataTypeCell = GetCell(dataGrid, row, 4);
                    if (dataTypeCell != null)
                    {
                        var dataTypeComboBox = FindVisualChild<ComboBox>(dataTypeCell);
                        if (dataTypeComboBox != null)
                        {
                            // 更新选项列表
                            dataTypeComboBox.ItemsSource = dataTypeOptions;
                            
                            // 确保选中的值在选项列表中，如果不在则设置为第一个选项
                            if (dataTypeOptions.Count > 0)
                            {
                                if (column.DataType == null || !dataTypeOptions.Contains(column.DataType))
                                {
                                    column.DataType = dataTypeOptions[0];
                                }
                                // 确保 ComboBox 的选中值与数据模型同步
                                dataTypeComboBox.SelectedItem = column.DataType;
                            }
                        }
                    }
                }
            }
        }), System.Windows.Threading.DispatcherPriority.DataBind);
    }

    /// <summary>
    /// 获取单元格
    /// </summary>
    private DataGridCell? GetCell(DataGrid dataGrid, DataGridRow row, int columnIndex)
    {
        if (columnIndex < 0) return null;
        
        var presenter = FindVisualChild<DataGridCellsPresenter>(row);
        if (presenter == null) return null;

        return presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
    }

    /// <summary>
    /// 查找子元素
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) return null;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

            if (child is T t)
            {
                return t;
            }

            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }

        return null;
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

