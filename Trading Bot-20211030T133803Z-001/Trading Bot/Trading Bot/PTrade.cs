using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot
{
  /// <summary>
  /// Trade object to describe a trade from the SQL Database.
  /// </summary>
  public class PTrade
  {
    #region Public
    /// <summary>
    /// The Id of the coin.
    /// </summary>
    public int Product_ID { get; set; }

    /// <summary>
    /// The price of the coin was bought/sold.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The last time the coin was bought/sold.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// The public name of the coin itself.
    /// </summary>
    public string Product_Name { get; set; }

    /// <summary>
    /// Character to represent what status of the trade was via single letter characters, i.e. (b/B) for 'Bought'.
    /// </summary>
    public char BuyOrSell { get; set; }

    /// <summary>
    /// The price the coin was bought for.
    /// </summary>
    public decimal Buy_Price { get; set; }

    /// <summary>
    /// The price of the coin was sold for.
    /// </summary>
    public decimal Sell_Price { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Trade constructor with non-nullable values to be entered / recieved from database.
    /// </summary>
    /// <param name="product_ID"></param>
    /// <param name="price"></param>
    /// <param name="time"></param>
    public PTrade(int product_ID, decimal price, DateTime time)
    {
      Product_ID = product_ID;
      Price = price;
      Time = time;
    }

    /// <summary>
    /// Trade constructor with all values to be entered/ recieved from database.
    /// </summary>
    /// <param name="product_ID"></param>
    /// <param name="price"></param>
    /// <param name="time"></param>
    /// <param name="product_Name"></param>
    /// <param name="buyOrSell"></param>
    /// <param name="buy_Price"></param>
    /// <param name="sell_Price"></param>
    public PTrade(int product_ID, decimal price, DateTime time, string product_Name, char buyOrSell, decimal buy_Price, decimal sell_Price)
    {
      Product_ID = product_ID;
      Price = price;
      Time = time;
      Product_Name = product_Name;
      BuyOrSell = buyOrSell;
      Buy_Price = buy_Price;
      Sell_Price = sell_Price;
    }
    #endregion
  }
}
