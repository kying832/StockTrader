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
    public sealed partial class MLPage : Page
    {
        public MLPage()
        {
            this.InitializeComponent();
        }

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
            else if (result == -1)
            {
                Result_display.Text = "Invalid Ticker";
            }
        }

        private void back_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }
    }
}
