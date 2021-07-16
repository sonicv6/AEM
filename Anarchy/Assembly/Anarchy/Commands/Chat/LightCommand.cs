using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anarchy;
using Anarchy.Commands.Chat;
using Anarchy.Configuration;
using Optimization.Caching;
using UnityEngine;

namespace AoTTG.Anarchy.Commands.Chat
{
    internal class LightCommand : ChatCommand
    {
        public LightCommand() : base("light", true, true, false)
        {
        }

        public override bool Execute(string[] args)
        {
            switch (args[0])
            {
                case "intensity":
                    CacheGameObject.Find("mainLight").GetComponent<Light>().intensity = float.Parse(args[1]);
                    break;
                default:
                    global::Anarchy.UI.Log.AddLineRaw("Command failed: argument 1 invalid.");
                    return false;
            }
            FengGameManagerMKII.FGM.BasePV.RPC("LightRPC", PhotonTargets.OthersBuffered, args[0], args[1]);
            return true;
        }
    }
}