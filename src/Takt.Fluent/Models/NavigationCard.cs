using Takt.Application.Dtos.Identity;

namespace Takt.Fluent.Models;

public class NavigationCard
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public MenuDto? MenuItem { get; set; }

    public NavigationCard(MenuDto menu)
    {
        Title = menu.MenuName;
        Description = string.Empty;
        Icon = menu.Icon;
        MenuItem = menu;
    }

    public NavigationCard(string title, string? description = null!, string? icon = null!, MenuDto? menuItem = null)
    {
        Title = title;
        Description = description ?? string.Empty;
        Icon = icon;
        MenuItem = menuItem;
    }
}

