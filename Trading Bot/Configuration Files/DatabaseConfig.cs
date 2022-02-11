using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Trading_Bot
{
  /// <summary>
  /// Static class to configure and open the database.
  /// </summary>
  public static class DatabaseConfig
  {

    #region Constants
    public const string Product_Id = "Product_Id";
    public const string Price = "Price";
    public const string Date_Time = "Date_Time";
    public const string Product_Name = "Product_Name";
    public const string BuyOrSell = "BuyOrSell";
    public const string Buy_Price = "Buy_Price";
    public const string Sell_Price = "Sell_Price";
    #endregion

    /// <summary>
    /// Indicates if the database is configured or not.
    /// </summary>
    public static bool Initialised { get; set; }

    /// <summary>
    /// The connection string for the database.
    /// </summary>
    public static string ConnectionString;

    /// <summary>
    /// The connection bridge for the mysql connection.
    /// </summary>
    public static MySqlConnection Connection;

    /// <summary>
    /// Attempts to initialise the database.
    /// </summary>
    /// <returns></returns>
    public static bool Initialise()
    {
      try
      {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Initialising Database...");
        ConnectionString = "";

        Connection = new MySqlConnection(ConnectionString);
        Connection.Open();

        if (Connection.State == System.Data.ConnectionState.Broken) { throw new Exception("Invalid MySQLConnection."); }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Database Initialised.");
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
        throw new Exception("Database Configuration failed initialisation, please restart.");
      }
    }
  }
}
