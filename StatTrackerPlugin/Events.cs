using Newtonsoft.Json;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

            if (plr == null || !Round.IsRoundStarted) return;

            if (plr.Role == RoleTypeId.Tutorial) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].Deaths += 1;
            else
                StatTracking.Add(plr.UserId, new TrackedStats(plr) { Deaths = 1 });
        }


        [PluginEvent(ServerEventType.PlayerDeath)] //Player death instead of dying for FF blocked events
        public void HumansKilledCount(PlayerDeathEvent args)
        {
            var plr = args.Player;
            var atckr = args.Attacker;
            if (plr == null || atckr == null || !(args.DamageHandler is AttackerDamageHandler aDH) || !Round.IsRoundStarted) return;

            if (!StatTracking.ContainsKey(plr.UserId))
                StatTracking.Add(plr.UserId, new TrackedStats(plr));

            if (!StatTracking.ContainsKey(atckr.UserId))
                StatTracking.Add(atckr.UserId, new TrackedStats(atckr));

            if (plr.IsSCP) // SCPs killed
                StatTracking[atckr.UserId].SCPsKilled += 1;

            if (atckr.IsSCP) // kills as SCP
                StatTracking[atckr.UserId].SCPKills += 1;

            if (plr.IsHuman) //humans killed
                StatTracking[atckr.UserId].HumansKilled += 1;

            //Uses the damage handler for edge cases when attacker is dead when kill happens, but caused it as a human (scp 018 for example)
            if (aDH.Attacker.Role.IsHuman()) //kills as human
                StatTracking[atckr.UserId].HumanKills += 1;
        }

        [PluginEvent(ServerEventType.PlayerEscape)]
        public void Escapes(PlayerEscapeEvent args)
        {
            var plr = args.Player;

            if (plr == null || !Round.IsRoundStarted) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].Escaped = true;
            else
                StatTracking.Add(plr.UserId, new TrackedStats(plr) { Escaped = true });
        }

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public void PlayerDisarm(PlayerHandcuffEvent args)
        {
            var plr = args.Player;
            var Target = args.Target;

            if (plr == null || !Round.IsRoundStarted) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].PlayersDisarmed += 1;
            else
                StatTracking.Add(plr.UserId, new TrackedStats(plr) { PlayersDisarmed = 1 });
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void MedicUsed(PlayerUsedItemEvent args)
        {
            var plr = args.Player;

            if (plr == null || !Round.IsRoundStarted) return;

            if (plr.CurrentItem.Category == ItemCategory.Medical) //Changed to detect any medical item, instead of just medkits
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].MedicalItems += 1;
                else
                    StatTracking.Add(plr.UserId, new TrackedStats(plr) { MedicalItems = 1 });
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void Playtime(PlayerJoinedEvent args)
        {
            var plr = args.Player;

            if (plr == null || !Round.IsRoundStarted) return;

            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].Jointime = DateTime.Now;
            else
                StatTracking.Add(plr.UserId, new TrackedStats(plr));

        }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        public void SCPRounds(PlayerSpawnEvent args)
        {
            var plr = args.Player;

            if (plr == null || !Round.IsRoundStarted) return;

            if (plr.IsSCP) //I think this will have quite a few issues with people being able to swap to human, and zombies maybe also triggering this event.
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].SCP = (int)plr.Role;
                else
                    StatTracking.Add(plr.UserId, new TrackedStats(plr) { SCP = (int)plr.Role });
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void PlaytimeAfterStart(RoundStartEvent args)
        {
            foreach (Player plr in Player.GetPlayers())
                if (StatTracking.ContainsKey(plr.UserId))
                    StatTracking[plr.UserId].Jointime = DateTime.Now;
                else
                    StatTracking.Add(plr.UserId, new TrackedStats(plr));
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        public void PlaytimeLeave(PlayerLeftEvent args)
        {
            var plr = args.Player;
            if (StatTracking.ContainsKey(plr.UserId))
                StatTracking[plr.UserId].SecondsPlayed += (int)(DateTime.Now - StatTracking[plr.UserId].Jointime).TotalSeconds;
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
                            StatTracking[plr.UserId].RoundWon = plr.Team.GetFaction() == Faction.SCP;
                            break;
                        case RoundSummary.LeadingTeam.FacilityForces:
                            StatTracking[plr.UserId].RoundWon = plr.Team.GetFaction() == Faction.FoundationStaff;
                            break;
                        case RoundSummary.LeadingTeam.ChaosInsurgency:
                            StatTracking[plr.UserId].RoundWon = plr.Team.GetFaction() == Faction.FoundationEnemy;
                            break;
                    }
                }
            }
            List<TrackedStats> stats = new List<TrackedStats>();

            foreach (var stat in StatTracking)
            {
                Log.Info($"Adding {stat.Key} | {stat.Value.UserID} | {stat.Value.DNT}");

                if (!stat.Value.DNT)
                {
                    stats.Add(stat.Value);
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
    }
}
