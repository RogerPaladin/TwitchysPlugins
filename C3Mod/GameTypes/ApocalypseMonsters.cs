using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace C3Mod.GameTypes
{
    public class ApocalypseMonsters
    {
        public static List<C3Monster> Monsters = new List<C3Monster>();

        public static void AddNPCs()
        {
            Monsters.Add(new C3Monster(1));
            Monsters.Add(new C3Monster(6));
            Monsters.Add(new C3Monster(23));
            Monsters.Add(new C3Monster(26));
            Monsters.Add(new C3Monster(27));
            Monsters.Add(new C3Monster(28));
            Monsters.Add(new C3Monster(29));
            Monsters.Add(new C3Monster(31));
            Monsters.Add(new C3Monster(32));
            Monsters.Add(new C3Monster(42));
            Monsters.Add(new C3Monster(47));
            Monsters.Add(new C3Monster(50));
            Monsters.Add(new C3Monster(62));
            Monsters.Add(new C3Monster(48));
            Monsters.Add(new C3Monster(34));
            Monsters.Add(new C3Monster(24));
            Monsters.Add(new C3Monster(23));
            Monsters.Add(new C3Monster(51));
        }
    }

    public class C3Monster
    {
        public int type { get; set; }

        public C3Monster(int id)
        {
            type = id;
        }
    }
}
