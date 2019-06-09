using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace IEXDataLibrary
{
    public static class IEXDataAccess
    {
        public async static Task<List<GeneralStockData>> GetGeneralData(string duration, string ticker)
        {
            var http = new HttpClient();
            string URI = String.Format("https://api.iextrading.com/1.0/stock/{0}/chart/{1}", ticker.ToLower(), duration);
            var response = await http.GetAsync(URI);
            var result = await response.Content.ReadAsStringAsync();

            // remove all instances of null
            result = result.Replace("null", "0.0");

            var serializer = new DataContractJsonSerializer(typeof(List<GeneralStockData>));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (List<GeneralStockData>)serializer.ReadObject(ms);

            return data;
        }

        public async static Task<List<OneDayData>> GetOneDayData(string ticker)
        {
            var http = new HttpClient();
            string URI = String.Format("https://api.iextrading.com/1.0/stock/{0}/chart/1d", ticker);
            var response = await http.GetAsync(URI);
            var result = await response.Content.ReadAsStringAsync();

            // remove all instances of null
            result = result.Replace("null", "0.0");

            var serializer = new DataContractJsonSerializer(typeof(List<OneDayData>));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));

            return (List<OneDayData>)serializer.ReadObject(ms);
        }

        public async static Task<List<TickerSuggestions>> GetTickerSuggestions()
        {
            var http = new HttpClient();
            string URI = "https://api.iextrading.com/1.0/ref-data/symbols";
            var response = await http.GetAsync(URI);
            var result = await response.Content.ReadAsStringAsync();

            var serializer = new DataContractJsonSerializer(typeof(List<TickerSuggestions>));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));

            return (List<TickerSuggestions>)serializer.ReadObject(ms);
        }

        //get 3 months trading, for swing strat

        public async static Task<List<ThreeMonthData>> GetThreeMonthData(string ticker)
        {
            var http = new HttpClient();
            string URI = String.Format("https://api.iextrading.com/1.0/stock/{0}/chart/3m", ticker);
            var response = await http.GetAsync(URI);
            var result = await response.Content.ReadAsStringAsync();

            // remove all instances of null
            result = result.Replace("null", "0.0");

            var serializer = new DataContractJsonSerializer(typeof(List<ThreeMonthData>));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));

            return (List<ThreeMonthData>)serializer.ReadObject(ms);
        }
    }
   
    
    [DataContract]
    public class GeneralStockData
    {
        [DataMember]
        public string date { get; set; }

        [DataMember]
        public double open { get; set; }

        [DataMember]
        public double high { get; set; }

        [DataMember]
        public double low { get; set; }

        [DataMember]
        public double close { get; set; }

        [DataMember]
        public int volume { get; set; }

        [DataMember]
        public int unadjustedVolume { get; set; }

        [DataMember]
        public double change { get; set; }

        [DataMember]
        public double changePercent { get; set; }

        [DataMember]
        public double vwap { get; set; }

        [DataMember]
        public string label { get; set; }

        [DataMember]
        public double changeOverTime { get; set; }
    }

    [DataContract]
    public class OneDayData
    {
        public string date { get; set; }

        [DataMember]
        public string minute { get; set; }

        [DataMember]
        public string label { get; set; }

        [DataMember]
        public double high { get; set; }

        [DataMember]
        public double low { get; set; }

        [DataMember]
        public double average { get; set; }

        [DataMember]
        public int volume { get; set; }

        [DataMember]
        public double notional { get; set; }

        [DataMember]
        public int numberOfTrades { get; set; }

        [DataMember]
        public double marketHigh { get; set; }

        [DataMember]
        public double marketLow { get; set; }

        [DataMember]
        public double marketAverage { get; set; }

        [DataMember]
        public int marketVolume { get; set; }

        [DataMember]
        public double marketNotional { get; set; }

        [DataMember]
        public int marketNumberOfTrades { get; set; }

        [DataMember]
        public double open { get; set; }

        [DataMember]
        public double close { get; set; }

        [DataMember]
        public double marketOpen { get; set; }

        [DataMember]
        public double marketClose { get; set; }

        [DataMember]
        public double changeOverTime { get; set; }

        [DataMember]
        public double marketChangeOverTime { get; set; }
    }

    [DataContract]
    public class TickerSuggestions
    {
        [DataMember]
        public string symbol { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string date { get; set; }

        [DataMember]
        public bool isEnabled { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public object iexId { get; set; }
    }

    [DataContract]
    public class ThreeMonthData
    {
        [DataMember]
        public string date { get; set; }

        [DataMember]
        public double open { get; set; }

        [DataMember]
        public double high { get; set; }

        [DataMember]
        public double low { get; set; }

        [DataMember]
        public double close { get; set; }

        [DataMember]
        public int volume { get; set; }

        [DataMember]
        public int unadjustedVolume { get; set; }

        [DataMember]
        public double change { get; set; }

        [DataMember]
        public double changePercent { get; set; }

        [DataMember]
        public double vwap { get; set; }

        [DataMember]
        public string label { get; set; }

        [DataMember]
        public double changeOverTime { get; set; }

    }


}
