using System.Collections.Generic;
using System.Linq;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Einstellungen;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>Schwierigkeitsstufe eines Auftrags (leicht / mittel / schwer).</summary>
    public enum EnumAuftragSchwierigkeit
    {
        Leicht,
        Mittel,
        Schwer
    }

    /// <summary>Statische Metadaten eines Auftrags (Name, Ziel, Schwierigkeit, Zielwert, Einheit).</summary>
    public class AuftragInfo
    {
        public EnumAuftrag Auftrag { get; set; }
        public string Name { get; set; }
        public string Ziel { get; set; }
        public EnumAuftragSchwierigkeit Schwierigkeit { get; set; }
        public int Zielwert { get; set; }

        /// <summary>Einheit für die Fortschrittsanzeige (z. B. „Taler", „Häuser"). Leer bei Ja/Nein-Zielen.</summary>
        public string Einheit { get; set; }

        /// <summary>Ist der Fortschrittswert ein Geldbetrag (mit Tausendertrennung formatiert)?</summary>
        public bool IstGeld { get; set; }
    }

    /// <summary>
    /// Kapselt die Spiel-Aufträge (Missionen, Issue #15, Vorbild „Die Fugger 2"): die Auftragsliste samt
    /// Metadaten, die Fortschritts-/Erfüllungsprüfung für einen Spieler sowie den Fortschrittstext für die
    /// Anzeige. Der aktive Auftrag steht in <see cref="Spieleinstellungen.Auftrag"/>; nur menschliche
    /// Spieler verfolgen ihn. Ist kein Auftrag gewählt, bleibt alles wirkungslos (freies/endloses Spiel).
    /// </summary>
    public class AuftragManager
    {
        private const int AmtIdDomherr = 10;

        private static readonly List<AuftragInfo> Auftraege = new List<AuftragInfo>
        {
            // leicht
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.Aufsteiger, Name = "Aufsteiger",
                Ziel = "Erlangt ein beliebiges Amt.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Leicht, Zielwert = 1, Einheit = ""
            },
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.KleinerWohlstand, Name = "Kleiner Wohlstand",
                Ziel = "Besitzt 100.000 Taler.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Leicht, Zielwert = 100000, Einheit = "Taler", IstGeld = true
            },

            // mittel
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.HerrDesDoms, Name = "Herr des Doms",
                Ziel = "Werdet Domherr und bleibt es 7 Jahre in Folge.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Mittel, Zielwert = 7, Einheit = "Jahre als Domherr"
            },
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.Maezen, Name = "Mäzen",
                Ziel = "Stiftet dem Reich Bauwerke im Gesamtwert von 160.000 Talern.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Mittel, Zielwert = 160000, Einheit = "Taler gestiftet", IstGeld = true
            },
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.Baumeister, Name = "Baumeister",
                Ziel = "Errichtet 10 Häuser.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Mittel, Zielwert = 10, Einheit = "Häuser"
            },

            // schwer
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.Talerrennen, Name = "Talerrennen",
                Ziel = "Besitzt als Erster 1.000.000 Taler.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Schwer, Zielwert = 1000000, Einheit = "Taler", IstGeld = true
            },
            new AuftragInfo
            {
                Auftrag = EnumAuftrag.Kriegsherr, Name = "Kriegsherr",
                Ziel = "Erobert 5 gegnerische Stützpunkte.",
                Schwierigkeit = EnumAuftragSchwierigkeit.Schwer, Zielwert = 5, Einheit = "Stützpunkte erobert"
            }
        };

        /// <summary>Alle verfügbaren Aufträge (ohne „Kein Auftrag") in Anzeige-Reihenfolge.</summary>
        [PublicAPI]
        public static IReadOnlyList<AuftragInfo> GetAlleAuftraege() => Auftraege;

        /// <summary>Aufträge der angegebenen Schwierigkeitsstufe.</summary>
        [PublicAPI]
        public static IReadOnlyList<AuftragInfo> GetAuftraege(EnumAuftragSchwierigkeit schwierigkeit) =>
            Auftraege.Where(a => a.Schwierigkeit == schwierigkeit).ToList();

        /// <summary>Metadaten zu einem Auftrag oder null bei <see cref="EnumAuftrag.KeinAuftrag"/>.</summary>
        [PublicAPI]
        public static AuftragInfo GetInfo(EnumAuftrag auftrag) => Auftraege.FirstOrDefault(a => a.Auftrag == auftrag);

        /// <summary>Der aktuell im Spiel gewählte Auftrag (oder <see cref="EnumAuftrag.KeinAuftrag"/>).</summary>
        [PublicAPI]
        public static EnumAuftrag GetAktiverAuftrag() =>
            SW.Dynamisch?.Spielstand?.Einstellungen?.Auftrag ?? EnumAuftrag.KeinAuftrag;

        /// <summary>Ist überhaupt ein Auftrag aktiv (also kein freies Spiel)?</summary>
        [PublicAPI]
        public static bool IstAuftragAktiv() => GetAktiverAuftrag() != EnumAuftrag.KeinAuftrag;

        /// <summary>
        /// Aktualisiert den auftragsbezogenen Fortschritt des Spielers (derzeit den Domherr-Zähler) und
        /// meldet, ob der aktive Auftrag damit erfüllt ist. Genau einmal je Spielerzug aufzurufen.
        /// Ohne aktiven Auftrag stets false.
        /// </summary>
        [PublicAPI]
        public bool AktualisiereFortschrittUndPruefe(HumSpieler spieler)
        {
            if (spieler == null || !IstAuftragAktiv())
                return false;

            // Fortlaufende Domherr-Jahre pflegen (Amt gehalten ⇒ +1, sonst zurücksetzen).
            if (spieler.GetAmtID() == AmtIdDomherr)
                spieler.DomherrJahreInFolge++;
            else
                spieler.DomherrJahreInFolge = 0;

            var info = GetInfo(GetAktiverAuftrag());
            return info != null && GetFortschrittswert(GetAktiverAuftrag(), spieler) >= info.Zielwert;
        }

        /// <summary>
        /// Fortschrittstext für die Anzeige, z. B. „Auftrag „Mäzen": 45.000 / 160.000 Taler gestiftet".
        /// Leer, wenn kein Auftrag aktiv ist.
        /// </summary>
        [PublicAPI]
        public string GetFortschrittText(HumSpieler spieler)
        {
            var auftrag = GetAktiverAuftrag();
            var info = GetInfo(auftrag);

            if (spieler == null || info == null)
                return "";

            // Ja/Nein-Ziel (Aufsteiger): kein Zahlenfortschritt.
            if (auftrag == EnumAuftrag.Aufsteiger)
                return "Auftrag „" + info.Name + "“: " +
                       (GetFortschrittswert(auftrag, spieler) >= info.Zielwert ? "erfüllt" : "noch kein Amt erlangt");

            int ist = GetFortschrittswert(auftrag, spieler);
            string istText = info.IstGeld ? ist.ToStringGeld(false) : ist.ToString();
            string zielText = info.IstGeld ? info.Zielwert.ToStringGeld(false) : info.Zielwert.ToString();

            return "Auftrag „" + info.Name + "“: " + istText + " / " + zielText + " " + info.Einheit;
        }

        /// <summary>Der aktuelle Ist-Wert des Fortschritts für den angegebenen Auftrag.</summary>
        private static int GetFortschrittswert(EnumAuftrag auftrag, HumSpieler spieler)
        {
            switch (auftrag)
            {
                case EnumAuftrag.Aufsteiger:
                    return spieler.GetAmtID() > 0 ? 1 : 0;
                case EnumAuftrag.KleinerWohlstand:
                    return spieler.GetTaler();
                case EnumAuftrag.HerrDesDoms:
                    return spieler.DomherrJahreInFolge;
                case EnumAuftrag.Maezen:
                    return spieler.GestifteterBauwert;
                case EnumAuftrag.Baumeister:
                    return spieler.GetSpielerStatistik().SoGebauteHaeuser;
                case EnumAuftrag.Talerrennen:
                    return spieler.GetTaler();
                case EnumAuftrag.Kriegsherr:
                    return spieler.GetSpielerStatistik().MiEroberteStuetzpunkte;
                default:
                    return 0;
            }
        }
    }
}
