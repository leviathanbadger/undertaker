using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Undertaker.Extensions
{
    public static class JobSchedulerExtensions
    {
        public static IJob Run<T>(this IJobScheduler scheduler, Expression<Action<T>> job)
        {
            return scheduler.BuildJob().Run(job);
        }
        public static IJob Run<T>(this IJobScheduler scheduler, Expression<Func<T, Task>> job)
        {
            return scheduler.BuildJob().Run(job);
        }
        public static IJob Run(this IJobScheduler scheduler, Expression<Action> job)
        {
            return scheduler.BuildJob().Run(job);
        }
        public static IJob Run(this IJobScheduler scheduler, Expression<Func<Task>> job)
        {
            return scheduler.BuildJob().Run(job);
        }
    }
}
