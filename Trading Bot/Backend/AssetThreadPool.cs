using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trading_Bot.Enums;
using Trading_Bot.Logging;

namespace Trading_Bot.Backend
{
  public static class AssetThreadPool
  {
    #region Members
    public static long WorkingThreadCount = 0;
    public static long RunningThreadCount = 0;
    public static long FinishedThreads = 0;
    public static long MaxThreadsUsed = 0;
    public static long TotalQueued = 0;
    public static Queue<WaitCallback> AssetQueue = new Queue<WaitCallback>();
    public static int MaximumThreads = 100;
    public static List<Thread> AssetThreadPoolWorkers = new();
    public static Semaphore DequeueSemaphore = new Semaphore(0, 100);
    #endregion
    #region Methods

    public static bool QueueUserWorkItem(WaitCallback callBack)
    {
      if (callBack is null)
        throw new Exception("Callback delegate is null.");

      lock (AssetQueue)
      {
        AssetQueue.Enqueue(callBack);

        if (MaximumThreads > AssetThreadPoolWorkers.Count)
        {
          long busyThreads = Interlocked.Read(ref WorkingThreadCount);

          if (busyThreads >= AssetThreadPoolWorkers.Count)
          {
            Log.Msg("Asset Thread Starting...", MessageLog.NORMAL);

            var param = callBack.Method.GetParameters();

            Thread thread = new Thread(new ParameterizedThreadStart(BuySystem.BeginAnalysis)) { IsBackground = true, Name = param[0].Name };
            AssetThreadPoolWorkers.Add(thread);
            thread.Start(param[0]);
            MaxThreadsUsed++;
            Interlocked.Increment(ref RunningThreadCount);
          }
        }
        DequeueSemaphore.Release();
      }
      return true;
    }

    public static bool HasRunningJobs()
    {
      bool running = 0 != Interlocked.Read(ref WorkingThreadCount);

      if (running)
        return true;

      lock (AssetQueue)
      {
        return 0 != AssetQueue.Count && 0 != RunningThreadCount;
      }
    }

    public static void ThreadFunc(object obj)
    {
      
    }
    #endregion
  }
}
