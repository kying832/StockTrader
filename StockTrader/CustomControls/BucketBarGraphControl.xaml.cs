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

using BucketStrategyLibrary;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace StockTrader.CustomControls
{
    public sealed partial class BucketBarGraphControl : UserControl
    {
        ObservableCollection<BarGraphDataPoint> m_data;

        public BucketBarGraphControl()
        {
            this.InitializeComponent();

            m_data = new ObservableCollection<BarGraphDataPoint>();

            this.DataContext = m_data;
        }

        public void DisplayPercentReturns(BucketStrategy strategy)
        {
            m_data.Clear();

            for(int jjj = 0; jjj < strategy.m_categories.Count(); ++jjj)
            {
                double sum = 0.0;
                int lastIndex = -1;

                // iterate through each saved window in the category
                for(int iii = 0; iii < strategy.m_categories[jjj].entries.Count(); ++iii)
                {
                    // compute the index for the percent increase which was stored at the end
                    lastIndex = strategy.m_categories[jjj].entries[iii].Count() - 1;

                    // increment the sum
                    sum += strategy.m_categories[jjj].entries[iii][lastIndex];
                }

                sum /= strategy.m_categories[jjj].entries.Count();

      //          if((jjj + 1) % 5 == 0)
                    m_data.Add(new BarGraphDataPoint() { Value = sum, CategoryName = (jjj + 1).ToString() });
        //        else
          //          m_data.Add(new BarGraphDataPoint() { Value = sum, CategoryName = " " });

            }
        }

        public void DisplayCategoryCount(BucketStrategy strategy)
        {
            m_data.Clear();

            for (int iii = 0; iii < strategy.m_categories.Count(); ++iii)
            {
                m_data.Add(new BarGraphDataPoint() { Value = strategy.m_categories[iii].entries.Count(), CategoryName = (iii + 1).ToString() });
            }
        }
    }

    public class BarGraphDataPoint
    {
        public double Value { get; set; }
        public string CategoryName { get; set; }
    }
}
