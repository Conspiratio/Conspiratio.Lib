using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Die jährliche Gesetzgebung der KI-Minister am Rundenende (Migration des Gesetze-Blocks aus
    /// Main.KIAktionen im WinForms-Client).
    ///
    /// Für jedes der drei Ressorts – Finanzen, Strafrecht und Kirche – gilt: Hat ein KI-Spieler das Amt
    /// inne, ändert er mit einer Wahrscheinlichkeit von <see cref="ChancePerRessort"/> sämtliche Gesetze
    /// seines Ressorts um je eine Stufe nach oben, unten oder gar nicht. Hält ein menschlicher Spieler das
    /// Amt, bleibt das Ressort unangetastet – er ändert die Gesetze selbst.
    /// </summary>
    public class KiGesetzgebungManager
    {
        /// <summary>
        /// Kehrwert der Wahrscheinlichkeit, mit der ein KI-Minister sein Ressort in einem Jahr anfasst:
        /// 7 entspricht rund 14 % (im Original <c>Rnd.Next(1, 8) == 1</c>).
        /// </summary>
        private const int ChancePerRessort = 7;

        /// <summary>
        /// Lässt die KI-Minister ihre Gesetze ändern und liefert je geändertem Ressort eine Meldung.
        /// Ändert niemand etwas, ist die Liste leer.
        /// </summary>
        [PublicAPI]
        public List<GesetzesaenderungsMeldung> FuehreGesetzesaenderungenDurch()
        {
            var meldungen = new List<GesetzesaenderungsMeldung>();
            var reich = SW.Dynamisch.GetReichWithID(1);

            AendereRessort(meldungen, "Finanzgesetze", reich.GetFinanzminister(),
                           erstesGesetz: 0,
                           anzahl: SW.Statisch.GetGesetzAnzahlFinanz());

            AendereRessort(meldungen, "Strafgesetze", reich.GetJustizminister(),
                           erstesGesetz: SW.Statisch.GetGesetzgrenzeFinanz(),
                           anzahl: SW.Statisch.GetGesetzAnzahlStraf());

            AendereRessort(meldungen, "Kirchengesetze", reich.GetErzbischof(),
                           erstesGesetz: SW.Statisch.GetGesetzgrenzeStraf(),
                           anzahl: SW.Statisch.GetGesetzAnzahlKirch());

            return meldungen;
        }

        /// <summary>
        /// Würfelt für ein Ressort, ob sein KI-Minister die Gesetze anfasst, und schaltet sie gegebenenfalls
        /// um je eine Stufe. Der Würfelwurf fällt bewusst auch dann, wenn das Amt von einem Menschen gehalten
        /// wird oder unbesetzt ist – so hängt der Zufallsstrom nicht an der Ämterlage.
        /// </summary>
        private static void AendereRessort(ICollection<GesetzesaenderungsMeldung> meldungen, string ressort,
                                           int ministerId, int erstesGesetz, int anzahl)
        {
            bool aendert = SW.Statisch.Rnd.Next(1, ChancePerRessort + 1) == 1;

            // Nur ein KI-Minister legiferiert hier; ein menschlicher Amtsinhaber tut es selbst.
            if (!aendert || ministerId < SW.Statisch.GetMinKIID())
                return;

            var geaenderteGesetze = new List<string>();

            for (int i = 0; i < anzahl; i++)
            {
                int gesetzId = erstesGesetz + i;

                SW.Dynamisch.GesetzXschaltenUmY(gesetzId, SW.Statisch.Rnd.Next(-1, 2));
                geaenderteGesetze.Add(SW.Dynamisch.GetGesetzXinText(gesetzId));
            }

            meldungen.Add(new GesetzesaenderungsMeldung(
                ressort,
                SW.Dynamisch.GetSpWithID(ministerId).GetKompletterName() + Environment.NewLine +
                "ändert die " + ressort + " wie folgt:" + Environment.NewLine + Environment.NewLine +
                string.Join(Environment.NewLine, geaenderteGesetze)));
        }
    }

    /// <summary>Eine Ressort-Meldung über geänderte Gesetze.</summary>
    public class GesetzesaenderungsMeldung
    {
        public GesetzesaenderungsMeldung(string ressort, string text)
        {
            Ressort = ressort;
            Text = text;
        }

        /// <summary>Überschrift der Meldung, z. B. "Finanzgesetze".</summary>
        public string Ressort { get; }

        /// <summary>Der Minister und die neue Gesetzeslage seines Ressorts.</summary>
        public string Text { get; }
    }
}
