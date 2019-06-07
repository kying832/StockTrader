using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLiteAccessLibrary;
using IEXDataLibrary;

namespace StockTrader
{
    public class BucketStrategy
    {
        public string       m_strategyName;
        public List<string> m_tickers;
        public string       m_dataTimeFrame;
        public string       m_futureReturnDate;
        public string       m_normalizationFunction;
        public float        m_similarityThreshold;

        public BucketStrategy(string sN, List<string> t, string dTF, string fRD, string nF, float sT)
        {
            m_strategyName = sN;
            m_dataTimeFrame = dTF;
            m_futureReturnDate = fRD;
            m_normalizationFunction = nF;
            m_similarityThreshold = sT;

            m_tickers = new List<string>();
            foreach (var ticker in t)
                m_tickers.Add(ticker);

            SQLiteAccess.AddBucketStrategy(m_strategyName, m_tickers, m_dataTimeFrame, m_futureReturnDate, m_normalizationFunction, m_similarityThreshold);

            Run();
        }

        private void Run()
        {
            // Gather data

            // 
        }
    }
}
