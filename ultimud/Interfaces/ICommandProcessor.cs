using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface ICommandProcessor
    {
        bool ProcessCommand(IUser user);
        ICommandProcessor Get(string name);
    }
}
