using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anarchy;
using Anarchy.Commands.Chat;
using Anarchy.Configuration;
using UnityEngine;

namespace AoTTG.Anarchy.Commands.Chat
{
    internal class FogCommand : ChatCommand
    {
        public FogCommand() : base("fog", true, true, false)
        {
        }

        public override bool Execute(string[] args)
        {
            switch (args[0])
            {
                case "start":
                    RenderSettings.fogStartDistance = float.Parse(args[1]);
                    break;
                case "end":
                    RenderSettings.fogEndDistance = float.Parse(args[1]);
                    break;
                case "density":
                    RenderSettings.fogDensity = float.Parse(args[1]);
                    break;
                case "color":
                    RenderSettings.fogColor = args[1].HexToColor();
                    break;
                case "enabled":
                    bool result = Convert.ToBoolean(int.Parse(args[1]));
                    VideoSettings.MCFogOverride = result;
                    RenderSettings.fog = result;
                    break;
                case "mode":
                    switch (args[1])
                    {
                        case "ExponentialSquared":
                            RenderSettings.fogMode = FogMode.ExponentialSquared;
                            break;
                        case "Exponential":
                            RenderSettings.fogMode = FogMode.Exponential;
                            break;
                        case "Linear":
                            RenderSettings.fogMode = FogMode.Linear;
                            break;
                    }
                    break;
                default:
                    global::Anarchy.UI.Log.AddLineRaw("Command failed: argument 1 invalid.");
                    return false;
            }
            FengGameManagerMKII.FGM.BasePV.RPC("FogRPC", PhotonTargets.OthersBuffered, args[0], args[1]);
            return true;
        }
    }
}