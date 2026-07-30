using System.Collections.Generic;

using Conspiratio.Lib.Extensions;
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

                // Bezahlten Moral-Bonus des angreifenden Stützpunkts abwickeln (bei Sieg zurückerstatten).
                string bonusMeldung = MoralBonusAbwickeln(ergebnis.StuetzpunktIDAngreifer, ergebnis.SpielerIDGewinner == ergebnis.SpielerIDAngreifer);
                if (!string.IsNullOrEmpty(bonusMeldung))
                    meldungen.Add(bonusMeldung);
            }

            // Verbliebene, ungenutzte Moral-Boni (z. B. ohne stattgefundenen Kampf) zurückerstatten.
            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
                if (stuetzpunkt.MoralBonusBezahlt > 0)
                    MoralBonusAbwickeln(stuetzpunkt.ID, true);

            if (meldungen.Count == 0)
                meldungen.Add("Dieses Jahr hat sich nichts Besonderes ereignet.");

            return meldungen;
        }

        /// <summary>
        /// Verrechnet einen für den angreifenden Stützpunkt bezahlten Moral-Bonus: Bei einem Sieg (oder einem
        /// ungenutzten Bonus) wird der Betrag dem menschlichen Besitzer zurückerstattet. Der Bonus wird
        /// anschließend zurückgesetzt. Liefert ggf. eine Meldung für den Spieler zurück.
        /// </summary>
        private static string MoralBonusAbwickeln(int stuetzpunktId, bool erstatten)
        {
            var stuetzpunkt = SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1];

            if (stuetzpunkt.MoralBonusBezahlt <= 0)
                return null;

            int betrag = stuetzpunkt.MoralBonusBezahlt;
            stuetzpunkt.MoralBonusBezahlt = 0;

            // Nur menschliche Besitzer bezahlen (und erhalten zurück).
            if (stuetzpunkt.Besitzer >= SW.Statisch.GetMinKIID())
                return null;

            if (!erstatten)
                return null;

            SW.Dynamisch.GetHumWithID(stuetzpunkt.Besitzer).ErhoeheTaler(betrag);
            return $"Eure Truppen aus {stuetzpunkt.Name} waren siegreich – der Moral-Bonus über {betrag.ToStringGeld()} wurde Euch zurückerstattet.";
        }
    }
}
