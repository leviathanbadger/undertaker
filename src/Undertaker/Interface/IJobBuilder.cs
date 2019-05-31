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

        IJob Run<T>(Expression<Action<T>> job);
        IJob Run<T>(Expression<Func<T, Task>> job);
        IJob Run(Expression<Action> job);
        IJob Run(Expression<Func<Task>> job);
    }
}
