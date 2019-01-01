using System;
using System.Collections.Generic;

namespace Ultimud
{
    public interface IDeathSettings
    {
        String Message { get; set; }
        Telnet.Color Color { get; set; }
        Globals.Range NpcBroadcastRange { get; set; }
        Globals.Range PcBroadcastRange { get; set; }
        List<IStatMod> VictimStatModifications { get; set; }
        List<IStatMod> KillerStatModifications { get; set; }
        Location VictimTransferLocation { get; set; }
        int LostItemPercentage { get; set; }
        int LostMoneyPercentage { get; set; }
        bool RestoreVictim { get; set; }
        string OnKilled { get; set; }

        void Apply(ICharacter victim, ICharacter killer);
    }
}
