﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Trading_Bot.Configuration_Files;
using System.Collections.Generic;
using System.Threading;
using RestSharp;
using Binance;
using Binance.Net;
using Binance.Net.Objects;
using BinanceExchange.API.Models.Response.Interfaces;
using BinanceExchange.API.Models.Request;

namespace Trading_Bot
{
  public class AutomatedTradeBot
  {
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
        AuthenticationConfig.SandBoxMode = false;

        // Initialise the authorisation codes.
        AuthenticationConfig.Initialise();

        // Initialise the database.
        //DatabaseConfig.Initialise();

        Client.Initialise();

        // Initialise the socket [DEPRECTAED]
        SocketConfig.Initialise();

        Console.ForegroundColor = ConsoleColor.White;
       
        Task.Run(async () =>
        {
          var response = Client.MakeRequest("/api/v3/account");
          System.Diagnostics.Debug.WriteLine(response.Content);
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
  }
}
