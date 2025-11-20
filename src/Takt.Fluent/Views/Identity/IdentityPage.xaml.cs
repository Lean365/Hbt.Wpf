using System.Windows.Controls;
using Takt.Fluent.ViewModels.Identity;

namespace Takt.Fluent.Views.Identity;

public partial class IdentityPage : UserControl
{
    public IdentityPageViewModel ViewModel { get; }

    public IdentityPage(IdentityPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
    }
}

