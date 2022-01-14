using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BinanceExchange;
using CryptoExchange.Net.Authentication;
using BinanceExchange.API.Client;
using BinanceExchange.API.Market;
using RestSharp;
using Newtonsoft.Json;

namespace Trading_Bot.Configuration_Files
{
  public static class Client
  {
    public static bool Initialised { get; set; }
    public static string API_KEY { get; set; } = AuthenticationConfig.Authentication[AuthenticationConfig.API_KEY];
    public static string API_SECRET { get; set; } = AuthenticationConfig.Authentication[AuthenticationConfig.API_SECRET];
    public static string API_URL { get; set; } = AuthenticationConfig.Authentication[AuthenticationConfig.API_URL];


    public static bool Initialise()
    {
      try
      {
        Console.WriteLine("Creating a client object...");

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
        Console.WriteLine("-------------------------------------------------------------------------\n");
        return false;
      }
      finally
      {
        Console.ForegroundColor = ConsoleColor.White;
      }
    }
  }
}
