using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteLibrary
{
    public class _BucketStrategy
    {
        public string m_strategyName { get; set; }
        public List<string> m_tickers { get; set; }
        public string m_dataTimeFrame { get; set; }
        public string m_slidingWindowSize { get; set; }
        public string m_futureReturnDate { get; set; }
        public string m_normalizationFunction { get; set; }
        public float m_similarityThreshold { get; set; }


    }
}
