using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace DuckBot.Modules.Finance.ServiceThreads
{
    public class UserMarketStocksUpdater
    {
        /// <summary>
        /// Updates the user market stocks
        /// </summary>
        public static void UpdateMarketStocks()
        {
            while (MainProgram._stopThreads == false)
            {
                //Get market stocks
                var marketStockStorage = XmlManager.FromXmlFile<MarketStockStorage>(CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

                List<MarketStock> updatedMarketStocks = new List<MarketStock>();

                //Get real price for each
                foreach (var stock in marketStockStorage.MarketStock)
                {
                    long stockPriceNew = 0;

                    try
                    {
                        stockPriceNew = Convert.ToInt64(OnlineStockHandler.GetOnlineStockInfo(stock.StockTicker).LatestPrice * 100);
                    }
                    catch (Exception)
                    {
                    }


                    updatedMarketStocks.Add(new MarketStock { StockTicker = stock.StockTicker, StockPrice = stockPriceNew });
                }

                //Write to file
                var marketStock = new MarketStockStorage
                {
                    MarketOpen = OnlineStockHandler.GetOnlineIsOpen(),
                    MarketStock = updatedMarketStocks
                };

                XmlManager.ToXmlFile(marketStock, CoreMethod.GetFileLocation(@"\MarketStocksValue.xml"));

                //Wait 10 seconds
                Thread.Sleep(10000);
            }
        }
    }

    public class OnlineStockHandler
    {
        public static CompanyInfoResponse GetOnlineStockInfo(string symbol)
        {
            try
            {
                var IEXTrading_API_PATH = "https://api.iextrading.com/1.0/stock/{0}/quote";

                IEXTrading_API_PATH = string.Format(IEXTrading_API_PATH, symbol);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    //For IP-API
                    client.BaseAddress = new Uri(IEXTrading_API_PATH);
                    HttpResponseMessage response = client.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var companysInfo = response.Content.ReadAsAsync<CompanyInfoResponse>().GetAwaiter().GetResult();
                        if (companysInfo != null)
                        {
                            return companysInfo;
                        }
                    }

                    return null;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Unable to update stocks with API");

                return null;
            }
        }

        public static bool GetOnlineIsOpen()
        {
            var returnStockInfo = GetOnlineStockInfo("aapl");

            if (returnStockInfo.LatestSource == "Close")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
