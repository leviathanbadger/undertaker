using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertaker.Agent
{
    public class Agent : IDisposable
    {
        private readonly object _syncLock = new object();

        private volatile bool _isDisposed;

        private bool _isRunning;
        private List<Worker> _workers;

        private IJobStorage _storage;
        private bool _ownsStorage;
        private IActivator _activator;
        private int? _concurrentWorkers;

        public Agent UseStorage([NotNull] IJobStorage storage, bool ownsStorage = true)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (_isDisposed) throw new ObjectDisposedException(nameof(Agent));

            lock (_syncLock)
            {
                if (_storage != null) throw new InvalidOperationException("A storage has already been specified for this agent.");
                _storage = storage;
                _ownsStorage = ownsStorage;
            }

            return this;
        }
        public Agent UseActivator([NotNull] IActivator activator)
        {
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            if (_isDisposed) throw new ObjectDisposedException(nameof(Agent));

            lock (_syncLock)
            {
                if (_activator != null) throw new InvalidOperationException("An activator has already been specified for this agent.");
                _activator = activator;
            }

            return this;
        }
        public Agent UseConcurrentWorkers(int count)
        {
            if (count <= 0) throw new ArgumentException("You must specify at least one concurrent worker.");
            if (count > 30) throw new ArgumentException("You cannot have more than thirty concurrent workers.");
            if (_isDisposed) throw new ObjectDisposedException(nameof(Agent));

            lock (_syncLock)
            {
                if (_isRunning) throw new NotSupportedException("The number of concurrent workers cannot be changed while the agent is running.");
                if (_concurrentWorkers != null) throw new InvalidOperationException("The concurrent worker count has already been specified for this agent.");
                _concurrentWorkers = count;
            }

            return this;
        }

        public void Start()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(Agent));
            lock (_syncLock)
            {
                if (_isRunning) throw new InvalidOperationException("This agent is already running");
                StartImpl();
            }
        }
        public void Stop(bool waitForWorkers = false)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(Agent));
            lock (_syncLock)
            {
                StopImpl(waitForWorkers);
            }
        }

        private void StartImpl()
        {
            if (_storage == null) throw new InvalidOperationException("You must specify a storage using UseStorage before you start the Agent.");
            if (_activator == null) throw new InvalidOperationException("You must specify an activator using UseActivator before you start the Agent.");
            if (_workers != null) StopImpl(true); //Make sure all previous workers are completely cleaned up before starting agent again
            _isRunning = true;
            _workers = new List<Worker>();
            var concurrentWorkerCount = _concurrentWorkers ?? 5;
            for (int q = 0; q < concurrentWorkerCount; q++)
            {
                CreateWorker();
            }
        }
        private void CreateWorker()
        {
            var worker = new Worker(_storage, _activator);
            _workers.Add(worker);
            worker.Start();
        }

        private void StopImpl(bool waitForWorkers)
        {
            _isRunning = false;

            //Soft-stop all workers, so that all can begin shutdown process
            foreach (var worker in _workers ?? Enumerable.Empty<Worker>())
            {
                worker.Stop(false);
            }

            if (waitForWorkers)
            {
                //Hard-stop all workers, so processing doesn't continue until they are cleaned up
                foreach (var worker in _workers ?? Enumerable.Empty<Worker>())
                {
                    worker.Stop(true);
                }
                _workers = null; //Proof all workers are cleaned up
            }
        }

        public bool IsRunning
        {
            get
            {
                lock (_syncLock)
                {
                    return _isRunning;
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            lock (_syncLock)
            {
                StopImpl(true);
            }

            if (_ownsStorage) _storage?.Dispose();
        }
    }
}
