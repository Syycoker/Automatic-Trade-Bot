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
        // Instantiate a Coinbase Pro Object using the now initialised Authentication.

        Console.WriteLine("Creating a client object...");

        AutomatedTradeBot.Client = new CoinbaseProClient(new Config
        {
          ApiKey = AuthenticationConfig.API_KEY,
          Secret = AuthenticationConfig.API_SECRET,
          Passphrase = AuthenticationConfig.API_PASS,
          ApiUrl = AuthenticationConfig.API_URL
        });

        if (AutomatedTradeBot.Client == null) { throw new Exception("Client is null or invalid."); }

        Console.WriteLine("Client successfully Initialised.");
        Console.WriteLine("-------------------------------------------------------------------------\n");

        Initialised = true;
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Initialised = false;
        // Failed initialisation of authentication config.
        throw new Exception("Client Configuration failed initialisation.");
      }
    }
  }
}
