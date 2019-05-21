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
        ObservableCollection<AddedStock> addedStockList;
        List<TickerAutoSuggestionEntry> tickerSuggestions;

        public TestStrategyPage()
        {
            this.InitializeComponent();

            addedStockList = new ObservableCollection<AddedStock>();

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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryNumberTextBox.PlaceholderText = "1 - " + NumberOfCategories.Text;

            tickerSuggestions = await TickerAutoSuggestionEntryManager.GetTickerAutoSuggestionEntriesList();
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

            // Load the chart data
            BucketBarGraph.Display(MainPage.runningBucketStrategies[index]);
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


        private void RunTestButton_Click(object sender, RoutedEventArgs e)
        {
            RunButtonGrid.Visibility = Visibility.Collapsed;
            RunTestGrid.Visibility = Visibility.Visible;
        }



        private void AddTickerAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // verify that the reason the text was changed was because the user entered input
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && tickerSuggestions != null)
            {
                // create an array of tickers that begin with the text the user has entered
                var tickers = tickerSuggestions
                    .Where(p => p.symbol.StartsWith(sender.Text.ToUpper()))
                    .Select(p => p.symbol)
                    .ToArray();

                // create a list of strings to hold the final output
                List<string> finalItems = new List<string>();

                // for each ticker, append the company name and add it to the list of final items
                foreach (var ticker in tickers)
                {
                    string name = tickerSuggestions.Where(p => p.symbol.Equals(ticker)).ToArray()[0].name;

                    string fullEntry = String.Format("{0} - {1}", ticker, name);

                    finalItems.Add(fullEntry);
                }

                // show only the top five items from the possible list
                sender.ItemsSource = finalItems.Take(5);
            }
        }

        private void AddTickerAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string[] tokens = ((string)args.SelectedItem).Split(' ');
            sender.Text = tokens[0];
        }

        private void AddTickerAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(AddTickerAutoSuggestBox.Text))
            {
                bool found = false;
                foreach (var entry in addedStockList)
                {
                    if (entry.Ticker == AddTickerAutoSuggestBox.Text.ToUpper())
                        found = true;
                }

                if (!found)
                    addedStockList.Add(new AddedStock(AddTickerAutoSuggestBox.Text.ToUpper()));
            }           
        }

        private void AddedStockListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            UIElementCollection collection = ((Grid)((Button)e.OriginalSource).Parent).Children;
            string ticker = ((TextBlock)(collection[0])).Text;

            int removeAtIndex = -1;
            for (int iii = 0; iii < addedStockList.Count; ++iii)
            {
                if (addedStockList[iii].Ticker == ticker)
                {
                    removeAtIndex = iii;
                    break;
                }
            }

            if (removeAtIndex != -1)
                addedStockList.RemoveAt(removeAtIndex);
        }

        private void ResetTestButton_Click(object sender, RoutedEventArgs e)
        {
            addedStockList.Clear();
            CategoryNumberTextBox.Text = "";

            RunTestGrid.Visibility = Visibility.Collapsed;
            RunButtonGrid.Visibility = Visibility.Visible;
        }


    }
}
