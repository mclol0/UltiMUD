using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IHelp : IIdentifiable
    {
        string Text { get; set; }
    }
}
