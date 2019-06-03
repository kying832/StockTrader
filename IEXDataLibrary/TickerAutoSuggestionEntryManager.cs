using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IEXDataLibrary
{
    public class TickerAutoSuggestionEntryManager
    {
        public static async Task<List<TickerAutoSuggestionEntry>> GetTickerAutoSuggestionEntriesList()
        {
            var tickerAutoSuggestionEntriesList = new List<TickerAutoSuggestionEntry>();

            List<TickerSuggestions> tickerList = await IEXDataAccess.GetTickerSuggestions();

            foreach (var item in tickerList)
            {
                tickerAutoSuggestionEntriesList.Add(new TickerAutoSuggestionEntry { symbol = item.symbol.ToUpper(), name = item.name });
            }

            return tickerAutoSuggestionEntriesList;
        }
    }
}
