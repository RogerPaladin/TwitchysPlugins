using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TShockAPI;

namespace C3Mod.GameTypes
{
    public class TDM
    {
        public static bool TDMRunning = false;
        public static bool TDMCountdown = false;
        public static Vector2[] TDMSpawns = new Vector2[2];
        public static int StartCount = 5;
        public static int VoteCount = 0;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static DateTime scoreNotify = DateTime.UtcNow;
        public static int RedTeamScore = 0;
        public static int BlueTeamScore = 0;

        public static void OnUpdate(GameTime gameTime)
        {
            C3Mod.UpdateLocked = true;

            if (C3Mod.VoteRunning && C3Mod.VoteType == "tdm")
            {
                int VotedPlayers = 0;
                int TotalPlayers = 0;

                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.GameType == "" || player.GameType == "tdm")
                        TotalPlayers++;
                    if (player.GameType == "tdm")
                        VotedPlayers++;
                }

                if (VotedPlayers == TotalPlayers)
                {
                    C3Tools.BroadcastMessageToGametype("tdm", "Vote to play Team Deathmatch passed, Teleporting to start positions", Color.DarkCyan);
                    C3Mod.VoteRunning = false;
                    C3Mod.VoteType = "";
                    BlueTeamScore = 0;
                    RedTeamScore = 0;
                    bool[] playersDead = new bool[Main.maxNetPlayers];
                    TpToSpawnPoint();
                    countDownTick = DateTime.UtcNow;
                    TDMCountdown = true;
                    return;
                }

                double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                {
                    if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                    {
                        C3Tools.BroadcastMessageToGametype("tdm", "Vote still in progress, please be patient", Color.Cyan);
                        C3Tools.BroadcastMessageToGametype("", "Vote to play Team Deathmatch in progress, type /join to join the lobby", Color.Cyan);
                    }

                    VoteCount--;
                    voteCountDown = DateTime.UtcNow;
                }

                else if (VoteCount == 0)
                {
                    C3Mod.VoteRunning = false;

                    int redteamplayers = 0;
                    int blueteamplayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.Team == 7)
                            redteamplayers++;
                        else if (player.Team == 8)
                            blueteamplayers++;
                    }

                    if (redteamplayers >= C3Mod.C3Config.VoteCTFPlayersMinimumPerTeam && blueteamplayers >= C3Mod.C3Config.VoteCTFPlayersMinimumPerTeam)
                    {
                        C3Tools.BroadcastMessageToGametype("tdm", "Vote to play Team Deathmatch passed, Teleporting to start positions", Color.DarkCyan);
                        BlueTeamScore = 0;
                        RedTeamScore = 0;
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        TpToSpawnPoint();
                        countDownTick = DateTime.UtcNow;
                        TDMCountdown = true;
                    }
                    else
                        C3Tools.BroadcastMessageToGametype("tdm", "Vote to play Team Deathmatch failed, Not enough players", Color.DarkCyan);
                }
            }

            if (TDMCountdown)
            {
                double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                if (tick > 1000 && StartCount > -1)
                {
                    if (TpToSpawnPoint() > 0)
                    {
                        if (StartCount == 0)
                        {
                            C3Tools.BroadcastMessageToGametype("tdm", "Fight!!!", Color.Cyan);
                            StartCount = 5;
                            TDMCountdown = false;
                            TDMRunning = true;
                        }
                        else
                        {
                            C3Tools.BroadcastMessageToGametype("tdm", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                            countDownTick = DateTime.UtcNow;
                            StartCount--;
                        }
                    }
                    else
                    {
                        StartCount = 5;
                        C3Tools.ResetGameType("tdm");
                        return;
                    }
                }
            }

            if (TDMRunning)
            {
                int RedTeamPlayers = 0;
                int BlueTeamPlayers = 0;

                double tick = (DateTime.UtcNow - scoreNotify).TotalMilliseconds;
                if (tick > (C3Mod.C3Config.TDMScoreNotifyInterval * 1000))
                {
                    C3Tools.BroadcastMessageToGametype("tdm", "Current score: Red - " + RedTeamScore.ToString() + " --- " + BlueTeamScore.ToString() + " - Blue", Color.Cyan);
                    scoreNotify = DateTime.UtcNow;
                }

                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.TSPlayer == null)
                    {
                        C3Mod.C3Players.Remove(player);
                        break;
                    }

                    if (player.GameType == "tdm")
                    {
                        if (!player.TSPlayer.TpLock)
                            if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                        if (player.Team == 7)
                            RedTeamPlayers++;
                        else if (player.Team == 8)
                            BlueTeamPlayers++;

                        if ((player.Team == 7 && Main.player[player.Index].team != 1))
                            TShock.Players[player.Index].SetTeam(1);
                        else if (player.Team == 8 && Main.player[player.Index].team != 3)
                            TShock.Players[player.Index].SetTeam(3);

                        if (!Main.player[player.Index].hostile)
                        {
                            Main.player[player.Index].hostile = true;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f, 0f);
                        }

                        //Respawn on flag
                        if (Main.player[player.Index].dead && !player.Dead)
                        {
                            if (player.Team == 7)
                            {
                                BlueTeamScore++;
                                player.Dead = true;
                            }
                            else if (player.Team == 8)
                            {
                                RedTeamScore++;
                                player.Dead = true;
                            }
                        }

                        if (!Main.player[player.Index].dead && player.Dead)
                        {
                            player.Dead = false;
                            player.TSPlayer.TpLock = false;
                            if(player.Team == 7)
                                TShock.Players[player.Index].Teleport((int)TDMSpawns[0].X, (int)TDMSpawns[0].Y);
                            else if (player.Team == 8)
                                TShock.Players[player.Index].Teleport((int)TDMSpawns[1].X, (int)TDMSpawns[1].Y);
                            if(C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }
                        }
                    }
                }

                if (RedTeamPlayers == 0 || BlueTeamPlayers == 0)
                {
                    C3Tools.BroadcastMessageToGametype("tdm", "Not enough players to continue, ending game", Color.DarkCyan);
                    TDMRunning = false;
                    TpToSpawns(false);
                    C3Tools.ResetGameType("tdm");
                    return;
                }

                if (BlueTeamScore == C3Mod.C3Config.TeamDeathmatchScorelimit)
                {
                    TDMRunning = false;
                    C3Tools.BroadcastMessageToGametype("tdm", "BLUE TEAM WINS!", Color.LightBlue);
                    TpToSpawns(false);
                    C3Tools.ResetGameType("tdm");
                    return;
                }

                if (RedTeamScore == C3Mod.C3Config.TeamDeathmatchScorelimit)
                {
                    TDMRunning = false;
                    C3Tools.BroadcastMessageToGametype("tdm", "RED TEAM WINS!", Color.OrangeRed);
                    TpToSpawns(false);
                    C3Tools.ResetGameType("tdm");
                    return;
                }
            }

            C3Mod.UpdateLocked = false;
        }        

        public static int TpToSpawnPoint()
        {
            try
            {
                bool RedTeamPlayer = false;
                bool BlueTeamPlayer = false;

                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if (C3Mod.C3Players[i].Team == 7)
                    {
                        RedTeamPlayer = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(TDMSpawns[0].X) || C3Mod.C3Players[i].tileY != (int)(TDMSpawns[0].Y - 3))
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)TDMSpawns[0].X, (int)TDMSpawns[0].Y);
                        }
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                    else if (C3Mod.C3Players[i].Team == 8)
                    {
                        BlueTeamPlayer = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(TDMSpawns[1].X) || C3Mod.C3Players[i].tileY != (int)(TDMSpawns[1].Y - 3))
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)TDMSpawns[1].X, (int)TDMSpawns[1].Y);
                        }
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }
                if (!RedTeamPlayer || !BlueTeamPlayer)
                {
                    C3Tools.BroadcastMessageToGametype("tdm", "Not enough players to continue, ending game", Color.DarkCyan);
                    TDMRunning = false;
                    TDMCountdown = false;
                    return 0;
                }
                return 1;
            }
            catch { return 0; }
        }

        public static void TpToSpawns(bool pvpstate)
        {
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].Team == 7)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
                if (C3Mod.C3Players[i].Team == 8)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
            }
        }
    }
}