using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IAction
    {
        void Start();
        void Go();
        void Update(); 
        bool Stop(bool silent);
        bool Wait();
        bool Finish();
    }
}
