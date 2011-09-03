using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using TShockAPI.DB;
using MySql.Data.MySqlClient;
using System.Threading;
using System.ComponentModel;
using C3Mod.GameTypes;
using System.IO;

namespace C3Mod
{
    [APIVersion(1, 7)]
    public class C3Mod : TerrariaPlugin
    {
        public static C3ConfigFile C3Config { get; set; }
        public static bool VoteRunning { get; set; }
        public static string VoteType { get; set; }
        public static List<C3Player> C3Players = new List<C3Player>();
        public static SqlTableEditor SQLEditor;
        public static SqlTableCreator SQLWriter;
        public static readonly Version VersionNum = Assembly.GetExecutingAssembly().GetName().Version;

        public override string Name
        {
            get { return "C3Mod"; }
        }
        public override string Author
        {
            get { return "Created by Twitchy. Modded by RogerPaladin."; }
        }
        public override string Description
        {
            get { return "The Great PvP War"; }
        }
        public override Version Version
        {
            get { return VersionNum; }
        }

        public override void Initialize()
        {
            C3Tools.SetupConfig();

            GameHooks.Update += CTF.OnUpdate;
            GameHooks.Update += Duel.OnUpdate;
            GameHooks.Update += OneFlagCTF.OnUpdate;
            GameHooks.Update += TDM.OnUpdate;
            GameHooks.Update += Apocalypse.OnUpdate;
            GameHooks.Update += OnUpdate;
            GameHooks.Initialize += OnInitialize;
            NetHooks.GreetPlayer += OnGreetPlayer;
            ServerHooks.Leave += OnLeave;
        }
        public override void DeInitialize()
        {
            GameHooks.Update -= CTF.OnUpdate;
            GameHooks.Update -= Duel.OnUpdate;
            GameHooks.Update -= OneFlagCTF.OnUpdate;
            GameHooks.Update -= TDM.OnUpdate;
            GameHooks.Update -= Apocalypse.OnUpdate;
            GameHooks.Update -= OnUpdate;
            GameHooks.Initialize -= OnInitialize;
            NetHooks.GreetPlayer -= OnGreetPlayer;
            ServerHooks.Leave -= OnLeave;
        }
        public C3Mod(Main game)
            : base(game)
        {
            C3Config = new C3ConfigFile();
            Order = -1;
        }

        public void OnInitialize()
        {
            SQLEditor = new SqlTableEditor(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            ApocalypseMonsters.AddNPCs();

            #region TestPerms
            //Checks to see if permissions have been moved around
            bool vote = false;
            bool joinvote = false;
            bool setflags = false;
            bool setspawns = false;
            bool managec3settings = false;
            bool cvote = false;
            bool duel = false;
            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("vote"))
                        vote = true;
                    if (group.HasPermission("joinvote"))
                        joinvote = true;
                    if (group.HasPermission("setflags"))
                        setflags = true;
                    if (group.HasPermission("managec3settings"))
                        managec3settings = true;
                    if (group.HasPermission("cvote"))
                        cvote = true;
                    if (group.HasPermission("duel"))
                        duel = true;
                    if (group.HasPermission("setspawns"))
                        setspawns = true;
                }
            }
            List<string> perm = new List<string>();
            if (!vote)
                perm.Add("vote");
            if (!joinvote)
                perm.Add("joinvote");
            if (!duel)
                perm.Add("duel");
            TShock.Groups.AddPermissions("default", perm);

            perm.Clear();
            if (!setflags)
                perm.Add("setflags");
            if (!setflags)
                perm.Add("setspawns");
            if (!managec3settings)
                perm.Add("managec3settings");
            if (!cvote)
                perm.Add("cvote");
            if (!setspawns)
                perm.Add("setspawns");
            TShock.Groups.AddPermissions("trustedadmin", perm);
            #endregion

            #region AddCommands
            //Adds C3Commands
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFRedFlag, "setctfredflag"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFBlueFlag, "setctfblueflag"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlag, "setoneflag"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagRedSpawn, "setoneflagredspawn"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagBlueSpawn, "setoneflagbluespawn"));
            Commands.ChatCommands.Add(new Command("vote", C3Commands.StartVote, "vote"));
            Commands.ChatCommands.Add(new Command("joinvote", C3Commands.JoinVote, "join"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetCTFLimit, "setctflimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetOneFlagLimit, "setoneflaglimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetDuelLimit, "setduellimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetTDMLimit, "settdmlimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.Stop, "stop"));
            Commands.ChatCommands.Add(new Command("cvote", C3Commands.CancelVote, "cvote"));
            Commands.ChatCommands.Add(new Command("duel", C3Commands.ChallengePlayer, "duel"));
            Commands.ChatCommands.Add(new Command(C3Commands.AcceptChallenge, "accept"));
            Commands.ChatCommands.Add(new Command(C3Commands.Quit, "quit"));
            Commands.ChatCommands.Add(new Command(C3Commands.Teams, "teams"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelBlueSpawn, "setduelbluespawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelRedSpawn, "setduelredspawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMBlueSpawn, "settdmbluespawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMRedSpawn, "settdmredspawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetApocPlayerSpawn, "setapocplayerspawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetApocMonsterSpawn, "setapocmonsterspawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetApocSpectatorSpawn, "setapocspectatorspawn"));
            #endregion

            #region FlagPoints

            var table = new SqlTable("FlagPoints",
                new SqlColumn("RedX", MySqlDbType.Int32),
                new SqlColumn("RedY", MySqlDbType.Int32),
                new SqlColumn("BlueX", MySqlDbType.Int32),
                new SqlColumn("BlueY", MySqlDbType.Int32)
            );            
            SQLWriter.EnsureExists(table);

            if (SQLEditor.ReadColumn("FlagPoints", "RedX", new List<SqlValue>()).Count == 0)
            {
                List<SqlValue> list = new List<SqlValue>();
                list.Add(new SqlValue("RedX", 0));
                list.Add(new SqlValue("RedY", 0));
                list.Add(new SqlValue("BlueX", 0));
                list.Add(new SqlValue("BlueY", 0));
                SQLEditor.InsertValues("FlagPoints", list);
            }
            else
            {
                CTF.flagPoints[0].X = float.Parse(SQLEditor.ReadColumn("FlagPoints", "RedX", new List<SqlValue>())[0].ToString());
                CTF.flagPoints[0].Y = float.Parse(SQLEditor.ReadColumn("FlagPoints", "RedY", new List<SqlValue>())[0].ToString());
                CTF.flagPoints[1].X = float.Parse(SQLEditor.ReadColumn("FlagPoints", "BlueX", new List<SqlValue>())[0].ToString());
                CTF.flagPoints[1].Y = float.Parse(SQLEditor.ReadColumn("FlagPoints", "BlueY", new List<SqlValue>())[0].ToString());
            }

            #endregion

            #region DuelSpawns

            table = new SqlTable("DuelSpawns",
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter.EnsureExists(table);

            if (SQLEditor.ReadColumn("DuelSpawns", "RedSpawnX", new List<SqlValue>()).Count == 0)
            {
                List<SqlValue> list = new List<SqlValue>();
                list.Add(new SqlValue("RedSpawnX", 0));
                list.Add(new SqlValue("RedSpawnY", 0));
                list.Add(new SqlValue("BlueSpawnX", 0));
                list.Add(new SqlValue("BlueSpawnY", 0));
                SQLEditor.InsertValues("DuelSpawns", list);
            }
            else
            {
                Duel.DuelSpawns[0].X = float.Parse(SQLEditor.ReadColumn("DuelSpawns", "RedSpawnX", new List<SqlValue>())[0].ToString());
                Duel.DuelSpawns[0].Y = float.Parse(SQLEditor.ReadColumn("DuelSpawns", "RedSpawnY", new List<SqlValue>())[0].ToString());
                Duel.DuelSpawns[1].X = float.Parse(SQLEditor.ReadColumn("DuelSpawns", "BlueSpawnX", new List<SqlValue>())[0].ToString());
                Duel.DuelSpawns[1].Y = float.Parse(SQLEditor.ReadColumn("DuelSpawns", "BlueSpawnY", new List<SqlValue>())[0].ToString());
            }

            #endregion

            #region OneFlagPoints

            table = new SqlTable("OneFlagPoints",
                new SqlColumn("FlagX", MySqlDbType.Int32),
                new SqlColumn("FlagY", MySqlDbType.Int32),
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),                
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter.EnsureExists(table);

            if (SQLEditor.ReadColumn("OneFlagPoints", "RedSpawnX", new List<SqlValue>()).Count == 0)
            {
                List<SqlValue> list = new List<SqlValue>();
                list.Add(new SqlValue("FlagX", 0));
                list.Add(new SqlValue("FlagY", 0));                
                list.Add(new SqlValue("RedSpawnX",0));
                list.Add(new SqlValue("RedSpawnY", 0)); 
                list.Add(new SqlValue("BlueSpawnX", 0));
                list.Add(new SqlValue("BlueSpawnY", 0));
                SQLEditor.InsertValues("OneFlagPoints", list);
            }
            else
            {
                OneFlagCTF.FlagPoint.X = float.Parse(SQLEditor.ReadColumn("OneFlagPoints", "FlagX", new List<SqlValue>())[0].ToString());
                OneFlagCTF.FlagPoint.Y = float.Parse(SQLEditor.ReadColumn("OneFlagPoints", "FlagY", new List<SqlValue>())[0].ToString());
                OneFlagCTF.SpawnPoint[0].X = float.Parse(SQLEditor.ReadColumn("OneFlagPoints", "RedSpawnX", new List<SqlValue>())[0].ToString());
                OneFlagCTF.SpawnPoint[0].Y = float.Parse(SQLEditor.ReadColumn("OneFlagPoints", "RedSpawnY", new List<SqlValue>())[0].ToString());
                OneFlagCTF.SpawnPoint[1].X = float.Parse(SQLEditor.ReadColumn("OneFlagPoints", "BlueSpawnX", new List<SqlValue>())[0].ToString());
                OneFlagCTF.SpawnPoint[1].Y = float.Parse(SQLEditor.ReadColumn("OneFlagPoints", "BlueSpawnY", new List<SqlValue>())[0].ToString());
            }

            #endregion

            #region TDMSpawns

            table = new SqlTable("TDMSpawns",
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter.EnsureExists(table);

            if (SQLEditor.ReadColumn("TDMSpawns", "RedSpawnX", new List<SqlValue>()).Count == 0)
            {
                List<SqlValue> list = new List<SqlValue>();               
                list.Add(new SqlValue("RedSpawnX",0));
                list.Add(new SqlValue("RedSpawnY", 0)); 
                list.Add(new SqlValue("BlueSpawnX", 0));
                list.Add(new SqlValue("BlueSpawnY", 0));
                SQLEditor.InsertValues("TDMSpawns", list);
            }
            else
            {
                TDM.TDMSpawns[0].X = float.Parse(SQLEditor.ReadColumn("TDMSpawns", "RedSpawnX", new List<SqlValue>())[0].ToString());
                TDM.TDMSpawns[0].Y = float.Parse(SQLEditor.ReadColumn("TDMSpawns", "RedSpawnY", new List<SqlValue>())[0].ToString());
                TDM.TDMSpawns[1].X = float.Parse(SQLEditor.ReadColumn("TDMSpawns", "BlueSpawnX", new List<SqlValue>())[0].ToString());
                TDM.TDMSpawns[1].Y = float.Parse(SQLEditor.ReadColumn("TDMSpawns", "BlueSpawnY", new List<SqlValue>())[0].ToString());
            }

            #endregion

            #region Apocalypse

            table = new SqlTable("Apocalypse",
                new SqlColumn("SpawnX", MySqlDbType.Int32),
                new SqlColumn("SpawnY", MySqlDbType.Int32),
                new SqlColumn("MonsterSpawnX", MySqlDbType.Int32),
                new SqlColumn("MonsterSpawnY", MySqlDbType.Int32),
                new SqlColumn("SpectatorSpawnX", MySqlDbType.Int32),
                new SqlColumn("SpectatorSpawnY", MySqlDbType.Int32)
            );
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter.EnsureExists(table);

            if (SQLEditor.ReadColumn("Apocalypse", "SpawnX", new List<SqlValue>()).Count == 0)
            {
                List<SqlValue> list = new List<SqlValue>();
                list.Add(new SqlValue("SpawnX", 0));
                list.Add(new SqlValue("SpawnY", 0));
                list.Add(new SqlValue("MonsterSpawnX", 0));
                list.Add(new SqlValue("MonsterSpawnY", 0));
                list.Add(new SqlValue("SpectatorSpawnX", 0));
                list.Add(new SqlValue("SpectatorSpawnY", 0));
                SQLEditor.InsertValues("Apocalypse", list);
            }
            else
            {
                Apocalypse.PlayerSpawn.X = float.Parse(SQLEditor.ReadColumn("Apocalypse", "SpawnX", new List<SqlValue>())[0].ToString());
                Apocalypse.PlayerSpawn.Y = float.Parse(SQLEditor.ReadColumn("Apocalypse", "SpawnY", new List<SqlValue>())[0].ToString());
                Apocalypse.MonsterSpawn.X = float.Parse(SQLEditor.ReadColumn("Apocalypse", "MonsterSpawnX", new List<SqlValue>())[0].ToString());
                Apocalypse.MonsterSpawn.Y = float.Parse(SQLEditor.ReadColumn("Apocalypse", "MonsterSpawnY", new List<SqlValue>())[0].ToString());
                Apocalypse.SpectatorArea.X = float.Parse(SQLEditor.ReadColumn("Apocalypse", "SpectatorSpawnX", new List<SqlValue>())[0].ToString());
                Apocalypse.SpectatorArea.Y = float.Parse(SQLEditor.ReadColumn("Apocalypse", "SpectatorSpawnY", new List<SqlValue>())[0].ToString());
            }

            #endregion
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            lock (C3Players)
                C3Players.Add(new C3Player(who));
            TShock.Players[who].SendMessage("This server is running C3Mod, created by Twitchy. Modded By RogerPaladin.", Color.Cyan);
        }

        public void OnUpdate(GameTime gametime)
        {
            if (C3Config.RedAndBlueTeamsLocked)
            {
                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.GameType == "")
                    {
                        if (Main.player[player.Index].team == 1 || Main.player[player.Index].team == 3)
                        {
                            player.SendMessage("This team is reserved for C3Mod, switching you to neutral", Color.DarkCyan);
                            TShock.Players[player.Index].SetTeam(0);
                        }
                    }
                }
            }
        }

        public void OnLeave(int ply)
        {
            C3Tools.RemovePlayer(ply);
        }
    }
}