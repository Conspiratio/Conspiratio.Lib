using System;

namespace Conspiratio.Lib.Gameplay.Personen
{
    [Serializable]
    public class SpielerStatistik
    {
        //Hinterzimmer
        public int HiSpionagen { get; set; }
        public int HiSabotagen { get; set; }
        public int HiVersuchteErmordungen { get; set; }
        public int HiErfolgreicheErmordungen { get; set; }
        public int HiBestechungen { get; set; }
        public int HiKartenSpielen { get; set; }
        public int HiAnschwaerzungen { get; set; }
        public int HiBestechungssumme { get; set; }

        // Kirche
        public int KgekaufteAblaesse { get; set; }
        public int KabgelegteBeichten { get; set; }
        public int KHochzeiten { get; set; }
        public int KKonvertierungen { get; set; }

        // Schreibstube
        public int SgenommeneKredite { get; set; }
        public int SWahlenGewonnen { get; set; }
        public int SWahlenTeilgenommen { get; set; }

        // Handel
        public int HaWarenVerkauft { get; set; }
        public int HaWarenEingekauft { get; set; }
        public int HaentrichteteSteuern { get; set; }
        public int HaentrichteteZoelle { get; set; }
        public int HaDurchschnProfitProVerkWare { get; set; }

        // Sonstiges
        public int SogebrocheneGesetze { get; set; }
        public int Soangeklagt { get; set; }
        public int SoSchuldturmaufenthalte { get; set; }
        public int SoHoechstesAmt { get; set; }
        public int SoGesamtumsatz { get; set; }
        public int SogezeugteKinder { get; set; }
        public int SoAmtseinkommen { get; set; }

        //Muss erst wegen der Arraylaenge durchdacht werden...
        ////ProRunde
        //public int[] PRGesundheit;
        //public int[] PRGesamtvermoegen;
        //public int[] PRBarvermoegen;

        public SpielerStatistik()
        {

        }
    }
}
