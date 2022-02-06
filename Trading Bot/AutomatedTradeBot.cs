using System;
using System.Linq;
using System.Threading.Tasks;
using Trading_Bot.Configuration_Files;
using System.Collections.Generic;
using System.Threading;
using RestSharp;
using BinanceDotNet;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Trading_Bot
{
  public class AutomatedTradeBot
  {
    #region Thread Safety
    private static SemaphoreSlim Semaphore = new SemaphoreSlim(1 , 1);
    #endregion

    public static BinanceService BClient { get; set; }

    /// <summary>
    /// Main method becomes asynchronous when all the system initialisations are complete as async calls propogate up...
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
      try
      {
        // Important!
        AuthenticationConfig.SandBoxMode = false;

        // Initialise the authorisation codes.
        AuthenticationConfig.Initialise();

        // Initialise the database.
        //DatabaseConfig.Initialise();

        // Initialise the socket [DEPRECTAED]
        // SocketConfig.Initialise();

        Console.ForegroundColor = ConsoleColor.White;

        HttpClient hClient = new HttpClient();
        BClient = new BinanceService(hClient);

        Console.WriteLine("Program Starting...");

        Task.Run(async () =>
        {
          await UpdateAvailableCoins();
          await GetAccountBalance();
        }).GetAwaiter().GetResult();
      }

      catch (Exception e)
      {
        // Send a push notification to phone if any error arises.
        // Restart program again if application closes...
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ReadKey();
      }
    }

    #region Main Procedures
    /// <summary>
    /// Stores the trade pairs (key).
    /// </summary>
    public static List<string> AvailableCoins = new();

    private bool StartRoutines(int buyThreadCount = 1, int sellThreadCount = 1)
    {
      try
      {
        // Create the threads.
        for (int i = 0; i < buyThreadCount; i++)
        {
          Thread buyThread = new Thread(Buy) { IsBackground = true };
          buyThread.Start();
        }

        for (int j = 0; j < sellThreadCount; j++)
        {
          Thread sellThread = new Thread(Sell) { IsBackground = true };
          sellThread.Start();
        }
        return true;
      }
      catch
      {
        // Swallow Exceptions
        return false;
      }
    }

    /// <summary>
    /// Updates the available coins in the exchange every minuite or so.
    /// </summary>
    /// <returns></returns>
    private static async Task UpdateAvailableCoins()
    {
      try
      {
        await Semaphore.WaitAsync();

        // Clear any coins saved already and start afresh.
        AvailableCoins.Clear();
        string response = await BClient.SendSignedAsync("/sapi/v1/capital/config/getall", HttpMethod.Get);
        var products = JArray.Parse(response);

        foreach (var product in products)
        {
          // Not interested in products that are not tradeable.
          if (!product["trading"].Value<bool>() == true) { continue; }

          AvailableCoins.Add(product["coin"].Value<string>());
        }
      }
      catch
      {
        // Swallow Exception for now, will create logging system
        // or maybe create frontend for mobile app that sends notifications to my phone, who knows. for now
        //do everything backened.
      }
      finally
      {
        Semaphore.Release();
      }
    }

    /// <summary>
    /// Returns the wallet balance for a transaction or information
    /// </summary>
    /// <returns></returns>
    private static async Task GetAccountBalance()
    {
      var response = await BClient.SendSignedAsync("/api/v3/account", HttpMethod.Get);
      var jResponse = JObject.Parse(response);
      var value = jResponse["balances"][0].ToString();
    }

    private void Buy()
    {

    }

    private void Sell()
    {

    }

    /// <summary>
    /// Returns an enumarable collection of coins that can be bought / traded for at *this* very moment.
    /// </summary>
    private static async Task<List<PTrade>> GetExchangeInfo()
    {
      try
      {
        // Get all the coins in the exchange
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
    #endregion
  }
}
