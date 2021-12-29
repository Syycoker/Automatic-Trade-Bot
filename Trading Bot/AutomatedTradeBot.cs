using System;
using System.Linq;
using System.Threading.Tasks;
using WebSocket4Net;
using Trading_Bot.Configuration_Files;
using System.Collections.Generic;
using System.Threading;
using RestSharp;

namespace Trading_Bot
{
  public class AutomatedTradeBot
  {
    #region Constants
    /// <summary>
    /// Returns a successful constant string.
    /// </summary>
    public const string SUCCESS = "Successful Operation.";

    /// <summary>
    /// Returns an failed constant string.
    /// </summary>
    public const string FAIL = "Failed Operation.";

    public const string URL_BASE = "https://api.coinbase.com/v2/";
    #endregion

    #region Initialised Object
    #endregion

    #region Thread Safety
    private static SemaphoreSlim Semaphore = new SemaphoreSlim(1 , 1);
    #endregion

    /// <summary>
    /// Main method becomes asynchronous when all the system initialisations are complete as async calls propogate up...
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
      try
      {
        AuthenticationConfig.Sandbox = false;
        Console.WriteLine("Sandbox is currently active : {0}.", AuthenticationConfig.Sandbox);

        // Initialise the authorisation codes.
        AuthenticationConfig.Initialise();

        // Initialise the database.
        //DatabaseConfig.Initialise();

        // Initialise the database.
        ClientConfig.Initialise();

        // Initialise the database.
        SocketConfig.Initialise();

        Console.ForegroundColor = ConsoleColor.White;
       
        Task.Run(async () =>
        {
          var responseTest = ClientConfig.JsonRequest(URL_BASE + "user", "GET");

          Console.WriteLine(responseTest);
          
        }).GetAwaiter().GetResult();
      }

      catch (Exception e)
      {
        // Senda push notification to phone if any error arises.
        // Restart program again if application closes...
        Console.WriteLine(e.Message);
        Console.ReadKey();
      }
    }

    #region Main Procedure

    /// <summary>
    /// Returns an enumarable collection of coins that can be bought / traded for at *this* very moment.
    /// </summary>
    private static async Task<List<PTrade>> FindPositiveCoins()
    {
      try
      {
        // Find all coins that have an 'AnalysisEval' of 5 or higher that have a positive trend.
        // 
        await Semaphore.WaitAsync();

        
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
    /// Chooses the coin with the most prospect to return a good 'AnalysisEval'.
    /// </summary>
    private async static void ChooseCoin()
    {

    }

    /// <summary>
    /// Runs and evalutation on the coin itself, decides whether to buy the coin.
    /// </summary>
    private async static void RunEvalutationOnCoin()
    {

    }


    /// <summary>
    /// Sells the coin from coinbase account.
    /// </summary>
    private async static void SellCoin()
    {

    }

    #endregion

    #region Events
    /// <summary>
    /// Event handler for when the websocket is closed for whatever reason.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void HandleWebSocketClosed(object sender, EventArgs e)
    {
      Console.WriteLine("The websocket closed.");
    }

    /// <summary>
    /// Event handler for when an error is occured when the websocket is active.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void HandleWebSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
    {
      Console.WriteLine("Websocket Error!");
      Console.WriteLine(e);
    }
    #endregion
  }
}
