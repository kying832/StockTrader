using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

using StockTrader.Models;
using System.Collections.ObjectModel;
using IEXDataLibrary;
using SQLiteAccessLibrary;

namespace StockTrader
{
    public sealed partial class TestStrategyPage : Page
    {
        ObservableCollection<BucketStrategyEntry> bucketStrategyList;

        public TestStrategyPage()
        {
            this.InitializeComponent();

            bucketStrategyList = new ObservableCollection<BucketStrategyEntry>();
            InitializeBucketStrategyList();

            if (bucketStrategyList.Count == 0)
            {
                TestStrategyPageHeader.Visibility = Visibility.Collapsed;
                TestGrid.Visibility = Visibility.Collapsed;
                SummaryGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                TestStrategyPageHeader.Visibility = Visibility.Visible;
                LoadStrategy(bucketStrategyList[0]);
            }
        }

        private void InitializeBucketStrategyList()
        {
            // add each strategy name to the list
            foreach (var entry in MainPage.runningBucketStrategies)
                bucketStrategyList.Add(new BucketStrategyEntry(entry.m_strategyName));
        }

        private void StrategiesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            LoadStrategy((BucketStrategyEntry)e.ClickedItem);
        }

        private void LoadStrategy(BucketStrategyEntry strategyEntry)
        {
            if(strategyEntry == null)
            {
                TestStrategyPageHeader.Visibility = Visibility.Collapsed;
                TestGrid.Visibility = Visibility.Collapsed;
                SummaryGrid.Visibility = Visibility.Collapsed;
                return;
            }

            SelectedStrategyTextBlock.Text = strategyEntry.BucketStrategyName;

            TestGrid.Visibility = Visibility.Collapsed;
            SummaryGrid.Visibility = Visibility.Visible;

            // Load the strategy summary ============================================

            // get the index of the strategy to load
            int index = -1;
            for(int iii = 0; iii < MainPage.runningBucketStrategies.Count; ++iii)
            {
                if (MainPage.runningBucketStrategies[iii].m_strategyName == strategyEntry.BucketStrategyName)
                {
                    index = iii;
                    break;
                }
            }

            // get the total number of categories
            NumberOfCategories.Text = MainPage.runningBucketStrategies[index].m_categories.Count.ToString();

            // largest and average category size
            int maxCategory = 0;
            double avgCategory = 0;

            foreach (var entry in MainPage.runningBucketStrategies[index].m_categories)
            {
                avgCategory += entry.entries.Count();

                if (entry.entries.Count() > maxCategory)
                    maxCategory = entry.entries.Count();
            }

            avgCategory /= MainPage.runningBucketStrategies[index].m_categories.Count;

            LargestCategorySize.Text = maxCategory.ToString();
            AverageCategorySize.Text = avgCategory.ToString();

        }

        private void ShowStrategySummaryButton_Click(object sender, RoutedEventArgs e)
        {
            if(SummaryGrid.Visibility == Visibility.Collapsed)
            {
                TestGrid.Visibility = Visibility.Collapsed;
                SummaryGrid.Visibility = Visibility.Visible;
            }
        }

        private void ShowTestPageButton_Click(object sender, RoutedEventArgs e)
        {
            if(TestGrid.Visibility == Visibility.Collapsed)
            {
                SummaryGrid.Visibility = Visibility.Collapsed;
                TestGrid.Visibility = Visibility.Visible;
            }
        }

        private void DeleteStrategy_Click(object sender, RoutedEventArgs e)
        {
            // get index for the strategy to remove
            int index = -1;
            for(int iii = 0; iii < bucketStrategyList.Count; ++iii)
            {
                if(bucketStrategyList[iii].BucketStrategyName == SelectedStrategyTextBlock.Text)
                {
                    index = iii;
                    break;
                }
            }

            // remove the strategy from the list and from main page list
            bucketStrategyList.RemoveAt(index);
            MainPage.runningBucketStrategies.RemoveAt(index);

            // Load the new first element on the list if there is one
            if (bucketStrategyList.Count == 0)
                LoadStrategy(null);
            else
                LoadStrategy(bucketStrategyList[0]);
        }
    }
}
