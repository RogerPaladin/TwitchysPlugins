﻿/*   
TShock, a server mod for Terraria
Copyright (C) 2011 The TShock Team

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
using TerrariaAPI;
using TShockAPI.Net;
using XNAHelpers;

namespace HousingDistricts
{
    public delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);
    public class GetDataHandlerArgs : EventArgs
    {
        public TSPlayer Player { get; private set; }
        public MemoryStream Data { get; private set; }

        public Player TPlayer
        {
            get { return Player.TPlayer; }
        }

        public GetDataHandlerArgs(TSPlayer player, MemoryStream data)
        {
            Player = player;
            Data = data;
        }
    }
    public static class GetDataHandlers
    {
        private static Dictionary<PacketTypes, GetDataHandlerDelegate> GetDataHandlerDelegates;

        public static void InitGetDataHandler()
        {
            GetDataHandlerDelegates = new Dictionary<PacketTypes, GetDataHandlerDelegate>
            {
                {PacketTypes.Tile, HandleTile},
                {PacketTypes.TileSendSquare, HandleSendTileSquare},
                {PacketTypes.TileKill, HandleTileKill},
                {PacketTypes.LiquidSet, HandleLiquidSet},
            };
        }

        public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data)
        {
            GetDataHandlerDelegate handler;
            if (GetDataHandlerDelegates.TryGetValue(type, out handler))
            {
                try
                {
                    return handler(new GetDataHandlerArgs(player, data));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            return false;
        }

        private static bool HandleSendTileSquare(GetDataHandlerArgs args)
        {
            short size = args.Data.ReadInt16();
            int tilex = args.Data.ReadInt32();
            int tiley = args.Data.ReadInt32();

            if (!args.Player.Group.HasPermission("edithouse"))
            {
                lock (HousingDistricts.HPlayers)
                {
                    foreach (House house in HousingDistricts.Houses)
                    {
                        if (house.HouseArea.Intersects(new Rectangle(tilex, tiley, 1, 1)) && house.WorldID == Main.worldID.ToString())
                        {
                            if (!HTools.OwnsHouse(args.Player.UserID.ToString(), house.Name))
                            {
                                args.Player.SendTileSquare(tilex, tiley);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool HandleTile(GetDataHandlerArgs args)
        {
            byte type = args.Data.ReadInt8();
            int x = args.Data.ReadInt32();
            int y = args.Data.ReadInt32();
            byte tiletype = args.Data.ReadInt8();

            int tilex = Math.Abs(x);
            int tiley = Math.Abs(y);

            if (args.Player.AwaitingTemp1)
            {
                args.Player.TempArea.X = tilex;
                args.Player.TempArea.Y = tiley;
                args.Player.SendMessage("Set Temp Point 1", Color.Yellow);
                args.Player.SendTileSquare(tilex, tiley);
                args.Player.AwaitingTemp1 = false;
                return true;
            }

            if (args.Player.AwaitingTemp2)
            {
                if (tilex > args.Player.TempArea.X && tiley > args.Player.TempArea.Y)
                {
                    args.Player.TempArea.Width = tilex - args.Player.TempArea.X;
                    args.Player.TempArea.Height = tiley - args.Player.TempArea.Y;
                    args.Player.SendMessage("Set Temp Point 2", Color.Yellow);
                    args.Player.SendTileSquare(tilex, tiley);
                    args.Player.AwaitingTemp2 = false;
                }
                else
                {
                    args.Player.SendMessage("Point 2 must be below and right of Point 1", Color.Yellow);
                    args.Player.SendMessage("Use /region clear to start again", Color.Yellow);
                    args.Player.SendTileSquare(tilex, tiley);
                    args.Player.AwaitingTemp2 = false;
                }
                return true;
            }

            if (!args.Player.Group.HasPermission("edithouse"))
            {
                lock (HousingDistricts.HPlayers)
                {
                    foreach (House house in HousingDistricts.Houses)
                    {
                        if (house.HouseArea.Intersects(new Rectangle(tilex, tiley, 1, 1)) && house.WorldID == Main.worldID.ToString())
                        {
                            if (!HTools.OwnsHouse(args.Player.UserID.ToString(), house.Name))
                            {
                                args.Player.SendTileSquare(tilex, tiley);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool HandleLiquidSet(GetDataHandlerArgs args)
        {
            int x = args.Data.ReadInt32();
            int y = args.Data.ReadInt32();
            int plyX = Math.Abs(args.Player.TileX);
            int plyY = Math.Abs(args.Player.TileY);
            int tilex = Math.Abs(x);
            int tiley = Math.Abs(y);

            if (!args.Player.Group.HasPermission("edithouse"))
            {
                lock (HousingDistricts.HPlayers)
                {
                    foreach (House house in HousingDistricts.Houses)
                    {
                        if (house.HouseArea.Intersects(new Rectangle(tilex, tiley, 1, 1)) && house.WorldID == Main.worldID.ToString())
                        {
                            if (!HTools.OwnsHouse(args.Player.UserID.ToString(), house.Name))
                            {
                                args.Player.SendTileSquare(tilex, tiley);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool HandleTileKill(GetDataHandlerArgs args)
        {
            int x = args.Data.ReadInt32();
            int y = args.Data.ReadInt32();
            int tilex = Math.Abs(x);
            int tiley = Math.Abs(y);

            if (args.Player.AwaitingTemp1)
            {
                args.Player.TempArea.X = tilex;
                args.Player.TempArea.Y = tiley;
                args.Player.SendMessage("Set Temp Point 1", Color.Yellow);
                args.Player.SendTileSquare(tilex, tiley);
                args.Player.AwaitingTemp1 = false;
                return true;
            }

            if (args.Player.AwaitingTemp2)
            {
                if (tilex > args.Player.TempArea.X && tiley > args.Player.TempArea.Y)
                {
                    args.Player.TempArea.Width = tilex - args.Player.TempArea.X;
                    args.Player.TempArea.Height = tiley - args.Player.TempArea.Y;
                    args.Player.SendMessage("Set Temp Point 2", Color.Yellow);
                    args.Player.SendTileSquare(tilex, tiley);
                    args.Player.AwaitingTemp2 = false;
                }
                else
                {
                    args.Player.SendMessage("Point 2 must be below and right of Point 1", Color.Yellow);
                    args.Player.SendMessage("Use /region clear to start again", Color.Yellow);
                    args.Player.SendTileSquare(tilex, tiley);
                    args.Player.AwaitingTemp2 = false;
                }
                return true;
            }

            if (!args.Player.Group.HasPermission("edithouse"))
            {
                lock (HousingDistricts.HPlayers)
                {
                    foreach (House house in HousingDistricts.Houses)
                    {
                        if (house.HouseArea.Intersects(new Rectangle(tilex, tiley, 1, 1)) && house.WorldID == Main.worldID.ToString())
                        {
                            if (!HTools.OwnsHouse(args.Player.UserID.ToString(), house.Name))
                            {
                                args.Player.SendTileSquare(tilex, tiley);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
