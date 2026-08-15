using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Ermittelt zum Tod eines Charakters eine passende Grabsteinaufschrift (Issue #15, nach Vorbild von
    /// „Die Fugger 2"): Anhand der Pro-Spiel-Statistik (<see cref="SpielerStatistik"/>) wird der prägendste
    /// „Typ" des Verstorbenen geschätzt und ein dazu passender kurzer Spruch gewählt.
    /// </summary>
    public class GrabsteinManager
    {
        /// <summary>
        /// Liefert einen zur Spielweise passenden Grabspruch. Sticht keine Spielweise deutlich heraus,
        /// wird ein allgemeiner Spruch gewählt.
        /// </summary>
        [PublicAPI]
        public string ErmittleGrabspruch(SpielerStatistik s)
        {
            var wertungen = new Dictionary<string, double>
            {
                ["Kriegsherr"] = s.MiKaempfeGewonnen * 3 + s.MiEroberteStuetzpunkte * 6 + s.MiUeberfalleneKarawanen * 2,
                ["Intrigant"] = s.HiVersuchteErmordungen * 6 + s.HiErfolgreicheErmordungen * 4 + s.HiSabotagen * 3
                                + s.HiSpionagen + s.HiAnschwaerzungen * 2 + s.HiBestechungen,
                ["Kaufmann"] = s.HaWarenVerkauft / 40.0 + s.SoGesamtumsatz / 15000.0 + s.HaWarenEingekauft / 60.0,
                ["Kirchenmann"] = s.KgekaufteAblaesse * 2 + s.KabgelegteBeichten * 2 + s.KKonvertierungen * 4 + s.KHochzeiten,
                ["Staatsmann"] = s.SoHoechstesAmt * 5 + s.SWahlenGewonnen * 4 + s.SoAmtseinkommen / 8000.0,
                ["Patriarch"] = s.SogezeugteKinder * 5.0,
                ["Gesetzloser"] = s.SogebrocheneGesetze * 3 + s.Soangeklagt * 4 + s.SoSchuldturmaufenthalte * 5
            };

            string bester = null;
            double besteWertung = 0;

            foreach (var eintrag in wertungen)
            {
                if (eintrag.Value > besteWertung)
                {
                    besteWertung = eintrag.Value;
                    bester = eintrag.Key;
                }
            }

            // Unauffälliges Leben: keine Spielweise sticht deutlich heraus.
            string typ = besteWertung >= 8 ? bester : "Unauffaellig";

            var sprueche = Sprueche[typ];
            return sprueche[SW.Statisch.Rnd.Next(sprueche.Count)];
        }

        private static readonly Dictionary<string, List<string>> Sprueche = new Dictionary<string, List<string>>
        {
            ["Kriegsherr"] = new List<string>
            {
                "Mit dem Schwert erwarb er, was ihm die Geburt versagte. Nun ist auch der letzte Feldzug beendet.",
                "Er kannte keine Grenze, die eine Klinge nicht verschieben konnte – nur den Tod nicht."
            },
            ["Intrigant"] = new List<string>
            {
                "Gift, Dolch und falsches Wort waren sein Handwerk. Möge das Jenseits wachsamer sein als seine Opfer.",
                "Er lächelte, während er Ränke schmiedete. Nun lächelt der Tod zurück."
            },
            ["Kaufmann"] = new List<string>
            {
                "Er zählte Taler, wie andere ihre Sünden zählen – gewissenhaft und ohne Reue. Sein Kontor überlebt ihn.",
                "Kein Handel war ihm zu klein, kein Gewinn zu unehrlich. Reich starb er, und das genügte ihm."
            },
            ["Kirchenmann"] = new List<string>
            {
                "Mit Ablässen kaufte er sich frei – vom Fegefeuer hoffentlich auch.",
                "Fromm im Wort, fleißig im Beichtstuhl. Der Herr wird die Bücher schon prüfen."
            },
            ["Staatsmann"] = new List<string>
            {
                "Ämter sammelte er wie andere Reliquien. Das Grab ist sein letztes und stummstes.",
                "Er regierte, ränkte und residierte. Nun hat ihn das höchste aller Ämter ereilt."
            },
            ["Patriarch"] = new List<string>
            {
                "Er hinterließ eine Sippe so zahlreich wie seine Taler. Die Dynastie trauert – und rechnet.",
                "Seine reichste Ernte waren seine Kinder. Möge sie ihn lange überdauern."
            },
            ["Gesetzloser"] = new List<string>
            {
                "Der Schuldturm war ihm ein zweites Heim. Nun bezieht er ein drittes, endgültiges.",
                "Gesetze hielt er für unverbindliche Vorschläge. Der Tod machte da keine Ausnahme."
            },
            ["Unauffaellig"] = new List<string>
            {
                "Er lebte, er handelte, er ging dahin. Ein Leben wie viele – doch es war das seine.",
                "Weder Held noch Schurke, nur ein Kaufmann seiner Zeit. Ruhe sanft."
            }
        };
    }
}
