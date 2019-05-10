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
        public string s_days; 
    


    public SwingStrategy(string sN, List<string> t, string daysToAnalyze)
        {
            s_stratName = sN;
            s_tickers = new List<string>();
            foreach (var ticker in t)
                s_tickers.Add(ticker);
            s_days = daysToAnalyze;


            SQLiteAccess.AddSwingStrategy(s_stratName, s_tickers, s_days);
        }

    }
}
