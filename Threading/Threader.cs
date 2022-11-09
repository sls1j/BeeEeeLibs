namespace Threading
{
    public static class Threader
    {
        public static void Run(string name, Action thread, Action? threadStarted = null, Action? threadEnded = null, Action<Exception>? onError = null)
        {
            Thread t = new Thread(o =>
            {
                try
                {
                    if (threadStarted != null)
                        threadStarted();

                    thread();

                    if (threadEnded != null)
                        threadEnded();
                }
                catch (Exception ex)
                {
                    if (onError != null)
                        onError(ex);
                }
            });

            t.Start();            
        }

        public static void Loop(string name, Func<bool> isRunning, Func<Action> threadInit, Action? threadStarted = null, Action? threadEnded = null, Action<Exception>? onError = null)
        {           
            Run(name, () =>
            {
                Action thread = threadInit();

                while (isRunning())
                {
                    try
                    {
                        thread();
                    }
                    catch(Exception ex)
                    {
                        if (onError != null)
                            onError(ex);
                    }
                }
            }, threadStarted, threadEnded, onError);
        }

        public static void Interval(string name, Func<bool> isRunning, TimeSpan interval, Func<Action> threadInit, Action? threadStarted = null, Action? threadEnded = null, Action<Exception>? onError = null, Func<DateTime>? getUtc = null, int loopDelayMili = 250 )
        {            
            getUtc = getUtc ?? (() => DateTime.UtcNow);
            DateTime expire = getUtc().Add(interval);
            Run(name, () =>
            {
                Action thread = threadInit();

                while (isRunning())
                {
                    try
                    {
                        DateTime now = getUtc();
                        if (expire <= now)
                        {
                            thread();
                            expire = now.Add(interval);
                        }
                        
                        if (loopDelayMili > 0)
                            Thread.Sleep(loopDelayMili);
                    }
                    catch(Exception ex)
                    {
                        if (onError != null)
                            onError(ex);

                    }
                }
            }, threadStarted, threadEnded, onError);
        }

    }
}