using BinanceDotNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trading_Bot
{
  public static class BuySystem
  {
    #region Client
    private static BinanceService BClient;
    #endregion
    #region Thread Safety
    private static SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    #endregion
    #region Constants
    private const decimal SPREAD_LEEWAY = 0.00001m;
    private const int BUY_QUANTITY = 1;
    private const string TIME_IN_FORCE = "GTC";
    private const decimal PRICE = 1.000m;
    #endregion
    #region Fees
    private static decimal MAKER_FEE = 0.00m;
    private static decimal TAKER_FEE = 0.00m;
    #endregion
    private static void SetDependencies()
    {
      BClient = AutomatedTradeBot.BClient;
      SetFees();
    }
    public static void AnalyseMarket()
    {
      try
      {
        SetDependencies();
        while (true)
        {
          // First step, look for every coin in the marketplace
          List<string> availableCoins = GetAllTradeableCoinsInExchange().Result;

          // Assign a rating to all the coins that are tardebale in the marketplace.
          var probableBuyOrders = AnalyseAssets(availableCoins).Result;

          foreach (var assetToBuy in probableBuyOrders)
          {
            if (assetToBuy.Item1 >= AnalysisEval.GOOD)
            {
              var RESP = PlaceBuyOrder(assetToBuy.Item2).Result;

              // If an unsuccessful order...
              if (RESP == false)
                continue;
            }
          }

          // Buy the assets if they've passed evaluations.

        }
      }
      catch(Exception e)
      {
        // If an exception has occured here, send a notification!!
        Console.WriteLine("Error has occured when analysing marketplace.");
        Console.WriteLine(e.Message);
      }
    }

    /// <summary>
    /// Gets the trade fee of a pair.
    /// </summary>
    /// <returns>Returns a decimal value of the trade fee. </returns>
    public static async Task<decimal> GetTradeFee(string pairSymbol = "")
    {
      try
      {
        Console.WriteLine("Getting Trade Fee...");

        Dictionary<string, object> param = new();
        param.Add("symbol", pairSymbol);

        string response = string.Empty;

        if (string.IsNullOrEmpty(pairSymbol)) { response = await BClient.SendPublicAsync("sapi/v1/asset/tradeFee", HttpMethod.Get); }
        else { response = await BClient.SendPublicAsync("sapi/v1/asset/tradeFee", HttpMethod.Get, param); }

        decimal responseVal = decimal.Parse(response);

        Console.WriteLine("Trade fee set.");
        return responseVal;
      }
      catch
      {
        // Swallow and send an invalid response.
        return decimal.MinValue;
      }
    }

    /// <summary>
    /// Attempts to buy a single product by placing a bid slightly below it's average sell price in the marketplace.
    /// </summary>
    /// <param name="synbol"></param>
    /// <param name="bidAmount"></param>
    /// <returns>True if the asset has been 'bought'.</returns>
    public static async Task<bool> BuyAsset(string symbol, decimal bidAmount)
    {
      try
      {
        await Semaphore.WaitAsync();

        Dictionary<string, object> parameters = new();

        // Setting the parameters for this request as i'll only ever want to buy one asset at a time.
        parameters.Add("symbol", symbol);
        parameters.Add("side", "BUY");
        parameters.Add("type", "LIMIT");
        parameters.Add("quantity", 1);
        parameters.Add("price", bidAmount);
        string response = await BClient.SendSignedAsync("/api/v3/order", HttpMethod.Post, parameters);
        return true;
      }
      catch
      {
        // Swallow Exception
        return false;
      }
      finally
      {
        Semaphore.Release();
      }
    }

    /// <summary>
    /// Returns a tuple of the asset name and amount held in mywallet(s).
    /// </summary>
    /// <returns></returns>
    public static async Task<List<(string, decimal)>> GetWallet()
    {
      try
      {
        await Semaphore.WaitAsync();

        List<(string, decimal)> result = new();

        string response = await BClient.SendSignedAsync("/api/v3/account", HttpMethod.Get);

        return result;
      }
      catch
      {
        return null;
      }
      finally
      {
        Semaphore.Release();
      }
    }

    /// <summary>
    /// Returns an enumarable collection of coins that can be bought / traded for at *this* very moment.
    /// </summary>
    private static async Task<IEnumerable<string>> GetExchangeInfo()
    {
      try
      {
        // Attempt to get all the 'desirable' coins in the exchange
        await Semaphore.WaitAsync();

        Dictionary<string, decimal> exchange = new();

        var response = await BClient.SendPublicAsync("/api/v1/exchangeInfo", HttpMethod.Get);
        JObject responseObj = JObject.Parse(response);

        foreach (var probableSymbol in responseObj["symbols"])
        {
          // I only want coins pairs that are trading
          if (probableSymbol["trading"].Equals(""))
          {
            string pairSymbol = probableSymbol["symbol"].Value<string>();
          }  
        }

        Console.WriteLine(response);

        // Sort each pTrade by their analysis eval
        return null;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        throw new Exception("Unable to find available coins from Client call.");
      }
      finally
      {
        Semaphore.Release();
      }
    }


    /// <summary>
    /// Sets the fees for your account, will be taken into account to determine whether the trade fee plus the asset will be a 'good' trade.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static void SetFees()
    {
      try
      {
        var feesResponse = BClient.SendSignedAsync("/api/v3/account", HttpMethod.Get);

        if (feesResponse is null || string.IsNullOrEmpty(feesResponse.Result))
          throw new ArgumentNullException("Unable to set fees for binance account.");

        JObject feeResponseObj = JObject.Parse(feesResponse.Result);

        MAKER_FEE = feeResponseObj["makerCommission"].Value<decimal>() / 10000;
        TAKER_FEE = feeResponseObj["takerCommission"].Value<decimal>() / 10000;
      }
      catch(Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }

    /// <summary>
    /// Returns all coins available to trade on the Bianance marketplace.
    /// </summary>
    /// <returns></returns>
    private static async Task<List<string>> GetAllTradeableCoinsInExchange()
    {
      try
      {
        await Semaphore.WaitAsync();
        string response = BClient.SendSignedAsync("/sapi/v1/capital/config/getall", HttpMethod.Get).Result;
        var products = JArray.Parse(response);

        List<string> coins = new();

        foreach (var product in products)
        {
          // Not interested in products that are not tradeable.
          if (!product["trading"].Value<bool>() == true) { continue; }

          coins.Add(product["coin"].Value<string>());
        }

        return coins;
      }
      catch(Exception e)
      {
        Console.WriteLine(e.Message);
        return new List<string>();
      }
      finally
      {
        Semaphore.Release();
      }
    }

    private static async Task<List<(AnalysisEval, string)>> AnalyseAssets(List<string> coins)
    {
      try
      {
        await Semaphore.WaitAsync();

        List<(AnalysisEval, string)> tempArr = new();

        // best to use an anonymous function
        coins.ForEach(c =>
        {
          // Get the perforamce of the coin and assign it an enum value.
          tempArr.Add(new(GetCoinPerformace(c).Result, c));
        });

        // Sort the deemable Coins by analysiseval, lowest to highest
        List<(AnalysisEval, string)> deemableCoins = new();
        deemableCoins = tempArr.OrderBy(c => c.Item1).ToList();

        return deemableCoins;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        return null;
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
        decimal fee = await GetTradeFee(coinSymbol + "BNB");

        // Get the average price of the asset
        Dictionary<string, object> assetParams = new();
        assetParams.Add("symbol", coinSymbol + "BNB");

        Task<string> spreadResponse = BClient.SendPublicAsync("/api/v3/ticker/bookTicker", HttpMethod.Get, assetParams);
        if (spreadResponse is null) { throw new ArgumentNullException("Response was null"); }
        JObject spreadResponseJson = JObject.Parse(spreadResponse.Result);

        // We want coins with a "big spread" -> the difference between the bid and ask is big
        decimal askPrice = spreadResponseJson["askPrice"].Value<decimal>();
        decimal bidPrice = spreadResponseJson["bidPrice"].Value<decimal>();

        // If the asking price is less than the bidding price for the underlaying asset, stay away from it!
        if (askPrice.CompareTo(bidPrice) < 0)
          return AnalysisEval.ABYSMAL;
        else if (askPrice.CompareTo(bidPrice) == 0) // If they're the same, still stay away from it as we haven't accounted for the trade fees yet.
          return AnalysisEval.BAD;

        // It's better to always buy "cheap" coins as their spread are usually larger.
        // We also want to buy a high volume of the coins as the profits will be greater.

        return AnalysisEval.NONE;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        // If the coin rating is ever a "none2, an error has occured, immediately rendering useless to us later on in the line, so we skip over.
        return AnalysisEval.NONE;
      }
    }

    private static async Task<bool> PlaceBuyOrder(string symbol)
    {
      try
      {
        await Semaphore.WaitAsync();

        Dictionary<string, object> param = new();
        param.Add("symbol", symbol);
        param.Add("side", "BUY");
        param.Add("type", "MARKET");
        param.Add("timeInForce", TIME_IN_FORCE);
        param.Add("quantity", BUY_QUANTITY);
        param.Add("price", PRICE);
        var response = BClient.SendSignedAsync("/api/v3/order", HttpMethod.Post);

        JObject responseObj = JObject.Parse(response.Result);
        // Once a successful buy order has been place, insert data into a db.
        return true;
      }
      catch(Exception e)
      {
        Console.WriteLine(e.Message);
        Console.WriteLine("Unable to Buy asset:" + symbol + ".");
        return false;
      }
      finally
      {
        Semaphore.Release();
      }
    }
  }
}
