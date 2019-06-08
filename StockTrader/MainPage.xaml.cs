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

using SQLiteAccessLibrary;
using SQLiteLibrary;
using BucketStrategyLibrary;

namespace StockTrader
{
    public sealed partial class MainPage : Page
    {
        // persistent data for active strategies
        public static List<BucketStrategy> runningBucketStrategies;
        public static List<SwingStrategy> runningSwingStrategies;

        public MainPage()
        {
            this.InitializeComponent();

            runningBucketStrategies = new List<BucketStrategy>();
            runningSwingStrategies = new List<SwingStrategy>();

            // navigate to the last viewed page from the user's previous session
            NavigateToLastViewedPage();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBucketStrategies();
        }

        private void LoadBucketStrategies()
        {
            // get list of existing strategy names
            List<string> strategyNames = SQLiteAccess.GetBucketStrategyNames();

            // Load each strategy into the runningStrategies variables
            _BucketStrategy _bucketStrategy;
            BucketStrategy bucketStrategy;

            foreach(var name in strategyNames)
            {
                _bucketStrategy = SQLiteAccess.GetBucketStrategyGeneralInfo(name);
                bucketStrategy = new BucketStrategy(
                    _bucketStrategy.m_strategyName, 
                    null, 
                    _bucketStrategy.m_dataTimeFrame,
                    _bucketStrategy.m_slidingWindowSize, 
                    _bucketStrategy.m_futureReturnDate, 
                    _bucketStrategy.m_normalizationFunction,
                    _bucketStrategy.m_similarityThreshold,
                    true);

                bucketStrategy.LoadCategoriesFromDB();

                runningBucketStrategies.Add(bucketStrategy);
            }
        }

        public static void RemoveBucketStrategyAt(int index)
        {
            string strategyName = runningBucketStrategies[index].m_strategyName;

            SQLiteAccess.DeleteBucketStrategy(strategyName);

            runningBucketStrategies.RemoveAt(index);
        }

        private void NavigateToLastViewedPage()
        {
            string pageName;

            // if this is the first time ever running the application, navigate to the create strategy page
            if((pageName = SQLiteAccess.GetLastPageVisited()) == null)
            {
                PageFrame.Navigate(typeof(CreateStrategyPage));
                TitleTextBlock.Text = "Create Strategy";
                SQLiteAccess.SetLastViewedPage("create");
            }

            // this is not the first time running the app, so navigate to the last viewed page
            switch(pageName)
            {
                case "create":
                    PageFrame.Navigate(typeof(CreateStrategyPage));
                    TitleTextBlock.Text = "Create Strategy";
                    break;
                case "test":
                    PageFrame.Navigate(typeof(TestStrategyPage));
                    TitleTextBlock.Text = "Test Strategy";
                    break;
                case "run":
                    PageFrame.Navigate(typeof(RunStrategyPage));
                    TitleTextBlock.Text = "Run Strategy";
                    break;
                default:
                    PageFrame.Navigate(typeof(CreateStrategyPage));
                    TitleTextBlock.Text = "Create Strategy";
                    break;
            }
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
                    SQLiteAccess.SetLastViewedPage("create");
                    MenuSplitView.IsPaneOpen = false;
                }
            }
            else if (TestStrategyListBoxItem.IsSelected)
            {
                if (!(PageFrame.CurrentSourcePageType == typeof(TestStrategyPage)))
                {
                    PageFrame.Navigate(typeof(TestStrategyPage));
                    TitleTextBlock.Text = "Test Strategy";
                    SQLiteAccess.SetLastViewedPage("test");
                    MenuSplitView.IsPaneOpen = false;
                }
            }
            else if (RunStrategyListBoxItem.IsSelected)
            {
                if (!(PageFrame.CurrentSourcePageType == typeof(RunStrategyPage)))
                {
                    PageFrame.Navigate(typeof(RunStrategyPage));
                    TitleTextBlock.Text = "Run Strategy";
                    SQLiteAccess.SetLastViewedPage("run");
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
