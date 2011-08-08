using System;
using System.IO;
using Newtonsoft.Json;

namespace C3Mod
{
    public class C3ConfigFile
    {
        public bool TPLockEnabled = true;

        public int VoteTime = 30;
        public int VoteCTFPlayersMinimumPerTeam = 1;
        public int VoteNotifyInterval = 7;

        public int DuelTimesToNotify = 5;
        public int DuelNotifyInterval = 7;
        public int DuelScoreLimit = 3;

        public int CTFScoreLimit = 3;
        public int OneFlagScorelimit = 3;
        public int TeamDeathmatchScorelimit = 20;

        public int TDMScoreNotifyInterval = 30;

        public bool HealPlayersOnFlagCapture = false;
        public bool RespawnPlayersOnFlagCapture = false;
        public bool ReCountdownOnFlagCapture = false;

        public int MonsterApocalypseLivesPerWave = 1;
        public int MonsterApocalypseIntermissionTime = 20;
        public int MonsterApocalypseScoreNotifyInterval = 15;
        public int MonsterApocalypseMinimumPlayers = 1;

        public bool DuelsEnabled = true;
        public bool CTFEnabled = true;
        public bool OneFlagEnabled = true;
        public bool TeamDeathmatchEnabled = true;
        public bool MonsterApocalypseEnabled = true;

        public static C3ConfigFile Read(string path)
        {
            if (!File.Exists(path))
                return new C3ConfigFile();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        public static C3ConfigFile Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<C3ConfigFile>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        public static Action<C3ConfigFile> ConfigRead;
    }
}