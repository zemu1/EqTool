﻿using System;
using System.Linq;

namespace EQToolShared.Map
{
    public static class ZoneSpawnTimes
    {
        public static TimeSpan GetSpawnTime(string npcName, string zone)
        {
            if (EQToolShared.Map.ZoneParser.ZoneInfoMap.TryGetValue(zone, out var zoneInfo))
            {
                if (!string.IsNullOrWhiteSpace(npcName))
                {
                    var foundnpc = zoneInfo.NpcSpawnTimes.FirstOrDefault(a => a.Name == npcName);
                    if (foundnpc != null)
                    {
                        return foundnpc.RespawnTime;
                    }
                    foundnpc = zoneInfo.NpcContainsSpawnTimes.FirstOrDefault(a => npcName.Contains(a.Name));
                    if (foundnpc != null)
                    {
                        return foundnpc.RespawnTime;
                    }
                }

                return zoneInfo.RespawnTime;
            }

            return new TimeSpan(0, 6, 40);
        }
    }
}
