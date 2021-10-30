using Coinbase.Pro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot.Configuration_Files
{
  public static class ClientConfig
  {
    public static bool Initialised { get; set; }

    public static bool Initialise()
    {
      try
      {
        Console.ForegroundColor = ConsoleColor.White;
        // Instantiate a Coinbase Pro Object using the now initialised Authentication.

        Console.WriteLine("Creating a client object...");

        AutomatedTradeBot.Client = new CoinbaseProClient(new Config
        {
          ApiKey = AuthenticationConfig.Authentication[AuthenticationConfig.API_KEY],
          Secret = AuthenticationConfig.Authentication[AuthenticationConfig.API_SECRET],
          Passphrase = AuthenticationConfig.Authentication[AuthenticationConfig.API_PASS],
          ApiUrl = AuthenticationConfig.Authentication[AuthenticationConfig.API_URL]
        });

        if (AutomatedTradeBot.Client == null) { throw new Exception("Client is null or invalid."); }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Client successfully Initialised.");
        Console.WriteLine("-------------------------------------------------------------------------\n");

        Initialised = true;
        return true;
      }
      catch (Exception e)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Initialised = false;
        // Failed initialisation of authentication config.
        throw new Exception("Client Configuration failed initialisation.");
      }
    }
  }
}
