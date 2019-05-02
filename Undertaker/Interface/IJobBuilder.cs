using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Undertaker
{
    public interface IJobBuilder
    {
        IJob Run<T>(Expression<Action<T>> job);
        IJob Run<T>(Expression<Func<T, Task>> job);
        IJob Run(Expression<Action> job);
        IJob Run(Expression<Func<Task>> job);
    }
}
