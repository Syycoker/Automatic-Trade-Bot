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
    private const decimal CHANGE_LEEWAY = 0.51m;
    private const decimal BAD_FLOOR = 0.100m;
    private const decimal BAD_CEILING = 0.001m;
    #endregion

    private static void SetDependencies()
    {
      BClient = AutomatedTradeBot.BClient;
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
          var items = RateCoins(availableCoins);
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
        await Semaphore.WaitAsync();

        Console.WriteLine("Getting Trade Fee...");

        Dictionary<string, object> param = new();
        param.Add("symbol", pairSymbol);

        string response = string.Empty;

        if (string.IsNullOrEmpty(pairSymbol)) { response = await BClient.SendSignedAsync("sapi/v1/asset/tradeFee", HttpMethod.Get); }
        else { response = await BClient.SendSignedAsync("sapi/v1/asset/tradeFee", HttpMethod.Get, param); }

        decimal responseVal = decimal.Parse(response);

        Console.WriteLine("Trade fee set.");
        return responseVal;
      }
      catch
      {
        // Swallow and send an invalid response.
        return decimal.MinValue;
      }
      finally
      {
        Semaphore.Release();
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

    private static async Task<List<(AnalysisEval, string)>> RateCoins(List<string> coins)
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
      finally
      {
        Semaphore.Release();
      }
    }

    private static async Task<AnalysisEval> GetCoinPerformace(string coinSymbol, int interval = 60)
    {
      try
      {
        // Get the average price of the asset
        Dictionary<string, object> assetParams = new();
        assetParams.Add("symbol", "BNB" + coinSymbol);

        // Check if the current average is greater or less than the 24 hour roling window price of the asset.
        var lastDayRollingAverageResponse = BClient.SendPublicAsync("/api/v3/ticker/24hr", HttpMethod.Get, assetParams);
        if (lastDayRollingAverageResponse.Result is null) { throw new Exception("24 Hour Rolling Price Response was null."); }

        JObject rollingPrice = JObject.Parse(lastDayRollingAverageResponse.Result);
        decimal weightedAveragePrice = rollingPrice["weightedAvgPrice"].Value<decimal>();
        decimal priceChange = rollingPrice["priceChange"].Value<decimal>();

        // If the rolling price is negative
        if (priceChange < 0)
        {
          if (BAD_FLOOR >= priceChange + CHANGE_LEEWAY && priceChange + CHANGE_LEEWAY <= BAD_CEILING)
          {
            return AnalysisEval.BAD;
          }
          else
          {
            return AnalysisEval.ABYSMAL;
          }
        }
        var averagePriceReponse = BClient.SendPublicAsync("/api/v3/avgPrice", HttpMethod.Get, assetParams);
        if (averagePriceReponse.Result is null) { throw new Exception("Average Price Response was null."); }

        JObject avgPrice = JObject.Parse(averagePriceReponse.Result);
        decimal avgPriceDec = avgPrice["price"].Value<decimal>();

        Dictionary<string, object> param = new();
        param.Add("symbol", "BNBUSDT");
        param.Add("interval", interval);
        param.Add("symbol", "BNBUSDT");
        param.Add("interval", interval);
        param.Add("symbol", "BNBUSDT");
        param.Add("interval", interval);
        

        var response = await BClient.SendPublicAsync("/api/v3/klines", HttpMethod.Get, param);

        JObject responseObj = JObject.Parse(response);

        return AnalysisEval.VERY_GOOD;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        // If the coin rating is ever a "none2, an error has occured, immediately rendering useless to us later on in the line, so we skip over.
        return AnalysisEval.NONE;
      }
    }
  }
}
