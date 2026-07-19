using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt alle Handels- und Produktionsaktionen des aktiven Spielers in einer Stadt:
    /// Werkstätten kaufen/verkaufen, Rohstoffe direkt kaufen/verkaufen sowie das Einstellen
    /// der beiden Produktionsslots (Produzieren bzw. Verkaufen/Exportieren) und der Karawane.
    /// Extrahiert aus der Stadt-Ansicht des alten WinForms-Clients.
    /// </summary>
    public class HandelsManager
    {
        #region Werkstätten

        [PublicAPI]
        public int RohstoffIdAnPlatz(int stadtId, int werkstattNr)
        {
            return SW.Dynamisch.GetStadtwithID(stadtId).GetSingleRohstoff(werkstattNr);
        }

        [PublicAPI]
        public bool HatRohstoffrecht(int stadtId, int werkstattNr)
        {
            return SW.Dynamisch.GetAktHum().GetRohstoffrechteX(RohstoffIdAnPlatz(stadtId, werkstattNr));
        }

        [PublicAPI]
        public bool HatWerkstatt(int stadtId, int werkstattNr)
        {
            return SW.Dynamisch.GetAktHum().GetSpielerHatInStadtXWerkstaettenY(werkstattNr, stadtId).GetEnabled();
        }

        [PublicAPI]
        public bool HatWohnsitz(int stadtId)
        {
            return SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(stadtId).GetHausID() != 0;
        }

        /// <summary>
        /// Gibt an, ob der aktive Spieler in der Stadt eine Präsenz hat, also entweder einen Wohnsitz
        /// oder eine Werkstätte mit Lagerraum besitzt (z. B. für das Wehen des Banners auf der Weltkarte).
        /// </summary>
        [PublicAPI]
        public bool HatPraesenzInStadt(int stadtId)
        {
            var spieler = SW.Dynamisch.GetAktHum();

            if (spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId).GetHausID() != 0)
                return true;

            for (int werkstattNr = 1; werkstattNr <= SW.Statisch.GetMaxWerkstaettenProStadt(); werkstattNr++)
            {
                if (spieler.GetSpielerHatInStadtXWerkstaettenY(werkstattNr, stadtId).GetSKillX(1) != 0)
                    return true;
            }

            return false;
        }

        [PublicAPI]
        public int GetWerkstattKaufpreis(int stadtId, int werkstattNr)
        {
            return SW.Dynamisch.GetRohstoffwithID(RohstoffIdAnPlatz(stadtId, werkstattNr)).GetWSKaufpreis();
        }

        [PublicAPI]
        public int GetWerkstattVerkaufspreis(int stadtId, int werkstattNr)
        {
            return (GetWerkstattKaufpreis(stadtId, werkstattNr) * 3) / 4;
        }

        /// <summary>
        /// Kauft die Werkstätte am Platz <paramref name="werkstattNr"/> der Stadt (setzt Wohnsitz,
        /// Rohstoffrecht und genügend Taler voraus).
        /// </summary>
        [PublicAPI]
        public bool KaufeWerkstatt(int stadtId, int werkstattNr, out string fehler)
        {
            fehler = "";
            var spieler = SW.Dynamisch.GetAktHum();

            if (!HatWohnsitz(stadtId))
            {
                fehler = "Ihr benötigt zuerst einen Wohnsitz in dieser Stadt";
                return false;
            }

            if (!HatRohstoffrecht(stadtId, werkstattNr))
            {
                fehler = "Euch fehlt das Rohstoffrecht für diesen Rohstoff";
                return false;
            }

            int preis = GetWerkstattKaufpreis(stadtId, werkstattNr);

            if (spieler.GetTaler() < preis)
            {
                fehler = "Dafür fehlen Euch die Taler.";
                return false;
            }

            var werkstatt = spieler.GetSpielerHatInStadtXWerkstaettenY(werkstattNr, stadtId);
            werkstatt.SetEnabled(true);
            werkstatt.SetRohstoffID(RohstoffIdAnPlatz(stadtId, werkstattNr));
            werkstatt.SetSkillX(1, SW.Statisch.GetStartLagerraum());

            spieler.ErhoeheTaler(-preis);
            return true;
        }

        /// <summary>
        /// Verkauft die Werkstätte für 3/4 des Kaufpreises und setzt laufende Produktionsaufträge
        /// dieses Rohstoffs zurück.
        /// </summary>
        /// <returns>Der erhaltene Verkaufspreis.</returns>
        [PublicAPI]
        public int VerkaufeWerkstatt(int stadtId, int werkstattNr)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int rohstoffId = RohstoffIdAnPlatz(stadtId, werkstattNr);
            int verkaufspreis = GetWerkstattVerkaufspreis(stadtId, werkstattNr);

            spieler.GetSpielerHatInStadtXWerkstaettenY(werkstattNr, stadtId).SetEnabled(false);
            spieler.ErhoeheTaler(verkaufspreis);

            // Überprüfen, ob von diesem Rohstoff Werkstätten zur Produktion eingestellt sind
            for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
            {
                if (spieler.GetProduktionsslot(stadtId, slot).GetProduktionRohstoff() == rohstoffId)
                {
                    spieler.GetProduktionsslot(stadtId, slot).SetProduktionStaetten(0);
                    spieler.GetProduktionsslot(stadtId, slot).SetProduktionArbeiter(0);
                    break;
                }
            }

            return verkaufspreis;
        }

        [PublicAPI]
        public int GetLagerbestand(int stadtId, int rohstoffId)
        {
            return SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(stadtId, rohstoffId);
        }

        [PublicAPI]
        public string GetLagerplatzInfo(int stadtId, int werkstattNr)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int rohstoffId = RohstoffIdAnPlatz(stadtId, werkstattNr);
            int belegterLagerplatz = SW.Dynamisch.GetRohstoffwithID(rohstoffId).ErmittleBenoetigtenLagerplatz(GetLagerbestand(stadtId, rohstoffId));
            int gesamterLagerplatz = spieler.ErmittleLagerplatzInStadt(stadtId, rohstoffId);
            int freierLagerplatz = gesamterLagerplatz - belegterLagerplatz;

            return "Freier Lagerplatz: " + freierLagerplatz + " m² (" + belegterLagerplatz + " m²/" + gesamterLagerplatz + " m² belegt)";
        }

        #endregion

        #region Rohstoffe direkt kaufen und verkaufen

        /// <summary>
        /// Verkauft Rohstoffe aus dem eigenen Lager zum Stadtpreis (wird auf den Lagerbestand begrenzt).
        /// </summary>
        /// <returns>Der erzielte Erlös.</returns>
        [PublicAPI]
        public int VerkaufeRohstoff(int stadtId, int rohstoffId, int anzahl)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int vorhanden = spieler.GetStadtRohstoffAnzahl(stadtId, rohstoffId);

            if (anzahl > vorhanden)
                anzahl = vorhanden;

            if (anzahl <= 0)
                return 0;

            SW.Dynamisch.CheckGesetzesVerstossMitRohIDx(rohstoffId);

            spieler.SetStadtRohstoffAnzahl(stadtId, rohstoffId, vorhanden - anzahl);
            spieler.ErhoeheEinVerkaeufeInStadtXVonRohstoffIDYUmZ(stadtId, rohstoffId, anzahl);

            int erloes = SW.Dynamisch.GetStadtwithID(stadtId).GetRohstoffPreisVonIDX(rohstoffId) * anzahl;
            spieler.ErhoeheTaler(erloes);
            spieler.ErhoeheUmsatzInStadtX(erloes, stadtId);

            return erloes;
        }

        /// <summary>
        /// Kauft Rohstoffe zum Stadtpreis plus Einkaufszuschlag (begrenzt durch Stadtvorrat,
        /// eigene Taler und den verfügbaren Lagerraum).
        /// </summary>
        /// <returns>Die tatsächlich gekaufte (und eingelagerte) Anzahl.</returns>
        [PublicAPI]
        public int KaufeRohstoff(int stadtId, int rohstoffId, int anzahl, out string fehler)
        {
            fehler = "";
            var spieler = SW.Dynamisch.GetAktHum();
            int aktuelleAnzahl = spieler.GetStadtRohstoffAnzahl(stadtId, rohstoffId);

            if (aktuelleAnzahl + anzahl > SW.Statisch.GetMaxAnzahlVonEinemRohstoff())
                anzahl = SW.Statisch.GetMaxAnzahlVonEinemRohstoff() - aktuelleAnzahl;

            if (anzahl <= 0)
                return 0;

            if (anzahl > SW.Dynamisch.GetStadtwithID(stadtId).GetRohstoffIDXVorrat(rohstoffId))
            {
                fehler = "Der Lagerstand in der Stadt reicht nicht aus.";
                return 0;
            }

            int preis = SW.Dynamisch.GetStadtwithID(stadtId).GetRohstoffPreisVonIDX(rohstoffId) + SW.Statisch.GetEinkaufspreisZuschlag();

            if (anzahl * preis >= spieler.GetTaler())
            {
                fehler = "Dafür fehlen Euch die Taler.";
                return 0;
            }

            int anzahlGesamtMoeglich = spieler.SetStadtRohstoffAnzahl(stadtId, rohstoffId, aktuelleAnzahl + anzahl);
            int gekauft = anzahlGesamtMoeglich - aktuelleAnzahl;

            if (gekauft <= 0)
            {
                fehler = "Ihr besitzt für diesen Rohstoff keinen ausreichenden Lagerraum.";
                return 0;
            }

            if (gekauft < anzahl)
                fehler = "Ihr besitzt für diesen Rohstoff nicht genügend Lagerraum. Es konnte nur eine Menge von " + gekauft + " eingelagert werden.";

            spieler.ErhoeheEinVerkaeufeInStadtXVonRohstoffIDYUmZ(stadtId, rohstoffId, -gekauft);
            spieler.ErhoeheTaler(-(preis * gekauft));

            return gekauft;
        }

        #endregion

        #region Produktionsslots

        [PublicAPI]
        public Produktionsslot GetProduktionsslot(int stadtId, int slot)
        {
            return SW.Dynamisch.GetAktHum().GetProduktionsslot(stadtId, slot);
        }

        /// <summary>
        /// Stellt die Aktionsart des Produktionsslots um. Wird ein Verkaufsauftrag verlassen,
        /// wandert die dafür reservierte Ware zurück ins Lager.
        /// </summary>
        [PublicAPI]
        public void SetzeTaetigkeit(int stadtId, int slot, EnumProduktionsslotAktionsart aktionsart)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);
            var alteAktionsart = (EnumProduktionsslotAktionsart)produktionsslot.GetTaetigkeit();

            if (alteAktionsart == aktionsart)
                return;

            bool warVerkauf = alteAktionsart == EnumProduktionsslotAktionsart.Verkaufen || alteAktionsart == EnumProduktionsslotAktionsart.PermanentVerkaufen;
            bool wirdVerkauf = aktionsart == EnumProduktionsslotAktionsart.Verkaufen || aktionsart == EnumProduktionsslotAktionsart.PermanentVerkaufen;

            if (warVerkauf && !wirdVerkauf)
            {
                spieler.VeraenderStadtRohstoffAnzahl(stadtId, produktionsslot.GetVerkaufRohstoff(), produktionsslot.GetVerkaufAnzahl());
                produktionsslot.SetVerkaufAnzahl(0);
            }

            produktionsslot.SetTaetigkeit((int)aktionsart);
        }

        /// <summary>
        /// Stellt sicher, dass der Produktionsrohstoff des Slots gültig ist (Rohstoffrecht und
        /// Werkstätte vorhanden), und wählt sonst den nächsten gültigen Rohstoff der Stadt.
        /// </summary>
        /// <returns>Die gültige Rohstoff-ID oder 0, wenn der Spieler in der Stadt keine Werkstätte besitzt.</returns>
        [PublicAPI]
        public int KorrigiereProduktionsRohstoff(int stadtId, int slot)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);
            int rohstoffId = produktionsslot.GetProduktionRohstoff();
            int werkstattNr = SW.Dynamisch.GetWerkposInStadtXzuRohIDy(stadtId, rohstoffId);

            if (rohstoffId != 0 && spieler.GetRohstoffrechteX(rohstoffId) && werkstattNr > 0 && HatWerkstatt(stadtId, werkstattNr))
                return rohstoffId;

            for (int i = 1; i <= SW.Statisch.GetMaxWerkstaettenProStadt(); i++)
            {
                if (HatRohstoffrecht(stadtId, i) && HatWerkstatt(stadtId, i))
                {
                    rohstoffId = RohstoffIdAnPlatz(stadtId, i);
                    produktionsslot.SetProduktionRohstoff(rohstoffId);
                    return rohstoffId;
                }
            }

            return 0;
        }

        /// <summary>
        /// Stellt sicher, dass Verkaufsrohstoff und Zielstadt des Slots gültig belegt sind.
        /// </summary>
        [PublicAPI]
        public void KorrigiereVerkaufsEinstellungen(int stadtId, int slot)
        {
            var produktionsslot = SW.Dynamisch.GetAktHum().GetProduktionsslot(stadtId, slot);

            if (produktionsslot.GetVerkaufRohstoff() == 0)
                produktionsslot.SetVerkaufRohstoff(RohstoffIdAnPlatz(stadtId, 1));

            if (produktionsslot.GetVerkaufStadt() == 0 || produktionsslot.GetVerkaufStadt() == stadtId)
            {
                for (int i = SW.Statisch.GetMinStadtID(); i < SW.Statisch.GetMaxStadtID(); i++)
                {
                    if (i != stadtId)
                    {
                        produktionsslot.SetVerkaufStadt(i);
                        break;
                    }
                }
            }
        }

        [PublicAPI]
        public void SetzeProduktionsRohstoff(int stadtId, int slot, int rohstoffId)
        {
            var produktionsslot = SW.Dynamisch.GetAktHum().GetProduktionsslot(stadtId, slot);
            produktionsslot.SetProduktionRohstoff(rohstoffId);
            produktionsslot.SetProduktionStaetten(0);
        }

        /// <summary>
        /// Schaltet den Produktionsrohstoff des Slots auf den nächsten Rohstoff der Stadt weiter, für den
        /// der Spieler sowohl das Rohstoffrecht als auch eine Werkstätte besitzt (mit Umbruch am Ende).
        /// </summary>
        [PublicAPI]
        public void NaechsterProduktionsRohstoff(int stadtId, int slot)
        {
            var produktionsslot = GetProduktionsslot(stadtId, slot);
            int werkstattNr = SW.Dynamisch.GetWerkposInStadtXzuRohIDy(stadtId, produktionsslot.GetProduktionRohstoff());

            for (int i = 0; i < SW.Statisch.GetMaxWerkstaettenProStadt(); i++)
            {
                werkstattNr++;

                if (werkstattNr > SW.Statisch.GetMaxWerkstaettenProStadt())
                    werkstattNr = 1;

                if (HatRohstoffrecht(stadtId, werkstattNr) && HatWerkstatt(stadtId, werkstattNr))
                {
                    int neueRohstoffId = RohstoffIdAnPlatz(stadtId, werkstattNr);

                    if (neueRohstoffId != produktionsslot.GetProduktionRohstoff())
                        SetzeProduktionsRohstoff(stadtId, slot, neueRohstoffId);

                    break;
                }
            }
        }

        [PublicAPI]
        public void SetzeProduktionsArbeiter(int stadtId, int slot, int anzahl)
        {
            if (anzahl < 0)
                anzahl = 0;

            if (anzahl > SW.Statisch.GetMaxArbeiterAnzahl())
                anzahl = SW.Statisch.GetMaxArbeiterAnzahl();

            SW.Dynamisch.GetAktHum().GetProduktionsslot(stadtId, slot).SetProduktionArbeiter(anzahl);
        }

        [PublicAPI]
        public void SetzeProduktionsStaetten(int stadtId, int slot, int anzahl)
        {
            if (anzahl < 0)
                anzahl = 0;

            SW.Dynamisch.GetAktHum().GetProduktionsslot(stadtId, slot).SetProduktionStaetten(anzahl);
        }

        /// <summary>
        /// Wechselt den Rohstoff eines Verkaufsauftrags. Die bisher reservierte Ware wandert zurück ins Lager.
        /// </summary>
        [PublicAPI]
        public void SetzeVerkaufsRohstoff(int stadtId, int slot, int rohstoffId)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

            if (produktionsslot.GetVerkaufAnzahl() != 0)
                spieler.VeraenderStadtRohstoffAnzahl(stadtId, produktionsslot.GetVerkaufRohstoff(), produktionsslot.GetVerkaufAnzahl());

            produktionsslot.SetVerkaufRohstoff(rohstoffId);
            produktionsslot.SetVerkaufAnzahl(0);
        }

        /// <summary>
        /// Schaltet den Verkaufsrohstoff des Slots auf den nächsten Rohstoff der Stadt weiter (mit Umbruch).
        /// </summary>
        [PublicAPI]
        public void NaechsterVerkaufsRohstoff(int stadtId, int slot)
        {
            var produktionsslot = GetProduktionsslot(stadtId, slot);
            int werkstattNr = SW.Dynamisch.GetWerkposInStadtXzuRohIDy(stadtId, produktionsslot.GetVerkaufRohstoff());

            werkstattNr++;

            if (werkstattNr > SW.Statisch.GetMaxWerkstaettenProStadt())
                werkstattNr = 1;

            int neueRohstoffId = RohstoffIdAnPlatz(stadtId, werkstattNr);

            if (neueRohstoffId != 0)
                SetzeVerkaufsRohstoff(stadtId, slot, neueRohstoffId);
        }

        /// <summary>
        /// Reserviert die zu verkaufende Anzahl aus dem Lager (begrenzt auf den Bestand).
        /// </summary>
        /// <returns>Die tatsächlich reservierte Anzahl.</returns>
        [PublicAPI]
        public int SetzeVerkaufsAnzahl(int stadtId, int slot, int anzahl)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);
            int rohstoffId = produktionsslot.GetVerkaufRohstoff();
            int vorhanden = spieler.GetStadtRohstoffAnzahl(stadtId, rohstoffId) + produktionsslot.GetVerkaufAnzahl();

            if (anzahl > vorhanden)
                anzahl = vorhanden;

            if (anzahl < 0)
                anzahl = 0;

            produktionsslot.SetVerkaufAnzahl(anzahl);
            spieler.SetStadtRohstoffAnzahl(stadtId, rohstoffId, vorhanden - anzahl);

            return anzahl;
        }

        [PublicAPI]
        public void SetzeVerkaufsStadt(int stadtId, int slot, int zielStadtId)
        {
            if (zielStadtId == stadtId)
                return;

            SW.Dynamisch.GetAktHum().GetProduktionsslot(stadtId, slot).SetVerkaufStadt(zielStadtId);
        }

        /// <summary>
        /// Setzt die Zielstadt des Verkaufsauftrags aus einem gewünschten Stadtwert (z. B. von einem
        /// Zähler): die eigene Stadt wird in Änderungsrichtung übersprungen und der Wert am Rand umgebrochen.
        /// </summary>
        [PublicAPI]
        public void SetzeVerkaufsStadtAusWert(int stadtId, int slot, int gewuenschteStadt)
        {
            var produktionsslot = GetProduktionsslot(stadtId, slot);
            int alteStadt = produktionsslot.GetVerkaufStadt();
            int neueStadt = gewuenschteStadt;

            if (neueStadt == stadtId)
                neueStadt += neueStadt > alteStadt ? 1 : -1;

            if (neueStadt >= SW.Statisch.GetMaxStadtID())
                neueStadt = SW.Statisch.GetMinStadtID();
            else if (neueStadt < SW.Statisch.GetMinStadtID())
                neueStadt = SW.Statisch.GetMaxStadtID() - 1;

            produktionsslot.SetVerkaufStadt(neueStadt);
        }

        [PublicAPI]
        public int BerechneKosten(int stadtId, int slot)
        {
            return SW.Dynamisch.BerechneProdKosten(stadtId, slot);
        }

        #endregion

        #region Karawane

        [PublicAPI]
        public Karawane GetKarawane(int stadtId)
        {
            return SW.Statisch.GetKarawane(SW.Dynamisch.GetAktHum().GetKarawaneInStadtX(stadtId));
        }

        /// <summary>
        /// Beauftragt die nächste verfügbare Karawane für die Transporte aus dieser Stadt.
        /// </summary>
        [PublicAPI]
        public Karawane NaechsteKarawane(int stadtId)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int karawaneId = spieler.GetKarawaneInStadtX(stadtId) + 1;

            if (karawaneId >= SW.Statisch.GetMaxKarawane())
                karawaneId = SW.Statisch.GetMinKarawane();

            spieler.SetKarawaneInStadtXzuY(stadtId, karawaneId);
            return SW.Statisch.GetKarawane(karawaneId);
        }

        #endregion
    }
}
