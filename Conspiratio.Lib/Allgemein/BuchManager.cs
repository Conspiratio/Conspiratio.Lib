using System;

using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Wendet zu Jahresbeginn die Produktions- und Verkaufsaufträge des aktiven Spielers an
    /// (das "Buch" des alten WinForms-Clients): Exporte werden verkauft und gutgeschrieben,
    /// die Produktion wird eingelagert und permanente Verkäufe reservieren den neuen Bestand.
    /// </summary>
    public class BuchManager
    {
        [PublicAPI]
        public BuchErgebnis ErstelleJahresbuchFuerAktivenSpieler()
        {
            var ergebnis = new BuchErgebnis(SW.Statisch.GetMaxRohID());
            var spieler = SW.Dynamisch.GetAktHum();

            #region Verkauf (Exporte des letzten Jahres abwickeln)

            for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
            {
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    if ((produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Verkaufen ||
                         produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.PermanentVerkaufen) &&
                        produktionsslot.GetVerkaufAnzahl() > 0)
                    {
                        ergebnis.EtwasExportiert = true;

                        int rohstoffId = produktionsslot.GetVerkaufRohstoff();
                        SW.Dynamisch.CheckGesetzesVerstossMitRohIDx(rohstoffId);

                        int anzahl = produktionsslot.GetVerkaufAnzahl();
                        int zielstadt = produktionsslot.GetVerkaufStadt();
                        int rohstoffpreis = SW.Dynamisch.GetStadtwithID(zielstadt).GetRohstoffPreisVonIDX(rohstoffId);

                        ergebnis.ExportierteWaren[rohstoffId] += anzahl;
                        ergebnis.ExportErloese[rohstoffId] += rohstoffpreis * anzahl;

                        // Änderung der Rohstoffvorräte der Stadt speichern
                        spieler.ErhoeheEinVerkaeufeInStadtXVonRohstoffIDYUmZ(stadtId, rohstoffId, anzahl);

                        // Einnahmen zum Umsatz hinzuzählen
                        spieler.ErhoeheUmsatzInStadtX(rohstoffpreis * anzahl, stadtId);

                        produktionsslot.SetVerkaufAnzahl(0);

                        // Wurde Ware bei einem Überfall gestohlen?
                        if (produktionsslot.GetGestohlenAnzahl() > 0)
                        {
                            ergebnis.EtwasGestohlen = true;
                            ergebnis.GestohleneWaren[rohstoffId] += produktionsslot.GetGestohlenAnzahl();
                            produktionsslot.SetGestohlenAnzahl(0);
                        }
                    }
                }
            }

            // Erlöse gutschreiben (im WinForms-Client geschah das bei der Anzeige der Buchseite)
            for (int rohstoffId = 1; rohstoffId < SW.Statisch.GetMaxRohID(); rohstoffId++)
            {
                if (ergebnis.ExportErloese[rohstoffId] != 0)
                    spieler.ErhoeheTaler(ergebnis.ExportErloese[rohstoffId]);
            }

            #endregion

            #region Produktion

            for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
            {
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    if (produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.Produzieren)
                        continue;

                    int rohstoffId = produktionsslot.GetProduktionRohstoff();

                    int qualitaetProzent = ergebnis.ProduktionsQualitaetProzent[rohstoffId];
                    int produziert = produktionsslot.GetProduktion(stadtId, rohstoffId, ref qualitaetProzent);
                    ergebnis.ProduktionsQualitaetProzent[rohstoffId] = qualitaetProzent;

                    if (spieler.CheckPrivilegX(32))
                        produziert = Convert.ToInt32(produziert * 1.02);

                    ergebnis.ProduzierteWaren[rohstoffId] += produziert;

                    if (produziert > 0)
                    {
                        ergebnis.EtwasProduziert = true;

                        int alteAnzahl = spieler.GetStadtRohstoffAnzahl(stadtId, rohstoffId);
                        int neueAnzahl = spieler.SetStadtRohstoffAnzahl(stadtId, rohstoffId, alteAnzahl + produziert);

                        // Was nicht eingelagert werden konnte, geht verloren
                        int verloren = (alteAnzahl + produziert) - neueAnzahl;

                        if (verloren > 0)
                        {
                            ergebnis.EtwasVerloren = true;
                            ergebnis.VerloreneWaren[rohstoffId] += verloren;
                        }
                    }
                }
            }

            #endregion

            #region Permanenter Verkauf (neuen Lagerbestand für das nächste Jahr reservieren)

            for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
            {
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    if (produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                        continue;

                    int rohstoffId = produktionsslot.GetVerkaufRohstoff();
                    int vorhandeneAnzahl = spieler.GetStadtRohstoffAnzahl(stadtId, rohstoffId);

                    if (vorhandeneAnzahl != 0)
                    {
                        produktionsslot.SetVerkaufAnzahl(vorhandeneAnzahl);
                        spieler.SetStadtRohstoffAnzahl(stadtId, rohstoffId, 0);
                    }
                }
            }

            #endregion

            return ergebnis;
        }

        /// <summary>
        /// Wandelt die Produktionsqualität in den anzeigbaren Text um (schlecht bis ausgezeichnet).
        /// </summary>
        [PublicAPI]
        public static string QualitaetAlsText(int qualitaetProzent)
        {
            if (qualitaetProzent <= 20)
                return "schlecht";

            if (qualitaetProzent <= 40)
                return "mäßig";

            if (qualitaetProzent <= 60)
                return "normal";

            if (qualitaetProzent <= 80)
                return "gut";

            return "ausgezeichnet";
        }
    }
}
