using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Undertaker
{
    public interface IJobBuilder
    {
        [MustUseReturnValue]
        IJobBuilder WithName([NotNull] string name);
        [MustUseReturnValue]
        IJobBuilder WithDescription([NotNull] string name);

        [MustUseReturnValue]
        IJobBuilder After(DateTime dateTime);
        [MustUseReturnValue]
        IJobBuilder After(IJob job);

        Task<IJob> EnqueueAsync<T>(Expression<Action<T>> job);
        Task<IJob> EnqueueAsync<T>(Expression<Func<T, Task>> job);
        Task<IJob> EnqueueAsync(Expression<Action> job);
        Task<IJob> EnqueueAsync(Expression<Func<Task>> job);
    }
}
