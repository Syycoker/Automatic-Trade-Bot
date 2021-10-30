using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coinbase.Pro;
using Coinbase.Pro.WebSockets;
using Coinbase.Pro.Models;
using WebSocket4Net;

namespace Trading_Bot.Configuration_Files
{
  public static class SocketConfig
  {
    public static bool Initialised { get; set; }

    public static bool Initialise()
    {
      try
      {
        // Instantiate a Coinbase Pro WebSocket Object using the now initialised Authentication.

        Console.WriteLine("Creating websocket object...");

        // Create an authenticated feed.
        AutomatedTradeBot.WebSocket = new CoinbaseProWebSocket(new WebSocketConfig
        {
          ApiKey = AuthenticationConfig.API_KEY,
          Secret = AuthenticationConfig.API_SECRET,
          Passphrase = AuthenticationConfig.API_PASS,
          SocketUri = AuthenticationConfig.SOCKET_URL,
        });

        if (AutomatedTradeBot.WebSocket == null) { throw new Exception("WebSocket is null or invalid."); }

        Console.WriteLine("WebSocket successfully Initialised.");
        Console.WriteLine("-------------------------------------------------------------------------\n");

        Initialised = true;
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);

        // Failed initialisation of authentication config.
        Initialised = false;
        throw new Exception("WebSocket Configuration failed initialisation, please restart.");
      }
    }
  }
}
