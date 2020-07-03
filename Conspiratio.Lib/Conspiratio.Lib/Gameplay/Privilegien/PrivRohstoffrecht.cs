using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivRohstoffrecht : Privileg
    {
        public PrivRohstoffrecht() : base("Handelszertifikate", 4)
        {
        }

        public override void PrivExecute()
        {
            string text = "Ihr besitzt die Fähigkeiten und Berechtigungen um folgende Betriebe zu führen:\n";

            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                if (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetRohstoffrechteX(i))
                {
                    text += SW.Dynamisch.GetRohstoffwithID(i).GetRohName() + ", ";
                }
            }

            text = text.Substring(0, text.Length - 2); //Beistrich entfernen
            SW.Dynamisch.BelTextAnzeigen(text);
        }
    }
}
