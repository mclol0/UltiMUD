using System;
using System.Collections.Generic;

namespace Ultimud
{
    public interface IPvpSettings
    {
        bool IsAllowed { get; set; }
        TimeSpan SafeTimer { get; set; }
    }
}
