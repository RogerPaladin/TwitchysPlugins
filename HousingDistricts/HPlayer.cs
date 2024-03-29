﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Microsoft.Xna.Framework;

namespace HousingDistricts
{
    public class HPlayer
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
        public string CurHouse { get; set; }
        public bool InHouse { get; set; }
        public Vector2 LastTilePos { get; set; }

        public HPlayer(int index, Vector2 lasttilepos)
        {
            Index = index;
            InHouse = false;
            CurHouse = "";
            LastTilePos = lasttilepos;
        }
    }
}
