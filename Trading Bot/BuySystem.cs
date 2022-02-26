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

        var fees = GetAccountFees();

        while (true)
        {        
          // Get all the available trade pairs currently in the marketplace.
          var tradePairs = GetAllAvailableCoins().Result;
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
        var feesResponse = await BClient.SendSignedAsync("/api/v3/account", HttpMethod.Get);

        if (feesResponse is null || string.IsNullOrEmpty(feesResponse))
          throw new ArgumentNullException("Unable to set fees for binance account.");

        JObject feeResponseObj = JObject.Parse(feesResponse);

        MAKER_FEE = feeResponseObj["makerCommission"].Value<decimal>() / 10000;
        TAKER_FEE = feeResponseObj["takerCommission"].Value<decimal>() / 10000;
      }
      catch (Exception e)
      {
        Log.Msg(e.Message, MessageLog.ERROR);
      }
    }

    /// <summary>
    /// Returns an enumarable collection of coins that can be bought / traded for at *this* very moment.
    /// </summary>
    private static async Task<Dictionary<string, (string, int, string, int)>> GetAllAvailableCoins()
    {
      try
      {
        Log.Msg("Collecting all tradeable assets in the marketplace...", MessageLog.NORMAL);
        // Attempt to get all the 'desirable' coins in the exchange

        Dictionary<string, (string, int, string, int)> tradePairs = new();

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

            (string, (string, int, string, int)) asset = (tradePairSymbol, (baseAsset,basePrecision, quoteAsset, quotePrecision));
            tradePairs.Add(tradePairSymbol,new(baseAsset, basePrecision, quoteAsset, quotePrecision));

            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginAnalysis), asset);
          }  
        }

        // Important to reset once all Assets have been checked.
        AssetsChecked = 0;
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
      (string, (string, int, string, int)) asset = (ValueTuple<string, (string, int, string, int)>)tradePairDetails;

      try
      {
        // Intentionally stopping too many requests from being made, will start using wbsockets when infrastructure is ready.
        Thread.Sleep(1000);
        Log.Msg($"Beginning Analysis On: '{ asset.Item1 }'.", MessageLog.NORMAL);

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

        // If the coin doesn't beat the average price change, don't consider the coin.
        if (priceChangePercent < averagePriceChange) { return; }

        // Only want asset if the price change is equal to the average price change or more.
        // Get the asset's current bid / ask price
        Dictionary<string, object> assetInforParams = new();
        assetInforParams.Add("symbol", asset.Item1);

        string assetInfoResponse = await BClient.SendPublicAsync("/api/v1/exchangeInfo", HttpMethod.Get, assetInforParams);
        JObject assetInfo = JObject.Parse(assetInfoResponse);
      }
      catch (Exception e)
      {
        Log.Msg(e.Message, MessageLog.ERROR);
        Log.Msg($"Unable to analyse: '{ asset.Item1 }.' ", MessageLog.ERROR);
      }
    }

    /// <summary>
    /// Evaluates the performace of the asset / pair and returns an 'AnalysisEval' based on its performance.
    /// </summary>
    /// <param name="coinSymbol"></param>
    /// <param name="interval"></param>
    /// <returns>An 'AnalysisEval'.</returns>
    private static async Task<AnalysisEval> GetCoinPerformace(string coinSymbol, int interval = 60)
    {
      try
      {
        Console.WriteLine("Checking performace of asset: " + coinSymbol);
        // Get the average price of the asset
        Dictionary<string, object> assetParams = new();
        assetParams.Add("symbol", coinSymbol);

        var spreadResponse = await BClient.SendPublicAsync("/api/v3/ticker/bookTicker", HttpMethod.Get, assetParams);
        if (spreadResponse is null) { throw new ArgumentNullException("Response was null"); }
        JObject spreadResponseJson = JObject.Parse(spreadResponse);

        // We want coins with a "big spread" -> the difference between the bid and ask is big
        decimal askPrice = spreadResponseJson["askPrice"].Value<decimal>();
        decimal bidPrice = spreadResponseJson["bidPrice"].Value<decimal>();

        // If the asking price is less than the bidding price for the underlaying asset, stay away from it!
        if (askPrice.CompareTo(bidPrice) < 0)
        {
          Console.WriteLine($"Asset: {coinSymbol} deemed {AnalysisEval.ABYSMAL}.");
          return AnalysisEval.ABYSMAL;
        }
        else if (askPrice.CompareTo(bidPrice) == 0)
        {
          Console.WriteLine($"Asset: {coinSymbol} deemed {AnalysisEval.BAD}.");
          return AnalysisEval.BAD;
        }

        // If the price of the asset in the last 24 hours is greater than its current avg price...
        string lastDayResponse = BClient.SendPublicAsync("/api/v3/ticker/24hr", HttpMethod.Get, assetParams).Result;
        var avgPriceResponse = await BClient.SendPublicAsync("/api/v3/avgPrice", HttpMethod.Get, assetParams);

        JObject lastDayResponseObj = JObject.Parse(lastDayResponse);
        JObject avgPriceResponseObj = JObject.Parse(avgPriceResponse);

        decimal lastDayAvg = lastDayResponseObj["weightedAvgPrice"].Value<decimal>();
        decimal avgPrice = avgPriceResponseObj["price"].Value<decimal>();

        if (lastDayAvg > avgPrice)
        {
          Console.WriteLine($"Asset: {coinSymbol} deemed {AnalysisEval.POOR}.");
          return AnalysisEval.POOR;
        }
        else if (lastDayAvg < avgPrice)
        {
          Console.WriteLine($"Asset: {coinSymbol} deemed {AnalysisEval.GOOD}.");
          await PlaceTakeProfitLimit(coinSymbol);
          return AnalysisEval.GOOD;
        }

        Console.WriteLine($"Asset: {coinSymbol} not given an analysis.");
        return AnalysisEval.NONE;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Console.WriteLine($"Asset: {coinSymbol} not given an analysis.");
        // If the coin rating is ever a "none", an error has occured, immediately rendering useless to us later on in the line, so we skip over.
        return AnalysisEval.NONE;
      }
    }

    /// <summary>
    /// Places an order just below the asking price of an asset.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>True if the operation was successful, False if peration was unsuccessful.</returns>
    private static async Task<bool> PlaceTakeProfitLimit(string symbol)
    {
      try
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Dictionary<string, object> tickerParam = new();
        tickerParam.Add("symbol", symbol);
        Console.WriteLine("Attempting to Buy: " + symbol + ".");
        var currentPriceResponse = await BClient.SendPublicAsync("/api/v3/ticker/bookTicker", HttpMethod.Get, tickerParam);
        JObject currentPriceObj = JObject.Parse(currentPriceResponse);
        decimal currentPrice = currentPriceObj["bidPrice"].Value<decimal>();
        decimal minimumAmount = 0.00m;// = await GetMinimumPriceOfAsset(symbol);
        minimumAmount = minimumAmount + (minimumAmount * MAKER_FEE); // The minimum amount of money needed to make a profit.

        Dictionary<string, object> param = new();
        param.Add("symbol", symbol);
        param.Add("side", ORDER_SIDE.BUY);
        param.Add("type", "TAKE_PROFIT_LIMIT");
        param.Add("timeInForce", TIME_IN_FORCE.GTC);
        // param.Add("quantity", BUY_QUANTITY);
        param.Add("price", Math.Round(minimumAmount, 8));
        param.Add("stopPrice",Math.Round(minimumAmount, 8));
        var response = await BClient.SendSignedAsync("/api/v3/order", HttpMethod.Post, param);

        JObject responseObj = JObject.Parse(response);
        Console.WriteLine("Asset Bought: " + symbol + ".");
        return true;
      }
      catch
      {
        // Unsuccessful order, swallow exception...
        Console.ForegroundColor = ConsoleColor.Red;
        return false;
      }
      finally
      {
        Console.ResetColor();
      }
    }

    /// <summary>
    /// Retrieves the current average price for an asset in the last 5 minutes.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>Returns the current average price for a symbol, i.e. BTCBNB -> 4.0029</returns>
    private static async Task<decimal> GetCurrentPrice(string symbol)
    {
      try
      {
        Console.WriteLine($"Getting the current price for '{ symbol }'.");
        Dictionary<string, object> param = new();
        param.Add("symbol", symbol);
        string response = await BClient.SendPublicAsync("/api/v3/avgPrice", HttpMethod.Get, param);
        if (string.IsNullOrEmpty(response)) { throw new ArgumentNullException("Response is invalid."); }
        JObject responseObject = JObject.Parse(response);
        if (responseObject is null) { throw new ArgumentNullException("Response is invalid."); }

        decimal currentPrice = responseObject["price"].Value<decimal>();

        return currentPrice;
      }
      catch
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Couldn't get the current price for '{ symbol }'.");
        Console.ResetColor();
        return decimal.MinValue;
      }
    }
  }
}
