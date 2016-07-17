﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MalClient.Shared.Flyouts.HamburgerFlyouts
{
    public sealed partial class HamburgerAnimeFiltersFlyout : MenuFlyoutPresenter
    {
        public HamburgerAnimeFiltersFlyout()
        {
            InitializeComponent();
        }

        public void ShowAt(FrameworkElement target)
        {
            Flyout.ShowAt(target);
        }

        private void FlyoutButtonPressed(object sender, RoutedEventArgs e)
        {
            Flyout.Hide();
        }
    }
}