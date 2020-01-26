namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    public class Zufallsereignis : IZufallsereignis
    {
        #region Variablen

        public string Ueberschrift { get; set; }
        public string Nachricht { get; set; }  // Der Talerbetrag (falls vorhanden) wird in den Platzhalter {0} eingefügt
        public EnumGueltigkeitReligion NurGueltigReligion { get; set; } = EnumGueltigkeitReligion.ReligionIstEgal;
        public int TalerMultiplikator { get; set; } = 0;
        public int AnsehenMultiplikator { get; set; } = 0;
        public int GesundheitMultiplikator { get; set; } = 0;

        #endregion

        #region Methoden

        /// <inheritdoc />
        public bool IstEreignisGueltig(int religionSpieler)
        {
            return (NurGueltigReligion == EnumGueltigkeitReligion.ReligionIstEgal) ||
                   ((NurGueltigReligion == EnumGueltigkeitReligion.NurGueltigMitReligion) && (religionSpieler != 0)) ||
                   ((NurGueltigReligion == EnumGueltigkeitReligion.NurGueltigOhneReligion) && (religionSpieler == 0));
        }

        #endregion
    }
}
