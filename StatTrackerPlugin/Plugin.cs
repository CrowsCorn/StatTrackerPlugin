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
    public class Plugin
    {
        [PluginEntryPoint("Stat Tracker Plugin", "1.0.0", "Tracks the stats of players on Dragon inn", "Crowscorn")]

        public void OnPluginStart()
        {
            Log.Info("Spinning the egg");
            EventManager.RegisterEvents<Events>(this);
        }
    }

    public class TrackedStats
    {
        public TrackedStats(Player plr)
        {
            UserID = plr.UserId;
            DNT = plr.DoNotTrack;
            Jointime = DateTime.Now;
        }

        public string UserID;
        public bool DNT = true;
        public int SCPsKilled = 0;
        public int SCPKills = 0;
        public int HumansKilled = 0;
        public int HumanKills = 0;
        public int MedicalItems = 0;
        public bool Escaped = false;
        public bool RoundWon = false;
        public int SecondsPlayed = 0;
        public int PlayersDisarmed = 0;
        public int DamageDealt = 0;
        public int DamageTaken = 0;
        public int Deaths = 0;
        public int SCP = (int)RoleTypeId.None; //SCP 173 is 0 in the enum
        public DateTime Jointime;
    }
}
