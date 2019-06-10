using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteLibrary
{
    public class _RunningStrategy
    {
        public string m_strategyName { get; set; }
        public string m_dataTimeFrame { get; set; }
        public string m_windowSize { get; set; }
        public string m_futureReturnDate { get; set; }
        public string m_normalizationFunction { get; set; }
        public string m_similarityThreshold { get; set; }
        public int m_bucketNumber { get; set; }
        public string m_startDate { get; set; }
    }
}
