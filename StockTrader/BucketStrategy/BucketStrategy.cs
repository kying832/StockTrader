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
        public string       m_slidingWindowSize;
        public string       m_futureReturnDate;
        public string       m_normalizationFunction;
        public float        m_similarityThreshold;

        public List<AnalysisCategory> m_categories;

        public delegate void NormalizationFunction(ref List<double> list);

        public BucketStrategy(string sN, List<string> t, string dTF, string sWS, string fRD, string nF, float sT)
        {
            m_strategyName              = sN;
            m_dataTimeFrame             = ProcessDuration(dTF);
            m_slidingWindowSize         = ProcessDuration(sWS);
            m_futureReturnDate          = ProcessDuration(fRD);
            m_normalizationFunction     = nF;
            m_similarityThreshold       = sT;

            m_tickers = new List<string>();
            foreach (var ticker in t)
                m_tickers.Add(ticker);

            m_categories = new List<AnalysisCategory>();

          //  SQLiteAccess.AddBucketStrategy(m_strategyName, m_tickers, m_dataTimeFrame, m_futureReturnDate, m_normalizationFunction, m_similarityThreshold);
        }

        public async Task Run()
        {
            // Gather data
            List<List<double>> allData = new List<List<double>>();

            foreach(string ticker in m_tickers)
                allData.Add(await GetStockData(ticker));

            // get window size, future return date
            int windowSize = durationToInt(m_slidingWindowSize);
            int futureReturnDate = durationToInt(m_futureReturnDate);

            // set the normalization function
            NormalizationFunction normalize = null;
            if (m_normalizationFunction == "Divide By Max")
                normalize = new NormalizationFunction(DivideByMax);
            else if (m_normalizationFunction == "Slopes")
                normalize = new NormalizationFunction(Slopes);

            // iterate over each stock and run the analysis loop
            int index, maxIndex, returnIndex, mostSimilarCategoryIndex;
            double similarityValue, mostSimilarValue;
            List<double> windowData = new List<double>();

            foreach(List<double> data in allData)
            {
                index = 0;
                maxIndex = data.Count() - 1;

                while(index + windowSize + futureReturnDate <= maxIndex)
                {
                    // get a window of data
                    for (int iii = index; iii < index + windowSize; ++iii)
                        windowData.Add(data[iii]);

                    // normalize the data
                    normalize(ref windowData);

                    // append the return percentage to the end of the list
                    returnIndex = index + windowSize + futureReturnDate;
                    windowData.Add((data[returnIndex] - data[returnIndex - 1]) / data[returnIndex - 1]);

                    // place data into a category
                    mostSimilarCategoryIndex = -1;
                    similarityValue = 0.0;
                    mostSimilarValue = 0.0;

                    for (int iii = 0; iii < m_categories.Count(); ++iii)
                    {
                        similarityValue = m_categories[iii].ComputeSimilarity(windowData);

                        if (similarityValue >= m_similarityThreshold && similarityValue > mostSimilarValue)
                        {
                            mostSimilarCategoryIndex = iii;
                            mostSimilarValue = similarityValue;
                        }
                    }

                    if (mostSimilarCategoryIndex != -1)
                    {
                        m_categories[mostSimilarCategoryIndex].Add(windowData);
                        mostSimilarCategoryIndex = -1;
                    }
                    else
                    {
                        m_categories.Add(new AnalysisCategory(windowData));
                    }

                    // clear the current window
                    windowData.Clear();

                    ++index;
                }
            }
        }


        

        async Task<List<double>> GetStockData(string ticker)
        {
            List<GeneralStockData> generalStockData = await IEXDataAccess.GetGeneralData(m_dataTimeFrame, ticker);
            List<double> data = new List<double>();

            foreach (var entry in generalStockData)
            {
                if(entry.open > 0.1)
                    data.Add(entry.open);
            }

            return data;
        }

        private void DivideByMax(ref List<double> data)
        {
            double max = data.Max();

            for (int iii = 0; iii < data.Count(); ++iii)
                data[iii] /= max;
        }

        private void Slopes(ref List<double> data)
        {
            // This needs to be implemented
            return;
        }

        // === HELPER FUNCTIONS ===========================================================================

        string ProcessDuration(string input)
        {
            switch(input)
            {
                case "1 Day":       return "1d";
                case "3 Days":      return "3d";
                case "5 Days":      return "5d";
                case "10 Days":     return "10d";
                case "1 Month":     return "1m";
                case "3 Months":    return "3m";
                case "6 Months":    return "6m";
                case "1 Year":      return "1y";
                case "2 Years":     return "2y";
                case "5 Years":     return "5y";
                default:
                                    return "5y";
            }
        }

        int durationToInt(string duration)
        {
            switch(duration)
            {
                case "1d":  return 1;
                case "3d":  return 3;
                case "5d":  return 5;
                case "10d": return 10;
                case "1m":  return 21;
                case "3m":  return 63;
                case "6m":  return 126;
                case "1y":  return 252;
                default:    return 0;
            }
        }

    }
}
