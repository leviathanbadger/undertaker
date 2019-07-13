using System;
using System.Threading;
using System.Threading.Tasks;

namespace Undertaker.Agent
{
    internal class Worker
    {
        const int MillisBetweenStopCheck = 200;

        private readonly object _syncLock = new object();

        private bool _isRunning;
        private Thread _workerThread;

        private readonly IJobStorage _storage;
        private readonly IActivator _activator;

        public Worker(
            IJobStorage storage,
            IActivator activator)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            _storage = storage;
            _activator = activator;
        }

        public void Start()
        {
            lock (_syncLock)
            {
                if (_isRunning) throw new InvalidOperationException("This worker is already running!");
                if (_workerThread != null) throw new InvalidOperationException("This worker has already been started and stopped once. Please create a new worker.");
                _isRunning = true;
                _workerThread = new Thread(() => WorkerLoop().Wait());
                _workerThread.Start();
            }
        }
        public void Stop(bool waitForWorkers)
        {
            Thread workerThread;
            lock (_syncLock)
            {
                _isRunning = false;
                workerThread = _workerThread;
            }

            if (waitForWorkers) workerThread?.Join();
        }

        private async Task WorkerLoop()
        {
            while (true)
            {
                if (ShouldStopWorkerLoop()) return;

                var nextJob = await PollForNextJobAsync();
                if (nextJob != null)
                {
                    await ProcessJobAsync(nextJob);
                }
                else
                {
                    for (int q = 0; q < 5000 / MillisBetweenStopCheck; q++)
                    {
                        await Task.Delay(MillisBetweenStopCheck);
                        if (ShouldStopWorkerLoop()) return;
                    }
                }
            }
        }

        private bool ShouldStopWorkerLoop()
        {
            lock (_syncLock)
            {
                return !_isRunning;
            }
        }

        private async Task<IJob> PollForNextJobAsync()
        {
            return await _storage.PollForNextJobAsync();
        }

        private Task ProcessJobAsync(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}
