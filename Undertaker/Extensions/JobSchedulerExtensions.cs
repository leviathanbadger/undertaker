using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Undertaker.Extensions
{
    public static class JobSchedulerExtensions
    {
        [MustUseReturnValue]
        public static IJobBuilder After(this IJobScheduler scheduler, IJob job)
        {
            return scheduler.BuildJob().After(job);
        }
        [MustUseReturnValue]
        public static IJobBuilder After(this IJobScheduler scheduler, DateTime dateTime, DateTimeKind kind = DateTimeKind.Local)
        {
            return scheduler.BuildJob().After(dateTime, kind);
        }

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
