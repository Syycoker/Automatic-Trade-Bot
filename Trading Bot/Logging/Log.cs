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
      lock (Console.Out)
      {
        switch (logType)
        {
          case MessageLog.NONE:
          case MessageLog.NORMAL:
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            break;

          case MessageLog.WARNING:
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            break;

          case MessageLog.ERROR:
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            break;

          case MessageLog.SUCCESS:
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            break;
        }

        Console.ResetColor();
      }
    }
  }
}
