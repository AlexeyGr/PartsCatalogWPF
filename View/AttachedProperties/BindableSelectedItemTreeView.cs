using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PartsCatalog.View.AttachedProperties
{
    /// <summary>
    /// Класс свойств зависимостей поля ввода пароля
    /// </summary>
    public static class BindableSelectedItemTreeView
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem",
            typeof(object), typeof(BindableSelectedItemTreeView));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(BindableSelectedItemTreeView), new PropertyMetadata(false, Attach));

        public static void SetAttach(DependencyObject dp, bool value) =>
            dp.SetValue(AttachProperty, value);

        public static bool GetAttach(DependencyObject dp) =>
            (bool)dp.GetValue(AttachProperty);

        public static object GetSelectedItem(DependencyObject dp) =>
            dp.GetValue(SelectedItemProperty);

        public static void SetSelectedItem(DependencyObject dp, object value) =>
            dp.SetValue(SelectedItemProperty, value);

        private static void Attach(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is TreeView treeView))
                return;

            if ((bool)e.OldValue)
                treeView.SelectedItemChanged -= SelectedItemChanged;

            if ((bool)e.NewValue)
                treeView.SelectedItemChanged += SelectedItemChanged;
        }

        private static void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            SetSelectedItem(treeView, treeView.SelectedItem);
        }
    }
}
