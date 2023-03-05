using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Hinterzimmer
{
    public class Kartenspiel
    {
        public string GegnerName { get; private set; }
        public string GegnerErSie { get; private set; }
        public string GegnerSeinenIhren { get; private set; }
        
        public bool FindetKartenspielStatt => SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetSpieltKartenGegenSpielerID() != 0;

        public bool HatSpielerGenugTaler() => SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetTaler() > SW.Statisch.GetKartenSpielenMinTaler();
        
        public void InitiiereKartenspielUndErmittleGegner()
        {
            if (SW.Dynamisch.GetGesetzX(4) > 0)
                SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).ErhoeheGesetzXUmEins(4);

            var kiGegner = SW.Dynamisch.GetKIwithID(SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetSpieltKartenGegenSpielerID());
            
            GegnerName = kiGegner.GetKompletterName();
            
            if (kiGegner.GetMaennlich())
            {
                GegnerErSie = "er";
                GegnerSeinenIhren = "seinen";
            }
            else
            {
                GegnerErSie = "sie";
                GegnerSeinenIhren = "ihren";
            }
        }
    }
}
