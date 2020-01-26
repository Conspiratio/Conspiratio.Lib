using System;
using System.Collections.Generic;
using System.Linq;

namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    public class Datumsereignis : Zufallsereignis, IDatumsereignis
    {
        #region Variablen

        public int ID { get; set; }
        public DateTime GueltigVonDatum { get; set; }
        public DateTime GueltigBisDatum { get; set; }
        public bool NurAnOsternGueltig { get; set; } = false;

        #endregion

        #region Methoden

        /// <inheritdoc />
        public bool IstEreignisGueltig(int religionSpieler, List<Ereigniszeitpunkt> EreignisseZuletztPassiert)
        {
            if (!IstEreignisGueltig(religionSpieler))
                return false;

            var AktuellesDatum = DateTime.Now;

            if ((EreignisseZuletztPassiert != null) && 
                 EreignisseZuletztPassiert.FirstOrDefault(Ereigniszeitpunkt => Ereigniszeitpunkt.EreignisID == ID)?.Zeitpunkt.Year == AktuellesDatum.Year)
                return false;  // Das Ereignis ist in diesem Jahr bereits einmal passiert

            if (NurAnOsternGueltig && (GueltigVonDatum == DateTime.MinValue) && (GueltigBisDatum == DateTime.MinValue))
            {
                // Berechne Ostern und setze es in die entsprechenden Datumsfelder
                var Ostersonntag = ErmittleOstersonntag(AktuellesDatum.Year);
                GueltigVonDatum = Ostersonntag.AddDays(-2);  // Karfreitag
                GueltigBisDatum = Ostersonntag.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);   // Ostermontag
            }

            return (GueltigVonDatum <= AktuellesDatum && GueltigBisDatum >= AktuellesDatum);
        }

        /// <summary>
        /// Errechnet das Datum des Ostersonntags aus dem übergebenen Jahr
        /// </summary>
        /// <param name="int">Das Jahr in YYYY Schreibweise</param>
        /// <returns>Das errechnete Datum des Ostersonntags in dem angegebene Jahr</returns>
        private DateTime ErmittleOstersonntag(int jahr)
        {
            // Quelle: https://de.wikibooks.org/wiki/Algorithmensammlung:_Kalender:_Feiertage
            int x = jahr;   // das Jahr
            int k;          // die Säkularzahl
            int m;          // die säkulare Mondschaltung
            int s;          // die säkulare Sonnenschaltung
            int a;          // der Mondparameter
            int d;          // der Keim für den ersten Vollmond im Frühling
            int r;          // die kalendarische Korrekturgröße
            int og;         // die Ostergrenze
            int sz;         // der ersten Sonntag im März
            int oe;         // die Entfernung des Ostersonntags von der Ostergrenze
            int os;         // das Datum des Ostersonntags als Märzdatum (32.März = 1.April)
            int OsterTag;
            int OsterMonat;

            k = x / 100;
            m = 15 + (3 * k + 3) / 4 - (8 * k + 13) / 25;
            s = 2 - (3 * k + 3) / 4;
            a = x % 19;
            d = (19 * a + m) % 30;
            r = (d + a / 11) / 29;
            og = 21 + d - r;
            sz = 7 - (x + x / 4 + s) % 7;
            oe = 7 - (og - sz) % 7;
            os = og + oe;

            OsterMonat = 2 + (int)(os + 30) / 31;
            OsterTag = os - 31 * ((int)OsterMonat / 4);

            return new DateTime(jahr, OsterMonat, OsterTag);
        }

        #endregion
    }
}
