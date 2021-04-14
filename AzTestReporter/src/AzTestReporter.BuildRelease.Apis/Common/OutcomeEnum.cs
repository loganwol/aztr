using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzTestReporter.BuildRelease.Apis.Common
{
    public enum OutcomeEnum
    {
        Passed,
        Failed,
        Unspecified, 
        None, 
        Inconclusive, 
        Timeout, 
        Aborted, 
        Blocked, 
        NotExecuted, 
        Warning, 
        Error, 
        NotApplicable, 
        Paused, 
        InProgress, 
        NotImpacted,
        Ignore
    }
}
