using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface ICommand
    {
        bool ProcessCommand(User user);
        ICommand Get(string name);
    }
}
