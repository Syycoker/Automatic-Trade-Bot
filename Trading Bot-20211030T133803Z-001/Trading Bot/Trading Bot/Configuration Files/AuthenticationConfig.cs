﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Net.Http;
using Trading_Bot.Configuration_Files;
using Newtonsoft.Json.Linq;

namespace Trading_Bot
{
  /// <summary>
  ///  Static class for the authentication.
  /// </summary>
  public static class AuthenticationConfig
  {
    #region Constants

    /// <summary>
    /// The file address for the authentication file.
    /// </summary>
    public const string Authentication_File = @"C:\Users\Sylas Coker\Desktop\UserAuthentication.xml";

    public const string API_KEY = "API_KEY";
    public const string API_SECRET = "API_SECRET";
    public const string API_PASS = "API_PASSPHRASE";
    public const string API_URL = "API_URL";
    public const string SOCKET_URL = "SOCKET_URL";

    #endregion

    #region Public
    /// <summary>
    /// Returns a successful constant string.
    /// </summary>
    private const string SUCCESS = "Successful Operation.";

    /// <summary>
    /// Returns an failed constant string.
    /// </summary>
    private const string FAIL = "Failed Operation.";

    /// <summary>
    ///  Checks if Authentication Config has been initialised.
    /// </summary>
    public static bool Initialised { get; set; }

    /// <summary>
    /// Check if you want to trade using the sandbox api url or not.
    /// </summary>
    public static bool Sandbox { get; set; }

    /// <summary>
    /// Storage to hold the 'secret' , 'key' and 'pass'.
    /// </summary>
    public static Dictionary<string, string> Authentication = new();
    #endregion

    #region Initialisation
    /// <summary>
    /// Try to establish valid Authentication.
    /// </summary>
    /// <returns></returns>
    public static bool Initialise()
    {
      try
      {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Reading authentication file...");

        // Try and open a file with credentials.
        //string parsedFile = File.ReadAllLines(Authentication_File);
        XmlDocument doc = new();
        doc.Load(Authentication_File);
        XmlElement docElement = doc.DocumentElement;

        foreach (XmlNode node in docElement.ChildNodes)
        {
          #region Sandbox
          if (Sandbox)
          {
            if (node.Name.Equals("Sandbox"))
            {
              foreach (XmlNode childNode in node.ChildNodes)
              {
                switch (childNode.Name)
                {
                  case "AUTH_KEY":
                    Authentication.Add(API_KEY, childNode.Attributes["value"].Value);
                    break;
                  case "AUTH_SECRET":
                    Authentication.Add(API_SECRET, childNode.Attributes["value"].Value);
                    break;
                  case "AUTH_PASSPHRASE":
                    Authentication.Add(API_PASS, childNode.Attributes["value"].Value);
                    break;
                  case "AUTH_URL":
                    Authentication.Add(API_URL, childNode.Attributes["value"].Value);
                    break;
                  case "SOCKET_URL":
                    Authentication.Add(SOCKET_URL, childNode.Attributes["value"].Value);
                    break;
                }
              }
            }
          }
          #endregion
          #region Non Sandbox
          else
          {
            if (node.Name.Equals("Non_Sandbox"))
            {
              foreach (XmlNode childNode in node.ChildNodes)
              {
                switch (childNode.Name)
                {
                  case "AUTH_KEY":
                    Authentication.Add(API_KEY, childNode.Attributes["value"].Value);
                    break;
                  case "AUTH_SECRET":
                    Authentication.Add(API_SECRET, childNode.Attributes["value"].Value);
                    break;
                  case "AUTH_PASSPHRASE":
                    Authentication.Add(API_PASS, childNode.Attributes["value"].Value);
                    break;
                  case "SOCKET_URL":
                    Authentication.Add(SOCKET_URL, childNode.Attributes["value"].Value);
                    break;
                }
              }
            }
          }
          #endregion
        }

        // By this stage we assume the autentication dictionary is now loaded and valid.
        // Now check if there's exactly 5 key value pairs, if so, successful, else, unsuccessful.
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Checking Authorisation keys...");
        // Example
        // var jsonObject = GetResponse(@"https://api.coinbase.com/v2/exchange-rates?currency=USD");
        Console.WriteLine("Authentication Initialised.");
        Console.WriteLine("-------------------------------------------------------------------------\n");

        return true;
      }
      catch (Exception e)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid file path.");
        Console.WriteLine("Failed Establishing Connection...");

        Console.WriteLine(e.Message);
        Initialised = false;
        // Failed initialisation of authentication config.
        throw new Exception("Authentication Configuration failed initialisation.");
      }
      finally
      {
        Console.ForegroundColor = ConsoleColor.White;
      }
    }

    #endregion

    #region Methods
    /// <summary>
    /// Wrapper to get any request using a uri to return a JSON Object.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static async Task<JObject> GetResponse(string uri)
    {
      try
      {
        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan end = DateTime.Now.ToUniversalTime() - start;
        var timestamp = (long)Math.Floor(end.TotalSeconds);

        var message = timestamp + "GET" + uri + "";

        string signature = ComputeHash(message);

        var request = new HttpRequestMessage
        {
          RequestUri = new Uri(uri),
          Method = HttpMethod.Get,
          Headers =
                {
                  { "CB-ACCESS-SIGN", signature },
                  { "CB-ACCESS-TIMESTAMP", timestamp.ToString() },
                  { "CB-ACCESS-KEY", Authentication[API_KEY] },
                },
        };

        HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = await httpClient.SendAsync(request);
        HttpContent responseContent = response.Content;
        string resultsString = string.Empty;

        using (var reader = new StreamReader(responseContent.ReadAsStreamAsync().Result))
        {
          // Write the output.
          resultsString = reader.ReadToEndAsync().Result;
        }

        if (resultsString == null) { throw new Exception("Result String was empty."); }

        //get the json result and go down one level and update the result
        JObject json = JObject.Parse(resultsString);

        return json;
      }
      catch
      {
        return null;
      }
      finally
      {
        Console.ForegroundColor = ConsoleColor.White;
      }
    }
    #endregion

    #region Private

    /// <summary>
    /// Given a parsed line from file and returns a formatted string that can be stored.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string[] Seperate(string str)
    {
      str = Regex.Replace(str, @"\s+", "");

      string[] dict = str.Trim().Split('~');

      return dict;
    }

    private static string ComputeHash(string message)
    {
      HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(Authentication[API_SECRET]));
      byte[] hashedMessage = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
      return Convert.ToBase64String(hashedMessage);
    }

    #endregion
  }
}
