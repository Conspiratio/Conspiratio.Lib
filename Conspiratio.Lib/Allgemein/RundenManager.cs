using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die Rundenrotation im Hot-Seat-Spiel: Zugbeginn, Zugende mit Spielerwechsel
    /// sowie den Jahreswechsel, nachdem der letzte aktive Spieler seinen Zug beendet hat.
    /// Extrahiert aus dem alten WinForms-Client (AktivenSpielerSchalten / RundeBeginnen / ZugBeenden).
    /// </summary>
    public class RundenManager
    {
        /// <summary>
        /// Bereitet den Zug des aktuell aktiven Spielers vor (Privilegien und Ansehen aktualisieren).
        /// </summary>
        [PublicAPI]
        public void BeginneZug()
        {
            SW.Dynamisch.PrivilegienAktualisieren();
            SW.Dynamisch.GetAktHum().AnsehenAktualisieren();

            // Die Duell-Sperre (max. ein Duell pro Zug, Issue #17) gilt nur für das laufende Jahr.
            SW.Dynamisch.GetAktHum().DuellGefuehrtDiesesJahr = false;
        }

        /// <summary>
        /// Prüft, ob der aktuell aktive Spieler der letzte Spieler des Jahres ist – nach seinem Zugende
        /// folgen also die Rundenende-Ereignisse und der Jahreswechsel.
        /// </summary>
        [PublicAPI]
        public bool IstLetzterSpielerImJahr()
        {
            return SW.Dynamisch.GetAktiverSpieler() >= SW.Dynamisch.GetAktivSpielerAnzahl();
        }

        /// <summary>
        /// Prüft, ob der aktuell aktive Spieler dieses Jahr im Schuldturm sitzt.
        /// </summary>
        [PublicAPI]
        public bool SitztAktiverSpielerImKerker()
        {
            return SW.Dynamisch.GetAktHum().GetSitztImKerker();
        }

        /// <summary>
        /// Schließt den Schuldturm-Aufenthalt des aktiven Spielers ab, sodass er nächstes Jahr wieder spielen kann.
        /// </summary>
        [PublicAPI]
        public void KerkerAufenthaltAbschliessen()
        {
            SW.Dynamisch.GetAktHum().GetSpielerStatistik().SoSchuldturmaufenthalte++;  // Statistik (Issue #19)
            SW.Dynamisch.GetAktHum().SetSitztImKerker(false);
        }

        /// <summary>
        /// Schließt den Zug des aktiven Spielers ab, ohne weiterzuschalten: Er und seine Kinder
        /// altern und die Zug-Flags werden zurückgesetzt. Muss vor den Zugnachrichten aufgerufen
        /// werden, damit z. B. die Sterbeprüfung mit dem neuen Alter rechnet (wie im WinForms-Client).
        /// </summary>
        [PublicAPI]
        public void SchliesseZugAb()
        {
            var spieler = SW.Dynamisch.GetAktHum();

            spieler.AlterPlusEins();
            spieler.KinderAltern();

            spieler.SetPrivilegKaufmannBenutzt(false);
            spieler.SetGebeichtet(false);
            spieler.HatAngebotFuerStuetzpunktAbgegeben = false;
        }

        /// <summary>
        /// Beendet den Zug des aktiven Spielers: lässt ihn und seine Kinder altern, setzt die
        /// Zug-Flags zurück und schaltet auf den nächsten Spieler weiter. Hat der letzte aktive
        /// Spieler seinen Zug beendet, beginnt ein neues Jahr mit Spieler 1.
        /// </summary>
        /// <returns>True, wenn mit dem Zugende ein neues Jahr begonnen hat.</returns>
        [PublicAPI]
        public bool BeendeZug()
        {
            SchliesseZugAb();
            return SchalteZumNaechstenSpieler();
        }

        /// <summary>
        /// Schaltet nur auf den nächsten Spieler weiter, ohne den Zug des aktiven Spielers
        /// abzuschließen (z. B. wenn dieser das Jahr im Schuldturm verbringt und daher nicht altert).
        /// Hat der letzte aktive Spieler seinen Zug beendet, beginnt ein neues Jahr mit Spieler 1.
        /// </summary>
        /// <returns>True, wenn mit dem Spielerwechsel ein neues Jahr begonnen hat.</returns>
        [PublicAPI]
        public bool SchalteZumNaechstenSpieler()
        {
            if (SW.Dynamisch.GetAktiverSpieler() < SW.Dynamisch.GetAktivSpielerAnzahl())
            {
                SW.Dynamisch.SetAktiverSpieler(SW.Dynamisch.GetAktiverSpieler() + 1);
                return false;
            }

            // Der letzte Spieler hat seinen Zug beendet: neues Jahr beginnen
            SW.Dynamisch.SetAktiverSpieler(1);
            SW.Dynamisch.KIsVonWahlenAbmelden();
            SW.Dynamisch.KlagenAbgewickelt();
            SW.Dynamisch.ErhoehAktuellesJahrUmEins();

            return true;
        }
    }
}
