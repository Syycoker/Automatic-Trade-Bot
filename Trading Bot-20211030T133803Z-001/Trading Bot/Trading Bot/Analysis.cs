using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coinbase;
using Coinbase.Pro;
using Trading_Bot.Configuration_Files;

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

    public static AnalysisEval GetPerformance(string coin, string uri)
    {
      try
      {
        // var response = await AutomatedTradeBot.Client.Transactions.ListTransactionsAsync();
        return AnalysisEval.NONE;
      }
      catch
      {
        return AnalysisEval.NONE;
      }
    }
    
  }
}
