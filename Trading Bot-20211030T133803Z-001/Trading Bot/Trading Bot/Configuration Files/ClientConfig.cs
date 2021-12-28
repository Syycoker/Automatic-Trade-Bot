using Coinbase;
using Coinbase.Pro;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
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

        AutomatedTradeBot.Client = new CoinbaseClient(new ApiKeyConfig 
        { ApiKey = AuthenticationConfig.Authentication[AuthenticationConfig.API_KEY],
          ApiSecret = AuthenticationConfig.Authentication[AuthenticationConfig.API_SECRET]
        });

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

			var webRequest = WebRequest.Create(url) as HttpWebRequest;
			if (webRequest != null)
			{
				webRequest.Accept = "*/*";
				webRequest.UserAgent = ".NET";
				webRequest.Method = method;
				webRequest.ContentType = "application/json";
				webRequest.Host = "coinbase.com";

				long timestamp = Convert.ToInt64(DateTime.UtcNow);
				string message = timestamp + "GET" + url;
				string signature = HashEncode(HashHMAC(StringEncode(
					AuthenticationConfig.Authentication[AuthenticationConfig.API_SECRET]), StringEncode(message)));

				var whc = new WebHeaderCollection();
				whc.Add("CB-ACCESS-KEY: " + AuthenticationConfig.Authentication[AuthenticationConfig.API_KEY]);
				whc.Add("CB-ACCESS-SIGN: " + signature);
				whc.Add("CB-ACCESS-TIMESTAMP: " + timestamp);
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

		private static byte[] StringEncode(string text)
		{
			var encoding = new ASCIIEncoding();
			return encoding.GetBytes(text);
		}

		private static string HashEncode(byte[] hash)
		{
			return BitConverter.ToString(hash).Replace("-", "").ToLower();
		}

		private static byte[] HashHMAC(byte[] key, byte[] message)
		{
			var hash = new HMACSHA256(key);
			return hash.ComputeHash(message);
		}
	}
}
