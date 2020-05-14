namespace Conspiratio.Lib.Allgemein
{
    public class UIHelper
    {
        public IJaNeinFrage JaNeinFrage { get; private set; }

        public void Initialisieren(IJaNeinFrage jaNeinFrage)
        {
            JaNeinFrage = jaNeinFrage;
        }
    }
}
