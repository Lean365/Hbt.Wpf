//===================================================================
// 项目名 : Lean.Hbt
// 文件名 : AboutView.xaml.cs
// 创建者 : AI Assistant
// 创建时间: 2025-10-31
// 版本号 : 1.0
// 描述    : 关于视图代码后台
//===================================================================

using System.Windows.Controls;

namespace Hbt.Fluent.Views.About;

/// <summary>
/// 关于视图
/// </summary>
public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        Loaded += AboutView_Loaded;
    }

    private void AboutView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 设置构建日期
        if (BuildDateTextBlock != null)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fileInfo = new System.IO.FileInfo(assembly.Location);
            BuildDateTextBlock.Text = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

