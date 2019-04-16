using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace StockTrader
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Navigate to the Create Strategy page when the app loads --> THIS COULD BE THE LAST PAGE THE USER WAS ON IF WE SAVE INFO IN SQLITE
            PageFrame.Navigate(typeof(CreateStrategyPage));
            TitleTextBlock.Text = "Create Strategy";
        }

        /* **************************************************************************
         * When the hamburger button is clicked, either extend or contract the pane.
         * ************************************************************************** */
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        /* **************************************************************************
         * When a new menu selection is made, navigate to the appropriate page.
         * ************************************************************************** */
        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check each list box item to see which is selected
            if (CreateStrategyListBoxItem.IsSelected)
            {
                // If the user selected the same page as the one they are on,
                // do not navigate or make any changes
                if (!(PageFrame.CurrentSourcePageType == typeof(CreateStrategyPage)))
                {
                    PageFrame.Navigate(typeof(CreateStrategyPage));
                    TitleTextBlock.Text = "Create Strategy";
                    MenuSplitView.IsPaneOpen = false;
                }
            }
            else if (TestStrategyListBoxItem.IsSelected)
            {
                if (!(PageFrame.CurrentSourcePageType == typeof(TestStrategyPage)))
                {
                    PageFrame.Navigate(typeof(TestStrategyPage));
                    TitleTextBlock.Text = "Test Strategy";
                    MenuSplitView.IsPaneOpen = false;
                }
            }
            else if (RunStrategyListBoxItem.IsSelected)
            {
                if (!(PageFrame.CurrentSourcePageType == typeof(RunStrategyPage)))
                {
                    PageFrame.Navigate(typeof(RunStrategyPage));
                    TitleTextBlock.Text = "Run Strategy";
                    MenuSplitView.IsPaneOpen = false;
                }
            }           
        }

        /* **************************************************************************
         * When the mouse hovers over the menu, expand the pane.
         * ************************************************************************** */
        private void Menu_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = true;
        }

        /* **************************************************************************
         * When the mouses ceases to hover over the menu, close the pane.
         * ************************************************************************** */
        private void Menu_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = false;
        }
    }
}
