using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TShockAPI;
using TShockAPI.DB;

namespace C3Mod.GameTypes
{
    public class OneFlagCTF
    {
        public static bool OneFlagGameRunning = false;
        public static bool OneFlagGameCountdown = false;
        public static Vector2 FlagPoint = new Vector2();
        public static Vector2[] SpawnPoint = new Vector2[2];
        public static int BlueTeamScore = 0;
        public static int RedTeamScore = 0;
        public static bool[] playersDead = new bool[Main.maxNetPlayers];
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static C3Player FlagCarrier;
        public static int StartCount = 5;
        public static int VoteCount = 0;

        public static void OnUpdate(GameTime gameTime)
        {
            lock (C3Mod.C3Players)
            {

                if (C3Mod.VoteRunning && C3Mod.VoteType == "oneflag")
                {
                    int VotedPlayers = 0;
                    int TotalPlayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "oneflag")
                            TotalPlayers++;
                        if (player.GameType == "oneflag")
                            VotedPlayers++;
                    }

                    if (VotedPlayers == TotalPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF passed, Teleporting to start positions", Color.DarkCyan);
                        C3Mod.VoteRunning = false;
                        C3Mod.VoteType = "";
                        FlagCarrier = null;
                        BlueTeamScore = 0;
                        RedTeamScore = 0;
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        TpToOneFlagSpawns();
                        countDownTick = DateTime.UtcNow;
                        OneFlagGameCountdown = true;
                        return;
                    }

                    double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                    {
                        if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                        {
                            C3Tools.BroadcastMessageToGametype("oneflag", "Vote still in progress, please be patient", Color.Cyan);
                            C3Tools.BroadcastMessageToGametype("", "Vote to play One Flag CTF in progress, type /join to join the lobby", Color.Cyan);
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
                            if (player.Team == 5)
                                redteamplayers++;
                            else if (player.Team == 6)
                                blueteamplayers++;
                        }

                        if (redteamplayers >= C3Mod.C3Config.VoteCTFPlayersMinimumPerTeam && blueteamplayers >= C3Mod.C3Config.VoteCTFPlayersMinimumPerTeam)
                        {
                            C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF passed, Teleporting to start positions", Color.DarkCyan);
                            FlagCarrier = null;
                            BlueTeamScore = 0;
                            RedTeamScore = 0;
                            bool[] playersDead = new bool[Main.maxNetPlayers];
                            TpToOneFlagSpawns();
                            countDownTick = DateTime.UtcNow;
                            OneFlagGameCountdown = true;
                        }
                        else
                            C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF failed, Not enough players", Color.DarkCyan);
                    }
                }
                if (OneFlagGameCountdown)
                {
                    double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                    if (tick > 1000 && StartCount > -1)
                    {
                        if (TpToOneFlagSpawns() > 0)
                        {
                            if (StartCount == 0)
                            {
                                C3Tools.BroadcastMessageToGametype("oneflag", "Capture...The...Flag!!!", Color.Cyan);
                                StartCount = 5;
                                OneFlagGameCountdown = false;
                                OneFlagGameRunning = true;
                            }
                            else
                            {
                                C3Tools.BroadcastMessageToGametype("oneflag", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                                countDownTick = DateTime.UtcNow;
                                StartCount--;
                            }
                        }
                        else
                        {
                            StartCount = 5;
                            C3Tools.ResetGameType("oneflag");
                            return;
                        }
                    }
                }

                if (OneFlagGameRunning)
                {
                    int redteamplayers = 0;
                    int blueteamplayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.TSPlayer == null)
                        {
                            C3Mod.C3Players.Remove(player);
                            break;
                        }

                        if (player.GameType == "oneflag")
                        {
                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (player.Team == 5)
                                redteamplayers++;
                            else if (player.Team == 6)
                                blueteamplayers++;

                            if ((player.Team == 5 && Main.player[player.Index].team != 1))
                                TShock.Players[player.Index].SetTeam(1);
                            else if (player.Team == 6 && Main.player[player.Index].team != 3)
                                TShock.Players[player.Index].SetTeam(3);

                            if (!Main.player[player.Index].hostile)
                            {
                                Main.player[player.Index].hostile = true;
                                NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f, 0f);
                            }

                            //Respawn on flag
                            if (Main.player[player.Index].dead)
                                player.Dead = true;
                            else
                            {
                                if (player.Dead)
                                {
                                    player.Dead = false;
                                    player.TSPlayer.TpLock = false;
                                    if (player.Team == 5)
                                        TShock.Players[player.Index].Teleport((int)SpawnPoint[0].X, (int)SpawnPoint[0].Y);

                                    else if (player.Team == 6)
                                        TShock.Players[player.Index].Teleport((int)SpawnPoint[1].X, (int)SpawnPoint[1].Y);
                                    if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }
                                }
                            }

                            //Grab flag
                            if (!player.Dead)
                            {
                                if (FlagCarrier == null)
                                {
                                    if ((int)player.tileX <= (int)FlagPoint.X + 2 && (int)player.tileX >= (int)FlagPoint.X - 2 && (int)player.tileY == (int)FlagPoint.Y - 3)
                                    {
                                        FlagCarrier = player;

                                        if (player.Team == 5)
                                            C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.OrangeRed);
                                        else if (player.Team == 6)
                                            C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightBlue);
                                    }
                                }
                            }
                        }
                    }

                    if (redteamplayers == 0 || blueteamplayers == 0)
                    {
                        C3Tools.BroadcastMessageToGametype("oneflag", "One Flag CTF stopped, Not enough players to continue", Color.DarkCyan);
                        OneFlagGameRunning = false;
                        SendToSpawn(false);
                        C3Tools.ResetGameType("oneflag");
                        return;
                    }

                    //Check on flag carrier
                    if (FlagCarrier != null)
                    {
                        //Make them drop the flag
                        if (Main.player[FlagCarrier.Index].dead)
                        {
                            if (FlagCarrier.Team == 5)
                                C3Tools.BroadcastMessageToGametype("oneflag", Main.player[FlagCarrier.Index].name + " dropped the flag!", Color.OrangeRed);
                            else if (FlagCarrier.Team == 6)
                                C3Tools.BroadcastMessageToGametype("oneflag", Main.player[FlagCarrier.Index].name + " dropped the flag!", Color.LightBlue);

                            FlagCarrier = null;
                        }
                        //Capture the flag
                        else
                        {
                            if (FlagCarrier.Team == 5)
                            {
                                if ((int)FlagCarrier.tileX <= (int)SpawnPoint[0].X + 2 && (int)FlagCarrier.tileX >= (int)SpawnPoint[0].X - 2 && (int)FlagCarrier.tileY == (int)SpawnPoint[0].Y - 3)
                                {
                                    RedTeamScore++;
                                    FlagCarrier = null;
                                    C3Tools.BroadcastMessageToGametype("oneflag", "Red team scores! Red - " + RedTeamScore.ToString() + " --- " + BlueTeamScore.ToString() + " - Blue", Color.OrangeRed);

                                    if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                        TpToOneFlagSpawns();

                                    if (C3Mod.C3Config.ReCountdownOnFlagCapture && BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                    {
                                        OneFlagGameRunning = false;
                                        OneFlagGameCountdown = true;
                                    }

                                    if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                                    {
                                        Item heart = Tools.GetItemById(58);
                                        Item star = Tools.GetItemById(184);

                                        foreach (C3Player player in C3Mod.C3Players)
                                        {
                                            if (player.GameType == "ctf")
                                            {
                                                player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                                player.GiveItem(star.type, star.name, star.width, star.height, 20);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (FlagCarrier.Team == 6)
                            {
                                if ((int)FlagCarrier.tileX <= (int)SpawnPoint[1].X + 2 && (int)FlagCarrier.tileX >= (int)SpawnPoint[1].X - 2 && (int)FlagCarrier.tileY == (int)SpawnPoint[1].Y - 3)
                                {
                                    BlueTeamScore++;
                                    FlagCarrier = null;
                                    C3Tools.BroadcastMessageToGametype("oneflag", "Blue team scores! Blue - " + BlueTeamScore.ToString() + " --- " + RedTeamScore.ToString() + " - Red", Color.LightBlue);

                                    if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                        TpToOneFlagSpawns();

                                    if (C3Mod.C3Config.ReCountdownOnFlagCapture && BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                    {
                                        OneFlagGameRunning = false;
                                        OneFlagGameCountdown = true;
                                    }

                                    if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                                    {
                                        Item heart = Tools.GetItemById(58);
                                        Item star = Tools.GetItemById(184);

                                        foreach (C3Player player in C3Mod.C3Players)
                                        {
                                            if (player.GameType == "ctf")
                                            {
                                                player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                                player.GiveItem(star.type, star.name, star.width, star.height, 20);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (BlueTeamScore == C3Mod.C3Config.CTFScoreLimit)
                    {
                        OneFlagGameRunning = false;
                        C3Tools.BroadcastMessageToGametype("oneflag", "BLUE TEAM WINS!", Color.LightBlue);
                        SendToSpawn(false);
                        C3Tools.ResetGameType("oneflag");
                        return;
                    }
                    if (RedTeamScore == C3Mod.C3Config.CTFScoreLimit)
                    {
                        OneFlagGameRunning = false;
                        C3Tools.BroadcastMessageToGametype("oneflag", "RED TEAM WINS!", Color.OrangeRed);
                        SendToSpawn(false);
                        C3Tools.ResetGameType("oneflag");
                        return;
                    }
                }
            }
        }   

        public static int TpToOneFlagSpawns()
        {
                int playersred = 0;
                int playersblue = 0;

                for(int i = 0; i<C3Mod.C3Players.Count;i++)
                {
                    if (C3Mod.C3Players[i].Team == 5)
                    {
                        playersred++;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(SpawnPoint[0].X) || C3Mod.C3Players[i].tileY != (int)(SpawnPoint[0].Y - 3))
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)SpawnPoint[0].X, (int)SpawnPoint[0].Y);
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                    else if (C3Mod.C3Players[i].Team == 6)
                    {
                        playersblue++;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(SpawnPoint[1].X) || C3Mod.C3Players[i].tileY != (int)(SpawnPoint[1].Y - 3))
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)SpawnPoint[1].X, (int)SpawnPoint[1].Y);
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }

                if (playersred == 0 || playersblue == 0)
                {
                    C3Tools.BroadcastMessageToGametype("oneflag", "Not enough players to start One Flag CTF", Color.DarkCyan);
                    OneFlagGameRunning = false;
                    OneFlagGameCountdown = false;
                    return 0;
                }
                return 1;
        }

        public static void SendToSpawn(bool pvpstate)
        {
                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if (C3Mod.C3Players[i].Team == 5)
                    {
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                        NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                        TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    }
                    else if (C3Mod.C3Players[i].Team == 6)
                    {
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                        NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                        TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    }
                }
        }

        public static void RedSpawnSet(float posX, float posY)
        {
            SpawnPoint[0].X = (int)(posX / 16);
            SpawnPoint[0].Y = (int)(posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("RedSpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("RedSpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("OneFlagPoints", values, new List<SqlValue>());
        }

        public static void BlueSpawnSet(float posX, float posY)
        {
            SpawnPoint[1].X = (int)(posX / 16);
            SpawnPoint[1].Y = (int)(posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("BlueSpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("BlueSpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("OneFlagPoints", values, new List<SqlValue>());
        }

        public static void FlagSet(float posX, float posY)
        {
            FlagPoint.X = (int)(posX / 16);
            FlagPoint.Y = (int)(posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("FlagX", (int)(posX / 16)));
            values.Add(new SqlValue("FlagY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("OneFlagPoints", values, new List<SqlValue>());
        }
    }
}
