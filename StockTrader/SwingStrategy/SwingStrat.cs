using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLiteAccessLibrary;
using IEXDataLibrary;



    /*
     * • User Inputs Stock Ticker
    • User Selects how many days the want to evaluate for the swing (Investopedia.com suggests 20 days)
    • My Software interacts with E-Trade for data
    • Calculates 20 day moving average (or user input) and compared to current
    moving average (5 days for the current week) does not change.
    • Compare averages, if 5 day moving average is < 20 day ,that is a buy
    • Aka long position
    • If long, sell in 10-15 days. Check on those days • ERROR if quarterly results are coming out. Looking for stock trends
    • If 20 day > 5 day, sell
    • Aka short sell in 10-15 days. Check on those days • ERROR if quarterly results are coming out. Looking for stock trends
    */




    namespace StockTrader
{




    public struct StockAverage
    {
        public string Ticker;
        public double Average;
        public StockAverage(string tick, double ave)
        {
            Ticker = tick;
            Average = ave;
        }
    }
    public class SwingStrategy
    {
        //variables to set
        public string s_stratName;
        public List<string> s_tickers;
        public int s_days;



        public SwingStrategy(string sN, List<string> t, string daysToAnalyze)
        {
            s_stratName = sN;
            s_tickers = new List<string>();
            foreach (var ticker in t)
                s_tickers.Add(ticker);
            s_days = buttonToDays(daysToAnalyze);


            SQLiteAccess.AddSwingStrategy(s_stratName, s_tickers, s_days);

            RunSwing();
        }

        //Helper to conver to days
        //Api only lets you pull months, cant do 7 days for example


        int buttonToDays(string inputString)
        {
            switch (inputString)
            {
                case "10 Days": return 10;
                case "15 Days": return 15;
                case "20 Days": return 20;
                case "25 Days": return 25;
                case "30 Days": return 30;
                case "35 Days": return 35;
                case "40 Days": return 40;
                default: return 20;
            }
        }

        //run swing
  
        private async Task RunSwing()
        {
            List<List<ThreeMonthData>> allSwingData = new List<List<ThreeMonthData>>();

            foreach (string ticker in s_tickers)
                allSwingData.Add(await GetStockDataThreeMonth(ticker));


            //find average based on high, low, open, close
            int ticker_count=s_tickers.Count();
            int numDays = s_days;

            List<StockAverage> averages = new List<StockAverage>();

            for (int i = 0; i< s_tickers.Count(); i++)
            {
                double sum = 0;
                for (int j = 0; j< numDays; j++)
                {
                    double tempSum=0;
                    tempSum += allSwingData[i][j].high;
                    tempSum += allSwingData[i][j].low;
                    tempSum += allSwingData[i][j].open;
                    tempSum += allSwingData[i][j].close;
                    tempSum /= 4;
                    sum = tempSum;
                }
                sum /= numDays;
                averages.Add(new StockAverage(s_tickers[i], sum));

            }

            //need to finish this, getting a database double add error
            //Mike continued here
            //return Task<averages>;
        }




        //get the information for each stock
        async Task<List<ThreeMonthData>> GetStockDataThreeMonth(string ticker)
        {
            List<ThreeMonthData> TotalThreeMonthData = await IEXDataAccess.GetThreeMonthData( ticker );
            return TotalThreeMonthData;

        }
    }
}
