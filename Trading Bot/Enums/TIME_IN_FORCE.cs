using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot.Enums
{
  public enum TIME_IN_FORCE
  {
    NONE = 0,
    GTC = 1,  // 'Good till cancelled',
    IOC = 2,  // Immediate or Cancelled,
    FOK = 3   // Fill or Kill.
  }
}
