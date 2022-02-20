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
    private static Queue<(string, (string, int, string, int))> AssetQueue = new();
    private static int MaximumThreads = 100;
    public static List<Thread> AssetThreadPoolWorkers = new();
    private static Semaphore semaphore = new Semaphore(1,1);

    private static Thread PollThread { get; set; }
    #endregion
    #region Methods

    public static bool Run((string, (string,int,string,int)) asset)
    {
      try
      {
        semaphore.WaitOne();

        lock (AssetThreadPoolWorkers)
        {
          if (AssetThreadPoolWorkers.Count >= MaximumThreads)
          {
            if (PollThread is null)
            {
              PollThread = new Thread(PollQueue) { IsBackground = true };
              PollThread.Start();
            }

            Log.Msg("Thread pool is full, queuing asset.", MessageLog.WARNING);
            AssetQueue.Enqueue(asset);
            return true;
          }

          Thread assetThread = new Thread(() => BuySystem.BeginAnalysis(asset)) { IsBackground = true, Name = asset.Item1 };
          assetThread.Start();

          AssetThreadPoolWorkers.Add(assetThread);
          Interlocked.Increment(ref RunningThreadCount);
        }

        return true;
      }
      catch(Exception e)
      {
        Log.Msg(e.Message, MessageLog.ERROR);
        return false;
      }
      finally
      {
        semaphore.Release();
      }
    }

    private static void PollQueue()
    {
      // keep polling the queue till there's an occupiable space within the assetthreadpool
      while (true)
      {
        if (AssetThreadPoolWorkers.Count >= MaximumThreads) { Thread.Sleep(1); continue; }

        if(AssetQueue.Count > 0)
          // Run the dequeued asset.
          Run(AssetQueue.Dequeue());
      }
    }
    #endregion
  }
}
