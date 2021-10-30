using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot
{
  /// <summary>
  /// Enums to declare how well a coin is at the *present* time and how well it will perform in 'x' amount of time.
  /// </summary>
  public enum AnalysisEval
  {
    /// <summary>
    /// No evalutation, i.e. null
    /// </summary>
    NONE = 0,

    Excellent = 1,

    VERY_GOOD = 2,

    GOOD = 3,

    SLIGHTLY_GOOD = 4,

    FAIR = 5,

    SLIGHTLY_POOR = 6,

    POOR = 7,

    VERY_POOR = 8,

    BAD = 9,

    ABYSMAL = 10
  }
}
