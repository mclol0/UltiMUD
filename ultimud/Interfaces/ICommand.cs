using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface ICommand : IIdentifiable
    {
        IScript Script { get; set; }
        uint Priority { get; set; }
        bool CanUseIncapacitated { get; set; }
        bool IsSystemCommand { get; set; }
        List<string> Aliases { get; set; }
        //List<Guid> MenuFunctions { get; set; }
    }
}
