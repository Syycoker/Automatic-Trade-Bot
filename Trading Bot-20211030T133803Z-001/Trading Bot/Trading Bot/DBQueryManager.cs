using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot
{
  /// <summary>
  /// Class to run a query on the database to retrieve / display information about Trades / Orders completed.
  /// </summary>
  public class DBQueryManager
  {
    #region Constructor
    /// <summary>
    /// Constructor to check if the Databse is actually configured already.
    /// </summary>
    public DBQueryManager()
    {
      if (DatabaseConfig.Initialised == false) { throw new Exception("Unable to Database Config is not initialised."); }
    }
    #endregion

    #region Inserts

    /// <summary>
    /// Adds non nullable information into the 'Trades' Table.
    /// </summary>
    public void AddToDB(int productId, decimal price, DateTime time)
    {
      MySqlCommand command = DatabaseConfig.Connection.CreateCommand();
      try
      {
        command.CommandText = $"INSERT INTO Trades({DatabaseConfig.Product_Id},{DatabaseConfig.Price},{DatabaseConfig.Date_Time}) VALUES({productId}, {price}, {time})";

        try
        {
          DatabaseConfig.Connection.Open();
          command.ExecuteNonQuery();
          //command.ExecuteNonQuery();
        }
        catch (Exception)
        {
          Console.WriteLine("Cannot open connection to Database.");
        }
      }
      catch (Exception)
      {
        Console.WriteLine("Invalid query token, cannot retrieve query result.");
        return;
      }
      finally
      {
        DatabaseConfig.Connection.Close();
      }
    }

    /// <summary>
    /// Adds all information about a trade into the 'Trades' Table.
    /// </summary>
    public void AddToDB(int productId, decimal price, DateTime time, string productName, char buyOrSell, decimal buyPrice, decimal sellPrice)
    {
      MySqlCommand command = DatabaseConfig.Connection.CreateCommand();
      try
      {
        command.CommandText = $"INSERT INTO Trades({DatabaseConfig.Product_Id},{DatabaseConfig.Price},{DatabaseConfig.Date_Time},{DatabaseConfig.Product_Name}, {DatabaseConfig.BuyOrSell}, {DatabaseConfig.Buy_Price}, {DatabaseConfig.Sell_Price}) VALUES({productId}, {price}, {time}, {productName}, {buyOrSell}, {buyPrice}, {sellPrice})";

        try
        {
          DatabaseConfig.Connection.Open();
          // Maybe asychronously??
          command.ExecuteNonQuery();

          //command.ExecuteNonQuery();
        }
        catch (Exception)
        {
          Console.WriteLine("Cannot open connection to Database.");
        }
      }
      catch (Exception)
      {
        Console.WriteLine("Invalid query token, cannot retrieve query result.");
        return;
      }
      finally
      {
        DatabaseConfig.Connection.Close();
      }
    }
    #endregion

    #region Selects

    /// <summary>
    /// Returns all trades within 'automatedtradingdb' from the'Trades' Table.
    /// </summary>
    /// <returns></returns>
    public List<Trade> GetAllTrades()
    {
      List<Trade> trades = new();

      MySqlCommand command = DatabaseConfig.Connection.CreateCommand();
      try
      {
        command.CommandText = "SELECT * FROM Trades";

        try
        {
          DatabaseConfig.Connection.Open();

          //Do this asynchronously!!!! using BeingExecuteReader();
          MySqlDataReader reader = command.ExecuteReader();
          while (reader.Read())
          {
            Trade trade = new((int)reader[DatabaseConfig.Product_Id], (decimal)reader[DatabaseConfig.Price], (DateTime)reader[DatabaseConfig.Date_Time], (string)reader[DatabaseConfig.Product_Name], (char)reader[DatabaseConfig.BuyOrSell], (decimal)reader[DatabaseConfig.Buy_Price], (decimal)reader[DatabaseConfig.Sell_Price]);

            trades.Add(trade);
          }
        }
        catch (Exception)
        {
          Console.WriteLine("Cannot open connection to database.");
        }

      }
      catch(Exception)
      {
        Console.WriteLine("Invalid query token, cannot retrieve query result.");
      }
      finally
      {
        DatabaseConfig.Connection.Close();
      }

      return trades;
    }
    #endregion
  }
}
