using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLiteAccessLibrary;
using IEXDataLibrary;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using SQLiteLibrary;

namespace StockTrader
{
    public class StockPurchaseInfo
    {
        public string ticker { get; set; }
        public double similarityValue { get; set; }
        public double price { get; set; }
        public double percentReturn { get; set; }
        public string date { get; set; }
    }

    public class StockDataPoint
    {
        public string ticker { get; set; }
        public string date { get; set; }
        public double value { get; set; }
    }

    public class BucketStrategy
    {
        public string       m_strategyName;
        public List<string> m_tickers;
        public string       m_dataTimeFrame;
        public string       m_slidingWindowSize;
        public string       m_futureReturnDate;
        public string       m_normalizationFunction;
        public float        m_similarityThreshold;

        // readable representations
        public string m_dataTimeFrame_Readable;
        public string m_slidingWindowSize_Readable;
        public string m_futureReturnDate_Readable;

        // this is just for testing backtest -> create an actual class to hold this and other data
        public double       m_ROR;
        public int          m_totalBuys;

        public List<AnalysisCategory> m_categories;

        public delegate void NormalizationFunction(ref List<double> list);

        public List<StockPurchaseInfo> m_backtestPurchaseRecord;

        public BucketStrategy(string sN, List<string> t, string dTF, string sWS, string fRD, string nF, float sT, bool createAutomatically = false)
        {
            if (createAutomatically || (!SQLiteAccess.BucketStrategyExists(sN)))
            {
                m_strategyName = sN;

                m_dataTimeFrame_Readable = dTF;
                m_slidingWindowSize_Readable = sWS;
                m_futureReturnDate_Readable = fRD;

                m_dataTimeFrame = ProcessDuration(dTF);
                m_slidingWindowSize = ProcessDuration(sWS);
                m_futureReturnDate = ProcessDuration(fRD);

                m_normalizationFunction = nF;
                m_similarityThreshold = sT;

                m_tickers = new List<string>();
                if (t != null)
                {
                    foreach (var ticker in t)
                        m_tickers.Add(ticker);
                }

                m_categories = new List<AnalysisCategory>();

                m_backtestPurchaseRecord = new List<StockPurchaseInfo>();

                if(!createAutomatically)
                    SQLiteAccess.AddBucketStrategy(m_strategyName, m_dataTimeFrame, m_slidingWindowSize, m_futureReturnDate, m_normalizationFunction, m_similarityThreshold);
            }
        }

        public async Task Create()
        {
            // Gather data
            List<List<StockDataPoint>> allData = new List<List<StockDataPoint>>();

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
            int index, maxIndex, mostSimilarCategoryIndex;
            double similarityValue, mostSimilarValue;
            List<double> windowData = new List<double>();

            foreach(List<StockDataPoint> data in allData)
            {
                index = 0;
                maxIndex = data.Count() - 1;

                while(index + windowSize - 1 + futureReturnDate <= maxIndex)
                {
                    // get a window of data
                    windowData.Clear();
                    for (int iii = index; iii < index + windowSize; ++iii)
                        windowData.Add(data[iii].value);

                    // normalize the data
                    normalize(ref windowData);

                    // append the return percentage to the end of the list
                    windowData.Add(ComputeReturnPercentage(data, index, windowSize, futureReturnDate));

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

                    ++index;
                }

                // add the data to the database
                double value = 0.0;

                for (int categoryNumber = 0; categoryNumber < m_categories.Count(); ++categoryNumber)
                {
                    for(int entry_index = 0; entry_index < m_categories[categoryNumber].entries.Count(); ++entry_index)
                    {
                        for(int data_point_index = 0; data_point_index < m_categories[categoryNumber].entries[entry_index].Count(); ++data_point_index)
                        {
                            value = m_categories[categoryNumber].entries[entry_index][data_point_index];
                            SQLiteAccess.AddBucketStrategyData(m_strategyName, categoryNumber, entry_index, data_point_index, value);
                        }
                    }
                }


            }

            // now redistribute up to the first 5 categories
            for(int iii = 0; (iii < 5) && (iii < m_categories.Count()); ++iii)
            {
                for(int jjj = m_categories[iii].entries.Count - 1; jjj >= 0; --jjj)
                {
                    // copy window of data
                    windowData.Clear();
                    for (int kkk = 0; kkk < windowSize + 1; ++kkk)
                        windowData.Add(m_categories[iii].entries[jjj][kkk]);

                    // determine if data should be moved to a new category
                    mostSimilarCategoryIndex = -1;
                    similarityValue = 0.0;
                    mostSimilarValue = 0.0;

                    for (int kkk = 0; kkk < m_categories.Count(); ++kkk)
                    {
                        similarityValue = m_categories[kkk].ComputeSimilarity(windowData);

                        if (similarityValue > mostSimilarValue)
                        {
                            mostSimilarCategoryIndex = kkk;
                            mostSimilarValue = similarityValue;
                        }
                    }

                    if (mostSimilarCategoryIndex != iii)
                    {
                        m_categories[mostSimilarCategoryIndex].Add(windowData);
                        m_categories[iii].entries.RemoveAt(jjj);
                    }
                }
            }
        }

        public async Task BackTest(int categoryIndex, List<string> tickers, string duration)
        {
            // get the aggregate for the specified category
            AnalysisCategory category = m_categories[categoryIndex];

            // obtain all data for all given tickers
            List<List<StockDataPoint>> tickerData = new List<List<StockDataPoint>>();
            string dur = ProcessDuration(duration);
            foreach (var ticker in tickers)
                tickerData.Add(await GetStockData(ticker, dur));

            List<double> windowData       = new List<double>();
            int          windowSize       = durationToInt(m_slidingWindowSize);
            int          futureReturnDate = durationToInt(m_futureReturnDate);
            double       similarityVal    = 0.0;
            double       pReturn          = 0.0;

            // begin with the first sliding window and iterate one day at a time, looking at all stocks at once
            for (int iii = 0; iii < tickerData[0].Count() - durationToInt(m_futureReturnDate) - windowSize; ++iii)
            {
                //iterate over each stock for the given day
                for (int jjj = 0; jjj < tickerData.Count(); ++jjj)
                {
                    // obtain the window of data
                    windowData.Clear();
                    for (int kkk = 0; kkk < windowSize; ++kkk)
                        windowData.Add(tickerData[jjj][iii + kkk].value);

                    // normalize the window
                    // set the normalization function
                    NormalizationFunction normalize = null;
                    if (m_normalizationFunction == "Divide By Max")
                        normalize = new NormalizationFunction(DivideByMax);
                    else if (m_normalizationFunction == "Slopes")
                        normalize = new NormalizationFunction(Slopes);

                    normalize(ref windowData);

                    // compute the similarity value between the aggregate and the window of data
                    similarityVal = category.ComputeSimilarity(windowData);

                    // if the similarity value is above the threshold value, generate a BUY by recording the following information
                    // Ticker, Date, Similarity Value, Price, % Return on the future return date
                    if(similarityVal >= m_similarityThreshold)
                    {
                        pReturn = ComputeReturnPercentage(tickerData[jjj], iii, windowSize, futureReturnDate);                        

                        m_backtestPurchaseRecord.Add(new StockPurchaseInfo() { ticker = tickers[jjj], similarityValue = similarityVal, price = windowData[windowSize - 1], percentReturn = pReturn, date = tickerData[jjj][iii].date });
                    }
                }
            }

            GenerateTestSummaryStatistics();
        }

        private void GenerateTestSummaryStatistics()
        {
            // Upon finishing, the overall return is computed by taking the product of 1+%return for each BUY
            m_ROR = 1.0;

            foreach(var entry in m_backtestPurchaseRecord)
                m_ROR *= (1 + entry.percentReturn);

            m_totalBuys = m_backtestPurchaseRecord.Count();
        }

        private double ComputeReturnPercentage(List<StockDataPoint> data, int index, int windowSize, int futureReturnDate)
        {
            int endOfWindowIndex = index + windowSize - 1;
            return (data[endOfWindowIndex + futureReturnDate].value - data[endOfWindowIndex].value) / data[endOfWindowIndex].value;
        }

        async Task<List<StockDataPoint>> GetStockData(string _ticker, string timeFrame = "")
        {
            if (timeFrame == "")
                timeFrame = m_dataTimeFrame;

            List<GeneralStockData> generalStockData = await IEXDataAccess.GetGeneralData(timeFrame, _ticker);
            List<StockDataPoint> data = new List<StockDataPoint>();

            foreach (var entry in generalStockData)
            {
                if (entry.open > 0.1)
                {
                    data.Add(new StockDataPoint() { ticker = _ticker, date = entry.date, value = entry.open });
                }
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

        public void ResetBackTestPurchaseRecords()
        {
            m_backtestPurchaseRecord.Clear();
        }

        public void LoadCategoriesFromDB()
        {
            List<_BucketStrategyData> _bSD = SQLiteAccess.GetBucketStrategyData(m_strategyName);

            int currentCategory = 0;
            int currentEntry = 0;
            int numberOfEntries = 0;

            List<double> data;
            AnalysisCategory ac;

            while(true)
            {
                IEnumerable<_BucketStrategyData> categoryData = _bSD.Where(p => p.category_index == currentCategory);
                if (categoryData.Count() == 0)
                    break;

                ac = new AnalysisCategory();

                currentEntry = 0;
                while (true)
                {
                    IEnumerable<_BucketStrategyData> entryData = categoryData.Where(p => p.entry_index == currentEntry);
                    if ((numberOfEntries = entryData.Count()) == 0)
                        break;

                    data = new List<double>();
                    for (int iii = 0; iii < numberOfEntries; ++iii)
                        data.Add(0);

                    foreach (var data_point in entryData)
                        data[data_point.data_point_index] = data_point.value;

                    // add entry to the category
                    ac.entries.Add(data);

                    ++currentEntry;
                }

                m_categories.Add(ac);

                ++currentCategory;
            }

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
