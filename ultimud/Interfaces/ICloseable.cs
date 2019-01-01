using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface ICloseable
    {
        bool Closeable { get; set; }
        bool Closed { get; set; }
        bool Locked { get; set; }

        IItem Key { get; set; }
        Guid KeyId { get; set; }
    }
}
