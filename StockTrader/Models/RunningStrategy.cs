using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader.Models
{
    public class RunningStrategy
    {
        public string       m_strategyName { get; set; }
        public int          m_bucketInUse { get; set; }
        public string       m_startDate { get; set; }
        public int          m_numberOfTradesMade { get; set; }
        public double       m_ROR { get; set; }
        public List<string> m_tickersToTrade { get; set; }
        public string       m_windowSize { get; set; }
        public string       m_futureReturnDate { get; set; }
        public string       m_similarityThreshold { get; set; } 
    }
}
