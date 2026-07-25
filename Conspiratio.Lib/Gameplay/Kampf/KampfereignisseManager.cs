using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Kapselt die militärischen Ereignisse am Jahresende (Migration von frmKampfereignisse): führt die
    /// Aktionen der KI-Stützpunkte aus, initialisiert die Landsicherheiten und ermittelt und wertet die
    /// stattfindenden Kämpfe aus. Liefert die anzuzeigenden Meldungen der Reihe nach zurück.
    /// </summary>
    public class KampfereignisseManager
    {
        /// <summary>
        /// Wickelt die Kampfereignisse ab und gibt die Meldungen (KI-Aktionen und Kampf-Zusammenfassungen)
        /// in der Reihenfolge des Originals zurück. Gab es nichts, enthält die Liste eine Standardmeldung.
        /// </summary>
        public List<string> ErmittleEreignisse()
        {
            var meldungen = new List<string>();
            var kampf = new Kampfberechnung();

            // KI-Aktionen je KI-Stützpunkt ausführen (menschliche Stützpunkte werden übersprungen).
            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
            {
                if (stuetzpunkt.Besitzer <= SW.Statisch.GetMinKIID())
                    continue;

                string text = stuetzpunkt.Art == EnumStuetzpunktArt.Zollburg
                    ? ((Zollburg)stuetzpunkt).RundenendeKIAktionenDurchfuehren()
                    : ((Raeuberlager)stuetzpunkt).RundenendeKIAktionenDurchfuehren();

                if (!string.IsNullOrEmpty(text))
                    meldungen.Add(text);
            }

            SW.Dynamisch.LandsicherheitenInitialisieren();

            // Stattfindende Kämpfe ermitteln, berechnen, anwenden und zusammenfassen.
            foreach (var einzelkampf in kampf.ErmittleStattfindendeKaempfe())
            {
                var ergebnis = kampf.BerechneKampfErgebnis(einzelkampf);
                kampf.KampfErgebnisAnwenden(ergebnis);

                // Im Original hebt "|" einzelne Spielernamen hervor; für die Textanzeige entfernen wir die Trenner.
                meldungen.Add(ergebnis.Zusammenfassung.Replace("|", ""));
            }

            if (meldungen.Count == 0)
                meldungen.Add("Dieses Jahr hat sich nichts Besonderes ereignet.");

            return meldungen;
        }
    }
}
