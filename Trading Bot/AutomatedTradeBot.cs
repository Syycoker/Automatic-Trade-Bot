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

        Console.ForegroundColor = ConsoleColor.White;

        // Set the Binance Client
        HttpClient hClient = new HttpClient();
        BClient = new BinanceService(hClient);

        Console.WriteLine("Program Starting...");

        Thread buyThread = new Thread(BuySystem.AnalyseMarket);
        buyThread.Start();
      }

      catch (Exception e)
      {
        // Send a push notification to phone if any error arises.
        // Restart program again if application closes...
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ResetColor();
        Console.ReadKey();
      }
    }
  }
}
