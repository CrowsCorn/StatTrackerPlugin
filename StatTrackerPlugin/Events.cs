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
using System.ComponentModel;
using PlayerStatsSystem;
using System.Runtime.Remoting.Messaging;
using InventorySystem.Items;
using Newtonsoft.Json;
using System.Net.Http;
using static UnityEngine.GraphicsBuffer;

namespace StatTrackerPlugin
{
    public class Events
    {
        public static Dictionary<string, TrackedStats> StatTracking = new Dictionary<string, TrackedStats>();
      

        [PluginEvent(ServerEventType.PlayerDamage)]
        public void DamageCount(PlayerDamageEvent args) //damage dealt and taken
        {
            var plr = args.Player;
            var target = args.Target;
            if (target == null || plr == null || !Round.IsRoundStarted) return;

            if (!(args.DamageHandler is AttackerDamageHandler attackerDamageHandler) ||
                (attackerDamageHandler.IsFriendlyFire && plr.Role != RoleTypeId.ClassD)) return;

            if (!StatTracking.ContainsKey(plr.UserId))
                StatTracking.Add(plr.UserId, new TrackedStats(plr));

            if (!StatTracking.ContainsKey(target.UserId))
                StatTracking.Add(target.UserId, new TrackedStats(target));

            StatTracking[plr.UserId].DamageDealt += (int)attackerDamageHandler.Damage;
            StatTracking[target.UserId].DamageTaken += (int)attackerDamageHandler.Damage;
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void PlayerDeaths(PlayerDeathEvent args)
        {
            var plr = args.Player;

            if (plr == null || !(Round.IsRoundStarted)) return;

            if (plr.Role == RoleTypeId.Tutorial) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].Deaths += 1;

            else
            {
                var Stats = new TrackedStats(plr);
                Stats.Deaths = 1;
                StatTracking.Add(plr.UserId, Stats);
            }
        }


       
        [PluginEvent(ServerEventType.PlayerEscape)]
        public void Escapes(PlayerEscapeEvent args)
        {
            var plr = args.Player;

            if (plr == null || !(Round.IsRoundStarted)) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].Escaped = true;

            else
            {
                StatTracking.Add(plr.UserId, new TrackedStats(plr) { Escaped = true });
            }
        }

        
        [PluginEvent(ServerEventType.PlayerDeath)]
        public void asSCPKillCount(PlayerDeathEvent args) // kills as SCP
        {
            var plr = args.Player;
            var Killer = args.Attacker;

            if (plr == null || Killer == null || !Round.IsRoundStarted) return;
            if (!Killer.IsSCP) return;
            if (StatTracking.ContainsKey(Killer.UserId))
                StatTracking[Killer.UserId].SCPKills += 1;

            else
            {
                var Stats = new TrackedStats(Killer);
                Stats.SCPKills = 1;
                StatTracking.Add(Killer.UserId, Stats);
            }
        }

        [PluginEvent(ServerEventType.PlayerDying)]
        public void KilledCount(PlayerDyingEvent args)// SCPs/Humans killed
        {
            var plr = args.Player;
            var Killer = args.Attacker;
            if (plr == null || Killer == null || !Round.IsRoundStarted) return;
            if (!StatTracking.ContainsKey(plr.UserId))
                StatTracking.Add(plr.UserId, new TrackedStats(plr));

            if (!StatTracking.ContainsKey(Killer.UserId))
                StatTracking.Add(Killer.UserId, new TrackedStats(Killer));

            if (plr.IsSCP) // SCPs killed
                StatTracking[Killer.UserId].SCPsKilled += 1;
            if (plr.IsHuman) //humans killed
                StatTracking[Killer.UserId].HumansKilled += 1;           
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void KillsAsCount(PlayerDeathEvent args)//kills as human/SCP
        {
            var plr = args.Player;
            var Killer = args.Attacker;
            if (plr == null || Killer == null || !Round.IsRoundStarted || !(args.DamageHandler is AttackerDamageHandler aDH)) return;

            if (!StatTracking.ContainsKey(Killer.UserId))
                StatTracking.Add(Killer.UserId, new TrackedStats(Killer));

            if (aDH.Attacker.Role.IsHuman())//As Human
                StatTracking[Killer.UserId].HumanKills += 1;
            if (Killer.IsSCP)//As SCP
                StatTracking[Killer.UserId].SCPKills += 1;
        }

       
        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public void PlayerDisarm(PlayerHandcuffEvent args)
        {
            var plr = args.Player;
            var Target = args.Target;

            if (plr == null || !(Round.IsRoundStarted)) return;
            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].PlayersDisarmed += 1;

            else
            {
                StatTracking.Add(plr.UserId, new TrackedStats(plr) { PlayersDisarmed = 1 });
            }
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void MedicUsed(PlayerUsedItemEvent args)
        {
            var plr = args.Player;

            if (plr == null || !(Round.IsRoundStarted)) return;
            if (plr.CurrentItem.Category == ItemCategory.Medical)
            {
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].MedicalItems += 1;

                else
                {
                    StatTracking.Add(plr.UserId, new TrackedStats(plr) { MedicalItems = 1 });
                }
            }
        } 

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void Playtime(PlayerJoinedEvent args)
        {
            var plr = args.Player;

            if (Round.IsRoundStarted)
            {
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].Jointime = DateTime.Now;

                else
                {
                    var Stats = new TrackedStats(plr);
                    StatTracking.Add(plr.UserId, Stats);
                }
            }
            
                     



        }
        [PluginEvent(ServerEventType.PlayerSpawn)]
        public void SCPRounds(PlayerSpawnEvent args)
        {
            var plr = args.Player;
            if(plr.IsSCP)
            {
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].SCP = (int)plr.Role;

                else
                {
                    var Stats = new TrackedStats(plr);
                    Stats.SCP = (int)plr.Role;
                    StatTracking.Add(plr.UserId, Stats);
                }
            }
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void PlaytimeAfterStart(RoundStartEvent args)
        {
            
            foreach (Player plr in Player.GetPlayers())
            {
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].Jointime = DateTime.Now;

                else
                {
                    var Stats = new TrackedStats(plr);
                    StatTracking.Add(plr.UserId, Stats);
                }
            }


        }
        [PluginEvent(ServerEventType.PlayerLeft)]
        public void PlaytimeLeave(PlayerLeftEvent args)
        {
            var plr = args.Player;
            if (StatTracking.ContainsKey(plr.UserId))
            {
                int secondsplayed = (int)(DateTime.Now - StatTracking[plr.UserId].Jointime).TotalSeconds;
                StatTracking[plr.UserId].SecondsPlayed += secondsplayed;
            }
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        public void SendData(RoundEndEvent args)
        {
            foreach (Player plr in Player.GetPlayers())
            {
                if (StatTracking.ContainsKey(plr.UserId))
                {
                    int secondsplayed = (int)(DateTime.Now - StatTracking[plr.UserId].Jointime).TotalSeconds;
                    StatTracking[plr.UserId].SecondsPlayed += secondsplayed;


                    switch (args.LeadingTeam)
                    {
                        case RoundSummary.LeadingTeam.Anomalies:
                            StatTracking[plr.UserId].RoundWon = plr.Team == Team.SCPs;
                            break;
                        case RoundSummary.LeadingTeam.FacilityForces:
                            StatTracking[plr.UserId].RoundWon = plr.Team == Team.FoundationForces;
                            break;
                        case RoundSummary.LeadingTeam.ChaosInsurgency:
                            StatTracking[plr.UserId].RoundWon = plr.Team == Team.ChaosInsurgency;
                            break;
                    }
                }
            }
			List<TrackedStats> stats = new List<TrackedStats>();

			foreach(var a in StatTracking)
			{
				Log.Info($"Adding {a.Key} | {a.Value.UserID} | {a.Value.DNT}");

				if (!a.Value.DNT)
				{
					stats.Add(a.Value);
				}
			}

			Log.Info($"{stats.Count} stats tracked");

            var jsonstring = JsonConvert.SerializeObject(stats.ToArray(), Formatting.Indented);
            Post("https://testapi.dragonscp.co.uk/scpsl/stattracker", new StringContent(jsonstring, Encoding.UTF8, "application/json"));
        }
        public async static Task<HttpResponseMessage> Post(string Url, StringContent Content)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);

                return await client.PostAsync(client.BaseAddress, Content);
            }
        }
        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void DataReset(WaitingForPlayersEvent args)
        {
            StatTracking.Clear();
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
            public int SCP = 0;//This is rounds where you spawned as any SCP
            public DateTime Jointime;
        }
      } 
}    
