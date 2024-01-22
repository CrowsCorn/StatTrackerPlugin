using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem.Commands.Shared;
using PluginAPI;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using PluginAPI.Enums;
using CustomPlayerEffects;
using PlayerRoles;
using UnityEngine;
using System.Diagnostics.Eventing.Reader;
using InventorySystem;
using PluginAPI.Core.Items;
using Utils;
using MEC;
using PlayerRoles.FirstPersonControl.Spawnpoints;
using PlayerRoles.FirstPersonControl;
using MapGeneration;

namespace StatTrackerPlugin
{
    public class Class1
    {
        [PluginEntryPoint("Stat Tracker Plugin", "1.0.0", "Tracks the stats of players on Dragon inn", "Crowscorn")]

        public void OnPluginStart()
        {
            Log.Info("Spinning the egg");
            EventManager.RegisterEvents<Events>(this);
        }
    }

    
}
