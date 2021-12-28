using System;
using System.Linq;
using System.Threading.Tasks;
using Coinbase;
using Coinbase.Pro;
using Coinbase.Pro.Models;
using Coinbase.Pro.WebSockets;
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
    public static CoinbaseClient Client { get; set; }
    public static CoinbaseProClient ProClient { get; set; }
    public static CoinbaseProWebSocket ProWebSocket { get; set; }
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
          var response = await Client.Data.GetSpotPriceAsync("ETH-USD");
          var accounts = await Client.Accounts.ListAccountsAsync();

          var responseTest = ClientConfig.JsonRequest(@"https://api.coinbase.com/v2/users/:user_id", "GET");

          Console.WriteLine("Hi.");
          
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

        var orders = await ProClient.Orders.GetAllOrdersAsync();
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
    /// If returned a decent - good 'AnalysisEval', then buy coin, else continue in loop.
    /// </summary>
    private async static Task<List<PaymentMethod>> BuyCoin()
    {
      // Get how much I currently have
      //await Client.Orders.PlaceLimitOrderAsync(OrderSide.Buy, "SHIB-BTC", limitPrice: 1, GoodTillTime.Day);

      return await ProClient.PaymentMethods.GetAllPaymentMethodsAsync();
    }

    /// <summary>
    /// Sells the coin from coinbase account.
    /// </summary>
    private async static void SellCoin()
    {

    }

    public string GetUserAccountBalance(string id)
    {
      return ClientConfig.JsonRequest(URL_BASE + "accounts/" + id + "/balance", "GET");
    }

    #endregion

    /// <summary>
    /// Allows the user to view available cryptos and gives them an option to buy said crypto.
    /// </summary>
    /// <param name="crypto"></param>
    private static async void ViewPerformance()
    {
      try
      {
        var products = await ProClient.MarketData.GetProductsAsync();
        var productIds = products.Select(p => p.Id).ToArray();
        Console.WriteLine(">> Available Product IDs:");

        foreach (var productId in productIds)
        {
          Console.WriteLine($">>  {productId}");
        }

        Console.WriteLine("\n Which crypto would you like to view? \r\n");

        string selectedCrypto;
        Stats data;

        while (true)
        {
          selectedCrypto = Console.ReadLine();

          data = await ProClient.MarketData.GetStatsAsync(selectedCrypto);

          if (data == null) { Console.WriteLine("Invalid entry."); continue; }

          break;
        }

        Console.WriteLine($">> {0} Volume: {data.Volume}", selectedCrypto);


        //Ask user if they'd like to buy this crypto?
        Console.WriteLine("Press 'y' if you would you like to buy {0}?, else press any other key", selectedCrypto);
        string answer = Console.ReadLine();

        if (answer.Equals('y'))
        {
          Console.WriteLine("Currently placing an order...");

          // Your account, selected crypro, how much you're willing to pay

          Console.WriteLine("Amount (USD): ");

          
          decimal amount = 0.00m;
          decimal.TryParse(Console.ReadLine(), out amount);


          PlaceAnOrder(ProClient, selectedCrypto, amount);
        }
        else
        {
          Console.WriteLine("Exited purchase of {0}.", selectedCrypto);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(FAIL + " " + e.Message);
        // Just swallow this exception...
      }
    }

    /// <summary>
    /// Place an order of any crypto (provided the string key) and the price they want to pay.
    /// </summary>
    private static async void PlaceAnOrder(CoinbaseProClient client, string crypto, decimal amount)
    {
      try
      {
        var order = await client.Orders.PlaceLimitOrderAsync(OrderSide.Buy, crypto, size: 1, limitPrice: amount);
        Console.WriteLine("Order Placed.");
      }
      catch (Exception e)
      {
        Console.WriteLine(await e.GetErrorMessageAsync());
        Console.WriteLine("Product not found.");
      }
    }

    /// <summary>
    /// View the web feed from coinbase.
    /// </summary>
    private static async void InitiateWebSocket()
    {
      Console.WriteLine(SUCCESS + " - Connecting to the websocket...");

      var result = await ProWebSocket.ConnectAsync();

      if (result.Success == false) { Console.WriteLine(FAIL + " - Unable to connect to wss://ws-feed-public.sandbox.pro.coinbase.com..."); return; }

      Console.WriteLine("Creating websocket event listeners...");

      ProWebSocket.RawSocket.MessageReceived += HandleMessage;
      ProWebSocket.RawSocket.Closed += HandleWebSocketClosed;
      ProWebSocket.RawSocket.Error += HandleWebSocketError;

      var sub = new Subscription
      {
        ProductIds =
        {
          //Cryptos.Bitcoin,
        },
        Channels =
        {
          "heartbeat",
        }
      };

      Console.WriteLine("Currently Viewing {0} in {1} channel...", sub.ProductIds, sub.Channels);

      Console.WriteLine("Subscribing socket event...");

      // subscribe the event.
      await ProWebSocket.SubscribeAsync(sub);

      Console.WriteLine("Waiting for data...");

      // now wait for data.
      await Task.Delay(TimeSpan.FromMinutes(1));
    }

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

    /// <summary>
    /// Parses the JSON object recieved from the coinbase websocket.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void HandleMessage(object sender, MessageReceivedEventArgs e)
    {
      // Try parsing the e.Message JSON Body.

      if (WebSocketHelper.TryParse(e.Message, out var msg))
      {
        if (msg is HeartbeatEvent hb)
        {
          Console.WriteLine($"Sequence: {hb.Sequence}, Last Trade Id: {hb.LastTradeId}");
        }
        else
          Console.WriteLine("Message recieved is not a heartbeat event.");
      }
    }
    #endregion
  }
}
