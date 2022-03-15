using BinanceDotNet;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trading_Bot.Enums;
using Trading_Bot.Logging;

namespace Trading_Bot
{
  public static class SellSystem
  {
    private static readonly BinanceService BClient;   
    private static readonly decimal SELL_FEE;
    public static readonly bool Initialised;
    private const float MINIMUM_PROFIT_THRESHOLD_PERCENT = 0.0001f; // The percentage that the asset must reach if (current market price - (sale price + fees))

    static SellSystem()
    {
      try
      {
        // Set the Binance Client
        BClient = AutomatedTradeBot.BClient;

        /*
         * Use the 'BClient' to make calls to Binance's endpoints, i.e. get the fees for the account.
         * Gets the fees for your specific account, which will be taken into account to determine whether the trade fee plus the asset will be a 'good' trade.
         */

        string feesResponse = BClient.SendSignedAsync("/api/v3/account", HttpMethod.Get).Result;

        if (string.IsNullOrEmpty(feesResponse))
          throw new ArgumentNullException("The fee response was null.");

        if (feesResponse.Contains("1021") || feesResponse.Contains("invalid"))
          throw new ArgumentNullException("The fee response threw an invalid operations exception.");

        JObject feeResponseObj = JObject.Parse(feesResponse);
        SELL_FEE = feeResponseObj["takerCommission"].Value<decimal>() / 10000;

        Initialised = true;
        Log.Msg($"{DateTime.UtcNow.ToString("dd-MM-yyyy")} - { nameof(SellSystem) } has been Initialised.", MessageLog.SUCCESS);
      }
      catch(Exception e)
      {
        Log.Msg($"{DateTime.UtcNow.ToString("dd-MM-yyyy")} - { e.Message }.", MessageLog.SUCCESS);
        Initialised = false;
      }
    }

    public static void Begin()
    {
      try
      {
        // Look through every asset that we own.

        Thread searchThroughAllAssetsThread = new Thread(() => LookThroughAssetsOwned()) { IsBackground = true, Name = nameof(Begin) };
        searchThroughAllAssetsThread.Start();
      }
      catch (Exception e)
      {
        Log.Msg($"{DateTime.UtcNow.ToString("dd-MM-yyyy")} - { e.Message }.", MessageLog.SUCCESS);
      }
    }

    private static Task LookThroughAssetsOwned()
    {
      Log.Msg($"{DateTime.UtcNow.ToString("dd-MM-yyyy")} - Looking through assets owned.", MessageLog.SUCCESS);

      // Make a call to the binance endpoint asking to retrieve all the assets owned.

      return Task.CompletedTask;
    }
  }
}
