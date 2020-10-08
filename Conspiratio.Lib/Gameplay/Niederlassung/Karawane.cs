namespace Conspiratio.Lib.Gameplay.Niederlassung
{
    public class Karawane
    {
        public int ID { get; }
        public int Fixpreis { get; }
        public int Sicherheit { get; }
        public int Verlaesslichkeit { get; }
        public string Beschreibung { get; }
        public int PreisProStueck { get; }
        public int Kapazitaet { get; }

        public Karawane(int id, int fixpreis, int preisProStueck, int kapazitaet, int sicherheit, int verlaesslichkeit, string beschreibung)
        {
            ID = id;
            Fixpreis = fixpreis;
            PreisProStueck = preisProStueck;
            Kapazitaet = kapazitaet;
            Sicherheit = sicherheit;
            Verlaesslichkeit = verlaesslichkeit;
            Beschreibung = beschreibung;
        }
    }
}
