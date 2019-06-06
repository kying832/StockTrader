using IEXDataLibrary;
using StockTrader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using BackgroundTaskLibrary;
using System.Threading.Tasks;

namespace StockTrader
{
    public class TargetedStockEntry
    {
        public string ticker { get; set; }
    }

    public sealed partial class RunStrategyPage : Page
    {
        public ObservableCollection<StrategyEntry> strategyList;
        public ObservableCollection<RunningStrategy> runningStrategies;
        public ObservableCollection<TargetedStockEntry> targetedStocks;
        public ObservableCollection<TargetedStockEntry> stocksToRunStrategyOn;
        public List<TickerAutoSuggestionEntry> tickerSuggestions;

        public int m_selectedIndex;

        private ApplicationTrigger _AppTrigger;

        public RunStrategyPage()
        {
            this.InitializeComponent();

            strategyList = new ObservableCollection<StrategyEntry>();
            InitializeStrategyList();

            runningStrategies = new ObservableCollection<RunningStrategy>();
            targetedStocks = new ObservableCollection<TargetedStockEntry>();
            stocksToRunStrategyOn = new ObservableCollection<TargetedStockEntry>();

            RunningStrategiesListGrid.Visibility = Visibility.Visible;
            TradingPerformanceGrid.Visibility = Visibility.Collapsed;

            RunningBucketStrategyGrid.Visibility = Visibility.Collapsed;
            RunningSwingStrategyGrid.Visibility = Visibility.Collapsed;


            _AppTrigger = new ApplicationTrigger();

            // for debugging purposes
            UnRegisterTasks();

        }

        private void UnRegisterTasks()
        {
            var tasks = BackgroundTaskRegistration.AllTasks;
            foreach (var task in tasks)
            {
                // You can check here for the name
                string name = task.Value.Name;

                task.Value.Unregister(true);
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
            stocksToRunStrategyOn.Clear();

            LoadStrategy((StrategyEntry)e.ClickedItem);
        }

        private void LoadStrategy(StrategyEntry strategyEntry)
        {
            if (strategyEntry == null)
            {                
                return;
            }

            if (strategyEntry.TypeName == "BucketStrategy")
            {
                RunningSwingStrategyGrid.Visibility = Visibility.Collapsed;
                RunningBucketStrategyGrid.Visibility = Visibility.Visible;

                LoadBucketStrategy((BucketStrategyEntry)strategyEntry);
            }

            if (strategyEntry.TypeName == "SwingStrategy")
            {
                RunningBucketStrategyGrid.Visibility = Visibility.Collapsed;
                RunningSwingStrategyGrid.Visibility = Visibility.Visible;

                LoadSwingStrategy((SwingStrategyEntry)strategyEntry);
            }

            //machine1 and 2
        }

        private int GetIndexForStrategy(string strategyName)
        {
            int index = -1;
            for (int iii = 0; iii < MainPage.runningBucketStrategies.Count(); ++iii)
            {
                if (MainPage.runningBucketStrategies[iii].m_strategyName == strategyName)
                    index = iii;
            }

            return index;
        }

        private void LoadBucketStrategy(BucketStrategyEntry strategyEntry)
        {
            m_selectedIndex = GetIndexForStrategy(strategyEntry.StrategyName);

            if(m_selectedIndex != -1)
            {
                StrategyNameTextBlock.Text = MainPage.runningBucketStrategies[m_selectedIndex].m_strategyName;
                SlidingWindowSizeTextBlock.Text = MainPage.runningBucketStrategies[m_selectedIndex].m_slidingWindowSize_Readable;
                FutureReturnDateTextBlock.Text = MainPage.runningBucketStrategies[m_selectedIndex].m_futureReturnDate_Readable;
                SimilarityThresholdTextBlock.Text = MainPage.runningBucketStrategies[m_selectedIndex].m_similarityThreshold.ToString();
                NumberOfBucketsTextBlock.Text = MainPage.runningBucketStrategies[m_selectedIndex].m_categories.Count().ToString();  
            }
        }

        private void LoadSwingStrategy(SwingStrategyEntry strategyEntry)
        {
            
        }

        private void ShowRunningStrategiesButton_Click(object sender, RoutedEventArgs e)
        {
            if(RunningStrategiesListGrid.Visibility == Visibility.Collapsed)
            {
                TradingPerformanceGrid.Visibility = Visibility.Collapsed;
                RunningStrategiesListGrid.Visibility = Visibility.Visible;
            }
        }

        private void ShowPerformanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (TradingPerformanceGrid.Visibility == Visibility.Collapsed)
            {
                RunningStrategiesListGrid.Visibility = Visibility.Collapsed;
                TradingPerformanceGrid.Visibility = Visibility.Visible;
            }
        }

        private async void AddStrategy_Click(object sender, RoutedEventArgs e)
        {
            // validate the bucket is a valid entry
            int bucket = -1;
            if (!int.TryParse(BucketToUseTextBox.Text, out bucket))
                return;

            if (bucket < 1 || bucket > MainPage.runningBucketStrategies[m_selectedIndex].m_categories.Count())
                return;
/*
            var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (requestStatus != BackgroundAccessStatus.AlwaysAllowed)
            {
                // Depending on the value of requestStatus, provide an appropriate response
                // such as notifying the user which functionality won't work as expected
                return;
            }
*/
            // create a list of tickers to trade and populate it
            List<string> tickersToTrade = new List<string>();
            foreach (var entry in stocksToRunStrategyOn)
                tickersToTrade.Add(entry.ticker);

            RunningStrategy rs = new RunningStrategy()
            {
                m_strategyName = StrategyNameTextBlock.Text,
                m_windowSize = SlidingWindowSizeTextBlock.Text,
                m_futureReturnDate = FutureReturnDateTextBlock.Text,
                m_similarityThreshold = SimilarityThresholdTextBlock.Text,
                m_bucketInUse = int.Parse(BucketToUseTextBox.Text),
                m_startDate = DateTime.Today.Month.ToString() + "/" + DateTime.Today.Day.ToString() + "/" + DateTime.Today.Year.ToString(),
                m_numberOfTradesMade = 0,
                m_ROR = 0,
                m_tickersToTrade = tickersToTrade
            };

            if (await AddBucketStrategyToBackground(rs))
            {
                runningStrategies.Add(rs);
            }
        }

        private async Task<bool> AddBucketStrategyToBackground(RunningStrategy rs)
        {            
            // check if background task already exists
            if (BucketStrategyBackgroundExists(rs.m_strategyName))
                return false;

            // Build the background task
            var builder = new BackgroundTaskBuilder();
            builder.Name = rs.m_strategyName;
            builder.TaskEntryPoint = "BackgroundTaskLibrary.BucketStrategyBackground";
            builder.SetTrigger(_AppTrigger);
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

            BackgroundTaskRegistration task = builder.Register();

            // this is primarily for dubugging purposes
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);

            var result = await _AppTrigger.RequestAsync();

            return true;
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            int iii = 1;
        }

        private bool BucketStrategyBackgroundExists(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                    return true;
            }

            

            return false;
        }
















        private void RunningStrategiesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            targetedStocks.Clear();

            foreach(var tic in ((RunningStrategy)e.ClickedItem).m_tickersToTrade)
                targetedStocks.Add(new TargetedStockEntry() { ticker = tic.ToUpper() });
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
                foreach (var entry in stocksToRunStrategyOn)
                {
                    if (entry.ticker == AddTickerAutoSuggestBox.Text.ToUpper())
                        found = true;
                }

                if (!found)
                    stocksToRunStrategyOn.Add(new TargetedStockEntry() { ticker = AddTickerAutoSuggestBox.Text.ToUpper() });
            }
        }

        private void AddedStockListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            UIElementCollection collection = ((Grid)((Button)e.OriginalSource).Parent).Children;
            string ticker = ((TextBlock)(collection[0])).Text;

            int removeAtIndex = -1;
            for (int iii = 0; iii < stocksToRunStrategyOn.Count; ++iii)
            {
                if (stocksToRunStrategyOn[iii].ticker == ticker)
                {
                    removeAtIndex = iii;
                    break;
                }
            }

            if (removeAtIndex != -1)
                stocksToRunStrategyOn.RemoveAt(removeAtIndex);
        }
    }
}
