using System;

namespace Conspiratio.Lib.Gameplay.Personen
{
    /// <summary>
    /// Ein lokales Spielerprofil, das die Statistik über mehrere Spiele hinweg bündelt. Profile werden
    /// spielübergreifend in profile.json gespeichert (nicht Teil eines einzelnen Spielstands) und über die
    /// <see cref="Id"/> mit einem <see cref="HumSpieler"/> eines Spiels verknüpft.
    /// </summary>
    [Serializable]
    public class Profil
    {
        /// <summary>Stabile GUID – bleibt auch bei Umbenennung erhalten.</summary>
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime ErstelltAm { get; set; }

        public DateTime ZuletztGespielt { get; set; }

        /// <summary>
        /// Aufsummierte additive Kennzahlen aller gewerteten Spiele (dieselbe Klasse wie die Pro-Spiel-Statistik).
        /// Das Max-Feld <c>SoHoechstesAmt</c> wird hier NICHT additiv geführt, sondern als Maximum in
        /// <see cref="ProfilMeta.HoechstesAmt"/>.
        /// </summary>
        public SpielerStatistik Gesamt { get; set; }

        public ProfilMeta Meta { get; set; }

        public Profil()
        {
            Gesamt = new SpielerStatistik();
            Meta = new ProfilMeta();
        }

        public Profil(string name) : this()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = name;
            ErstelltAm = DateTime.Now;
            ZuletztGespielt = DateTime.Now;
        }
    }
}
