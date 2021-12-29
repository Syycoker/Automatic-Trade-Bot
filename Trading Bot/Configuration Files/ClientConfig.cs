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

namespace Trading_Bot.Configuration_Files
{
  public static class ClientConfig
  {
    public static bool Initialised { get; set; }
    public static string URL_BASE { get; set; } = "";

    public static bool Initialise()
    {
      try
      {
        Console.ForegroundColor = ConsoleColor.White;
        // Instantiate a Coinbase Pro Object using the now initialised Authentication.

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
        // Failed initialisation of authentication config.
        throw new Exception("Client Configuration failed initialisation.");
      }
    }

    public static string JsonRequest(string url, string method)
    {
      // take care of any spaces in params
      url = Uri.EscapeUriString(url);

      string returnData = string.Empty;

      WebRequest webRequest = WebRequest.Create(url);

      if (webRequest != null)
      { 
        webRequest.Method = method;
        webRequest.ContentType = "application/json";

        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.CurrentCulture);
        string body = "";
        string signature = GenerateSignature(timestamp, method, url, body, AuthenticationConfig.Authentication[AuthenticationConfig.API_SECRET]);

        var whc = new WebHeaderCollection();
        whc.Add("CB-ACCESS-SIGN", signature);
        whc.Add("CB-ACCESS-TIMESTAMP", timestamp);
        whc.Add("CB-ACCESS-KEY", AuthenticationConfig.Authentication[AuthenticationConfig.API_KEY]);
        whc.Add("CB-VERSION", "2017-08-07");
        webRequest.Headers = whc;

        using (WebResponse response = webRequest.GetResponse())
        {
          using (Stream stream = response.GetResponseStream())
          {
            StreamReader reader = new StreamReader(stream);
            returnData = reader.ReadToEnd();
          }
        }
      }

      return returnData;
    }

    public static string GenerateSignature(string timestamp, string method, string url, string body, string appSecret)
    {
      return GetHMACInHex(appSecret, timestamp + method + url + body).ToLower();
    }
    internal static string GetHMACInHex(string key, string data)
    {
      var hmacKey = Encoding.UTF8.GetBytes(key);
      var dataBytes = Encoding.UTF8.GetBytes(data);

      using (var hmac = new HMACSHA256(hmacKey))
      {
        var sig = hmac.ComputeHash(dataBytes);
        return ByteToHexString(sig);
      }
    }
    
    static string ByteToHexString(byte[] bytes)
    {
      char[] c = new char[bytes.Length * 2];
      int b;
      for (int i = 0; i < bytes.Length; i++)
      {
        b = bytes[i] >> 4;
        c[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
        b = bytes[i] & 0xF;
        c[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
      }
      return new string(c);
    }
  }
}
