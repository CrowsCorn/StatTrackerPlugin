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

        public void DamageCount(PlayerDamageEvent args) //damage dealt
        {

            var plr = args.Player;
            var target = args.Target;
            if (target == null || plr == null || !(Round.IsRoundStarted)) return;

            if (!(args.DamageHandler is AttackerDamageHandler attackerDamageHandler) ||
                attackerDamageHandler.IsFriendlyFire || plr.Role != RoleTypeId.ClassD) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].DamageDealt += (int)attackerDamageHandler.Damage;

            else
            {
                var Stats = new TrackedStats(plr);
                Stats.DamageDealt = (int)attackerDamageHandler.Damage;
                StatTracking.Add(plr.UserId, Stats);
            }
        }
        [PluginEvent(ServerEventType.PlayerDamage)]
        public void DamageTakenCount(PlayerDamageEvent args) //damage taken
        {

            var plr = args.Target;
            var target = args.Player;
            if (target == null || plr == null || !(Round.IsRoundStarted)) return;

            if (!(args.DamageHandler is AttackerDamageHandler attackerDamageHandler) ||
                attackerDamageHandler.IsFriendlyFire || plr.Role != RoleTypeId.ClassD) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].DamageTaken += (int)attackerDamageHandler.Damage;

            else
            {
                var Stats = new TrackedStats(plr);
                Stats.DamageTaken = (int)attackerDamageHandler.Damage;
                StatTracking.Add(plr.UserId, Stats);
            }
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
                var Stats = new TrackedStats(plr);
                Stats.Escaped = true;
                StatTracking.Add(plr.UserId, Stats);
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
                StatTracking[Killer.UserId].Deaths += 1;

            else
            {
                var Stats = new TrackedStats(Killer);
                Stats.Deaths = 1;
                StatTracking.Add(Killer.UserId, Stats);
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void SCPsKilledCount(PlayerDeathEvent args)// SCPs killed
        {
            var plr = args.Player;
            var Killer = args.Attacker;
            if (plr == null || Killer == null || !Round.IsRoundStarted) return;
            if (!plr.IsSCP) return;
            if (StatTracking.ContainsKey(Killer.UserId))
                StatTracking[Killer.UserId].SCPsKilled += 1;

            else
            {
                var Stats = new TrackedStats(Killer);
                Stats.SCPsKilled = 1;
                StatTracking.Add(Killer.UserId, Stats);
            }
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void HumansKilledCount(PlayerDeathEvent args)//humans killed
        {
            var plr = args.Player;
            var Killer = args.Attacker;
            if (plr == null || Killer == null || !Round.IsRoundStarted) return;
            if (!plr.IsHuman) return;
            if (StatTracking.ContainsKey(Killer.UserId))
                StatTracking[Killer.UserId].HumansKilled += 1;

            else
            {
                var Stats = new TrackedStats(plr);
                Stats.HumansKilled = 1;
                StatTracking.Add(Killer.UserId, Stats);
            }
        }

    
        [PluginEvent(ServerEventType.PlayerDeath)]
        public void HumanKillCount(PlayerDeathEvent args)//kills as human
        {
            var plr = args.Player;
            var Killer = args.Attacker;
            if (plr == null || Killer == null || !Round.IsRoundStarted) return;
            if (!Killer.IsHuman) return;
            if (StatTracking.ContainsKey(Killer.UserId))
                StatTracking[Killer.UserId].HumanKills += 1;

            else
            {
                var Stats = new TrackedStats(Killer);
                Stats.HumanKills = 1;
                StatTracking.Add(Killer.UserId, Stats);
            }
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
                var Stats = new TrackedStats(plr);
                Stats.PlayersDisarmed = 1;
                StatTracking.Add(plr.UserId, Stats);
            }
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void MedicUsed(PlayerUsedItemEvent args)
        {
            var plr = args.Player;

            if (plr == null || !(Round.IsRoundStarted)) return;
            if (plr.CurrentItem.ItemTypeId == ItemType.Medkit)
            {
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].MedicalItems += 1;

                else
                {
                    var Stats = new TrackedStats(plr);
                    Stats.MedicalItems = 1;
                    StatTracking.Add(plr.UserId, Stats);
                }
            }
        }

        
        [PluginEvent(ServerEventType.RoundEnd)]
        public void RoundsWon(RoundEndEvent args)
        {
            foreach (Player plr in Player.GetPlayers())
            {
                if (plr == null || !(Round.IsRoundStarted)) return;
                if (plr.Team == Team.SCPs && args.LeadingTeam == RoundSummary.LeadingTeam.Anomalies)
                {
                    if (StatTracking.ContainsKey(plr.UserId))
                        StatTracking[plr.UserId].RoundWon = true;

                    else
                    {
                        var Stats = new TrackedStats(plr);
                        Stats.RoundWon = true;
                        StatTracking.Add(plr.UserId, Stats);
                    }
                }

                if (plr.Team == Team.FoundationForces && args.LeadingTeam == RoundSummary.LeadingTeam.FacilityForces)
                {
                    if (StatTracking.ContainsKey(plr.UserId))
                        StatTracking[plr.UserId].RoundWon = true;

                    else
                    {
                        var Stats = new TrackedStats(plr);
                        Stats.RoundWon = true;
                        StatTracking.Add(plr.UserId, Stats);
                    }
                }

                if (plr.Team == Team.ChaosInsurgency && args.LeadingTeam == RoundSummary.LeadingTeam.ChaosInsurgency)
                {
                    if (StatTracking.ContainsKey(plr.UserId))
                        StatTracking[plr.UserId].RoundWon = true;

                    else
                    {
                        var Stats = new TrackedStats(plr);
                        Stats.RoundWon = true;
                        StatTracking.Add(plr.UserId, Stats);
                    }
                }
                if (args.LeadingTeam == RoundSummary.LeadingTeam.Draw) return;


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
                    StatTracking[plr.UserId].SCP += 1;

                else
                {
                    var Stats = new TrackedStats(plr);
                    Stats.SCP = 1;
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

            var jsonstring = JsonConvert.SerializeObject(stats.ToArray());
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
            public int SCP = 0;
            public DateTime Jointime;
        }
      } 
}    
