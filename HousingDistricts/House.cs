using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TShockAPI.DB;
using TShockAPI;
using Terraria;

namespace HousingDistricts
{
    public class House
    {
        public Rectangle HouseArea { get; set; }
        public List<string> Owners { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string WorldID { get; set; }

        public House(Rectangle housearea, List<string> owners, int id, string name, string worldid)
        {
            HouseArea = housearea;
            Owners = owners;
            ID = id;
            Name = name;
            WorldID = worldid;
        }
    }

    public class HouseTools
    {
        public static bool AddHouse(int tx, int ty, int width, int height, string housename)
        {
            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("Name", "'" + housename + "'"));
            values.Add(new SqlValue("TopX", tx));
            values.Add(new SqlValue("TopY", ty));
            values.Add(new SqlValue("BottomX", width));
            values.Add(new SqlValue("BottomY", height));
            values.Add(new SqlValue("Owners", "0"));
            HousingDistricts.SQLEditor.InsertValues("HousingDistrict", values);
            HousingDistricts.Houses.Add(new House(new Rectangle(tx, ty, width, height), new List<string>(), (HousingDistricts.Houses.Count + 1), housename, Main.worldID.ToString()));
            return true;
        }

        public static bool AddNewUser(string houseName, string id)
        {
            var house = GetHouseByName(houseName);
            StringBuilder sb = new StringBuilder();
            int count = 0;
            house.Owners.Add(id);
            foreach(string owner in house.Owners)
            {
                count++;
                sb.Append(owner);
                if(count != house.Owners.Count)
                    sb.Append(",");
            }
            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("Owners", "'" + sb.ToString() + "'"));

            List<SqlValue> wheres = new List<SqlValue>();
            wheres.Add(new SqlValue("Name", "'" + houseName + "'"));

            HousingDistricts.SQLEditor.UpdateValues("HousingDistrict", values, wheres);

            return true;
        }

        public static House GetHouseByName(string name)
        {
            foreach (House house in HousingDistricts.Houses)
            {
                if (house.Name == name)
                    return house;
            }
            return null;
        }
    }
}
