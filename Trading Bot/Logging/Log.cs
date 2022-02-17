using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trading_Bot.Enums;

namespace Trading_Bot.Logging
{
  public static class Log
  {
    public static void Msg(string message, MessageLog logType)
    {
      switch (logType)
      {
        case MessageLog.NONE:
        case MessageLog.NORMAL:
          Console.ForegroundColor = ConsoleColor.White;
          Console.WriteLine(message);
          break;

        case MessageLog.WARNING:
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(message);
          break;

        case MessageLog.ERROR:
          Console.ForegroundColor = ConsoleColor.DarkRed;
          Console.WriteLine(message);
          break;
      }

      Console.ResetColor();
    }
  }
}
