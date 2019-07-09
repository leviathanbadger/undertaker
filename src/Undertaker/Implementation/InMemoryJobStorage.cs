using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Undertaker
{
    public class InMemoryJobStorage : IJobStorage
    {
        private readonly object _syncLock = new object();
        private bool _isDisposed;

        private readonly List<InMemoryJob> _nextJobs;
        private readonly List<InMemoryJob> _processingJobs;
        private readonly List<InMemoryJob> _blockedJobs;
        private readonly List<InMemoryJob> _completedJobs;
        private readonly List<InMemoryJob> _erroredJobs;

        private readonly TimeSpan _timeSpanBeforeReclaimingJob;

        public InMemoryJobStorage(TimeSpan? timeSpanBeforeReclaimingJob = null)
        {
            _nextJobs = new List<InMemoryJob>();
            _blockedJobs = new List<InMemoryJob>();
            _processingJobs = new List<InMemoryJob>();
            _completedJobs = new List<InMemoryJob>();
            _erroredJobs = new List<InMemoryJob>();

            _timeSpanBeforeReclaimingJob = timeSpanBeforeReclaimingJob ?? TimeSpan.FromMinutes(5);
        }

        public Task<IJob> CreateJobAsync(JobDefinition jobDefinition)
        {
            if (jobDefinition == null) throw new ArgumentNullException(nameof(jobDefinition));

            lock (_syncLock)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(InMemoryJobStorage));

                foreach (var runAfterJob in jobDefinition.RunAfterJobs)
                {
                    if (!(runAfterJob is InMemoryJob imJob) || imJob.Storage != this)
                    {
                        throw new KeyNotFoundException($"Job {runAfterJob} was created using a different job scheduler or storage.");
                    }
                }

                var job = CreateJobModel(jobDefinition);

                if (jobDefinition.RunAfterJobs.Count > 0)
                {
                    foreach (var runAfterJob in jobDefinition.RunAfterJobs.Cast<InMemoryJob>())
                    {
                        runAfterJob.AddBlockingJob(job);
                    }
                }

                UpdateJobStatusImpl(job, JobStatus.Scheduled);

                return Task.FromResult<IJob>(job);
            }
        }
        private InMemoryJob CreateJobModel(JobDefinition jobDefinition)
        {
            return new InMemoryJob(this, jobDefinition);
        }

        public Task<IJob> PollForNextJobAsync()
        {
            lock (_syncLock)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(InMemoryJobStorage));

                var job = _nextJobs.FirstOrDefault();
                if (job == null || job.RunAtTime > DateTime.UtcNow) return null;

                _nextJobs.RemoveAt(0);
                job.RunAtTime = DateTime.UtcNow.Add(_timeSpanBeforeReclaimingJob);
                InsertJob(_processingJobs, job);
                return Task.FromResult<IJob>(job);
            }
        }

        public Task UpdateJobStatusAsync(IJob job, JobStatus status)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(InMemoryJobStorage));

            if (!(job is InMemoryJob imJob) || imJob.Storage != this)
            {
                throw new KeyNotFoundException($"Job {job} was created using a different job scheduler or storage.");
            }

            lock (_syncLock)
            {
                UpdateJobStatusImpl(imJob, status);
            }

            return Task.CompletedTask;
        }
        /// <summary>
        /// Assumptions:
        /// - <see cref="_syncLock"/> is already locked when entering this method.
        /// </summary>
        /// <param name="job">The job for which to update the status.</param>
        /// <param name="status">The new job status.</param>
        private void UpdateJobStatusImpl(InMemoryJob job, JobStatus status)
        {
            if (job.Status == status) return;
            if (status == JobStatus.Creating) throw new NotSupportedException("Can't set the job status back to Creating.");

            switch (job.Status)
            {
            case JobStatus.Creating:
                break;

            case JobStatus.Scheduled:
                if (job.IsBlocked) throw new NotSupportedException("In-memory job storage does not support changing the status of a blocked job.");
                (job.IsBlocked ? _blockedJobs : _nextJobs).Remove(job);
                break;

            case JobStatus.Processing:
                _processingJobs.Remove(job);
                break;

            case JobStatus.Completed:
                _completedJobs.Remove(job);
                break;

            case JobStatus.Error:
                _erroredJobs.Remove(job);
                break;

            default:
                throw new InvalidOperationException($"Unknown job status: {job.Status}");
            }

            switch (status)
            {
            case JobStatus.Scheduled:
                InsertJob(job.IsBlocked ? _blockedJobs : _nextJobs, job);
                break;

            case JobStatus.Processing:
                InsertJob(_processingJobs, job);
                break;

            case JobStatus.Completed:
                InsertJob(_completedJobs, job);
                var blockedJobs = job.ClearBlockingJobs();
                foreach (var blockedJob in blockedJobs)
                {
                    if (!blockedJob.IsBlocked)
                    {
                        _blockedJobs.Remove(blockedJob);
                        InsertJob(_nextJobs, blockedJob);
                    }
                }
                break;

            case JobStatus.Error:
                InsertJob(_erroredJobs, job);
                break;

            default:
                throw new InvalidOperationException($"Unknown job status: {status}");
            }

            job.Status = status;
        }

        /// <summary>
        /// Assumptions:
        /// - <see cref="_syncLock"/> is already locked when entering this method.
        /// - <see cref="job"/> is not in any queue.
        /// </summary>
        /// <param name="jobQueue">The queue to add the job to.</param>
        /// <param name="job">The job to add to the queue</param>
        private void InsertJob(List<InMemoryJob> jobQueue, InMemoryJob job)
        {
            var runAtTime = job.RunAtTime;

            int lowerBound = 0;
            int upperBound = jobQueue.Count;

            while (lowerBound != upperBound)
            {
                var mid = lowerBound + (upperBound - lowerBound) / 2;
                if (runAtTime > jobQueue[mid].RunAtTime) upperBound = mid;
                else lowerBound = mid;
            }

            jobQueue.Insert(lowerBound, job);
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                if (_isDisposed) return;
                _isDisposed = true;

                _nextJobs.Clear();
                _processingJobs.Clear();
                _blockedJobs.Clear();
                _completedJobs.Clear();
                _erroredJobs.Clear();
            }
        }

        public class InMemoryJob : IJob
        {
            private readonly InMemoryJobStorage _storage;
            private readonly JobDefinition _jobDefinition;

            private readonly List<InMemoryJob> _blockingJobs;
            private readonly List<InMemoryJob> _blockedByJobs;
            private DateTime _runAtTime;
            private JobStatus _status;

            public InMemoryJob(InMemoryJobStorage storage, JobDefinition jobDefinition)
            {
                _storage = storage;
                _jobDefinition = jobDefinition;

                _blockingJobs = new List<InMemoryJob>();
                _blockedByJobs = new List<InMemoryJob>();
                _status = JobStatus.Creating;

                _runAtTime = _jobDefinition.RunAtTime ?? DateTime.UtcNow;
            }

            public IJobStorage Storage
            {
                get
                {
                    return _storage;
                }
            }

            public void AddBlockingJob(InMemoryJob blockingJob)
            {
                if (_status != JobStatus.Completed)
                {
                    _blockingJobs.Add(blockingJob);
                    blockingJob._blockedByJobs.Add(this);
                }
            }
            public List<InMemoryJob> ClearBlockingJobs()
            {
                var blockingJobs = _blockingJobs.AsEnumerable().ToList();
                _blockingJobs.Clear();
                foreach (var blockingJob in blockingJobs)
                {
                    blockingJob._blockedByJobs.Remove(this);
                }
                return blockingJobs;
            }

            public bool IsBlocked
            {
                get
                {
                    return _blockedByJobs.Count > 0;
                }
            }

            public JobStatus Status
            {
                get
                {
                    return _status;
                }
                set
                {
                    _status = value;
                }
            }

            public DateTime RunAtTime
            {
                get
                {
                    return _runAtTime;
                }
                set
                {
                    _runAtTime = value;
                }
            }

            public string Name
            {
                get
                {
                    return _jobDefinition.Name;
                }
            }
            public string Description
            {
                get
                {
                    return _jobDefinition.Description;
                }
            }

            public override string ToString()
            {
                return $"{Name} ({base.ToString()})";
            }
        }
    }
}
