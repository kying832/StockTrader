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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StockTrader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestStrategyPage : Page
    {
        public TestStrategyPage()
        {
            this.InitializeComponent();
 master

            addedStockList = new ObservableCollection<AddedStock>();
            //we need away to differentiat between swing/bucket machine 1 and 2

            strategyList = new ObservableCollection<StrategyEntry>();
            InitializeStrategyList();       

            if (strategyList.Count == 0)
            {
                TestStrategyPageHeader.Visibility = Visibility.Collapsed;
                TestGrid.Visibility = Visibility.Collapsed;
                SummaryGrid.Visibility = Visibility.Collapsed;
                DisplaySwingStrategySummary.Visibility = Visibility.Collapsed;
            }
            else
            {
                TestStrategyPageHeader.Visibility = Visibility.Visible;
                LoadStrategy(strategyList[0]);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            tickerSuggestions = await TickerAutoSuggestionEntryManager.GetTickerAutoSuggestionEntriesList();
        }

        private void InitializeStrategyList()
        {
            // add each bucket strategy name to the list
            foreach (var entry in MainPage.runningBucketStrategies)
                strategyList.Add(new BucketStrategyEntry(entry.m_strategyName));

            // also add swing strategies
            foreach (var entry in MainPage.runningSwingStrategies)
                strategyList.Add(new SwingStrategyEntry(entry.s_stratName));
        }

        private void StrategiesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            LoadStrategy((StrategyEntry)e.ClickedItem);            
        }

        private void LoadStrategy(StrategyEntry strategyEntry)
        {
            if(strategyEntry == null)
            {
                TestStrategyPageHeader.Visibility = Visibility.Collapsed;
                TestGrid.Visibility = Visibility.Collapsed;
                SummaryGrid.Visibility = Visibility.Collapsed;
                DisplaySwingStrategySummary.Visibility = Visibility.Collapsed;
                return;
            }

            if (strategyEntry.TypeName == "BucketStrategy")
                LoadBucketStrategy((BucketStrategyEntry)strategyEntry);

            if (strategyEntry.TypeName == "SwingStrategy")
                LoadSwingStrategy((SwingStrategyEntry)strategyEntry);

            //machine1 and 2
        }

        private void LoadBucketStrategy(BucketStrategyEntry strategyEntry)
        {
            DisplaySwingStrategySummary.Visibility = Visibility.Collapsed;
            DisplayBucketStrategySummary.Visibility = Visibility.Visible;

            SelectedStrategyTextBlock.Text = strategyEntry.StrategyName;
            DisplaySwingStrategySummary.Visibility = Visibility.Collapsed;
            TestGrid.Visibility = Visibility.Collapsed;

            SummaryGrid.Visibility = Visibility.Visible;

            // Load the strategy summary ============================================

            // get the index of the strategy to load
            currentbucketStrategyIndex = -1;
            for(int iii = 0; iii < MainPage.runningBucketStrategies.Count; ++iii)
            {
                if (MainPage.runningBucketStrategies[iii].m_strategyName == strategyEntry.StrategyName)
                {
                    currentbucketStrategyIndex = iii;
                    break;
                }
            }

            // get the total number of categories
            NumberOfCategories.Text = MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_categories.Count.ToString();

            // largest and average category size
            int maxCategory = 0;
            double avgCategory = 0;

            foreach (var entry in MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_categories)
            {
                avgCategory += entry.entries.Count();

                if (entry.entries.Count() > maxCategory)
                    maxCategory = entry.entries.Count();
            }

            avgCategory /= MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_categories.Count;

            LargestCategorySize.Text = maxCategory.ToString();
            AverageCategorySize.Text = avgCategory.ToString();

            // Load the chart data
            BucketBarGraph.DisplayPercentReturns(MainPage.runningBucketStrategies[currentbucketStrategyIndex]);
            BucketBarGraphCount.DisplayCategoryCount(MainPage.runningBucketStrategies[currentbucketStrategyIndex]);


            // Load info for the test page
            CategoryNumberTextBox.PlaceholderText = "1 - " + MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_categories.Count.ToString();
        }

        private void LoadSwingStrategy(SwingStrategyEntry strategyEntry)
        {
            SelectedStrategyTextBlock.Text = strategyEntry.StrategyName;

            //turns on grid
            SummaryGrid.Visibility = Visibility.Collapsed;
            TestGrid.Visibility = Visibility.Collapsed;
            DisplaySwingStrategySummary.Visibility = Visibility.Visible;

            //load the strat, variable list in main


            currentSwingIndex = -1;
            for (int i = 0; i < MainPage.runningSwingStrategies.Count; ++i)
            {
                if (MainPage.runningSwingStrategies[i].s_stratName == strategyEntry.StrategyName)
                {
                    currentSwingIndex = i;
                    break;
                }
            }
            //int currentNumStock;
            //currentNumStock=MainPage.runningSwingStrategies[currentSwingIndex].s_numStocks;

            SwingTickerAnswer.Text = MainPage.runningSwingStrategies[currentSwingIndex].s_tickers[0].ToString();
            SwingNumberOfDays.Text = MainPage.runningSwingStrategies[currentSwingIndex].s_days.ToString();
            SwingAverage.Text = MainPage.runningSwingStrategies[currentSwingIndex].s_average.ToString();
            SwingMovingAverage.Text = MainPage.runningSwingStrategies[currentSwingIndex].s_MovingAverage.ToString();

            if (MainPage.runningSwingStrategies[currentSwingIndex].s_buy == 1)
            {
                SwingBuy.Text = "Yes, buy";
            }
            else if (MainPage.runningSwingStrategies[currentSwingIndex].s_buy == 0)
            {
                SwingBuy.Text = "Do not buy";
            }
            else
                SwingBuy.Text = "Error, cannot be determined at this time";
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
            for(int iii = 0; iii < strategyList.Count; ++iii)
            {
                if(strategyList[iii].StrategyName == SelectedStrategyTextBlock.Text)
                {
                    index = iii;
                    break;
                }
            }

            if (strategyList[index].TypeName == "BucketStrategy")
            {
                for (int iii = 0; iii < MainPage.runningBucketStrategies.Count; ++iii)
                {
                    if(MainPage.runningBucketStrategies[iii].m_strategyName == strategyList[index].StrategyName)
                    {
                        MainPage.RemoveBucketStrategyAt(iii);
                        break;
                    }
                }
            }
            // else if it is a swing strategy, remove from that list instead
            if (strategyList[index].TypeName == "SwingStrategy")
            {
                for (int i = 0; i < MainPage.runningSwingStrategies.Count; ++i)
                {
                    if (MainPage.runningSwingStrategies[i].s_stratName == strategyList[index].StrategyName)
                    {
                        MainPage.runningSwingStrategies.RemoveAt(i);
                        break;
                    }
                }
            }
            // remove the strategy from the list
            strategyList.RemoveAt(index);

            // Load the new first element on the list if there is one
            if (strategyList.Count == 0)
                LoadStrategy(null);
            else
                LoadStrategy(strategyList[0]);
        }
        
        private async void RunTestButton_Click(object sender, RoutedEventArgs e)
        {
            RunButtonGrid.Visibility = Visibility.Collapsed;
            RunTestGrid.Visibility = Visibility.Visible;

            int categoryIndex = int.Parse(CategoryNumberTextBox.Text) - 1;

            List<string> tickers = new List<string>();
            foreach (var stock in addedStockList)
                tickers.Add(stock.Ticker);

            string duration = (string)((ComboBoxItem)((ComboBox)BackTestTimeFrameComboBox).SelectedValue).Content;



            // determine which strategy is selected then run back test
            await MainPage.runningBucketStrategies[currentbucketStrategyIndex].BackTest(categoryIndex, tickers, duration);

            // RORTextBlock.Text = MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_ROR.ToString();
            // TotalBuysTextBlock.Text = MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_totalBuys.ToString();

            RORPerformanceChart.DisplayROR(MainPage.runningBucketStrategies[currentbucketStrategyIndex].m_backtestPurchaseRecord);
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

            MainPage.runningBucketStrategies[currentbucketStrategyIndex].ResetBackTestPurchaseRecords();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void StrategiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {


 master
        }
    }
}
