﻿using JetBrains.Annotations;

namespace Undertaker
{
    public interface IJobScheduler
    {
        [MustUseReturnValue]
        IJobBuilder BuildJob();
    }
}