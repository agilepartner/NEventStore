using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading.Tasks
{
    [Flags]
    public enum TaskCreationOptions
    {
        AttachedToParent = 4,
        LongRunning = 2,
        None = 0,
        PreferFairness = 1
    }
}
