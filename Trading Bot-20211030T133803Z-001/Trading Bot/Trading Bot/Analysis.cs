using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coinbase;
using Coinbase.Pro;

namespace Trading_Bot
{
  /// <summary>
  /// Runs an evaluation of a coin and determines the likehood of 'success' based on it's returned AnalysisEval Enum.
  /// </summary>
  public static class Analysis
  {
    /// <summary>
    /// The '+ / -' percentage of the coin's buy / sell limit.
    /// </summary>
    public static double Threshold { get; private set; }

    /// <summary>
    /// Determines how well a stock is doing now comapred to last buy time.
    /// </summary>
    public static async void PastPerformance(string productId)
    {
      // All criteria is ran through the basis of a threshold.

      // If the coin's price is greater than its last buy price, sell.
      // If the coin's price has not changed in the past hour or so, sell.
      // If the coin's price is less than its last buy price, sell.
      
    }
    
  }
}
