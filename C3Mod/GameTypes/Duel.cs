using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TShockAPI;

namespace C3Mod.GameTypes
{
    public class Duel
    {
        public static bool DuelRunning = false;
        public static bool DuelCountdown = false;
        public static Vector2[] DuelSpawns = new Vector2[2];
        public static int StartCount = 5;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static int RedPlayerScore = 0;
        public static int BluePlayerScore = 0;

        public static C3Player RedPlayer;
        public static C3Player BluePlayer;

        public static void OnUpdate(GameTime gameTime)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.Challenging != null)
                {
                    double tick = (DateTime.UtcNow - player.ChallengeTick).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.DuelNotifyInterval * 1000))
                    {
                        if (player.ChallengeNotifyCount != 1)
                        {
                            player.Challenging.SendMessage("You have been challenged to a duel by: " + player.PlayerName, Color.Cyan);
                            player.Challenging.SendMessage("Type /accept to accept this challenge!", Color.Cyan);
                        }
                        player.ChallengeNotifyCount--;
                        player.ChallengeTick = DateTime.UtcNow;
                    }
                    else if (player.ChallengeNotifyCount == 0)
                    {
                        player.Challenging.SendMessage("Challenge by: " + player.PlayerName + " timed out", Color.Cyan);
                        player.SendMessage(player.Challenging.PlayerName + ": Did not answer your challenge in the required amount of time", Color.DarkCyan);
                        player.Challenging = null;
                        player.ChallengeNotifyCount = 5;
                    }
                }
            }

            if (DuelCountdown)
            {
                double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                if (tick > 1000 && StartCount > -1)
                {
                    if (TpToSpawnPoint() > 0)
                    {
                        if (StartCount == 0)
                        {
                            C3Tools.BroadcastMessageToGametype("1v1", "Fight!!!", Color.Cyan);
                            StartCount = 5;
                            DuelCountdown = false;
                            DuelRunning = true;
                        }
                        else
                        {
                            C3Tools.BroadcastMessageToGametype("1v1", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                            countDownTick = DateTime.UtcNow;
                            StartCount--;
                        }
                    }
                    else
                    {
                        StartCount = 5;
                        C3Tools.ResetGameType("1v1");
                        return;
                    }
                }
            }

            if (DuelRunning)
            {
                int RedTeamPlayer = 0;
                int BlueTeamPlayer = 0;

                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.TSPlayer == null)
                    {
                        C3Mod.C3Players.Remove(player);
                        break;
                    }

                    if (player.GameType == "1v1")
                    {
                        if (!player.TSPlayer.TpLock)
                            if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                        if (player.Team == 3)
                            RedTeamPlayer++;
                        else if (player.Team == 4)
                            BlueTeamPlayer++;

                        if ((player.Team == 3 && Main.player[player.Index].team != 1))
                            TShock.Players[player.Index].SetTeam(1);
                        else if (player.Team == 4 && Main.player[player.Index].team != 3)
                            TShock.Players[player.Index].SetTeam(3);

                        if (!Main.player[player.Index].hostile)
                        {
                            Main.player[player.Index].hostile = true;
                            NetMessage.SendData((int)PacketTypes.TogglePVP, -1, -1, "", player.Index, 0f, 0f, 0f);
                        }

                        //Respawn on flag
                        if (Main.player[player.Index].dead)
                        {
                            if (player.Team == 3)
                            {
                                BluePlayerScore++;

                                if (BluePlayerScore != C3Mod.C3Config.DuelScoreLimit)
                                {
                                    C3Tools.BroadcastMessageToGametype("1v1", BluePlayer.PlayerName + ": Scores!", Color.DarkCyan);
                                    TpToSpawnPoint();
                                    DuelRunning = false;
                                    DuelCountdown = true;

                                    Item heart = Tools.GetItemById(58);
                                    Item star = Tools.GetItemById(184);
                                    foreach (C3Player players in C3Mod.C3Players)
                                    {
                                        if (players.GameType == "1v1")
                                        {
                                            players.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                            players.GiveItem(star.type, star.name, star.width, star.height, 20);
                                        }
                                    }
                                }
                            }
                            else if (player.Team == 4)
                            {
                                RedPlayerScore++;

                                if (RedPlayerScore != C3Mod.C3Config.DuelScoreLimit)
                                {
                                    C3Tools.BroadcastMessageToGametype("1v1", RedPlayer.PlayerName + ": Scores!", Color.DarkCyan);
                                    TpToSpawnPoint();
                                    DuelRunning = false;
                                    DuelCountdown = true;

                                    Item heart = Tools.GetItemById(58);
                                    Item star = Tools.GetItemById(184);
                                    foreach (C3Player players in C3Mod.C3Players)
                                    {
                                        if (players.GameType == "1v1")
                                        {
                                            players.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                            players.GiveItem(star.type, star.name, star.width, star.height, 20);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (RedTeamPlayer == 0 || BlueTeamPlayer == 0)
                {
                    C3Tools.BroadcastMessageToGametype("1v1", "Opponent left, ending game", Color.DarkCyan);
                    DuelRunning = false;
                    TpToSpawns(false);
                    C3Tools.ResetGameType("1v1");
                    RedPlayer = null;
                    BluePlayer = null;
                    return;
                }

                if (BluePlayerScore == C3Mod.C3Config.DuelScoreLimit)
                {
                    DuelRunning = false;
                    C3Tools.BroadcastMessageToGametype("1v1", BluePlayer.PlayerName + ": WINS!", Color.LightBlue);
                    C3Tools.BroadcastMessageToGametype("", BluePlayer.PlayerName + ": Beat " + RedPlayer.PlayerName + " in a duel!", Color.DarkCyan);
                    TpToSpawns(false);
                    C3Tools.ResetGameType("1v1");
                    RedPlayer = null;
                    BluePlayer = null;
                    return;
                }

                if (RedPlayerScore == C3Mod.C3Config.DuelScoreLimit)
                {
                    DuelRunning = false;
                    C3Tools.BroadcastMessageToGametype("1v1", RedPlayer.PlayerName + ": WINS!", Color.OrangeRed);
                    C3Tools.BroadcastMessageToGametype("", RedPlayer.PlayerName + ": Beat " + BluePlayer.PlayerName + " in a duel!", Color.DarkCyan);
                    TpToSpawns(false);
                    C3Tools.ResetGameType("1v1");
                    RedPlayer = null;
                    BluePlayer = null;
                    return;
                }
            }
        }        

        public static int TpToSpawnPoint()
        {
            try
            {
                bool RedTeamPlayer = false;
                bool BlueTeamPlayer = false;

                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if (C3Mod.C3Players[i].Team == 3)
                    {
                        RedTeamPlayer = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(DuelSpawns[0].X) || C3Mod.C3Players[i].tileY != (int)(DuelSpawns[0].Y - 3))
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)DuelSpawns[0].X, (int)DuelSpawns[0].Y);
                        }
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                    else if (C3Mod.C3Players[i].Team == 4)
                    {
                        BlueTeamPlayer = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(DuelSpawns[1].X) || C3Mod.C3Players[i].tileY != (int)(DuelSpawns[1].Y - 3))
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)DuelSpawns[1].X, (int)DuelSpawns[1].Y);
                        }
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }
                if (!RedTeamPlayer || !BlueTeamPlayer)
                {
                    C3Tools.BroadcastMessageToGametype("1v1", "Opponent left, ending game", Color.DarkCyan);
                    DuelRunning = false;
                    DuelCountdown = false;
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
                if (C3Mod.C3Players[i].Team == 3)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
                if (C3Mod.C3Players[i].Team == 4)
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