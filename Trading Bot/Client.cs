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
    public static string API_URL { get; set; }


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

    public static void MakeRequest(string endpoint)
    {
      var client = new RestClient("https://api.binance.com");

      RestRequest request = new RestRequest("/api/v3/time", Method.GET); 

      RestResponse response = (RestResponse)client.Get(request);

      long timestamp = GetTimestamp();

      request = new RestRequest(endpoint, Method.GET);

      request.AddHeader("X-MBX-APIKEY", API_KEY);

      request.AddQueryParameter("recvWindow", "5000");
      request.AddQueryParameter("timestamp", timestamp.ToString());

      request.AddQueryParameter("signature", CreateSignature(request.Parameters, API_SECRET));

      response = (RestResponse)client.Get(request);

      System.Diagnostics.Debug.WriteLine(response.Content);

    }

    public static string CreateSignature(List<Parameter> parameters, string secret)
    {
      var signature = "";
      if (parameters.Count > 0)
      {
        foreach (var item in parameters)
        {
          if (item.Name != "X-MBX-APIKEY")
            signature += $"{item.Name}={item.Value}&";
        }
        signature = signature.Substring(0, signature.Length - 1);
      }


      byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
      byte[] queryStringBytes = Encoding.UTF8.GetBytes(signature);
      HMACSHA256 hmacsha256 = new HMACSHA256(keyBytes);

      byte[] bytes = hmacsha256.ComputeHash(queryStringBytes);

      return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    private static long GetTimestamp()
    {
      return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
  }
}
