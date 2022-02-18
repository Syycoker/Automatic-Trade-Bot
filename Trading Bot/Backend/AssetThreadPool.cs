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
    private static long WorkingThreadCount = 0;
    private static long RunningThreadCount = 0;
    public static long FinishedThreads = 0;
    public static long MaxThreadsUsed = 0;
    public static long TotalQueued = 0;
    private static Queue<WaitCallback> AssetQueue = new Queue<WaitCallback>();
    private static int MaximumThreads = 100;
    public static List<Thread> AssetThreadPoolWorkers = new();
    static Semaphore DequeueSemaphore = new Semaphore(0, 100);
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
            Log.Msg("Asset Thread Starting...", MessageLog.WARNING);

            var param = callBack.Method.GetParameters();

            Thread thread = new Thread(new ParameterizedThreadStart(ThreadFunc)) { IsBackground = true };
            AssetThreadPoolWorkers.Add(thread);
            thread.Start(null);
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
      while (true) //from now on, I'm dequeueing/invoking jobs from the queue.
      {
        try
        {
          DequeueSemaphore.WaitOne();

          Interlocked.Increment(ref WorkingThreadCount);

          WaitCallback wcb = null;

          lock (AssetQueue)
          {
            if (AssetQueue.Count > 0) //help a non-empty queue to get rid of its load
            {
              wcb = AssetQueue.Dequeue();
            }
          }

          if (wcb != null)
          {
            wcb.Invoke(null);
          }
          else
          {
            Debug.WriteLine("Exiting Thread");
            //could not dequeue from the queue, terminate the thread
            Interlocked.Increment(ref FinishedThreads);
            Interlocked.Decrement(ref RunningThreadCount);
            return;
          }
        }
        finally
        {
          DequeueSemaphore.Release();
          Interlocked.Decrement(ref WorkingThreadCount);
        }
      }
    }
    #endregion
  }
}
