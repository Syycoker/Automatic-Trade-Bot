using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading_Bot.Enums
{
  public enum ORDER_STATUS
  {
    NONE                = 0,
    INSUFFICIENT_FUNDS  = 1,
    SUCCESS             = 2,
    INVALID_PARAMETER   = 3,
    UNEXPECTED          = 4,
    TIME_OUT_OF_SYNC    = 5
  }
}
