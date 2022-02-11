using BinanceDotNet;
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
    public static BinanceService BClient { get; set; }
    #endregion
    #region Thread Safety
    private static SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    #endregion
    public static void AnalyseMarket()
    {
      try
      {
        while (true)
        {
          // First step, look for every coin in the marketplace
          List<string> availableCoins = GetExchangeInfo().Result.ToList();
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
    /// Returns a generalised trade fee.
    /// </summary>
    public static decimal Fee { get; set; } = GetTradeFee().Result;

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
    /// Returns a tuple of the asset name and amount held buy the asset
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
        // Attempt to get all the coins in the exchange
        await Semaphore.WaitAsync();

        Dictionary<string, decimal> exchange = new();

        var response = await BClient.SendSignedAsync("/api/v1/exchangeInfo", HttpMethod.Get);

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
    // Write a 'rollback' system for when a buy order has been unsuccessful.
  }
}
