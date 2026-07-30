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
        /// <param name="zeigeKiStuetzpunktereignisse">Ob Meldungen zu KI-Stützpunkt-Aktionen (Ausbau, neue
        /// Rekruten) angezeigt werden. Die Aktionen werden unabhängig davon immer ausgeführt.</param>
        /// <param name="zeigeKiMilitaerereignisse">Ob auch Kämpfe ohne menschliche Beteiligung angezeigt
        /// werden. Ist dies false, erscheinen nur Kämpfe, an denen ein menschlicher Spieler beteiligt ist.
        /// Der Kampf wird unabhängig davon immer berechnet und angewendet.</param>
        public List<string> ErmittleEreignisse(bool zeigeKiStuetzpunktereignisse = true, bool zeigeKiMilitaerereignisse = true)
        {
            var meldungen = new List<string>();
            var kampf = new Kampfberechnung();

            // Angeworbene Truppen treffen zum Rundenende ein (einmal je Runde), bevor die Kämpfe stattfinden.
            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
            {
                string angeworben = stuetzpunkt.GeworbeneTruppenEinstellen();
                if (!string.IsNullOrEmpty(angeworben) && stuetzpunkt.Besitzer < SW.Statisch.GetMinKIID())
                    meldungen.Add(angeworben);
            }

            // KI-Aktionen je KI-Stützpunkt ausführen (menschliche Stützpunkte werden übersprungen).
            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
            {
                if (stuetzpunkt.Besitzer <= SW.Statisch.GetMinKIID())
                    continue;

                string text = stuetzpunkt.Art == EnumStuetzpunktArt.Zollburg
                    ? ((Zollburg)stuetzpunkt).RundenendeKIAktionenDurchfuehren()
                    : ((Raeuberlager)stuetzpunkt).RundenendeKIAktionenDurchfuehren();

                // Die Aktion wurde bereits ausgeführt; die Meldung nur zeigen, wenn KI-Stützpunktereignisse gewünscht sind.
                if (zeigeKiStuetzpunktereignisse && !string.IsNullOrEmpty(text))
                    meldungen.Add(text);
            }

            SW.Dynamisch.LandsicherheitenInitialisieren();

            // Stattfindende Kämpfe ermitteln, berechnen, anwenden und zusammenfassen.
            foreach (var einzelkampf in kampf.ErmittleStattfindendeKaempfe())
            {
                var ergebnis = kampf.BerechneKampfErgebnis(einzelkampf);
                kampf.KampfErgebnisAnwenden(ergebnis);

                // Kampf immer anwenden; die Zusammenfassung nur zeigen, wenn KI-Militärereignisse gewünscht sind
                // oder ein menschlicher Spieler beteiligt ist (Angreifer, Verteidiger oder überfallene Karawane).
                bool menschlicherSpielerBeteiligt =
                    ergebnis.SpielerIDAngreifer <= SW.Statisch.GetMinKIID() ||
                    ergebnis.SpielerIDVerteidiger <= SW.Statisch.GetMinKIID() ||
                    (ergebnis.Karawane != null && ergebnis.Karawane.SpielerID <= SW.Statisch.GetMinKIID());

                // Die Zusammenfassung enthält Spielernamen in |...|-Markern (die Markup-Konvention der Lib).
                // Sie bleiben erhalten, damit die Ansicht die Namen hervorheben kann (fett, menschliche Spieler
                // zusätzlich dunkelrot – wie im WinForms-Original). Ansichten ohne Formatierung entfernen sie.
                if (zeigeKiMilitaerereignisse || menschlicherSpielerBeteiligt)
                    meldungen.Add(ergebnis.Zusammenfassung);

                // Ein bezahlter Moral-Bonus ist mit dem Kampf verbraucht – bei Sieg wie bei Niederlage
                // gibt es keine Rückerstattung.
                MoralBonusAbwickeln(ergebnis.StuetzpunktIDAngreifer, erstatten: false);
            }

            // Verbliebene, ungenutzte Moral-Boni (ohne stattgefundenen Kampf) werden zurückerstattet.
            foreach (var stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
                if (stuetzpunkt.MoralBonusBezahlt > 0)
                {
                    string bonusMeldung = MoralBonusAbwickeln(stuetzpunkt.ID, erstatten: true);
                    if (!string.IsNullOrEmpty(bonusMeldung))
                        meldungen.Add(bonusMeldung);
                }

            if (meldungen.Count == 0)
                meldungen.Add("Dieses Jahr hat sich nichts Besonderes ereignet.");

            return meldungen;
        }

        /// <summary>
        /// Setzt einen für den angreifenden Stützpunkt bezahlten Moral-Bonus zurück. Ist der Bonus mit einem
        /// Kampf verbraucht worden, verfällt er (<paramref name="erstatten"/> = false); wurde er nicht genutzt,
        /// wird der Betrag dem menschlichen Besitzer zurückerstattet. Liefert ggf. eine Meldung zurück.
        /// </summary>
        private static string MoralBonusAbwickeln(int stuetzpunktId, bool erstatten)
        {
            var stuetzpunkt = SW.Dynamisch.GetStuetzpunkte()[stuetzpunktId - 1];

            if (stuetzpunkt.MoralBonusBezahlt <= 0)
                return null;

            int betrag = stuetzpunkt.MoralBonusBezahlt;
            stuetzpunkt.MoralBonusBezahlt = 0;

            // Nur menschliche Besitzer bezahlen (und erhalten einen ungenutzten Bonus zurück).
            if (stuetzpunkt.Besitzer >= SW.Statisch.GetMinKIID())
                return null;

            if (!erstatten)
                return null;

            SW.Dynamisch.GetHumWithID(stuetzpunkt.Besitzer).ErhoeheTaler(betrag);
            return $"Mangels Kampf blieb der Moral-Bonus über {betrag.ToStringGeld()} für die Truppen aus {stuetzpunkt.Name} ungenutzt und wurde Euch zurückerstattet.";
        }
    }
}
