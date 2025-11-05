using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Hbt.Fluent.Views.Identity;

public partial class UserView : UserControl
{
    public UserView()
    {
        InitializeComponent();

        if (App.Services == null)
        {
            throw new InvalidOperationException("App.Services 为 null，无法获取服务");
        }

        var vm = App.Services.GetRequiredService<Hbt.Fluent.ViewModels.Identity.UserViewModel>();
        DataContext = vm;
        _ = vm.LoadAsync();

        if (vm is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != null && e.PropertyName.StartsWith("Show"))
                {
                    ApplyColumnVisibility(vm);
                }
            };
            // 初始应用
            ApplyColumnVisibility(vm);
        }
    }

    private void ApplyColumnVisibility(Hbt.Fluent.ViewModels.Identity.UserViewModel vm)
    {
        if (UsersGrid?.Columns == null) return;
        colId.Visibility = vm.ShowId ? Visibility.Visible : Visibility.Collapsed;
        colUsername.Visibility = vm.ShowUsername ? Visibility.Visible : Visibility.Collapsed;
        colRealName.Visibility = vm.ShowRealName ? Visibility.Visible : Visibility.Collapsed;
        colEmail.Visibility = vm.ShowEmail ? Visibility.Visible : Visibility.Collapsed;
        colPhone.Visibility = vm.ShowPhone ? Visibility.Visible : Visibility.Collapsed;
        colUserType.Visibility = vm.ShowUserType ? Visibility.Visible : Visibility.Collapsed;
        colUserGender.Visibility = vm.ShowUserGender ? Visibility.Visible : Visibility.Collapsed;
        colUserStatus.Visibility = vm.ShowStatus ? Visibility.Visible : Visibility.Collapsed;
        colRemarks.Visibility = Visibility.Visible;
        colCreatedBy.Visibility = Visibility.Visible;
        colCreated.Visibility = Visibility.Visible;
        colUpdatedBy.Visibility = Visibility.Visible;
        colUpdated.Visibility = Visibility.Visible;
        colIsDeleted.Visibility = Visibility.Visible;
        colDeletedBy.Visibility = Visibility.Visible;
        colDeletedTime.Visibility = Visibility.Visible;
    }
}


