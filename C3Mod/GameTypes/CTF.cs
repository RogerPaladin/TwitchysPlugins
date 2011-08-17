using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TShockAPI;
using System.Threading;
using TShockAPI.DB;

namespace C3Mod.GameTypes
{
    public class CTF
    {
        public static bool CTFGameRunning = false;
        public static bool CTFGameCountdown = false;
        public static Vector2[] flagPoints = new Vector2[2];
        public static int BlueTeamScore = 0;
        public static int RedTeamScore = 0;
        public static bool[] playersDead = new bool[Main.maxNetPlayers];
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static C3Player BlueTeamFlagCarrier;
        public static C3Player RedTeamFlagCarrier;
        public static int StartCount = 5;
        public static int VoteCount = 0;

        public static void OnUpdate(GameTime gameTime)
        {
            lock (C3Mod.C3Players)
            {

                if (C3Mod.VoteRunning && C3Mod.VoteType == "ctf")
                {
                    int VotedPlayers = 0;
                    int TotalPlayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "ctf")
                            TotalPlayers++;
                        if (player.GameType == "ctf")
                            VotedPlayers++;
                    }

                    if (VotedPlayers == TotalPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("ctf", "Vote to play Capture the Flag passed, Teleporting to start positions", Color.DarkCyan);
                        C3Mod.VoteRunning = false;
                        C3Mod.VoteType = "";
                        CTF.BlueTeamFlagCarrier = null;
                        CTF.RedTeamFlagCarrier = null;
                        CTF.BlueTeamScore = 0;
                        CTF.RedTeamScore = 0;
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        CTF.TpToFlag();
                        CTF.countDownTick = DateTime.UtcNow;
                        CTF.CTFGameCountdown = true;
                        return;
                    }

                    double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                    {
                        if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                        {
                            C3Tools.BroadcastMessageToGametype("ctf", "Vote still in progress, please be patient", Color.Cyan);
                            C3Tools.BroadcastMessageToGametype("", "Vote to play Capture the Flag in progress, type /join to join the lobby", Color.Cyan);
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
                            if (player.Team == 1)
                                redteamplayers++;
                            else if (player.Team == 2)
                                blueteamplayers++;
                        }

                        if (redteamplayers >= C3Mod.C3Config.VoteCTFPlayersMinimumPerTeam && blueteamplayers >= C3Mod.C3Config.VoteCTFPlayersMinimumPerTeam)
                        {
                            C3Tools.BroadcastMessageToGametype("ctf", "Vote to play Capture the Flag passed, Teleporting to start positions", Color.DarkCyan);
                            CTF.BlueTeamFlagCarrier = null;
                            CTF.RedTeamFlagCarrier = null;
                            CTF.BlueTeamScore = 0;
                            CTF.RedTeamScore = 0;
                            bool[] playersDead = new bool[Main.maxNetPlayers];
                            CTF.TpToFlag();
                            CTF.countDownTick = DateTime.UtcNow;
                            CTF.CTFGameCountdown = true;
                        }
                        else
                            C3Tools.BroadcastMessageToGametype("ctf", "Vote to play Capture the Flag failed, Not enough players", Color.DarkCyan);
                    }
                }

                if (CTFGameCountdown)
                {
                    double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                    if (tick > 1000 && StartCount > -1)
                    {
                        if (TpToFlag() > 0)
                        {
                            if (StartCount == 0)
                            {
                                C3Tools.BroadcastMessageToGametype("ctf", "Capture...The...Flag!!!", Color.Cyan);
                                StartCount = 5;
                                CTFGameCountdown = false;
                                CTFGameRunning = true;
                            }
                            else
                            {
                                C3Tools.BroadcastMessageToGametype("ctf", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                                countDownTick = DateTime.UtcNow;
                                StartCount--;
                            }
                        }
                        else
                        {
                            StartCount = 5;
                            C3Tools.ResetGameType("ctf");
                            return;
                        }
                    }
                }

                if (CTFGameRunning)
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

                        if (player.GameType == "ctf")
                        {
                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (player.Team == 1)
                                redteamplayers++;
                            else if (player.Team == 2)
                                blueteamplayers++;

                            if ((player.Team == 1 && Main.player[player.Index].team != 1))
                                TShock.Players[player.Index].SetTeam(1);
                            else if (player.Team == 2 && Main.player[player.Index].team != 3)
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
                                    if (player.Team == 1)
                                        TShock.Players[player.Index].Teleport((int)flagPoints[0].X, (int)flagPoints[0].Y);
                                    else if (player.Team == 2)
                                        TShock.Players[player.Index].Teleport((int)flagPoints[1].X, (int)flagPoints[1].Y);
                                    if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }
                                }
                            }

                            //Grab flag
                            if (!player.Dead)
                            {
                                if (player.Team == 1 && RedTeamFlagCarrier == null)
                                {
                                    if ((int)player.tileX >= flagPoints[1].X - 2 && (int)player.tileX <= flagPoints[1].X + 2 && (int)player.tileY == (int)(flagPoints[1].Y - 3))
                                    {
                                        RedTeamFlagCarrier = player;
                                        C3Tools.BroadcastMessageToGametype("ctf", Main.player[player.Index].name + " has the flag!", Color.OrangeRed);
                                    }
                                }
                                if (player.Team == 2 && BlueTeamFlagCarrier == null)
                                {
                                    if ((int)player.tileX >= flagPoints[0].X - 2 && (int)player.tileX <= flagPoints[0].X + 2 && (int)player.tileY == (int)(flagPoints[0].Y - 3))
                                    {
                                        BlueTeamFlagCarrier = player;
                                        C3Tools.BroadcastMessageToGametype("ctf", Main.player[player.Index].name + " has the flag!", Color.LightBlue);
                                    }
                                }
                            }
                        }
                    }

                    if (redteamplayers == 0 || blueteamplayers == 0)
                    {
                        C3Tools.BroadcastMessageToGametype("ctf", "Capture the Flag stopped, Not enough players to continue", Color.DarkCyan);
                        CTFGameRunning = false;
                        TpToSpawns(false);
                        C3Tools.ResetGameType("ctf");
                        return;
                    }

                    //Check on flag carriers
                    if (BlueTeamFlagCarrier != null)
                    {
                        //Make them drop the flag
                        if (BlueTeamFlagCarrier.TerrariaDead)
                        {
                            C3Tools.BroadcastMessageToGametype("ctf", BlueTeamFlagCarrier.PlayerName + ": Dropped the flag!", Color.LightBlue);
                            BlueTeamFlagCarrier = null;
                        }
                        //Capture the flag
                        else
                        {
                            if ((int)BlueTeamFlagCarrier.tileX >= flagPoints[1].X - 2 && (int)BlueTeamFlagCarrier.tileX <= flagPoints[1].X + 2 && (int)BlueTeamFlagCarrier.tileY == (int)(flagPoints[1].Y - 3))
                            {
                                BlueTeamScore++;
                                C3Tools.BroadcastMessageToGametype("ctf", BlueTeamFlagCarrier.PlayerName + ": Scores!  Blue - " + BlueTeamScore.ToString() + " --- " + RedTeamScore.ToString() + " - Red", Color.LightBlue);
                                RedTeamFlagCarrier = null;
                                BlueTeamFlagCarrier = null;

                                if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                    TpToFlag();

                                if (C3Mod.C3Config.ReCountdownOnFlagCapture && BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                {
                                    CTFGameRunning = false;
                                    CTFGameCountdown = true;
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
                    if (RedTeamFlagCarrier != null)
                    {
                        if (RedTeamFlagCarrier.TerrariaDead)
                        {
                            C3Tools.BroadcastMessageToGametype("ctf", RedTeamFlagCarrier.PlayerName + ": Dropped the flag!", Color.OrangeRed);
                            RedTeamFlagCarrier = null;
                        }
                        else
                        {
                            if ((int)RedTeamFlagCarrier.tileX >= flagPoints[0].X - 2 && (int)RedTeamFlagCarrier.tileX <= flagPoints[0].X + 2 && (int)RedTeamFlagCarrier.tileY == (int)(flagPoints[0].Y - 3))
                            {
                                RedTeamScore++;
                                C3Tools.BroadcastMessageToGametype("ctf", RedTeamFlagCarrier.PlayerName + ": Scores!  Red - " + RedTeamScore.ToString() + " --- " + BlueTeamScore.ToString() + " - Blue", Color.OrangeRed);
                                RedTeamFlagCarrier = null;
                                BlueTeamFlagCarrier = null;

                                if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && RedTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                    TpToFlag();

                                if (C3Mod.C3Config.ReCountdownOnFlagCapture && RedTeamScore != C3Mod.C3Config.CTFScoreLimit)
                                {
                                    CTFGameRunning = false;
                                    CTFGameCountdown = true;
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
                    CTFGameRunning = false;
                    C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.LightBlue);
                    TpToSpawns(false);
                    C3Tools.ResetGameType("ctf");
                    return;
                }
                if (RedTeamScore == C3Mod.C3Config.CTFScoreLimit)
                {
                    CTFGameRunning = false;
                    C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
                    TpToSpawns(false);
                    C3Tools.ResetGameType("ctf");
                    return;
                }
            }
        }

        public static int TpToFlag()
        {
            try
            {
                int playersred = 0;
                int playersblue = 0;

                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if (C3Mod.C3Players[i].Team == 1)
                    {
                        playersred++;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(flagPoints[0].X) || C3Mod.C3Players[i].tileY != (int)(flagPoints[0].Y))
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)flagPoints[0].X, (int)flagPoints[0].Y);
                        if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }

                    if (C3Mod.C3Players[i].Team == 2)
                    {
                        playersblue++;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(flagPoints[1].X) || C3Mod.C3Players[i].tileY != (int)(flagPoints[1].Y))
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)flagPoints[1].X, (int)flagPoints[1].Y);
                        if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }

                if (playersred == 0 || playersblue == 0)
                {
                    C3Tools.BroadcastMessageToGametype("ctf", "Not enough players to start CTF", Color.DarkCyan);
                    CTFGameRunning = false;
                    CTFGameCountdown = false;
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
                if (C3Mod.C3Players[i].Team == 1)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                }
                if (C3Mod.C3Players[i].Team == 2)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                }
            }
        }

        public static void RedFlagSet(float posX, float posY)
        {
            flagPoints[0].X = (posX / 16);
            flagPoints[0].Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("RedX", (int)(posX / 16)));
            values.Add(new SqlValue("RedY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("FlagPoints", values, new List<SqlValue>());
        }

        public static void BlueFlagSet(float posX, float posY)
        {
            flagPoints[1].X = (posX / 16);
            flagPoints[1].Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("BlueX", (int)(posX / 16)));
            values.Add(new SqlValue("BlueY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("FlagPoints", values, new List<SqlValue>());
        }
    }
}
