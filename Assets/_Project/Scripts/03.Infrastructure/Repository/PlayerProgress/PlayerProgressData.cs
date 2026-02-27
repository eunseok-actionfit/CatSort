using System;
using AES.Tools;


namespace Infrastructure
{
    [Serializable]
    [SaveData( "player_progress", UseSlot = false, Backend = SaveBackend.CloudFirst)]
    public sealed class PlayerProgressData
    {
        public int lastClearedLevel;
    }
}


