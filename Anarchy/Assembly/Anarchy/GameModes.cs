﻿using System.Collections.Generic;
using System.Text;
using Anarchy.Configuration;
using Anarchy.UI;
using ExitGames.Client.Photon;
using GameLogic;

namespace Anarchy
{
    public class GameModes
    {
        public static StringSetting DisabledColor = new StringSetting(nameof(DisabledColor), "FFAACC");
        public static StringSetting EnabledColor = new StringSetting(nameof(EnabledColor), "CCFFCC");

        private static List<GameModeSetting> allGameSettings;
        public static List<int> AntiReviveList = new List<int>();

        private static Hashtable oldHash = new Hashtable();
        private static Hashtable infection = new Hashtable();

        public static readonly GameModeSetting CustomAmount = new GameModeSetting("titanc", new int[1] { 5 });
        public static readonly GameModeSetting SpawnRate = new GameModeSetting("spawnMode,nRate,aRate,jRate,cRate,pRate", new float[5] { 20f, 20f, 20f, 20f, 20f }).AddApplyCallback(CheckCustomSpawn);
        public static readonly GameModeSetting SizeMode = new GameModeSetting("sizeMode,sizeLower,sizeUpper", new float[2] { 0.7f, 3f });
        public static readonly GameModeSetting HealthMode = new GameModeSetting("healthMode,healthLower,healthUpper", 0, new int[2] { 200, 500 });
        public static readonly GameModeSetting DamageMode = new GameModeSetting("damage", new int[1] { 500 });
        public static readonly GameModeSetting ExplodeMode = new GameModeSetting("explode", new int[1] { 30 });
        public static readonly GameModeSetting NoRocks = new GameModeSetting("rock");
        public static readonly GameModeSetting TitansWaveAmount = new GameModeSetting("waveModeOn,waveModeNum", new int[] { 5 });

        public static readonly GameModeSetting PointMode = new GameModeSetting("point", new int[] { 50 });
        public static readonly GameModeSetting BombMode = new GameModeSetting("bomb");
        public static readonly GameModeSetting TeamMode = new GameModeSetting("team", 0);
        public static readonly GameModeSetting InfectionMode = new GameModeSetting("infection", new int[] { 1 });
        public static readonly GameModeSetting FriendlyMode = new GameModeSetting("friendly");
        public static readonly GameModeSetting BladePVP = new GameModeSetting("pvp", 0);
        public static readonly GameModeSetting NoAHSSReload = new GameModeSetting("ahssReload");
        public static readonly GameModeSetting CannonsKillHumans = new GameModeSetting("deadlycannons");

        public static readonly GameModeSetting MaxWave = new GameModeSetting("maxwave", new int[] { 20 });
        public static readonly GameModeSetting PunkOverride = new GameModeSetting("punkWaves");
        public static readonly GameModeSetting MinimapDisable = new GameModeSetting("globalDisableMinimap");
        public static readonly GameModeSetting EndlessRespawn = new GameModeSetting("endless", new int[] { 5 });
        public static readonly GameModeSetting KickEren = new GameModeSetting("eren");
        public static readonly GameModeSetting AllowHorses = new GameModeSetting("horse");
        public static readonly StringSetting MOTD = new StringSetting("motd", string.Empty);

        public static readonly AnarchyGameModeSetting RacingStartTime = (AnarchyGameModeSetting)new AnarchyGameModeSetting("startTime,startTimeValue", new int[] { 20 }).AddChangedCallback(RacingLogic.StartTimeCheck);
        public static readonly AnarchyGameModeSetting RacingFinishersRestart = new AnarchyGameModeSetting("restartOnFinishers,finishersCount", new int[] { 5 });
        public static readonly AnarchyGameModeSetting RacingTimeLimit = new AnarchyGameModeSetting("racingTimeLimit,racingTimeLimitValue", new int[] { 500 });
        public static readonly AnarchyGameModeSetting RacingRestartTime = (AnarchyGameModeSetting)new AnarchyGameModeSetting("racingRestartTime,restartTimeValue", new int[] { 999 }).RemoveChangedCallback(AnarchyGameModeSetting.AnarchySettingCallback).AddChangedCallback(RacingLogic.RestartTimeCheck);
        public static readonly AnarchyGameModeSetting NoGuest = new AnarchyGameModeSetting("noGuest");
        public static readonly AnarchyGameModeSetting AntiRevive = new AnarchyGameModeSetting("antiRevive");
        public static readonly AnarchyGameModeSetting AFKKill = new AnarchyGameModeSetting("afkKill,afkKillTime", new int[] { 20 });
        public static readonly GameModeSetting ASORacing = new GameModeSetting("asoracing").AddChangedCallback(RacingLogic.ASORacingCheck);


        public static bool AntiReviveAdd(int ID)
        {
            if (AntiReviveEnabled() && IN_GAME_MAIN_CAMERA.GameMode != GameMode.RACING && FengGameManagerMKII.Level.RespawnMode != RespawnMode.DEATHMATCH && FengGameManagerMKII.FGM.Logic.RoundTime > 10f)
            {
                if (AntiReviveList.Contains(ID))
                {
                    return false;
                }
                AntiReviveList.Add(ID);
                return true;
            }
            return false;
        }

        public static bool AntiReviveCheck(int ID, HERO hero)
        {
            if (AntiReviveEnabled())
            {
                if (AntiReviveList.Contains(ID))
                {
                    hero.BasePV.RPC("netDie2", PhotonTargets.All, new object[] { -1, "Anti-Revive " });
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void AntiReviveClear()
        {
            if (AntiReviveEnabled())
            {
                AntiReviveList.Clear();
            }
        }

        public static bool AntiReviveEnabled()
        {
            return IN_GAME_MAIN_CAMERA.GameType == GameType.MultiPlayer && PhotonNetwork.IsMasterClient && AntiRevive.Enabled && !EndlessRespawn.Enabled;
        }

        public static bool AntiReviveRemove(int ID)
        {
            if (AntiReviveEnabled())
            {
                return AntiReviveList.Remove(ID);
            }
            return false;
        }

        public static void AddSetting(GameModeSetting set)
        {
            if (allGameSettings == null)
                allGameSettings = new List<GameModeSetting>();
            if (!allGameSettings.Contains(set))
                allGameSettings.Add(set);
        }

        private static void CheckCustomSpawn(GameModeSetting set, bool state, int selection, float[] floats, int[] integers)
        {
            if(state)
            {
                float summ = 0f;
                for(int i = 0; i < 5; i++)
                {
                    summ += floats[i];
                }
                if(summ > 100f)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        floats[i] = 20f;
                    }
                }
            }
        }

        private static System.Collections.IEnumerator CheckEndless(int id, float time)
        {
            yield return new UnityEngine.WaitForSeconds(time);
            PhotonPlayer player = PhotonPlayer.Find(id);
            if (player != null && player.Dead && !player.IsTitan)
            {
                FengGameManagerMKII.FGM.BasePV.RPC("respawnHeroInNewRound", player, new object[0]);
            }
            yield break;
        }

        public static System.Collections.IEnumerator CheckGameEnd()
        {
            yield return new UnityEngine.WaitForSeconds(0.05f);
            if (PhotonNetwork.IsMasterClient && CheckPVPWinner())
            {
                yield break;
            }
            if (PhotonNetwork.IsMasterClient && FengGameManagerMKII.FGM.IsPlayerAllDead())
            {
                FengGameManagerMKII.FGM.GameLose();
                yield break;
            }
            yield break;
        }

        public static bool CheckPVPWinner()
        {
            if (!BombMode.Enabled && !BladePVP.Enabled || FengGameManagerMKII.FGM.IsWinning)
            {
                return false;
            }
            if (!TeamMode.Enabled)
            {
                if (PointMode.Enabled)
                {
                    List<PhotonPlayer> alives = new List<PhotonPlayer>();
                    string winnerName = "";
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        if (!player.Dead)
                        {
                            alives.Add(player);
                        }
                    }
                    if (alives.Count > 1)
                    {
                        return false;
                    }
                    winnerName = (alives.Count > 0) ? alives[0].UIName.ToHTMLFormat() : "Nobody";
                    FengGameManagerMKII.FGM.GameWin();
                    FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[]
                    {
                    winnerName + " wins. 5 points added.",
                    ""
                    });
                    if (alives.Count > 0)
                        alives[0].Kills += 5;
                }
                else
                {
                    foreach(PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        if (player.Kills > PointMode.GetInt(0))
                        {
                            FengGameManagerMKII.FGM.GameWin();
                            FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[]
                            {
                     player.UIName.ToHTMLFormat() + " wins. 5 points added.",
                    ""
                            });
                        }
                    }
                }
                return true;
            }
            else
            {
                bool teamCyanWin = false;
                bool teamMagentaWin = false;
                if (PointMode.Enabled)
                {
                    int cyanKills = 0;
                    int magentaKills = 0;
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        if (player.RCteam == 1)
                            cyanKills += player.Kills;
                        else if (player.RCteam == 2)
                            magentaKills += player.Kills;
                    }
                    if (cyanKills >= PointMode.GetInt(0))
                    {
                        teamCyanWin = true;
                    }
                    else if(magentaKills >= PointMode.GetInt(0))
                    {
                        teamMagentaWin = true;
                    }
                }
                else
                {
                    List<PhotonPlayer> cyan = new List<PhotonPlayer>();
                    List<PhotonPlayer> magenta = new List<PhotonPlayer>();
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        if (!player.Dead)
                        {
                            int rcteam = player.RCteam;
                            if (rcteam == 1)
                            {
                                cyan.Add(player);
                            }
                            else if (rcteam == 2)
                            {
                                magenta.Add(player);
                            }
                        }
                    }
                    if (cyan.Count < 1)
                    {
                        teamCyanWin = true;
                    }
                    else if (magenta.Count < 1)
                    {
                        teamMagentaWin = true;
                    }
                }
                if(teamMagentaWin ^ teamCyanWin)
                {
                    FengGameManagerMKII.FGM.GameWin();
                    FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[]
                    {
                        $"Team <color={(teamCyanWin ? "cyan>cyan" : "magenta>magenta")}</color> wins.",
                        ""
                    });
                    return true;
                }
                else if(teamMagentaWin && teamCyanWin)
                {
                    FengGameManagerMKII.FGM.GameWin();
                    FengGameManagerMKII.FGM.BasePV.RPC("Chat", PhotonTargets.All, new object[]
                    {
                        $"Nobody wins.",
                        ""
                    });
                    return true;
                }
                return false;
            }
        }

        public static void EndlessMode(int ID)
        {
            if (!EndlessRespawn.Enabled)
                return;
            FengGameManagerMKII.FGM.StartCoroutine(CheckEndless(ID, EndlessRespawn.GetInt(0)));
        }

        public static void ForceChange()
        {
            foreach(GameModeSetting set in allGameSettings)
            {
                set.ForceChange();
            }
        }

        public static string GetGameModesInfo()
        {
            StringBuilder bld = new StringBuilder();
            int count = 0;
            foreach (GameModeSetting set in allGameSettings)
            {
                if (set.Enabled)
                {
                    bld.Append((count > 0 ? "\n" : string.Empty) + set.ToStringLocal());
                    count++;
                }
            }
            if (count == 0)
            {
                return string.Empty;
            }
            return bld.ToString();
        }

        public static void HandleRPC(Hashtable hash)
        {
            if (oldHash.Equals(hash))
                return;
            int count = 0;
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < allGameSettings.Count; i++)
            {
                GameModeSetting set = allGameSettings[i];
                set.ReadFromHashtable(hash);
                if(set.HasChangedReceived)
                {
                    set.ApplyReceived();
                    if (count > 0)
                        bld.Append("\n");
                    bld.Append(set.ToStringLocal());
                    count++;
                }
            }
            if(hash.ContainsKey("motd") && oldHash.ContainsKey("motd") && oldHash["motd"] as string != hash["motd"] as string)
            {
                if (count > 0)
                    bld.Append("\n");
                bld.Append("MOTD: " + hash["motd"].ToString());
            }
            oldHash = new Hashtable();
            Dictionary<object, object> clone = (Dictionary<object, object>)hash.Clone();
            foreach(KeyValuePair<object, object> pair in clone)
            {
                oldHash.Add(pair.Key, pair.Value);
            }
            Chat.Add(bld.ToString());
        }

        public static void InfectionOnDeath(PhotonPlayer owner)
        {
            if ( PhotonNetwork.IsMasterClient && InfectionMode.Enabled && !FengGameManagerMKII.FGM.IsLosing && !FengGameManagerMKII.FGM.IsWinning && FengGameManagerMKII.FGM.Logic.RoundTime > 5f)
            {
                if (!infection.ContainsKey(owner.ID))
                {
                    infection.Add(owner, owner.ID);
                    owner.IsTitan = true;
                    FengGameManagerMKII.FGM.BasePV.RPC("spawnTitanRPC", owner, new object[0]);
                }
            }
        }

        public static void InfectionOnSpawn(HERO hero)
        {
            if (InfectionMode.Enabled && PhotonNetwork.IsMasterClient && !FengGameManagerMKII.FGM.IsLosing && !FengGameManagerMKII.FGM.IsWinning && FengGameManagerMKII.FGM.Logic.RoundTime > 5f)
            {
                if (infection.ContainsKey(hero.BasePV.owner.ID))
                {
                    hero.markDie();
                    hero.BasePV.RPC("netDie2", PhotonTargets.All, new object[] { -1, "noswitchingfgt" });
                }
            }
        }

        private static void InfectionUpdate()
        {
            int count = InfectionMode.GetInt(0);
            if(count < 0 || count > PhotonNetwork.playerList.Length)
            {
                count = 1;
            }
            infection.Clear();
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                player.IsTitan = false;
            }
            int players = PhotonNetwork.playerList.Length;
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                PhotonPlayer player = PhotonNetwork.playerList[i];
                if (UnityEngine.Random.Range(0f, 1f) <= (float)count / (float)players)
                {
                    player.IsTitan = true;
                    infection.Add(player.ID, 2);
                    if (--count == 0)
                    {
                        break;
                    }
                }
                players--;
            }
        }

        public static void Load()
        {
            foreach (GameModeSetting set in allGameSettings)
            {
                set.Load();
            }
        }

        public static void OnRestart()
        {
            if (InfectionMode.Enabled)
            {
                return;
            }
            else if (TeamMode.Enabled)
            {
                SendTeamInfo();
            }
            if (PointMode.Enabled)
            {
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    player.Kills = player.Deaths = player.Total_Dmg = player.Max_Dmg = 0;
                }
            }
        }

        public static void ResetOnLoad()
        {
            foreach (GameModeSetting set in allGameSettings)
            {
                set.ForceDisable();
                set.Apply();
            }
            oldHash = new Hashtable();
        }

        public static void Save()
        {
            foreach(GameModeSetting set in allGameSettings)
            {
                set.Save();
            }
        }

        private static void SendTeamInfo()
        {
            int team = 1;
            for(int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                FengGameManagerMKII.FGM.BasePV.RPC("setTeamRPC", PhotonNetwork.playerList[i], new object[] { (team++ % 2 == 0 ? 2 : 1) });
            }
        }

        public static void SendRPC()
        {
            StringBuilder bld = new StringBuilder();
            Hashtable hash = new Hashtable();
            int count = 0;
            int countSend = 0;
            for(int i = 0; i < allGameSettings.Count; i++)
            {
                GameModeSetting set = allGameSettings[i];
                if (set.Enabled && !set.HasChanged)
                {
                    set.WriteToHashtable(hash);
                    count++;
                    continue;
                }
                if (set.HasChanged)
                {
                    bool oldstate = set.State;
                    set.Apply();
                    if(oldstate != set.State && set.State == false)
                    {
                        set.Save();
                        continue;
                    }
                    set.WriteToHashtable(hash);
                    if (countSend > 0)
                        bld.Append("\n");
                    bld.Append(set.ToStringLocal());
                    set.Save();
                    count++;
                    countSend++;
                }
            }
            if (count <= 0)
                return;
            if (InfectionMode.Enabled)
            {
                InfectionUpdate();
            }
            if (TeamMode.Enabled)
            {
                SendTeamInfo();
            }
            FengGameManagerMKII.FGM.BasePV.RPC("settingRPC", PhotonTargets.Others, new object[] { hash });
            if (bld.ToString() != string.Empty)
            {
                Chat.Add(bld.ToString());
            }
        }

        public static void SendRPCToPlayer(PhotonPlayer player)
        {
            Hashtable hash = new Hashtable();
            string vanillaString = string.Empty;
            string anarchyString = string.Empty;
            int count = 0;
            for (int i = 0; i < allGameSettings.Count; i++)
            {
                GameModeSetting set = allGameSettings[i];
                if (set.Enabled)
                {
                    set.WriteToHashtable(hash);
                    count++;
                    if(!player.AnarchySync && set is AnarchyGameModeSetting setting)
                    {
                        if(anarchyString.Length > 0)
                        {
                            anarchyString += "\n";
                        }
                        anarchyString += setting.ToString();
                    }
                    else if (!player.RCSync)
                    {
                        if (vanillaString.Length > 0)
                        {
                            vanillaString += "\n";
                        }
                        vanillaString += set.ToString();
                    }
                }
            }
            if (count <= 0)
            {
                if(MOTD.Value.Length > 0)
                {
                    FengGameManagerMKII.FGM.BasePV.RPC("Chat", player, new object[] { "MOTD: " + MOTD.Value, string.Empty });
                }
                return;
            }
            FengGameManagerMKII.FGM.BasePV.RPC("settingRPC", player, new object[] { hash });
            if (!player.RCSync & vanillaString.Length > 0)
            {
                FengGameManagerMKII.FGM.BasePV.RPC("Chat", player, new object[] { vanillaString, string.Empty });
            }
            if (!player.AnarchySync && anarchyString.Length > 0)
            {
                FengGameManagerMKII.FGM.BasePV.RPC("Chat", player, new object[] { anarchyString, string.Empty });
            }
            if (MOTD.Value != string.Empty)
            {
                FengGameManagerMKII.FGM.BasePV.RPC("Chat", player, new object[] { "MOTD: " + MOTD.Value, string.Empty });
            }
        }
    }
}
