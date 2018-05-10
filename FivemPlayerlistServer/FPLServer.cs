using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FivemPlayerlistServer
{
    public class FPLServer : BaseScript
    {
        public FPLServer()
        {
            EventHandlers.Add("fs:getMaxPlayers", new Action<Player>(ReturnMaxPlayers));
        }

        private void ReturnMaxPlayers([FromSource] Player source)
        {
            source.TriggerEvent("fs:setMaxPlayers", int.Parse(GetConvar("sv_maxClients", "30").ToString()));
        }
    }
}
