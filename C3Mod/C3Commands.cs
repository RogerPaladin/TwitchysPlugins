using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using TShockAPI.DB;
using Microsoft.Xna.Framework;
using C3Mod.GameTypes;

namespace C3Mod
{
    public class C3Commands
    {
        public static void Stop(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                switch (args.Parameters[0])
                {
                    case "ctf":
                        {
                            if (CTF.CTFGameRunning || CTF.CTFGameCountdown)
                            {
                                C3Tools.BroadcastMessageToGametype("ctf", "CTF stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("CTF Game Stopped", Color.DarkCyan);
                                CTF.TpToSpawns(false);
                                C3Tools.ResetGameType("ctf");
                                CTF.CTFGameRunning = false;
                                CTF.CTFGameCountdown = false;
                            }
                            else
                            {
                                args.Player.SendMessage("CTF game not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "duel":
                        {
                            if (Duel.DuelRunning || Duel.DuelCountdown)
                            {
                                C3Tools.BroadcastMessageToGametype("1v1", "Duel stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Duel Stopped", Color.DarkCyan);
                                Duel.TpToSpawns(false);
                                C3Tools.ResetGameType("1v1");
                                Duel.DuelRunning = false;
                                Duel.DuelCountdown = false;
                            }
                            else
                            {
                                args.Player.SendMessage("Duel not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "oneflag":
                        {
                            if (OneFlagCTF.OneFlagGameCountdown || OneFlagCTF.OneFlagGameRunning)
                            {
                                C3Tools.BroadcastMessageToGametype("oneflag", "One flag stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("One Flag Stopped", Color.DarkCyan);
                                OneFlagCTF.SendToSpawn(false);
                                C3Tools.ResetGameType("oneflag");
                                OneFlagCTF.OneFlagGameRunning = false;
                                OneFlagCTF.OneFlagGameCountdown = false;
                            }
                            else
                            {
                                args.Player.SendMessage("One Flag not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "apocalypse":
                        {
                            if (Apocalypse.Intermission || Apocalypse.Running)
                            {
                                C3Tools.BroadcastMessageToGametype("apoc", "Apocalypse stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Apocalypse Stopped", Color.DarkCyan);
                                Apocalypse.TpToSpawns(false);
                                C3Tools.ResetGameType("apoc");
                                Apocalypse.Running = false;
                                Apocalypse.Intermission = false;
                            }
                            else
                            {
                                args.Player.SendMessage("Apocalypse not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "tdm":
                        {
                            if (TDM.TDMCountdown || TDM.TDMRunning)
                            {
                                C3Tools.BroadcastMessageToGametype("tdm", "Team Deathmatch stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Team Deathmatch Stopped", Color.DarkCyan);
                                TDM.TpToSpawns(false);
                                C3Tools.ResetGameType("tdm");
                                TDM.TDMRunning = false;
                                TDM.TDMCountdown = false;
                            }
                            else
                            {
                                args.Player.SendMessage("Team Deathmatch not running", Color.DarkCyan);
                            }
                            break;
                        }
                }
            }
            else
                args.Player.SendMessage("Please enter a gametype", Color.DarkCyan);
        }

        public static void SetCTFRedFlag(CommandArgs args)
        {
            CTF.RedFlagSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("CTF Red flag set at your position (4x4 area)", Color.OrangeRed);
        }

        public static void SetCTFBlueFlag(CommandArgs args)
        {
            CTF.BlueFlagSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("CTF Blue flag set at your position (4x4 area)", Color.LightBlue);
        }

        public static void SetOneFlag(CommandArgs args)
        {
            OneFlagCTF.FlagSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("One Flag set at your position (4x4 area)", Color.Cyan);
        }

        public static void SetOneFlagBlueSpawn(CommandArgs args)
        {
            OneFlagCTF.BlueSpawnSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("One Flag CTF Blue spawn set at your position (4x4 area)", Color.LightBlue);
        }

        public static void SetOneFlagRedSpawn(CommandArgs args)
        {
            OneFlagCTF.RedSpawnSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("One Flag CTF Red spawn set at your position (4x4 area)", Color.OrangeRed);
        }

        public static void StartVote(CommandArgs args)
        {
            if (!C3Mod.VoteRunning)
            {
                if (args.Parameters.Count > 0)
                {
                    //Gametype Divider
                    ///CTF
                    if (args.Parameters[0].ToLower() == "ctf")
                    {
                        if (C3Mod.C3Config.CTFEnabled)
                        {
                            if (!CTF.CTFGameRunning && !CTF.CTFGameCountdown)
                            {
                                if (CTF.flagPoints[0] != Vector2.Zero && CTF.flagPoints[1] != Vector2.Zero)
                                {
                                    C3Tools.BroadcastMessageToGametype("", "Vote to play Capture the Flag started by: " + args.Player.Name, Color.Cyan);
                                    C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game!", Color.Cyan);
                                    CTF.BlueTeamScore = 0;
                                    CTF.RedTeamScore = 0;
                                    CTF.RedTeamFlagCarrier = null;
                                    CTF.BlueTeamFlagCarrier = null;
                                    C3Mod.VoteType = "ctf";
                                    CTF.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                    C3Mod.VoteRunning = true;
                                }
                                else
                                    args.Player.SendMessage("Flag points not set up yet", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Capture Flag game already running!", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("Capture Flag disabled on this server", Color.DarkCyan);
                    }
                    //Gametype Divider
                    ///One Flag
                    else if (args.Parameters[0].ToLower() == "oneflag")
                    {
                        if (C3Mod.C3Config.OneFlagEnabled)
                        {
                            if (!OneFlagCTF.OneFlagGameRunning && !OneFlagCTF.OneFlagGameCountdown)
                            {
                                if (OneFlagCTF.FlagPoint != Vector2.Zero && OneFlagCTF.SpawnPoint[0] != Vector2.Zero && OneFlagCTF.SpawnPoint[1] != Vector2.Zero)
                                {
                                    C3Tools.BroadcastMessageToGametype("", "Vote to play One Flag CTF started by: " + args.Player.Name, Color.Cyan);
                                    C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game!", Color.Cyan);
                                    OneFlagCTF.RedTeamScore = 0;
                                    OneFlagCTF.BlueTeamScore = 0;
                                    OneFlagCTF.FlagCarrier = null;
                                    C3Mod.VoteType = "oneflag";
                                    OneFlagCTF.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                    C3Mod.VoteRunning = true;
                                }
                                else
                                    args.Player.SendMessage("Points not set up yet", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("One Flag CTF game already running!", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("One Flag disabled on this server", Color.DarkCyan);
                    }
                    //Gametype Divider
                    ///TDM
                    else if (args.Parameters[0].ToLower() == "tdm")
                    {
                        if (C3Mod.C3Config.TeamDeathmatchEnabled)
                        {
                            if (!TDM.TDMRunning && !TDM.TDMCountdown)
                            {
                                if (TDM.TDMSpawns[0] != Vector2.Zero && TDM.TDMSpawns[1] != Vector2.Zero)
                                {
                                    C3Tools.BroadcastMessageToGametype("", "Vote to play Team Deathmatch started by: " + args.Player.Name, Color.Cyan);
                                    C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game!", Color.Cyan);
                                    TDM.BlueTeamScore = 0;
                                    TDM.RedTeamScore = 0;
                                    C3Mod.VoteType = "tdm";
                                    TDM.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                    C3Mod.VoteRunning = true;
                                }
                                else
                                    args.Player.SendMessage("Spawns not set up yet", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Team Deathmatch already running!", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("Team Deathmatch disabled on this server", Color.DarkCyan);
                    }
                    ///Gametype Divider
                    ///Apocalypse
                    else if (args.Parameters[0].ToLower() == "apocalypse")
                    {
                        if (C3Mod.C3Config.MonsterApocalypseEnabled)
                        {
                            if (!Apocalypse.Running && ! Apocalypse.Intermission)
                            {
                                if (Apocalypse.SpectatorArea != Vector2.Zero && Apocalypse.PlayerSpawn != Vector2.Zero && Apocalypse.MonsterSpawn != Vector2.Zero)
                                {
                                    C3Tools.BroadcastMessageToGametype("", "Vote to play Apocalypse started by: " + args.Player.Name, Color.Cyan);
                                    C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game!", Color.Cyan);
                                    Apocalypse.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                    C3Mod.VoteType = "apoc";
                                    Apocalypse.LastMonster = 0;
                                    Apocalypse.Wave = 0;
                                    C3Mod.VoteRunning = true;
                                }
                                else
                                    args.Player.SendMessage("Spawns not set up yet", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Apocalypse already running!", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("Apocalypse disabled on this server", Color.DarkCyan);
                    }
                    else
                        args.Player.SendMessage("Not an available gametype", Color.DarkCyan);
                }
                else
                    args.Player.SendMessage("Incorrect format: /vote <gametype>", Color.OrangeRed);
            }
            else
                args.Player.SendMessage("Vote already running!", Color.DarkCyan);
        }

        public static void JoinVote(CommandArgs args)
        {
            var player = C3Tools.GetPlayerByIndex(args.Player.Index);

            if (C3Mod.VoteRunning)
            {
                if (player.GameType == "")
                {
                    switch (C3Mod.VoteType)
                    {
                        case "ctf":
                            {
                                args.Player.SendMessage("You have joined the lobby for Capture the Flag", Color.Cyan);

                                string team = C3Tools.AssignTeam(C3Tools.GetPlayerByIndex(args.Player.Index), "ctf");

                                switch (team)
                                {
                                    case "Red":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Red team!", Color.OrangeRed);
                                            break;
                                        }
                                    case "Blue":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Blue team!", Color.LightBlue);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "oneflag":
                            {
                                args.Player.SendMessage("You have joined the lobby for One Flag CTF", Color.Cyan);

                                string team = C3Tools.AssignTeam(C3Tools.GetPlayerByIndex(args.Player.Index), "oneflag");

                                switch (team)
                                {
                                    case "Red":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Red team!", Color.OrangeRed);
                                            break;
                                        }
                                    case "Blue":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Blue team!", Color.LightBlue);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "tdm":
                            {
                                args.Player.SendMessage("You have joined the lobby for Team Deathmatch", Color.Cyan);

                                string team = C3Tools.AssignTeam(C3Tools.GetPlayerByIndex(args.Player.Index), "tdm");

                                switch (team)
                                {
                                    case "Red":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Red team!", Color.OrangeRed);
                                            break;
                                        }
                                    case "Blue":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Blue team!", Color.LightBlue);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "apoc":
                            {
                                args.Player.SendMessage("You have joined the lobby for Apocalypse...", Color.Cyan);
                                C3Tools.GetPlayerByIndex(args.Player.Index).GameType = "apoc";
                                break;
                            }
                    }
                }
                else
                    args.Player.SendMessage("You are already ingame!", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("No vote running!", Color.DarkCyan);
        }

        public static void SetCTFLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.CTFScoreLimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setctflimit <limit>", Color.OrangeRed);
        }

        public static void SetOneFlagLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.OneFlagScorelimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setoneflaglimit <limit>", Color.OrangeRed);
        }

        public static void SetDuelLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.DuelScoreLimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setoneflaglimit <limit>", Color.OrangeRed);
        }

        public static void SetTDMLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.TeamDeathmatchScorelimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setctflimit <limit>", Color.OrangeRed);
        }

        public static void CancelVote(CommandArgs args)
        {
            if (C3Mod.VoteRunning)
            {
                C3Mod.VoteRunning = false;
                C3Tools.BroadcastMessageToGametype("", args.Player.Name + " stopped the current vote!", Color.DarkCyan);

                switch (C3Mod.VoteType)
                {
                    case "ctf":
                        {
                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if(player.GameType == "ctf")
                                {
                                    player.GameType = "";
                                    player.Team = 0;
                                }
                            }
                            break;
                        }
                }
            }
            else
                args.Player.SendMessage("No vote running!", Color.DarkCyan);
        }

        public static void ChallengePlayer(CommandArgs args)
        {
            var player = C3Tools.GetPlayerByIndex(args.Player.Index);

            if (args.Parameters.Count > 0)
            {
                if (C3Mod.C3Config.DuelsEnabled)
                {
                    if (Duel.DuelSpawns[0].X != 0 && Duel.DuelSpawns[1].X != 0)
                    {
                        if (!Duel.DuelRunning && !Duel.DuelCountdown)
                        {
                            if (player.Challenging == null)
                            {
                                StringBuilder sb = new StringBuilder();
                                int count = 0;
                                foreach (string arg in args.Parameters)
                                {
                                    if (count != args.Parameters.Count - 1)
                                        sb.Append(arg + " ");
                                    else
                                        sb.Append(arg);

                                    count++;
                                }
                                C3Player challenging;

                                if ((challenging = C3Tools.GetPlayerByName(sb.ToString().ToLower())) != null)
                                {
                                    if (challenging.GameType == "")
                                    {
                                        player.Challenging = challenging;
                                        player.ChallengeNotifyCount = 5;
                                        args.Player.SendMessage("Challenge has been made to: " + challenging.PlayerName, Color.Cyan);
                                    }
                                    else
                                        args.Player.SendMessage(challenging.PlayerName + ": Is unavailable to be challenged at this time", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Could not find player with that name", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("You are already challenging someone!", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("A duel is already running. Try again later", Color.DarkCyan);
                    }
                    else
                        args.Player.SendMessage("Duel spawns have not been set yet", Color.DarkCyan);
                }
                else
                    args.Player.SendMessage("Dueling disabled on this server", Color.DarkCyan);
            }
        }

        public static void AcceptChallenge(CommandArgs args)
        {
            var player = C3Tools.GetPlayerByIndex(args.Player.Index);

            foreach (C3Player opponent in C3Mod.C3Players)
            {
                if (opponent.Challenging == player)
                {
                    if (!Duel.DuelRunning && !Duel.DuelCountdown)
                    {
                        Duel.RedPlayerScore = 0;
                        Duel.BluePlayerScore = 0;

                        Random r = new Random();
                        switch (r.Next(2) + 1)
                        {
                            case 1:
                                {
                                    Duel.RedPlayer = player;
                                    Duel.BluePlayer = opponent;
                                    player.Team = 3;
                                    player.GameType = "1v1";
                                    opponent.Team = 4;
                                    opponent.GameType = "1v1";
                                    opponent.Challenging = null;
                                    Duel.DuelCountdown = true;
                                    break;
                                }
                            case 2:
                                {
                                    Duel.RedPlayer = opponent;
                                    Duel.BluePlayer = player;
                                    player.Team = 4;
                                    player.GameType = "1v1";
                                    opponent.Team = 3;
                                    opponent.GameType = "1v1";
                                    opponent.Challenging = null;
                                    Duel.DuelCountdown = true;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        opponent.SendMessage("A Dual is already running, challenge cancelled", Color.DarkCyan);
                        opponent.Challenging.SendMessage("A Dual is already running, challenge cancelled", Color.DarkCyan);
                        opponent.Challenging = null;
                    }
                }
            }
        }

        public static void SetDuelRedSpawn(CommandArgs args)
        {
            Duel.DuelSpawns[0].X = args.Player.X / 16;
            Duel.DuelSpawns[0].Y = (args.Player.Y / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("RedSpawnX", (int)(args.Player.X / 16)));
            values.Add(new SqlValue("RedSpawnY", (int)(args.Player.Y / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("DuelSpawns", values, new List<SqlValue>());

            args.Player.SendMessage("Red spawn set at your position", Color.OrangeRed);
        }

        public static void SetDuelBlueSpawn(CommandArgs args)
        {
            Duel.DuelSpawns[1].X = args.Player.X / 16;
            Duel.DuelSpawns[1].Y = (args.Player.Y / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("BlueSpawnX", (int)(args.Player.X / 16)));
            values.Add(new SqlValue("BlueSpawnY", (int)(args.Player.Y / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("DuelSpawns", values, new List<SqlValue>());

            args.Player.SendMessage("Blue spawn set at your position", Color.LightBlue);
        }

        public static void SetTDMRedSpawn(CommandArgs args)
        {
            TDM.TDMSpawns[0].X = args.Player.X / 16;
            TDM.TDMSpawns[0].Y = (args.Player.Y / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("RedSpawnX", (int)(args.Player.X / 16)));
            values.Add(new SqlValue("RedSpawnY", (int)(args.Player.Y / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("TDMSpawns", values, new List<SqlValue>());

            args.Player.SendMessage("Red spawn set at your position", Color.OrangeRed);
        }

        public static void SetTDMBlueSpawn(CommandArgs args)
        {
            TDM.TDMSpawns[1].X = args.Player.X / 16;
            TDM.TDMSpawns[1].Y = (args.Player.Y / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("BlueSpawnX", (int)(args.Player.X / 16)));
            values.Add(new SqlValue("BlueSpawnY", (int)(args.Player.Y / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("TDMSpawns", values, new List<SqlValue>());

            args.Player.SendMessage("Blue spawn set at your position", Color.LightBlue);
        }

        public static void SetApocPlayerSpawn(CommandArgs args)
        {
            Apocalypse.SpawnSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("Player spawn set at your position", Color.OrangeRed);
        }

        public static void SetApocMonsterSpawn(CommandArgs args)
        {
            Apocalypse.MonsterSpawnSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("Monster spawn set at your position", Color.OrangeRed);
        }

        public static void SetApocSpectatorSpawn(CommandArgs args)
        {
            Apocalypse.SpectatorSpawnSet(args.Player.X, args.Player.Y);
            args.Player.SendMessage("Spectator spawn set at your position", Color.OrangeRed);
        }

        public static void Quit(CommandArgs args)
        {
            var player = C3Tools.GetPlayerByIndex(args.Player.Index);

            if (player.GameType != "")
            {
                switch (player.GameType)
                {
                    case "apoc":
                        {
                            player.SendMessage("Quit the Apocalypse - Wuss.", Color.Cyan);
                            break;
                        }
                    case "ctf":
                        {
                            player.SendMessage("Quit Capture the Flag", Color.Cyan);
                            break;
                        }
                    case "oneflag":
                        {
                            player.SendMessage("Quit One Flag", Color.Cyan);
                            break;
                        }
                    case "1v1":
                        {
                            player.SendMessage("Quit your Duel", Color.Cyan);
                            break;
                        }
                    case "tdm":
                        {
                            player.SendMessage("Quit Team Deathmatch", Color.Cyan);
                            break;
                        }
                }                
                player.GameType = "";
                player.TSPlayer.SetTeam(0);
                player.TSPlayer.SetPvP(false);
                player.LivesUsed = 0;
            }
            else
                player.SendMessage("You are not in a game!", Color.Cyan);
        }

        public static void Teams(CommandArgs args)
        {
            if (CTF.CTFGameRunning || CTF.CTFGameCountdown ||
                TDM.TDMRunning || TDM.TDMCountdown ||
                Duel.DuelRunning || Duel.DuelCountdown ||
                OneFlagCTF.OneFlagGameRunning || OneFlagCTF.OneFlagGameCountdown ||
                Apocalypse.Running)
            {
                string RedTeam = string.Empty;
                string BlueTeam = string.Empty;

                #region CTF
                if (CTF.CTFGameRunning || CTF.CTFGameCountdown)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.Team == 1)
                        {
                            RedTeam = string.Format("{0} {1}", RedTeam, player.PlayerName);
                        }
                        if (player.Team == 2)
                        {
                            BlueTeam = string.Format("{0} {1}", BlueTeam, player.PlayerName);
                        }
                    }
                    RedTeam = RedTeam.Remove(0, 1);
                    BlueTeam = BlueTeam.Remove(0, 1);
                    args.Player.SendMessage("CTF Game Started", Color.DarkCyan);
                    args.Player.SendMessage("Red Team: " + RedTeam, Color.OrangeRed);
                    args.Player.SendMessage("Blue Team: " + BlueTeam, Color.LightBlue);
                }
#endregion
                #region TDM
                if (TDM.TDMRunning || TDM.TDMCountdown)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.Team == 7)
                        {
                            RedTeam = string.Format("{0} {1}", RedTeam, player.PlayerName);
                        }
                        if (player.Team == 8)
                        {
                            BlueTeam = string.Format("{0} {1}", BlueTeam, player.PlayerName);
                        }
                    }
                    RedTeam = RedTeam.Remove(0, 1);
                    BlueTeam = BlueTeam.Remove(0, 1);
                    args.Player.SendMessage("TDM Game Started", Color.DarkCyan);
                    args.Player.SendMessage("Red Team: " + RedTeam, Color.OrangeRed);
                    args.Player.SendMessage("Blue Team: " + BlueTeam, Color.LightBlue);
                }
                #endregion
                #region Duel
                if (Duel.DuelRunning || Duel.DuelCountdown)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.Team == 3)
                        {
                            RedTeam = string.Format("{0} {1}", RedTeam, player.PlayerName);
                        }
                        if (player.Team == 4)
                        {
                            BlueTeam = string.Format("{0} {1}", BlueTeam, player.PlayerName);
                        }
                    }
                    RedTeam = RedTeam.Remove(0, 1);
                    BlueTeam = BlueTeam.Remove(0, 1);
                    args.Player.SendMessage("Duel Game Started", Color.DarkCyan);
                    args.Player.SendMessage("Red Team: " + RedTeam, Color.OrangeRed);
                    args.Player.SendMessage("Blue Team: " + BlueTeam, Color.LightBlue);
                }
                #endregion
                #region OneFlagCTF
                if (OneFlagCTF.OneFlagGameRunning || OneFlagCTF.OneFlagGameCountdown)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.Team == 5)
                        {
                            RedTeam = string.Format("{0} {1}", RedTeam, player.PlayerName);
                        }
                        if (player.Team == 6)
                        {
                            BlueTeam = string.Format("{0} {1}", BlueTeam, player.PlayerName);
                        }
                    }
                    RedTeam = RedTeam.Remove(0, 1);
                    BlueTeam = BlueTeam.Remove(0, 1);
                    args.Player.SendMessage("OneFlagCTF Game Started", Color.DarkCyan);
                    args.Player.SendMessage("Red Team: " + RedTeam, Color.OrangeRed);
                    args.Player.SendMessage("Blue Team: " + BlueTeam, Color.LightBlue);
                }
                #endregion
                #region Apocalypse
                if (Apocalypse.Running)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                            RedTeam = string.Format("{0} {1}", RedTeam, player.PlayerName);
                    }
                    RedTeam = RedTeam.Remove(0, 1);
                    args.Player.SendMessage("Apocalypse Game Started", Color.DarkCyan);
                    args.Player.SendMessage("Apocalypse Team: " + RedTeam, Color.OrangeRed);
                }
                #endregion
            }
            else
            {
                args.Player.SendMessage("No events started.", Color.Red);
            }
        }
    }
}
