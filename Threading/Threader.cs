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
            Action thread = threadInit();

            Run(name, () =>
            {
                while (isRunning())
                {
                    thread();
                }
            }, threadStarted, threadEnded, onError);
        }

        public static void Interval(string name, Func<bool> isRunning, TimeSpan interval, Func<Action> threadInit, Action? threadStarted = null, Action? threadEnded = null, Action<Exception>? onError = null, Func<DateTime> getUtc = null)
        {
            Action thread = threadInit();
            getUtc = getUtc ?? (() => DateTime.UtcNow);
            DateTime expire = getUtc().Add(interval);
            Run(name, () =>
            {
                while (isRunning())
                {
                    DateTime now = getUtc();
                    if (expire <= now)
                    {
                        thread();
                        expire = now.Add(interval);
                    }
                    Thread.Sleep(250);
                }
            }, threadStarted, threadEnded, onError);
        }

    }
}