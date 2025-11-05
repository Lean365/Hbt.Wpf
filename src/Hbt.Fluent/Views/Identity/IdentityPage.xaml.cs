using System.Windows.Controls;
using Hbt.Fluent.ViewModels.Identity;

namespace Hbt.Fluent.Views.Identity;

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

