using System;

namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    [Serializable]
    public class Ereigniszeitpunkt
    {
        public int EreignisID { get; set; }
        public DateTime Zeitpunkt { get; set; }
    }
}
