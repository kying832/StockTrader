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
    public sealed partial class CreateStrategyPage : Page
    {
        public CreateStrategyPage()
        {
            this.InitializeComponent();
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
    }
}
