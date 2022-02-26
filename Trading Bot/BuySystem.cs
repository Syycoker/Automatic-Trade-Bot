using BinanceDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trading_Bot.Enums;
using Trading_Bot.Logging;

namespace Trading_Bot
{
  public static class BuySystem
  {
    #region Dependencies
    #region Client
    private static BinanceService BClient;
    #endregion
    #region DO NOT TOUCH
    private static long AssetsChecked { get; set; } = 0;
    private static decimal TotalPriceChange { get; set; } = 0.00000000m;
    private static decimal MAKER_FEE = 0.00m;
    private static decimal TAKER_FEE = 0.00m;
    #endregion
    #region Properties
    public static Dictionary<string, (string, int, string, int)> TradePairs { get; set; } = new();
    #endregion
    #endregion
    public static void AnalyseMarket()
    {
      try
      {
        // Set the Client.
        BClient = AutomatedTradeBot.BClient;

        GetAccountFees().GetAwaiter();

        while (true)
        {        
          // Get all the available trade pairs currently in the marketplace.
          var tradePairs = GetAllAvailableCoins().Result;

          // Important to reset once all Assets have been checked.
          AssetsChecked = 0;
        }
      }
      catch(Exception e)
      {
        Log.Msg(e.Message, MessageLog.ERROR);
        Log.Msg("Error has occured when analysing marketplace.", MessageLog.ERROR);
      }
    }

    /// <summary>
    /// Gets the fees for your specific account, which will be taken into account to determine whether the trade fee plus the asset will be a 'good' trade.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static async Task GetAccountFees()
    {
      try
      {
        string feesResponse = await BClient.SendSignedAsync("/api/v3/account", HttpMethod.Get);
        JObject feeResponseObj = JObject.Parse(feesResponse);

        MAKER_FEE = feeResponseObj["makerCommission"].Value<decimal>() / 10000;
        TAKER_FEE = feeResponseObj["takerCommission"].Value<decimal>() / 10000;
      }
      catch
      {
        Log.Msg("Unable to set fees for binance account.", MessageLog.ERROR);
      }
    }

    /// <summary>
    /// Returns an enumarable collection of coins that can be bought / traded for at *this* very moment.
    /// </summary>
    private static async Task<Dictionary<string, (string, string, decimal, int)>> GetAllAvailableCoins()
    {
      try
      {
        Log.Msg("Collecting all tradeable assets in the marketplace...", MessageLog.NORMAL);
        // Attempt to get all the 'desirable' coins in the exchange

        Dictionary<string, (string, string, decimal, int)> tradePairs = new();

        var response = await BClient.SendPublicAsync("/api/v1/exchangeInfo", HttpMethod.Get);
        JObject responseObj = JObject.Parse(response);

        foreach (var probableSymbol in responseObj["symbols"])
        {
          // Check if the symbol is being actively traded in the marketplace.
          if (probableSymbol["status"].Value<string>().Equals("TRADING"))
          {
            string tradePairSymbol = probableSymbol["symbol"].Value<string>();
            string baseAsset = probableSymbol["baseAsset"].Value<string>();
            string quoteAsset = probableSymbol["quoteAsset"].Value<string>();
            int basePrecision = probableSymbol["baseAssetPrecision"].Value<int>();
            int quotePrecision = probableSymbol["quotePrecision"].Value<int>();

            // Get th filters for the asset.
            decimal lotSize = decimal.MinValue;
            var filters = probableSymbol["filters"].Values<JObject>();
            var lotSizeJObject = filters.FirstOrDefault(f => f["filterType"].Value<string>().Equals("LOT_SIZE"));
            lotSize = lotSizeJObject["minQty"].Value<decimal>();

            // int quantityPrecision = probableSymbol["quantityPrecision"].Value<int>();

            (string, (string,string, decimal, int)) asset = (tradePairSymbol, (baseAsset,quoteAsset, lotSize, 8));

            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginAnalysis), asset);
          }  
        }

        return tradePairs;
      }
      catch (Exception e)
      {
        // Can't work with an empty/invalid/incomplete set of assets, so throw exepction up a level.
        Log.Msg(e.Message, MessageLog.ERROR);
        Log.Msg("Unable to find available coins from Client call.", MessageLog.ERROR);

        throw;
      }
    }

    public static async void BeginAnalysis(object tradePairDetails)
    {
      // Cast object into asset Tuple
      (string, (string, string, decimal, int)) asset = (ValueTuple<string, (string, string, decimal, int)>)tradePairDetails;

      try
      {
        // Intentionally stopping too many requests from being made, will start using wbsockets when infrastructure is ready.
        Thread.Sleep(1000);
        Log.Msg($"Analysing: '{ asset.Item1 }'.", MessageLog.NORMAL);

        Dictionary<string, object> twentyFourHourPerformanceParams = new();
        twentyFourHourPerformanceParams.Add("symbol", asset.Item1);
        string twentyFourHourPerformanceResponseString = await BClient.SendPublicAsync("/api/v3/ticker/24hr", HttpMethod.Get, twentyFourHourPerformanceParams);
        JObject twentyFourHourPerforamce = JObject.Parse(twentyFourHourPerformanceResponseString);

        decimal lastPrice = twentyFourHourPerforamce["lastPrice"].Value<decimal>();
        decimal openPrice = twentyFourHourPerforamce["openPrice"].Value<decimal>();

        decimal priceChangePercent = (lastPrice - openPrice) / openPrice;

        TotalPriceChange += priceChangePercent;
        AssetsChecked++;

        decimal averagePriceChange = TotalPriceChange / AssetsChecked;

        // If the coin doesn't have a positive price change and doesn't beat the average price change, don't consider the coin.
        if (priceChangePercent < 0 && priceChangePercent < averagePriceChange) { return; }

        // Only want asset if the price change is equal to the average price change or more.
        // Get the asset's current bid / ask price
        Dictionary<string, object> assetInforParams = new();
        assetInforParams.Add("symbol", asset.Item1);

        // Hmmm, shouln't be doing this but...
        decimal minimumQuantity = decimal.Round(asset.Item2.Item3, asset.Item2.Item4);

        Dictionary<string, object> orderParam = new();
        orderParam.Add("symbol", asset.Item1);
        orderParam.Add("side", "BUY");
        orderParam.Add("type", "MARKET");
        orderParam.Add("quantity", minimumQuantity);

        Log.Msg($"Buying asset: '{ asset.Item1}' .", MessageLog.NORMAL);

        string orderResponseString = await BClient.SendSignedAsync("/fapi/v1/order", HttpMethod.Post, orderParam);

      }
      catch (Exception e)
      {
        Log.Msg(e.Message, MessageLog.ERROR);
        Log.Msg($"Unable to analyse: '{ asset.Item1 }.' ", MessageLog.ERROR);
      }
    }
  }
}
