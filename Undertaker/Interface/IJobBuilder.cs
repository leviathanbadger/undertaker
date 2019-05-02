using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Undertaker
{
    public interface IJobBuilder
    {
        [MustUseReturnValue]
        IJobBuilder After(IJob job);
        [MustUseReturnValue]
        IJobBuilder After(DateTime dateTime, DateTimeKind kind = DateTimeKind.Local);

        IJob Run<T>(Expression<Action<T>> job);
        IJob Run<T>(Expression<Func<T, Task>> job);
        IJob Run(Expression<Action> job);
        IJob Run(Expression<Func<Task>> job);
    }
}
