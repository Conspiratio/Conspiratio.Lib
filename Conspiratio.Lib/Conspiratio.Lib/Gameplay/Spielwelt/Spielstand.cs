using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Justiz;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Privilegien;
using Conspiratio.Lib.Gameplay.Privilegien.FestGeben;
using Conspiratio.Lib.Gameplay.Rohstoffe;
using Conspiratio.Lib.Gameplay.Schreibstube;

namespace Conspiratio.Lib.Gameplay.Spielwelt
{
    /// <summary>
    /// Datenklasse mit allen Daten des aktuellen Spielstandes. Enthält keine Logik! Der Zugriff erfolgt i.d.R. ausschließlich über die Klasse VW.
    /// </summary>
    [Serializable]
    public class Spielstand
    {
        public int AktiverSpielerID;  // Gibt die ID des aktiven Spieler an
        public int AktiveSpielerAnzahl;  // Wie viele Menschliche Spieler am Spiel beteiligt sind
        public int AktuellesJahr;
        public int[] Gesetze;
        public string[] GesetzesTexte;  // Ändert sich bei Gesetzesänderung und ist deswegen nicht in SVW

        public string SpielName;
        public bool Cheatmodus;
        public bool Testmodus;
        public bool TodesfaelleAnzeigen;

        public Gerichtsverhandlung[] Gerichtshandlungen { get; set; }
        public HumSpieler[] HSpieler { get; set; }
        public KISpieler[] KSpieler { get; set; }
        public Stadt[] Staedte { get; set; }
        public Land[] Laender { get; set; }
        public Reich[] Reiche { get; set; }
        public Stuetzpunkt[] Stuetzpunkte { get; set; }
        public WahlAbhalten[] Wahlen { get; set; }
        public Amtsenthebung[] Amtsenthebungen { get; set; }
        public Rohstoff[] Rohstoffe { get; set; }
        public Landsicherheit[] Landsicherheiten { get; set; }

        public List<Fest> Feste { get; set; } = new List<Fest>();

        [OnDeserializing]
        void OnDeserializing(StreamingContext c)
        {
            if (Feste == null)
                Feste = new List<Fest>();
        }
    }
}
