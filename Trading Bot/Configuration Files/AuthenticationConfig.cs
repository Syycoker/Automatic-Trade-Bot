using System;
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
using System.Globalization;
using System.Net;
using Trading_Bot.Exceptions;

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
    public static string Authentication_File() { return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + "UserAuthentication.xml"; }
    public const string API_NAME = "DEFAULT_API";
    public const string API_TEST_NAME = "DEFAULT_API_TEST";
    public const string API_KEY = "API_KEY";
    public const string API_SECRET = "API_SECRET";
    public const string API_PASS = "API_PASSPHRASE";
    public const string API_URL = "API_URL";
    public const string SOCKET_URL = "SOCKET_URL";
    #endregion
    #region Public
    /// <summary>
    ///  Checks if Authentication Config has been initialised.
    /// </summary>
    public static bool Initialised { get; set; }

    /// <summary>
    /// To check wheter the authentication strings should be from the sandbox api or not.
    /// </summary>
    public static bool SandBoxMode { get; set; } = true;

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
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Reading authentication file...");

        string filePath = Authentication_File();
        if (!File.Exists(filePath))
        {
          throw new InvalidAuthenticationException();
        }

        // Try and open a file with credentials.
        //string parsedFile = File.ReadAllLines(Authentication_File);
        XmlDocument doc = new();
        doc.Load(filePath);
        XmlElement docElement = doc.DocumentElement;

        foreach (XmlNode node in docElement.ChildNodes)
        {
          if (SandBoxMode == false && node.Name.ToLower().Equals(API_NAME.ToLower())) { ParseNode(node); break; }
          if (SandBoxMode && node.Name.ToLower().Equals(API_TEST_NAME.ToLower())) { ParseNode(node); break; }
        }
        // By this stage we assume the autentication dictionary is now loaded and valid.
        // Now check if there's exactly 5 key value pairs, if so, successful, else, unsuccessful.
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Checking Authorisation keys...");

        Console.WriteLine("Authentication Initialised.");
        Console.WriteLine("-------------------------------------------------------------------------\n");

        return true;
      }
      catch (Exception e)
      {
        // Just check if the exception is of this type, if so, control the exception
        if (e.GetType() == typeof(InvalidAuthenticationException))
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine("The system has detected that you do not have the correct file type to parse.");
          Console.WriteLine("The system will now create the default file in  your desktop area.");
          Console.WriteLine("Fill In the values in the element attributes.");
          Console.WriteLine("Please restart the system...");

          return false;
        }
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid file path.");
        Console.WriteLine("Failed Establishing Connection...");

        Console.WriteLine(e.Message);
        Initialised = false;
        return false;
      }
      finally
      {
        Console.ForegroundColor = ConsoleColor.White;
      }
    }

    #endregion
    #region Private
    private static void ParseNode(XmlNode node)
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
    #endregion
  }
}
