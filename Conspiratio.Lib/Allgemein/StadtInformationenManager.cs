using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Stellt die Kenndaten einer Stadt für die Anzeige bereit (Migration von <c>StadtInformationen</c>):
    /// Reichtum, Umsatzsteuer, Einwohner, Kriminalität sowie die Rohstoff-bezogenen Angaben
    /// (Haupt-/Nebenproduktion, Nachfrage, mögliche Werkstätten und der Lagerstand des Landes je Rohstoff).
    /// </summary>
    public class StadtInformationenManager
    {
        private readonly Stadt _stadt;

        public StadtInformationenManager(int stadtId)
        {
            _stadt = SW.Dynamisch.GetStadtwithID(stadtId);

            var produktion = _stadt.GetHauptproduktion(6);
            Hauptproduktion = BaueRohstoffliste(produktion, 0, 3);
            Nebenproduktion = BaueRohstoffliste(produktion, 3, 3);

            Nachfrage = BaueRohstoffliste(_stadt.GetBedarf(stadtId), 0, 3);

            var rohstoffe = _stadt.GetRohstoffe();
            var werkstaetten = new List<Rohstoffangabe>();
            for (int i = 1; i <= SW.Statisch.GetMaxWerkstaettenProStadt(); i++)
            {
                if (i < rohstoffe.Length && rohstoffe[i] > 0)
                    werkstaetten.Add(ErstelleRohstoffangabe(rohstoffe[i]));
            }
            Werkstaetten = werkstaetten;

            var anteile = SW.Dynamisch.BerechneAnteilRohstoffvorratImLand(stadtId);
            var lager = new List<Lagerbestand>();
            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                int anteil = i < anteile.Length ? anteile[i] : 0;
                lager.Add(new Lagerbestand(i, SW.Dynamisch.GetRohstoffwithID(i).GetRohName(), anteil, BestimmeStufe(anteil)));
            }
            Lagerstand = lager;
        }

        /// <summary>Der Anzeigename der Stadt (Überschrift).</summary>
        public string StadtName => _stadt.GetGebietsName();

        /// <summary>Reichtum der Stadt (1..14 Münzsymbole).</summary>
        public int Reichtum => _stadt.GetReichtum();

        /// <summary>Umsatzsteuer als ganzzahliger Prozentwert samt Prozentzeichen (z. B. "5%").</summary>
        public string Umsatzsteuer => (int)Math.Round(_stadt.GetUmsatzsteuer() * 100) + "%";

        /// <summary>Einwohnerzahl der Stadt.</summary>
        public int Einwohner => _stadt.GetEinwohner();

        /// <summary>Kriminalität der Stadt (0..5 Dolchsymbole).</summary>
        public int Kriminalitaet => _stadt.GetKriminalitaet();

        /// <summary>Die drei effizientesten Rohstoffe der Stadt (Hauptproduktion).</summary>
        public IReadOnlyList<Rohstoffangabe> Hauptproduktion { get; }

        /// <summary>Die drei nächstbesten Rohstoffe der Stadt (Nebenproduktion).</summary>
        public IReadOnlyList<Rohstoffangabe> Nebenproduktion { get; }

        /// <summary>Die drei am stärksten nachgefragten Rohstoffe der Stadt.</summary>
        public IReadOnlyList<Rohstoffangabe> Nachfrage { get; }

        /// <summary>Rohstoffe, für die in der Stadt Werkstätten möglich sind.</summary>
        public IReadOnlyList<Rohstoffangabe> Werkstaetten { get; }

        /// <summary>Für jeden Rohstoff der Anteil des Stadtvorrats am Landesvorrat samt Bewertung.</summary>
        public IReadOnlyList<Lagerbestand> Lagerstand { get; }

        private static Lagerstufe BestimmeStufe(int anteil)
        {
            if (anteil <= 33)
                return Lagerstufe.Niedrig;
            if (anteil <= 66)
                return Lagerstufe.Normal;
            return Lagerstufe.Hoch;
        }

        private static List<Rohstoffangabe> BaueRohstoffliste(int[] rohIds, int start, int anzahl)
        {
            var liste = new List<Rohstoffangabe>();
            for (int i = start; i < start + anzahl && i < rohIds.Length; i++)
            {
                if (rohIds[i] > 0)
                    liste.Add(ErstelleRohstoffangabe(rohIds[i]));
            }
            return liste;
        }

        private static Rohstoffangabe ErstelleRohstoffangabe(int rohId)
            => new Rohstoffangabe(rohId, SW.Dynamisch.GetRohstoffwithID(rohId).GetRohName());
    }

    /// <summary>Ein Rohstoff mit seiner ID (für das Icon) und seinem Namen (für den Tooltip).</summary>
    public class Rohstoffangabe
    {
        public Rohstoffangabe(int rohId, string name)
        {
            RohId = rohId;
            Name = name;
        }

        public int RohId { get; }

        public string Name { get; }
    }

    /// <summary>Der Lagerstand eines Rohstoffs: Anteil des Stadtvorrats am Landesvorrat und dessen Bewertung.</summary>
    public class Lagerbestand
    {
        public Lagerbestand(int rohId, string name, int anteil, Lagerstufe stufe)
        {
            RohId = rohId;
            Name = name;
            Anteil = anteil;
            Stufe = stufe;
        }

        public int RohId { get; }

        public string Name { get; }

        /// <summary>Anteil in Prozent (0..100).</summary>
        public int Anteil { get; }

        public Lagerstufe Stufe { get; }
    }

    /// <summary>Bewertung des Lagerstands eines Rohstoffs (niedrig/normal/hoch = rot/orange/grün).</summary>
    public enum Lagerstufe
    {
        Niedrig,
        Normal,
        Hoch
    }
}
