using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    /// <summary>
    /// Kapselt die Verleihung eines neuen Handelszertifikats an den aktiven Spieler (Migration von
    /// Handelszertifikat/HandelszertifikatAnzeigen aus dem WinForms-Client). Ob eines ansteht, wird
    /// beim Amtsgewinn bzw. Stützpunktkauf über die Lib gesetzt ("BekamHandelszertifikat"). Steht eines
    /// an, liefert dieser Manager den Urkundentext (samt ausstellendem Rat) und quittiert die Verleihung.
    /// </summary>
    public class HandelszertifikatManager
    {
        /// <summary>Prüft, ob dem Spieler ein neues Handelszertifikat verliehen werden soll.</summary>
        public bool StehtZertifikatverleihungAn()
        {
            return SW.Dynamisch.GetAktHum().GetBekamHandeslzertifikatX() != 0;
        }

        /// <summary>
        /// Vollzieht die Verleihung: erstellt den Urkundentext (mit dem ausstellenden Rat) und setzt den
        /// "BekamHandelszertifikat"-Vermerk zurück. Vor dem Aufruf sollte <see cref="StehtZertifikatverleihungAn"/> gelten.
        /// </summary>
        public HandelszertifikatErgebnis Vollziehe()
        {
            var spieler = SW.Dynamisch.GetAktHum();
            int rohstoffId = spieler.GetBekamHandeslzertifikatX();
            string rohstoffName = SW.Dynamisch.GetRohstoffwithID(rohstoffId).GetRohName();

            string aussteller = ErmittleAussteller(rohstoffId, spieler);

            string text = "Aufgrund Eurer besonderen Erfolge\nin Amt und Würden wird Euch,\n" + spieler.GetName() +
                          ",\n ab heute gestattet, mit\n" + rohstoffName + " zu handeln.\n \n" + aussteller;

            spieler.SetBekamHandelszertifikatX(0);

            return new HandelszertifikatErgebnis
            {
                UrkundenText = text,
                RohstoffName = rohstoffName,
                RohstoffId = rohstoffId
            };
        }

        /// <summary>
        /// Ermittelt den ausstellenden Rat je nach Tier-Stufe des Rohstoffs: für niedrige Rohstoffe der
        /// Rat der (Amts- bzw. Wohnsitz-)Stadt, für mittlere der Rat eines Landes mit mindestens zwei
        /// Wohnsitzen (sonst des Reichs) und für hohe der Rat des Reichs.
        /// </summary>
        private static string ErmittleAussteller(int rohstoffId, HumSpieler spieler)
        {
            if (rohstoffId < 8)
            {
                // Die Stadt, in der der Spieler ein Amt bekleidet ...
                if (spieler.GetAmtID() != 0 && spieler.GetAmtID() < SW.Statisch.GetMaxAmtStadtID())
                    return " der Rat der Stadt " + SW.Dynamisch.GetStadtwithID(spieler.GetAmtGebiet()).GetGebietsName();

                // ... oder eine Stadt, in der er einen Wohnsitz besitzt.
                string aussteller = "";

                for (int stadtId = SW.Statisch.GetMinStadtID(); stadtId < SW.Statisch.GetMaxStadtID(); stadtId++)
                {
                    if (spieler.GetSpielerHatHausVonStadtAnArraystelle(stadtId).GetHausID() != 0)
                        aussteller = " der Rat der Stadt " + SW.Dynamisch.GetStadtwithID(stadtId).GetGebietsName();
                }

                return aussteller;
            }

            if (rohstoffId < 15)
            {
                // Ein Land, in dem der Spieler mindestens zwei Wohnsitze hat.
                for (int landId = 1; landId < SW.Statisch.GetMaxLandID(); landId++)
                {
                    var land = SW.Dynamisch.GetLandWithID(landId);
                    int wohnsitze = 0;

                    for (int j = 0; j < land.GetAnzahlStaedte(); j++)
                    {
                        if (spieler.GetSpielerHatHausVonStadtAnArraystelle(land.GetStadtX(j)).GetHausID() != 0)
                            wohnsitze++;
                    }

                    if (wohnsitze >= 2)
                        return " der Rat des Landes " + land.GetGebietsName();
                }

                return " der Rat des Reichs " + SW.Dynamisch.GetReichWithID(1).GetGebietsName();
            }

            return " der Rat des Reichs " + SW.Dynamisch.GetReichWithID(1).GetGebietsName();
        }
    }

    /// <summary>Das Ergebnis einer Handelszertifikat-Verleihung mit dem anzuzeigenden Urkundentext.</summary>
    public class HandelszertifikatErgebnis
    {
        public string UrkundenText { get; set; }

        /// <summary>Der Rohstoffname (für die Sprachausgabe des Clients).</summary>
        public string RohstoffName { get; set; }

        public int RohstoffId { get; set; }
    }
}
