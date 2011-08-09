﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrariaAPI;
using TShockAPI;
using TShockAPI.DB;
using Microsoft.Xna.Framework;
using Terraria;

namespace HousingDistricts
{
    public class HCommands
    {
        public static void House(CommandArgs args)
        {
            string cmd = "help";
            if (args.Parameters.Count > 0)
            {
                cmd = args.Parameters[0].ToLower();
            }
            switch (cmd)
            {
                case "set":
                    {
                        if (args.Parameters.Count == 2)
                        {
                            if (args.Parameters[1] == "1")
                            {
                                if (!args.Player.AwaitingTemp2)
                                {
                                    args.Player.SendMessage("Hit a block to Set Point 1", Color.Yellow);
                                    args.Player.AwaitingTemp1 = true;
                                }
                                else
                                    args.Player.SendMessage("Awaiting you to Set Point 2", Color.Yellow);
                            }
                            else if (args.Parameters[1] == "2")
                            {
                                if (args.Player.TempArea.X != 0)
                                {
                                    if (!args.Player.AwaitingTemp1)
                                    {
                                        args.Player.SendMessage("Hit a block to Set Point 2", Color.Yellow);
                                        args.Player.AwaitingTemp2 = true;
                                    }
                                    else
                                        args.Player.SendMessage("Awaiting you to Set Point 1", Color.Yellow);
                                }
                                else
                                {
                                    args.Player.SendMessage("You have not set Point 1 yet", Color.Red);
                                }
                            }
                            else
                                args.Player.SendMessage("Invalid syntax! Proper syntax: /house set [1/2]", Color.Red);
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /house set [1/2]", Color.Red);
                        break;
                    }
                case "add":
                    {
                        if (args.Parameters.Count > 1)
                        {
                            if (!args.Player.TempArea.IsEmpty)
                            {
                                string houseName = String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));


                                if (HouseTools.AddHouse(args.Player.TempArea.X, args.Player.TempArea.Y,
                                                            args.Player.TempArea.Width, args.Player.TempArea.Height, houseName, 0))
                                {
                                    args.Player.TempArea = Rectangle.Empty;
                                    args.Player.SendMessage("Set house " + houseName, Color.Yellow);
                                }
                                else
                                {
                                    args.Player.SendMessage("House " + houseName + " already exists", Color.Red);
                                }
                            }
                            else
                                args.Player.SendMessage("Points not set up yet", Color.Red);
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /house add [name]", Color.Red);
                        break;
                    }
                case "allow":
                    {
                        if (args.Parameters.Count > 2)
                        {
                            string playerName = args.Parameters[1];
                            string houseName = "";
                            User playerID;

                            for (int i = 2; i < args.Parameters.Count; i++)
                            {
                                if (houseName == "")
                                {
                                    houseName = args.Parameters[2];
                                }
                                else
                                {
                                    houseName = houseName + " " + args.Parameters[i];
                                }
                            }
                            if ((playerID = TShock.Users.GetUserByName(playerName)) != null)
                            {
                                if (HouseTools.AddNewUser(houseName, playerID.ID.ToString()))
                                {
                                    args.Player.SendMessage("Added user " + playerName + " to " + houseName, Color.Yellow);
                                }
                                else
                                    args.Player.SendMessage("House " + houseName + " not found", Color.Red);
                            }
                            else
                            {
                                args.Player.SendMessage("Player " + playerName + " not found", Color.Red);
                            }
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /house allow [name] [house]", Color.Red);
                        break;
                    }
                case "delete":
                    {
                        if (args.Parameters.Count > 1)
                        {
                            string houseName = "";

                            for (int i = 1; i < args.Parameters.Count; i++)
                            {
                                if (houseName == "")
                                {
                                    houseName = args.Parameters[1];
                                }
                                else
                                {
                                    houseName = houseName + " " + args.Parameters[i];
                                }
                            }
                            List<SqlValue> where = new List<SqlValue>();
                            where.Add(new SqlValue("Name", "'" + houseName + "'"));
                            HousingDistricts.SQLWriter.DeleteRow("HousingDistrict", where);

                            foreach (House house in HousingDistricts.Houses)
                            {
                                if (house.Name == houseName)
                                {
                                    HousingDistricts.Houses.Remove(house);
                                    break;
                                }
                            }
                            args.Player.SendMessage("House: " + houseName + " deleted", Color.Yellow);
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /house delete [house]", Color.Red);
                        break;
                    }
                case "clear":
                    {
                        args.Player.TempArea = Rectangle.Empty;
                        args.Player.SendMessage("Cleared temp area", Color.Yellow);
                        args.Player.AwaitingTemp1 = false;
                        args.Player.AwaitingTemp2 = false;
                        break;
                    }
                case "list":
                    {
                        //How many regions per page
                        const int pagelimit = 15;
                        //How many regions per line
                        const int perline = 5;
                        //Pages start at 0 but are displayed and parsed at 1
                        int page = 0;


                        if (args.Parameters.Count > 1)
                        {
                            if (!int.TryParse(args.Parameters[1], out page) || page < 1)
                            {
                                args.Player.SendMessage(string.Format("Invalid page number ({0})", page), Color.Red);
                                return;
                            }
                            page--; //Substract 1 as pages are parsed starting at 1 and not 0
                        }

                        List<House> houses = new List<House>();

                        foreach(House house in HousingDistricts.Houses)
                        {
                            if (house.WorldID == Main.worldID.ToString())
                            {
                                houses.Add(house);
                            }
                        }

                        // Are there even any houses to display?
                        if (houses.Count == 0)
                        {
                            args.Player.SendMessage("There are currently no houses defined.", Color.Red);
                            return;
                        }

                        //Check if they are trying to access a page that doesn't exist.
                        int pagecount = houses.Count / pagelimit;
                        if (page > pagecount)
                        {
                            args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                            return;
                        }

                        //Display the current page and the number of pages.
                        args.Player.SendMessage(string.Format("Current Houses ({0}/{1}):", page + 1, pagecount + 1), Color.Green);

                        //Add up to pagelimit names to a list
                        var nameslist = new List<string>();
                        for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < houses.Count; i++)
                        {
                            nameslist.Add(houses[i].Name);
                        }

                        //convert the list to an array for joining
                        var names = nameslist.ToArray();
                        for (int i = 0; i < names.Length; i += perline)
                        {
                            args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                        }

                        if (page < pagecount)
                        {
                            args.Player.SendMessage(string.Format("Type /house list {0} for more houses.", (page + 2)), Color.Yellow);
                        }
                        break;
                    }
            }
        }

        public static void TellAll(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                string text = "";
                foreach (string word in args.Parameters)
                {
                    text = text + " " + word;
                }

                if (args.Player.Group.HasPermission("adminchat") && !text.StartsWith("/") && TShock.Config.AdminChatEnabled)
                {
                    Tools.Broadcast(TShock.Config.AdminChatPrefix + "<" + args.Player.Name + ">" + text,
                                    args.Player.Group.R, args.Player.Group.G,
                                    args.Player.Group.B);
                    return;
                }

                Tools.Broadcast("{2}<{0}>{1}".SFormat(args.Player.Name, text, TShock.Config.ChatDisplayGroup ? "[{0}] ".SFormat(args.Player.Group.Name) : ""),
                                args.Player.Group.R, args.Player.Group.G,
                                args.Player.Group.B);
            }
        }

        public static void Convert(CommandArgs args)
        {
            List<SqlValue> value = new List<SqlValue>();
            value.Add(new SqlValue("WorldID", "'" + Main.worldID.ToString() + "'"));
            HousingDistricts.SQLEditor.UpdateValues("HousingDistrict", value, new List<SqlValue>());

            foreach (House house in HousingDistricts.Houses)
            {
                house.WorldID = Main.worldID.ToString();
            }
        }

        public static void ChangeLock(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                string houseName = "";

                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    if (houseName == "")
                    {
                        houseName = args.Parameters[i];
                    }
                    else
                    {
                        houseName = houseName + " " + args.Parameters[i];
                    }
                }

                bool locked = HouseTools.ChangeLock(houseName);

                if (locked)
                    args.Player.SendMessage("House: " + houseName + " locked", Color.Yellow);
                else
                    args.Player.SendMessage("House: " + houseName + " unlocked", Color.Yellow);
            }
            else
                args.Player.SendMessage("Invalid syntax! Proper syntax: /changelock [house]", Color.Red);
        }
    }
}
