﻿using Libria.Areas.Admin.Models;

namespace Libria.Areas.Admin.ViewModels
{
    public class SidebarViewModel
    {
        public SidebarViewModel() { }

        public SidebarViewModel(MenuItemType activeItem)
        {
            ActiveMenuItem = activeItem;
        }

        public List<SidebarMenuItem> SidebarMenuItems { get; set; } = new List<SidebarMenuItem>()
        {
            new SidebarMenuItem
            {
                ItemType = MenuItemType.Main,
                ActionName = "Index",
                Name = "Головна"
            },
            new SidebarMenuItem
            {
                ItemType = MenuItemType.Orders,
                ActionName = "Orders",
                Name = "Замовлення"
            },
            new SidebarMenuItem
            {
                ItemType = MenuItemType.Categories,
                ActionName = "Categories",
                Name = "Категорії"
            },
            new SidebarMenuItem
            {
                ItemType = MenuItemType.Products,
                ActionName = "Products",
                Name = "Товари"
            }
        };

        public MenuItemType ActiveMenuItem { get; set; } = MenuItemType.Main;
    }
}