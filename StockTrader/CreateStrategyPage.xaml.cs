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
using BucketStrategyLibrary;

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


        //swing
        ObservableCollection<AddedStock> SwingaddedStockList;



        public CreateStrategyPage()
        {
            this.InitializeComponent();

            // data used for bucket strategy
            addedStockList = new ObservableCollection<AddedStock>();


            // data used for Machine Learning Strategy #1


            // data used for Machine Learning Strategy #2

            //swing trading
            SwingaddedStockList = new ObservableCollection<AddedStock>();

            // other initialization features
            StrategySelectionBucketStrategy.IsSelected = true;

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
           // MachineLearningStrategy2Grid.Visibility = Visibility.Collapsed;
            SwingStradingStrategyGrid.Visibility = Visibility.Collapsed;

            // Now only show the one corresponding the to selected choice
            if (StrategySelectionBucketStrategy.IsSelected)
                BucketStrategyGrid.Visibility = Visibility.Visible;
            else if (StrategySelectionMLStrategy1.IsSelected)
                MachineLearningStrategy1Grid.Visibility = Visibility.Visible;
            //else if (StrategySelectionMLStrategy2.IsSelected)
            //    MachineLearningStrategy2Grid.Visibility = Visibility.Visible;
            else if (StrategySwingTradingStrategy.IsSelected)
                SwingStradingStrategyGrid.Visibility = Visibility.Visible;
            else        
                BucketStrategyGrid.Visibility = Visibility.Visible;             // simply default to this page
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


        private void SwingAddedStockListRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            UIElementCollection collection = ((Grid)((Button)e.OriginalSource).Parent).Children;
            string ticker = ((TextBlock)(collection[0])).Text;

            int removeAtIndex = -1;
            for (int iii = 0; iii < SwingaddedStockList.Count; ++iii)
            {
                if (SwingaddedStockList[iii].Ticker == ticker)
                {
                    removeAtIndex = iii;
                    break;
                }
            }

            if (removeAtIndex != -1)
                SwingaddedStockList.RemoveAt(removeAtIndex);
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
            if (!string.IsNullOrEmpty(AddTickerAutoSuggestBox.Text)&& (StrategySelectionBucketStrategy.IsSelected))
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



            //swing
            if (!string.IsNullOrEmpty(AddTickerAutoSuggestBoxSwing.Text)&& (StrategySwingTradingStrategy.IsSelected))
            {
                bool found = false;
                foreach (var entry in SwingaddedStockList)
                {
                    if (entry.Ticker == AddTickerAutoSuggestBoxSwing.Text.ToUpper())
                        found = true;
                }

                if (!found)
                    SwingaddedStockList.Add(new AddedStock(AddTickerAutoSuggestBoxSwing.Text.ToUpper()));
            }
        }



        private void RunBucketStrategyButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> errorMessages = new List<string>();

            // Validate the name of the strategy is not empty and not already used
            if (BucketStrategyNameTextBox.Text == string.Empty)
                errorMessages.Add("Error: Strategy name must not be left empty.");
            else if(BucketStrategyNameTextBox.Text.Length > 50)
                errorMessages.Add("Error: Strategy name cannot be more than 50 characters.");
            else if (SQLiteAccess.BucketStrategyExists(BucketStrategyNameTextBox.Text))
                errorMessages.Add("Error: Strategy name already exists. Please use a different name.");

            // Verify that at least one valid ticker has been specified
            if(addedStockList.Count() <= 0)
                errorMessages.Add("Error: You must select at least one stock to add to the strategy.");

            // Verify that data to gather has been entered
            if (((ComboBoxItem)BucketStrategyTimeFrameComboBox.SelectedValue) == null)
                errorMessages.Add("Error: You must select how far back to gather data.");

            // Verify that time frame has been specified
            if (((ComboBoxItem)BucketStrategyFutureReturnComboBox.SelectedValue) == null)
                errorMessages.Add("Error: You must select how far into the future to predict the return.");

            // Verify that the normalization function is specified
            if (((ComboBoxItem)BucketStrategyNormalizationFunctionComboBox.SelectedValue) == null)
                errorMessages.Add("Error: You must select a normalization function to use.");

            // Verify similarity threshold is specified and in valid range

            if(!float.TryParse(BucketStrategySimilarityThresholdTextBox.Text, out float threshold))
                errorMessages.Add("Error: The threshold value was not recognized as a valid number.");
            else if (threshold < 0.5f || threshold > 1.0f)
                errorMessages.Add("Error: The threshold value must be between 0.5 and 1.0");

            // if error, display error to user
            if (errorMessages.Count() > 0)
            {
                ErrorMessageTextBlock.Text = "";
                foreach(var entry in errorMessages)
                    ErrorMessageTextBlock.Text += (entry + '\n');
            }
            else // run strategy
            {
                ErrorMessageTextBlock.Text = "";
                RunBucketStrategy();
            }
        }

        private async void RunBucketStrategy()
        {
            string strategyName          = BucketStrategyNameTextBox.Text;
            string dataTimeFrame         = (string)((ComboBoxItem)BucketStrategyTimeFrameComboBox.SelectedValue).Content;
            string slidingWindowSize     = (string)((ComboBoxItem)BucketStrategyWindowSizeComboBox.SelectedValue).Content;
            string futureReturnDate      = (string)((ComboBoxItem)BucketStrategyFutureReturnComboBox.SelectedValue).Content;
            string normalizationFunction = (string)((ComboBoxItem)BucketStrategyNormalizationFunctionComboBox.SelectedValue).Content;
            float.TryParse(BucketStrategySimilarityThresholdTextBox.Text, out float similarityThreshold);

            List<string> tickerList = new List<string>();
            foreach (var ticker in addedStockList)
                tickerList.Add(ticker.Ticker);

            // can display a loading page

            // can make this multi-threaded
            ErrorMessageTextBlock.Text = "Running...";
            MainPage.runningBucketStrategies.Add(new BucketStrategy(strategyName, tickerList, dataTimeFrame, slidingWindowSize, futureReturnDate, normalizationFunction, similarityThreshold));
            await MainPage.runningBucketStrategies[MainPage.runningBucketStrategies.Count() - 1].Create();
            ErrorMessageTextBlock.Text = "Finished";
        }

        //swing
        private void RunSwingStrategyButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> errorMessages = new List<string>();

            // Validate the name of the strategy is not empty and not already used
            if (SwingStrategyNameTextBox.Text == string.Empty)
                errorMessages.Add("Error: Strategy name must not be left empty.");
            else if (SwingStrategyNameTextBox.Text.Length > 50)
                errorMessages.Add("Error: Strategy name cannot be more than 50 characters.");
            else if (SQLiteAccess.SwingStrategyExists(SwingStrategyNameTextBox.Text))
                errorMessages.Add("Error: Strategy name already exists. Please use a different name.");

            // Verify that at least one valid ticker has been specified
            if (SwingaddedStockList.Count() <= 0)
                errorMessages.Add("Error: You must select at least one stock to add to the strategy.");
            //may change this later but doesnt make logic sense to look at more than 1
            if (SwingaddedStockList.Count() >= 2)
                errorMessages.Add("Error: You must add only one stock to the strategy.");

            // Verify that data to gather has been entered
            if (((ComboBoxItem)DaySelectionForSwing.SelectedValue) == null)
                errorMessages.Add("Error: You must select how many days for the swing.");

            // if error, display error to user
            if (errorMessages.Count() > 0)
            {
                ErrorMessageTextBlockSwing.Text = "";
                foreach (var entry in errorMessages)
                    ErrorMessageTextBlockSwing.Text += (entry + '\n');
            }
            else // run strategy
            {
                ErrorMessageTextBlockSwing.Text = "";
                RunStrategySwing();
            }
        }

        private async void RunStrategySwing()
        {
            string strategyName = SwingStrategyNameTextBox.Text;
            string daysToAnalyze = (string)((ComboBoxItem)DaySelectionForSwing.SelectedValue).Content;

            List<string> tickerList = new List<string>();
            foreach (var ticker in SwingaddedStockList)
                tickerList.Add(ticker.Ticker);

            ErrorMessageTextBlockSwing.Text = "Running...";
            MainPage.runningSwingStrategies.Add(new SwingStrategy(strategyName, tickerList, daysToAnalyze));
            await MainPage.runningSwingStrategies[MainPage.runningSwingStrategies.Count() - 1].RunSwing();
            ErrorMessageTextBlockSwing.Text = "Finished";



        }
        /*
            this function will provide the user interface to get predictions
             */

        private async void submit_button_Click(object sender, RoutedEventArgs e)
        {
            //get text from input box
            String text = Input_ticker.Text;
            //pass text to ML interface
            int result = await ML_Model.ML_interface.GetPrediction(text);
            if (result == 0)
            {
                //print ignore signal
                Result_display.Text = "Ignore/Sell";
            }
            else if (result == 1)
            {
                //print buy signal
                Result_display.Text = "Buy/Hold";
            }
        }


        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void DaySelectionForSwingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        

        private void BucketStrategyFutureReturnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Input_ticker_GotFocus(object sender, RoutedEventArgs e)
        {
            Input_ticker.Text = string.Empty;
            Input_ticker.GotFocus -= Input_ticker_GotFocus;
        }

        private void Input_ticker_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }
    }
}
