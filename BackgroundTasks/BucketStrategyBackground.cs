using SQLiteAccessLibrary;
using SQLiteLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

using BucketStrategyLibrary;

namespace BackgroundTaskLibrary
{
    public sealed class BucketStrategyBackground : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            // Build the strategy to run by querying the database
            string taskName = taskInstance.Task.Name;

            _RunningStrategy _rs = SQLiteAccess.GetRunningBucketStrategy(taskName);
            BucketStrategy bs = new BucketStrategy(
                _rs.m_strategyName,
                null,
                _rs.m_dataTimeFrame,
                _rs.m_windowSize,
                _rs.m_futureReturnDate,
                _rs.m_normalizationFunction,
                float.Parse(_rs.m_similarityThreshold),
                true);

            bs.LoadCategoriesFromDB();

            // Run the appropriate strategy given its type
            await RunBucketStrategy(bs, _rs.m_bucketNumber);

            _deferral.Complete();
        }

        private async Task RunBucketStrategy(BucketStrategy bs, int bucketNumber)
        {
            DateTime lastRunDate = DateTime.Today;

            while (true)
            {
                if (!SQLiteAccess.StrategyStillExists(bs.m_strategyName))
                    break;

                if(lastRunDate != DateTime.Today && TimeIsAfterTradingHours() && MarketIsOpen())
                {
                    // get list of tickers to trade
                    List<string> tickersToTrade = SQLiteAccess.GetTickersToTrade(bs.m_strategyName);

                    // for each ticker, check if we have already made a buy
                    foreach(var ticker in tickersToTrade)
                    {
                        if(SQLiteAccess.TickerIsInBuyState(bs.m_strategyName, ticker))
                        {
                            if(SQLiteAccess.DecrimentDaysToHold(bs.m_strategyName, ticker) <= 0)
                            {
                                await bs.MakeSell(ticker);
                            }                            
                        }
                        else
                        {
                            await bs.MakeBuy(ticker, bucketNumber);
                        }
                    }
                }

                // wait 24 hours to run again
                System.Threading.Thread.Sleep(1000 * 60 * 60 * 24);
            }
        }

        private bool TimeIsAfterTradingHours()
        {
            if (DateTime.UtcNow.Hour > 17)
                return true;
            else
                return false;
        }

        private bool MarketIsOpen()
        {
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday)
                return false;

            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return true;
        }
    }
}
