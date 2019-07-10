using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Undertaker
{
    public class JobBuilder : IJobBuilder
    {
        private readonly IJobScheduler _jobScheduler;

        public JobBuilder(IJobScheduler jobScheduler)
        {
            _jobScheduler = jobScheduler;

            _runAfter = new List<IJob>();
        }

        private string _name;
        private string _desc;
        private DateTime? _runAt;
        private readonly List<IJob> _runAfter;

        public IJobBuilder WithName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_name != null) throw new InvalidOperationException("This job has already been given a name.");
            _name = name;
            return this;
        }
        public IJobBuilder WithDescription(string desc)
        {
            if (desc == null) throw new ArgumentNullException(nameof(desc));
            if (_desc != null) throw new InvalidOperationException("This job has already been given a description.");
            _desc = desc;
            return this;
        }

        public IJobBuilder After(DateTime dateTime)
        {
            if (_runAt != null) throw new InvalidOperationException("A run-after time has already been specified. You can't schedule a different time.");
            _runAt = dateTime;
            return this;
        }
        public IJobBuilder After(IJob job)
        {
            //TODO: validate that this job is a valid prerequisite for the job currently being built
            _runAfter.Add(job);
            return this;
        }

        public Task<IJob> EnqueueAsync<T>(Expression<Action<T>> job)
        {
            return EnqueueAsync((LambdaExpression)job);
        }
        public Task<IJob> EnqueueAsync<T>(Expression<Func<T, Task>> job)
        {
            return EnqueueAsync((LambdaExpression)job);
        }
        public Task<IJob> EnqueueAsync(Expression<Action> job)
        {
            return EnqueueAsync((LambdaExpression)job);
        }
        public Task<IJob> EnqueueAsync(Expression<Func<Task>> job)
        {
            return EnqueueAsync((LambdaExpression)job);
        }

        private async Task<IJob> EnqueueAsync(LambdaExpression job)
        {
            ValidateExpression(job, out var fullyQualifiedTypeName, out var methodName, out var isStatic, out var parameters);

            var jobDefinition = new JobDefinition(
                name: _name ?? methodName,
                desc: _desc,
                fullyQualifiedTypeName: fullyQualifiedTypeName,
                methodName: methodName,
                isStatic: isStatic,
                parameters: parameters,
                runAt: _runAt,
                runAfter: _runAfter.ToArray());

            return await _jobScheduler.ScheduleJobAsync(jobDefinition);
        }

        private void ValidateExpression(LambdaExpression job, out string fullyQualifiedTypeName, out string methodName, out bool isStatic, out ParameterDefinition[] parameters)
        {
            if (!(job.Body is MethodCallExpression expr)) throw new ArgumentException("The job expression must be a method call expression.");

            var param = job.Parameters.SingleOrDefault();
            if (param == null)
            {
                if (expr.Object != null) throw new ArgumentException($"No activation class selected, so the method call must be static. (Found instance method on {expr.Object})");
                isStatic = true;
            }
            else
            {
                if (expr.Object != param) throw new ArgumentException("An activation class was selected, but was not used as the left-hand side of the method call expression.");
                isStatic = false;
            }

            var declaringType = expr.Method.DeclaringType;
            Debug.Assert(declaringType != null, $"{nameof(declaringType)} != null");

            fullyQualifiedTypeName = declaringType.AssemblyQualifiedName;
            methodName = expr.Method.Name;

            var paramsList = new List<ParameterDefinition>();

            foreach (var arg in expr.Arguments)
            {
                throw new NotImplementedException("Job parameters aren't supported yet.");
            }

            parameters = paramsList.ToArray();
        }
    }
}
