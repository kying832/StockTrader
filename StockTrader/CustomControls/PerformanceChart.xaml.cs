using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace StockTrader.CustomControls
{
    public sealed partial class PerformanceChart : UserControl
    {
        ObservableCollection<LineSeriesDataPoint> m_data;

        public PerformanceChart()
        {
            this.InitializeComponent();

            m_data = new ObservableCollection<LineSeriesDataPoint>();

            this.DataContext = m_data;
        }

        public void DisplayROR(List<StockPurchaseInfo> purchaseRecord)
        {
            m_data.Clear();

            // insert 0 for the ROR the day before the first entry
            m_data.Add(new LineSeriesDataPoint() { date = GetDateForTheDayBefore(purchaseRecord[0].date), value = 0.0 });

            for(int iii = 0; iii < purchaseRecord.Count(); ++iii)
                m_data.Add(new LineSeriesDataPoint() { date = purchaseRecord[iii].date, value = ((((m_data[iii].value / 100) + 1) * (1 + purchaseRecord[iii].percentReturn)) - 1) * 100 });
        }

        private string GetDateForTheDayBefore(string date)
        {
            DateTime dateTime = DateTime.Parse(date);

            if (dateTime.Day == 1)
            {
                if (dateTime.Month == 1)
                    return (dateTime.Year - 1).ToString() + "-12-31";

                if (dateTime.Month == 3)
                    return dateTime.Year.ToString() + "2-28";

                if (dateTime.Month == 2 || dateTime.Month == 4 || dateTime.Month == 6 || dateTime.Month == 8 || dateTime.Month == 11)
                    return dateTime.Year.ToString() + "-" + (dateTime.Month - 1).ToString() + "-31";
                else
                    return dateTime.Year.ToString() + "-" + (dateTime.Month - 1).ToString() + "-30";
            }

            return dateTime.Year.ToString() + "-" + dateTime.Month.ToString() + "-" + (dateTime.Day - 1).ToString();
        }
    }

    public class LineSeriesDataPoint
    {
        public string date { get; set; }
        public double value { get; set; }
    }
}
