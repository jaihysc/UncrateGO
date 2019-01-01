using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Models
{
    public class UserStockStorage
    {
        public List<UserStock> UserStock { get; set; }
    }
    public class UserStock
    {
        public string StockTicker { get; set; }
        public long StockAmount { get; set; }
        public long StockBuyPrice { get; set; }
    }


    public class MarketStockStorage
    {
        public bool MarketOpen { get; set; }
        public List<MarketStock> MarketStock { get; set; }
    }
    public class MarketStock
    {
        public string StockTicker { get; set; }
        public long StockPrice { get; set; }
    }
    public class CompanyInfoResponse
    {
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public string PrimaryExchange { get; set; }
        public string Sector { get; set; }
        public string CalculationPrice { get; set; }
        public double Open { get; set; }
        public long OpenTime { get; set; }
        public double Close { get; set; }
        public long CloseTime { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double LatestPrice { get; set; }
        public string LatestSource { get; set; }
        public string LatestTime { get; set; }
        public long LatestUpdate { get; set; }
        public double LatestVolume { get; set; }
        public string IexRealtimePrice { get; set; }
        public string IexRealtimeSize { get; set; }
        public string IexLastUpdated { get; set; }
        public double DelayedPrice { get; set; }
        public long DelayedPriceTime { get; set; }
        public double PreviousClose { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public string IexMarketPercent { get; set; }
        public string IexVolume { get; set; }
        public double AvgTotalVolume { get; set; }
        public string IexBidPrice { get; set; }
        public string IexBidSize { get; set; }
        public string IexAskPrice { get; set; }
        public string IexAskSize { get; set; }
        public long MarketCap { get; set; }
        public double PeRatio { get; set; }
        public double Week52High { get; set; }
        public double Week52Low { get; set; }
        public double YtdChange { get; set; }
    }
}
