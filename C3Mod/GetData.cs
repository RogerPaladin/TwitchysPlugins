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
using C3Mod.GameTypes;
using XNAHelpers;

namespace C3Mod
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
                {PacketTypes.PlayerKillMe, HandlePlayerKillMe},
                {PacketTypes.PlayerUpdate, HandlePlayerUpdate},
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

        private static bool HandlePlayerKillMe(GetDataHandlerArgs args)
        {
            byte id = args.Data.ReadInt8();
            if (id != args.Player.Index)
            {
                return Tools.HandleGriefer(args.Player, TShock.Config.KillMeAbuseReason);
            }
            else
            {

            }
            return false;
        }

        private static bool HandlePlayerUpdate(GetDataHandlerArgs args)
        {
            byte plr = args.Data.ReadInt8();
            byte control = args.Data.ReadInt8();
            byte item = args.Data.ReadInt8();
            float posx = args.Data.ReadSingle();
            float posy = args.Data.ReadSingle();
            float velx = args.Data.ReadSingle();
            float vely = args.Data.ReadSingle();

            if (plr != args.Player.Index)
            {
                return Tools.HandleGriefer(args.Player, TShock.Config.UpdatePlayerAbuseReason);
            }

            #region CTF
            var player = C3Tools.GetPlayerByIndex(plr);

            if (!player.TerrariaDead)
            {
                if (player.Team == 1 && CTF.RedTeamFlagCarrier == null)
                {
                    if ((int)player.tileX >= CTF.flagPoints[1].X - 2 && (int)player.tileX <= CTF.flagPoints[1].X + 2 && (int)player.tileY == (int)(CTF.flagPoints[1].Y - 3))
                    {
                        CTF.RedTeamFlagCarrier = player;
                        C3Tools.BroadcastMessageToGametype("ctf", player.PlayerName + " has the flag!", Color.OrangeRed);
                    }
                }
                if (player.Team == 2 && CTF.BlueTeamFlagCarrier == null)
                {
                    if ((int)player.tileX >= CTF.flagPoints[0].X - 2 && (int)player.tileX <= CTF.flagPoints[0].X + 2 && (int)player.tileY == (int)(CTF.flagPoints[0].Y - 3))
                    {
                        CTF.BlueTeamFlagCarrier = player;
                        C3Tools.BroadcastMessageToGametype("ctf", player.PlayerName + " has the flag!", Color.LightBlue);
                    }
                }
            }
            if (player == CTF.RedTeamFlagCarrier)
            {
                if ((int)player.tileX >= CTF.flagPoints[0].X - 2 && (int)player.tileX <= CTF.flagPoints[0].X + 2 && (int)player.tileY == (int)(CTF.flagPoints[0].Y - 3))
                {
                    CTF.RedTeamScore++;
                    C3Tools.BroadcastMessageToGametype("ctf", CTF.RedTeamFlagCarrier.PlayerName + ": Scores!  Red - " + CTF.RedTeamScore.ToString() + " --- " + CTF.BlueTeamScore.ToString() + " - Blue", Color.OrangeRed);
                    CTF.RedTeamFlagCarrier = null;
                    CTF.BlueTeamFlagCarrier = null;

                    if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && CTF.RedTeamScore != C3Mod.C3Config.CTFScoreLimit)
                        CTF.TpToFlag();

                    if (C3Mod.C3Config.ReCountdownOnFlagCapture && CTF.RedTeamScore != C3Mod.C3Config.CTFScoreLimit)
                    {
                        CTF.CTFGameRunning = false;
                        CTF.CTFGameCountdown = true;
                    }

                    if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                    {
                        Item heart = Tools.GetItemById(58);
                        Item star = Tools.GetItemById(184);

                        foreach (C3Player playerh in C3Mod.C3Players)
                        {
                            if (playerh.GameType == "ctf")
                            {
                                playerh.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                playerh.GiveItem(star.type, star.name, star.width, star.height, 20);
                            }
                        }
                    }
                }
            }
            else if (player == CTF.BlueTeamFlagCarrier)
            {
                if ((int)player.tileX >= CTF.flagPoints[1].X - 2 && (int)player.tileX <= CTF.flagPoints[1].X + 2 && (int)player.tileY == (int)(CTF.flagPoints[1].Y - 3))
                {
                    CTF.BlueTeamScore++;
                    C3Tools.BroadcastMessageToGametype("ctf", player.PlayerName + ": Scores!  Blue - " + CTF.BlueTeamScore.ToString() + " --- " + CTF.RedTeamScore.ToString() + " - Red", Color.LightBlue);
                    CTF.RedTeamFlagCarrier = null;
                    CTF.BlueTeamFlagCarrier = null;

                    if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && CTF.BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                        CTF.TpToFlag();

                    if (C3Mod.C3Config.ReCountdownOnFlagCapture && CTF.BlueTeamScore != C3Mod.C3Config.CTFScoreLimit)
                    {
                        CTF.CTFGameRunning = false;
                        CTF.CTFGameCountdown = true;
                    }

                    if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                    {
                        Item heart = Tools.GetItemById(58);
                        Item star = Tools.GetItemById(184);

                        foreach (C3Player playerh in C3Mod.C3Players)
                        {
                            if (playerh.GameType == "ctf")
                            {
                                playerh.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                playerh.GiveItem(star.type, star.name, star.width, star.height, 20);
                            }
                        }
                    }
                }
            }
            #endregion

            return false;
        }
    }
}
