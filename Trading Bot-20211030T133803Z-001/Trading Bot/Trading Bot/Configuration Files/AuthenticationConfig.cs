using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    public const string Authentication_File = @"C:\Users\coker\Documents\TradingBotAssets\cb_authentication_file.txt";

    /// <summary>
    /// Dictionary accessor for the api key string.
    /// </summary>
    private const string Api_Key = "api_key";

    /// <summary>
    /// Dictionary accessor for the api secret string.
    /// </summary>
    private const string Api_Secret = "api_secret";

    /// <summary>
    /// Dictionary accessor for the api pass string.
    /// </summary>
    private const string Api_Pass = "api_pass";

    /// <summary>
    /// Dictionary accessor for the api url string.
    /// </summary>
    private const string Api_Url = "api_url";

    /// <summary>
    /// Dictionary accessor for the socket url string.
    /// </summary>
    private const string Socket_Url = "socket_url";
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
    /// Private api key.
    /// </summary>
    private static string api_key = string.Empty;

    /// <summary>
    /// Public api key.
    /// </summary>
    public static string API_KEY
    {
      get => api_key;
      set
      {
        if (value == null || value == string.Empty) { return; }

        api_key = value;
        Console.WriteLine("Succesfully initialised {0}.", Api_Key);
      }
    }

    /// <summary>
    /// Private api secret.
    /// </summary>
    private static string api_secret = string.Empty;

    /// <summary>
    /// Public api secret.
    /// </summary>
    public static string API_SECRET
    {
      get => api_secret;
      set
      {
        if (value == null || value == string.Empty) { return; }

        api_secret = value;
        Console.WriteLine("Succesfully initialised {0}.", Api_Secret);
      }
    }

    /// <summary>
    /// Private api pass.
    /// </summary>
    private static string api_pass = string.Empty;

    /// <summary>
    /// Public api pass.
    /// </summary>
    public static string API_PASS
    {
      get => api_pass;
      set
      {
        if (value == null || value == string.Empty) { return; }

        api_pass = value;
        Console.WriteLine("Succesfully initialised {0}.", Api_Pass);
      }
    }

    /// <summary>
    /// Private api url.
    /// </summary>
    private static string api_url = string.Empty;

    /// <summary>
    /// Public api url.
    /// </summary>
    public static string API_URL
    {
      get => api_url;
      set
      {
        if (value == null || value == string.Empty) { return; }

        api_url = value;
        Console.WriteLine("Succesfully initialised {0}.", Api_Url);
      }
    }

    /// <summary>
    /// Private socket url.
    /// </summary>
    private static string socket_url = string.Empty;

    /// <summary>
    /// Public socket url.
    /// </summary>
    public static string SOCKET_URL
    {
      get => socket_url;
      set
      {
        if (value == null || value == string.Empty) { return; }

        socket_url = value;
        Console.WriteLine("Succesfully initialised {0}.", Socket_Url);
      }
    }

    /// <summary>
    ///  Checks if Authentication Config has been initialised.
    /// </summary>
    public static bool Initialised { get; set; }

    /// <summary>
    /// Storage to hold the 'secret' , 'key' and 'pass'.
    /// </summary>
    private static Dictionary<string, string> Authentication = new();
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
        Console.WriteLine("Reading authentication file...");

        // Try and open a file with credentials.
        string[] parsedFile = File.ReadAllLines(Authentication_File);

        foreach (string line in parsedFile)
        {
          try
          {
            string[] temp = Seperate(line);

            Authentication.Add(temp[0], temp[1]);
          }
          catch (NullReferenceException)
          {
            Console.WriteLine("Unable to get either index 0, or index 1 from seperated line. Invalid sequence.");
            return false;
          }
        }

        // By this stage we assume the autentication dictionary is now loaded and valid.
        // Now check if there's exactly 5 key value pairs, if so, successful, else, unsuccessful.

        Console.WriteLine("Checking Authorisation keys...");

        // Set the authentication keys.
        SetAuthenticionKeys();

        if (Authentication.Count == 5) { Initialised = true; Console.WriteLine(SUCCESS); } else { Initialised = false; Console.WriteLine(FAIL); }

        Console.WriteLine("Authentication Initialised.");
        Console.WriteLine("-------------------------------------------------------------------------\n");

        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine("Invalid file path.");
        Console.WriteLine("Failed Establishing Connection...");

        Console.WriteLine(e.Message);
        Initialised = false;
        // Failed initialisation of authentication config.
        throw new Exception("Authentication Configuration failed initialisation.");
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

    /// <summary>
    /// Set all the authentication variables.
    /// </summary>
    private static void SetAuthenticionKeys()
    {
      try
      {
        API_KEY = Authentication[Api_Key];

        API_SECRET = Authentication[Api_Secret];

        API_PASS = Authentication[Api_Pass];

        API_URL = Authentication[Api_Url];

        SOCKET_URL = Authentication[Socket_Url];

        // Successful initialisation of authentication config.
        Console.WriteLine(SUCCESS + " - Authentication Configuration succesfully initialised.");
      }
      catch (Exception)
      {
        // Failed initialisation of authentication config.
        Console.WriteLine(FAIL + " - Authentication Configuration  failed initialisation, please restart.");
        throw;
      }
    }

    #endregion
  }
}
