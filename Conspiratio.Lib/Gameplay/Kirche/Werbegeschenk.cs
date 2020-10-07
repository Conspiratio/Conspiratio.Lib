namespace Conspiratio.Lib.Gameplay.Kirche
{
    public class Werbegeschenk
    {
        public int ID { get; }
        public int BonusRomantik { get; }
        public int BonusBoese { get; }
        public int BonusPreis { get; }
        public int Basispreis { get; }
        public double Vermoegensfaktor { get; }
        public string Text { get; }

        public Werbegeschenk(int id, int bonusRomantik, int bonusBoese, int bonusPreis, double vermoegensfaktor, int basispreis, string text)
        {
            ID = id;
            BonusRomantik = bonusRomantik;
            BonusBoese = bonusBoese;
            BonusPreis = bonusPreis;
            Basispreis = basispreis;
            Vermoegensfaktor = vermoegensfaktor;
            Text = text;
        }
    }
}
