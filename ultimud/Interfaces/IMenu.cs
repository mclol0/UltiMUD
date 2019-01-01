using System;
using System.Collections.Generic;
using System.Text;
using Damdai;

namespace Ultimud
{
    public interface IMenu
    {
        string Prompt { get; set; }
        List<IMenuOption> Options { get; set; }
        Function<bool, IUser> InputHandler { get; }

        bool ProcessCommand(IUser user);
    }
}
