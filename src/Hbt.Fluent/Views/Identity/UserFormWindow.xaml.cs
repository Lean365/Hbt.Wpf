using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Hbt.Fluent.Services;

namespace Hbt.Fluent.Views.Identity;

public partial class UserFormWindow : Window
{
    private readonly LanguageService _languageService;

    public UserFormWindow(Hbt.Fluent.ViewModels.Identity.UserFormViewModel vm, LanguageService languageService)
    {
        InitializeComponent();
        _languageService = languageService;
        DataContext = vm;
        
        // 确保语言服务已初始化
        Loaded += async (s, e) =>
        {
            if (_languageService.AvailableLanguages.Count == 0)
            {
                await _languageService.InitializeAsync();
            }
            vm.AttachPasswordAccess(() => (Pwd.Password, Pwd2.Password));
        };
        
        // 订阅语言变化事件，更新窗口标题
        _languageService.LanguageChanged += (sender, langCode) =>
        {
            if (vm.IsCreate)
            {
                vm.Title = _languageService.GetTranslation("Identity.User.Create", "新建用户");
            }
            else
            {
                vm.Title = _languageService.GetTranslation("Identity.User.Edit", "编辑用户");
            }
        };
    }
}


