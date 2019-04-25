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

using StockTrader.Models;
using System.Collections.ObjectModel;
using IEXDataLibrary;
using SQLiteAccessLibrary;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StockTrader
{
    public sealed partial class CreateStrategyPage : Page
    {
        // data used for bucket strategy
        ObservableCollection<AddedStock> addedStockList;
        List<TickerAutoSuggestionEntry> tickerSuggestions;


        // data used for Machine Learning Strategy #1


        // data used for Machine Learning Strategy #2


        public CreateStrategyPage()
        {
            this.InitializeComponent();

            // data used for bucket strategy
            addedStockList = new ObservableCollection<AddedStock>();

            
            // data used for Machine Learning Strategy #1


            // data used for Machine Learning Strategy #2

        }

        private async void CreateStrategyPage_Loaded(object sender, RoutedEventArgs e)
        {
            tickerSuggestions = await TickerAutoSuggestionEntryManager.GetTickerAutoSuggestionEntriesList();
        }


        private void StrategySelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Make each page invisible to start
            BucketStrategyGrid.Visibility = Visibility.Collapsed;
            MachineLearningStrategy1Grid.Visibility = Visibility.Collapsed;
            MachineLearningStrategy2Grid.Visibility = Visibility.Collapsed;

            // Now only show the one corresponding the to selected choice
            if (StrategySelectionBucketStrategy.IsSelected)
                BucketStrategyGrid.Visibility = Visibility.Visible;
            else if (StrategySelectionMLStrategy1.IsSelected)
                MachineLearningStrategy1Grid.Visibility = Visibility.Visible;
            else if (StrategySelectionMLStrategy2.IsSelected)
                MachineLearningStrategy2Grid.Visibility = Visibility.Visible;
            else        
                BucketStrategyGrid.Visibility = Visibility.Visible;             // simply default to this page
        }

        private void BucketStrategyTimeFrameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BucketStrategyNormalizationFunctionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BucketStrategyFutureReturnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AddTickerBucketStrategyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddedStockListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            UIElementCollection collection = ((Grid)((Button)e.OriginalSource).Parent).Children;
            string ticker = ((TextBlock)(collection[0])).Text;

            int removeAtIndex = -1;
            for (int iii = 0; iii < addedStockList.Count; ++iii)
            {
                if(addedStockList[iii].Ticker == ticker)
                {
                    removeAtIndex = iii;
                    break;
                }
            }

            if (removeAtIndex != -1)
                addedStockList.RemoveAt(removeAtIndex);
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
                foreach(var entry in addedStockList)
                {
                    if (entry.Ticker == AddTickerAutoSuggestBox.Text.ToUpper())
                        found = true;
                }

                if(!found)
                    addedStockList.Add(new AddedStock(AddTickerAutoSuggestBox.Text.ToUpper()));
            }
        }
    }
}
