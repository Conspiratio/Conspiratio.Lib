using System;

namespace Conspiratio.Lib.Gameplay.Privilegien.FestGeben
{
    [Serializable]
    public class Fest
    {
        public int SpielerID { get; set; }

        public int StadtID { get; set; }

        public EnumFestGroesse Groesse { get; set; }

        public EnumFestMusiker Musiker { get; set; }

        public int Jahr { get; set; }

        public int GeplanteKosten { get; set; }

        public Fest(int spielerID, int stadtID, EnumFestGroesse groesse, EnumFestMusiker musiker, int jahr, int geplanteKosten)
        {
            SpielerID = spielerID;
            StadtID = stadtID;
            Groesse = groesse;
            Musiker = musiker;
            Jahr = jahr;
            GeplanteKosten = geplanteKosten;
        }
    }
}
