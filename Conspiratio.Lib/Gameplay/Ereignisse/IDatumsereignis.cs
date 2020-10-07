using System;
using System.Collections.Generic;

namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    public interface IDatumsereignis: IZufallsereignis
    {
        int ID { get; }
        DateTime GueltigBisDatum { get; set; }
        DateTime GueltigVonDatum { get; set; }
        bool NurAnOsternGueltig { get; set; }

        /// <summary>
        /// Die Methode prüft, ob das Ereignis für die übergebene Relegion (des Spielers) gültig ist anhand der "NurGueltigReligion" Property und 
        /// anhand des aktuellen Datums und gibt true (gültig) oder false (nicht gültig) zurück.
        /// </summary>
        /// <param name="religionSpieler">Aktuelle Relegion des Spielers, für den geprüft werden soll, ob das Ereignis gültig ist.</param>
        /// <param name="EreignisseZuletztPassiert">Eine Liste von Ereigniszeitpunkten des aktuellen Spielers, die angeben, wann die Ereignisse zuletzt passiert sind</param>
        /// <returns>Gültig (true) oder nicht gültig (false)</returns>
        bool IstEreignisGueltig(int religionSpieler, List<Ereigniszeitpunkt> EreignisseZuletztPassiert);
    }
}