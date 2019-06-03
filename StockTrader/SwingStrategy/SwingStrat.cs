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






 

    public class SwingStrategy
    {
        //variables to set
        public string s_stratName;
        public List<string> s_tickers;
        public int s_days;
        public double s_average;
        public double s_MovingAverage;
        public int s_buy;
        public int s_numStocks;


        public SwingStrategy(string sN, List<string> t, string daysToAnalyze)
        {
            s_stratName = sN;
            s_tickers = new List<string>();
            foreach (var ticker in t)
                s_tickers.Add(ticker);
            s_days = buttonToDays(daysToAnalyze);


            //SQLiteAccess.AddSwingStrategy(s_stratName, s_tickers, s_days);

            //RunSwing();
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
  
        public async Task RunSwing()
        {
            List<List<ThreeMonthData>> allSwingData = new List<List<ThreeMonthData>>();

            foreach (string ticker in s_tickers)
                allSwingData.Add(await GetStockDataThreeMonth(ticker));


            //find average based on high, low, open, close
            int ticker_count=s_tickers.Count();
            int numDays = s_days;


            s_numStocks = ticker_count;


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
                    sum += tempSum;
                }

                //calculate 5 day moving average
                double movingaverage = 0;
                for (int k = 0; k < 5; k++)
                {
                    double tempSum = 0;
                    tempSum += allSwingData[i][k].high;
                    tempSum += allSwingData[i][k].low;
                    tempSum += allSwingData[i][k].open;
                    tempSum += allSwingData[i][k].close;
                    tempSum /= 4;
                    movingaverage += tempSum;
                }
          
                //average of total month
                sum /= numDays;
                s_average = sum;
                //normalize to 5 days
                movingaverage /= 5;
                s_MovingAverage = movingaverage;



                //now average and moving average have been compared
                //now evaluate swing
                //Compare averages, if 5 day moving average is < 20 day ,that is a buy
                if (movingaverage * .85 > sum)
                { s_buy = 1; }
                else if (movingaverage * .85 < sum)
                { s_buy = 0; }

   
            }


        }


        //get the information for each stock
        async Task<List<ThreeMonthData>> GetStockDataThreeMonth(string ticker)
        {
            List<ThreeMonthData> TotalThreeMonthData = await IEXDataAccess.GetThreeMonthData( ticker );
            return TotalThreeMonthData;

        }
    }
}
