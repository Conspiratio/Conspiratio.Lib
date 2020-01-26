namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    public interface IZufallsereignis
    {
        int AnsehenMultiplikator { get; set; }
        int GesundheitMultiplikator { get; set; }
        string Nachricht { get; set; }
        EnumGueltigkeitReligion NurGueltigReligion { get; set; }
        int TalerMultiplikator { get; set; }
        string Ueberschrift { get; set; }

        /// <summary>
        /// Die Methode prüft, ob das Ereignis für die übergebene Relegion (des Spielers) gültig ist anhand der "NurGueltigReligion" Property und 
        /// gibt true (gültig) oder false (nicht gültig) zurück.
        /// </summary>
        /// <param name="religionSpieler">Aktuelle Relegion des Spielers, für den geprüft werden soll, ob das Ereignis gültig ist.</param>
        /// <returns>Gültig (true) oder nicht gültig (false)</returns>
        bool IstEreignisGueltig(int religionSpieler);
    }
}