using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using TShockAPI;
using Terraria;
using C3Mod.GameTypes;

namespace C3Mod
{
    public class C3Tools
    {
        internal static string C3ConfigPath { get { return Path.Combine(TShock.SavePath, "c3modconfig.json"); } }

        public static void SetupConfig()
        {
            try
            {
                if (File.Exists(C3ConfigPath))
                {
                    C3Mod.C3Config = C3ConfigFile.Read(C3ConfigPath);
                    // Add all the missing config properties in the json file
                }
                C3Mod.C3Config.Write(C3ConfigPath);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in config file");
                Console.ForegroundColor = ConsoleColor.Gray;
                Log.Error("Config Exception");
                Log.Error(ex.ToString());
            }
        }

        public static string AssignTeam(C3Player who, string gametype)
        {
            switch (gametype)
            {
                case "ctf":
                    {
                        if (who.Team != 1 || who.Team != 2)
                        {
                            int redteamplayers = 0;
                            int blueteamplayers = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if (player.Team == 1)
                                    redteamplayers++;
                                else if (player.Team == 2)
                                    blueteamplayers++;
                            }

                            if (redteamplayers > blueteamplayers)
                            {
                                who.Team = 2;
                                who.GameType = "ctf";
                                return "Blue";
                            }
                            else if (blueteamplayers > redteamplayers)
                            {
                                who.Team = 1;
                                who.GameType = "ctf";
                                return "Red";
                            }

                            else
                            {
                                Random r = new Random();

                                switch (r.Next(2) + 1)
                                {
                                    case 1:
                                        {
                                            who.Team = 1;
                                            who.GameType = "ctf";
                                            return "Red";
                                        }
                                    case 2:
                                        {
                                            who.Team = 2;
                                            who.GameType = "ctf";
                                            return "Blue";
                                        }
                                }
                            }
                        }
                        break;
                    }
                case "oneflag":
                    {
                        if (who.Team != 5 || who.Team != 6)
                        {
                            int redteamplayers = 0;
                            int blueteamplayers = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if (player.Team == 5)
                                    redteamplayers++;
                                else if (player.Team == 6)
                                    blueteamplayers++;
                            }

                            if (redteamplayers > blueteamplayers)
                            {
                                who.Team = 6;
                                who.GameType = "oneflag";
                                return "Blue";
                            }
                            else if (blueteamplayers > redteamplayers)
                            {
                                who.Team = 5;
                                who.GameType = "oneflag";
                                return "Red";
                            }

                            else
                            {
                                Random r = new Random();

                                switch (r.Next(2) + 1)
                                {
                                    case 1:
                                        {
                                            who.Team = 5;
                                            who.GameType = "oneflag";
                                            return "Red";
                                        }
                                    case 2:
                                        {
                                            who.Team = 6;
                                            who.GameType = "oneflag";
                                            return "Blue";
                                        }
                                }
                            }
                        }
                        break;
                    }
                case "tdm":
                    {
                        if (who.Team != 7 || who.Team != 8)
                        {
                            int redteamplayers = 0;
                            int blueteamplayers = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if (player.Team == 7)
                                    redteamplayers++;
                                else if (player.Team == 8)
                                    blueteamplayers++;
                            }

                            if (redteamplayers > blueteamplayers)
                            {
                                who.Team = 8;
                                who.GameType = "tdm";
                                return "Blue";
                            }
                            else if (blueteamplayers > redteamplayers)
                            {
                                who.Team = 7;
                                who.GameType = "tdm";
                                return "Red";
                            }

                            else
                            {
                                Random r = new Random();

                                switch (r.Next(2) + 1)
                                {
                                    case 1:
                                        {
                                            who.Team = 7;
                                            who.GameType = "tdm";
                                            return "Red";
                                        }
                                    case 2:
                                        {
                                            who.Team = 8;
                                            who.GameType = "tdm";
                                            return "Blue";
                                        }
                                }
                            }
                        }
                        break;
                    }
            }
            return "";
        }

        public static void BroadcastMessageToGametype(string gametype, string message, Color color)
        {
            foreach (C3Player player in C3Mod.C3Players)
                if (player.GameType == gametype)
                    player.SendMessage(message, color);
        }

        public static C3Player GetPlayerByIndex(int index)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.Index == index)
                    return player;
            }
            return new C3Player(-1);
        }

        public static C3Player GetPlayerByName(string name)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.PlayerName.ToLower() == name)
                    return player;
            }
            return null;
        }

        public static TSPlayer GetTSPlayerByIndex(int index)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Index == index)
                    return player;
            }
            return null;
        }

        public static NPC GetNPCByIndex(int index)
        {
            foreach (NPC npc in Main.npc)
            {
                if (npc.whoAmI == index)
                    return npc;
            }
            return new NPC();
        }

        public static void ResetGameType(string gametype)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.GameType == gametype)
                {
                    player.GameType = "";
                    player.Team = 0;
                }
            }
        }
    }
}
