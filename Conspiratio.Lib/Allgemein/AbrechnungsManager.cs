using System;

using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Berechnet und verbucht die Jahresabrechnung des aktiven Spielers beim Zugende.
    /// Extrahiert aus dem alten WinForms-Dialog "Abrechnung".
    /// </summary>
    public class AbrechnungsManager
    {
        /// <summary>
        /// Berechnet alle Kostenpositionen des aktiven Spielers, zahlt Zölle an die
        /// Zollburg-Besitzer aus, zieht die Gesamtkosten von den Talern des Spielers ab
        /// und setzt seine Umsätze zurück.
        /// </summary>
        [PublicAPI]
        public AbrechnungsErgebnis ErstelleAbrechnungFuerAktivenSpieler()
        {
            var ergebnis = new AbrechnungsErgebnis();
            var spieler = SW.Dynamisch.GetAktHum();

            #region Arbeiter-, Betriebs- und Transportkosten

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                    if (produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Produzieren)
                    {
                        int rohstoffId = produktionsslot.GetProduktionRohstoff();

                        ergebnis.Arbeiterkosten += produktionsslot.GetProduktionArbeiter() * SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetWSArbeiterpreis();
                        ergebnis.Betriebskosten += produktionsslot.GetProduktionStaetten() * SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetWSEinzelpreis();
                    }

                    if (produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Verkaufen ||
                        produktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                    {
                        int karawaneId = spieler.GetKarawaneInStadtX(stadtId);
                        int anzahlExportiert = produktionsslot.GetVerkaufAnzahl();

                        if (anzahlExportiert != 0)
                        {
                            // anzahlExportiert - 1, damit bei z. B. 400 Waren und Kapazität 100 noch 4 Fuhren und nicht 5 genommen werden
                            double bruch = (anzahlExportiert - 1) / SW.Statisch.GetKarawane(karawaneId).Kapazitaet;
                            double anzahlFuhren = Convert.ToInt32(bruch) + 1;

                            ergebnis.Transportkosten += Convert.ToInt32(SW.Statisch.GetKarawane(karawaneId).Fixpreis + SW.Statisch.GetKarawane(karawaneId).PreisProStueck * anzahlFuhren);
                        }
                    }
                }
            }

            ergebnis.Gesamtkosten += ergebnis.Arbeiterkosten + ergebnis.Betriebskosten + ergebnis.Transportkosten;

            #endregion

            #region Verkaufssteuern (angezeigt wird der volle Betrag, abgezogen der um die Steuerhinterziehung reduzierte)

            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
            {
                ergebnis.Verkaufssteuern += Convert.ToInt32(SW.Dynamisch.GetStadtwithID(stadtId).GetUmsatzsteuer() * spieler.GetUmsatzInStadtX(stadtId));
            }

            double steuerhinterziehung = 0;

            if (spieler.CheckPrivilegX(27))
                steuerhinterziehung = 0.2;

            if (spieler.CheckPrivilegX(28))
                steuerhinterziehung = 0.4;

            if (spieler.CheckPrivilegX(29))
                steuerhinterziehung = 0.6;

            ergebnis.Gesamtkosten += Convert.ToInt32(ergebnis.Verkaufssteuern * (1 - steuerhinterziehung));

            #endregion

            #region Informanten und Saboteure

            for (int i = 1; i < SW.Statisch.GetMaxKIID(); i++)
            {
                ergebnis.Informantenkosten += spieler.GetAktiveSpionage(i).GetKosten();
                ergebnis.Saboteurekosten += spieler.GetAktiveSabotage(i).GetKosten();
            }

            ergebnis.Gesamtkosten += ergebnis.Informantenkosten + ergebnis.Saboteurekosten;

            #endregion

            #region Kreditzinsen

            for (int i = 0; i < SW.Statisch.GetMaxKredite(); i++)
            {
                if (spieler.GetKreditMitID(i).GetTaler() != 0)
                    ergebnis.Kreditzinsen += Convert.ToInt32(spieler.GetKreditMitID(i).GetTaler() * spieler.GetKreditMitID(i).GetZinsen() / 100);
            }

            ergebnis.Gesamtkosten += ergebnis.Kreditzinsen;

            #endregion

            #region Kirchenzehnt

            int gesamtUmsatz = 0;

            if (spieler.CheckPrivilegX(16) == false)
            {
                for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                    gesamtUmsatz += spieler.GetUmsatzInStadtX(stadtId);
            }

            ergebnis.Kirchenzehnt = Convert.ToInt32(SW.Statisch.GetKirchenzehnt() * gesamtUmsatz);
            ergebnis.Gesamtkosten += ergebnis.Kirchenzehnt;

            #endregion

            #region Zölle (Zollburg-Besitzer erhalten ihren Anteil sofort ausgezahlt)

            // Privilegien: Zollfreiheit (23) bzw. 50%-Chance auf Zollfreiheit (31). Ist der Spieler zollfrei,
            // zahlt er keinen Zoll – dann erhalten auch die Zollburg-Besitzer keinen Anteil ausgezahlt.
            bool zollfrei = spieler.CheckPrivilegX(23) || (spieler.CheckPrivilegX(31) && SW.Statisch.Rnd.Next(0, 2) == 0);

            if (!zollfrei)
            {
                for (int slot = 0; slot < SW.Statisch.GetMaxProdSlots(); slot++)
                {
                    for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                    {
                        var produktionsslot = spieler.GetProduktionsslot(stadtId, slot);

                        if (produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.Verkaufen &&
                            produktionsslot.GetTaetigkeit() != (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
                            continue;

                        int startland = SW.Dynamisch.GetStadtwithID(stadtId).GetLandID();
                        int zielland = SW.Dynamisch.GetStadtwithID(produktionsslot.GetVerkaufStadt()).GetLandID();

                        if (startland == zielland)
                            continue;

                        // Es wird mindestens eine Grenze überschritten
                        int grundumsatz = SW.Dynamisch.GetRohstoffwithID(produktionsslot.GetVerkaufRohstoff()).GetPreisStd() * produktionsslot.GetVerkaufAnzahl();

                        var zollburgStart = SW.Dynamisch.GetZollburgWithIDx(SW.Dynamisch.GetLandWithID(startland).GetZollburgIndex());
                        var zollburgZiel = SW.Dynamisch.GetZollburgWithIDx(SW.Dynamisch.GetLandWithID(zielland).GetZollburgIndex());

                        double zollsatzStart = zollburgStart.Zoll;
                        double zollsatzZiel = zollburgZiel.Zoll;

                        if (zollburgStart.Besitzer == SW.Dynamisch.GetAktiverSpieler())
                            zollsatzStart = 0;
                        else
                            ZahleZollAusUndSammle(zollburgStart.Besitzer, Convert.ToInt32(zollsatzStart * grundumsatz));

                        if (zollburgZiel.Besitzer == SW.Dynamisch.GetAktiverSpieler())
                            zollsatzZiel = 0;
                        else
                            ZahleZollAusUndSammle(zollburgZiel.Besitzer, Convert.ToInt32(zollsatzZiel * grundumsatz));

                        ergebnis.Zollkosten += Convert.ToInt32((zollsatzStart + zollsatzZiel) * grundumsatz);
                    }
                }
            }

            ergebnis.Gesamtkosten += ergebnis.Zollkosten;

            #endregion

            #region Sold

            foreach (Stuetzpunkt stuetzpunkt in SW.Dynamisch.GetStuetzpunkte())
            {
                if (stuetzpunkt.Besitzer != SW.Dynamisch.GetAktiverSpieler())
                    continue;

                foreach (Einheit einheit in stuetzpunkt.Einheiten)
                    ergebnis.Sold += einheit.Basispreis;
            }

            ergebnis.Gesamtkosten += ergebnis.Sold;

            #endregion

            spieler.ErhoeheTaler(-ergebnis.Gesamtkosten);

            // Umsätze des Spielers wieder auf 0 setzen
            for (int stadtId = 1; stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                spieler.SetUmsatzInStadtX(0, stadtId);

            return ergebnis;
        }

        /// <summary>
        /// Zahlt einem Zollburg-Besitzer seinen Zollanteil aus und summiert ihn – sofern der Besitzer ein
        /// menschlicher Spieler ist – in dessen Zolleinnahmen-Zähler, damit er ihm zu Zugbeginn gemeldet werden kann.
        /// </summary>
        private static void ZahleZollAusUndSammle(int besitzerId, int betrag)
        {
            SW.Dynamisch.GetSpWithID(besitzerId).ErhoeheTaler(betrag);

            if (besitzerId < SW.Statisch.GetMinKIID())
                SW.Dynamisch.GetHumWithID(besitzerId).ZolleinnahmenGesammelt += betrag;
        }
    }
}
