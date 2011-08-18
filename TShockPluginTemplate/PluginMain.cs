using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;

namespace PluginTemplate
{
    [APIVersion(1, 7)]
    public class PluginTemplate : TerrariaPlugin
    {
        public override string Name
        {
            get { return "PluginTemplate"; }
        }
        public override string Author
        {
            get { return "Created by Twitchy"; }
        }
        public override string Description
        {
            get { return ""; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override void Initialize()
        {
            GameHooks.Update += OnUpdate;
            GameHooks.Initialize += OnInitialize;
            NetHooks.GreetPlayer += OnGreetPlayer;
            ServerHooks.Leave += OnLeave;
            ServerHooks.Chat += OnChat;
        }
        public override void DeInitialize()
        {
            GameHooks.Update -= OnUpdate;
            GameHooks.Initialize -= OnInitialize;
            NetHooks.GreetPlayer -= OnGreetPlayer;
            ServerHooks.Leave -= OnLeave;
            ServerHooks.Chat -= OnChat;
        }
        public PluginTemplate(Main game)
            : base(game)
        {
        }

        public void OnInitialize()
        {
        }

        public void OnUpdate(GameTime gametime)
        {
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
        }

        public void OnLeave(int ply)
        {
        }

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
        }
    }
}