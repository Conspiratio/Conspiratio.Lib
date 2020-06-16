namespace Conspiratio.Lib.Allgemein
{
    public class UIHelper
    {
        public IJaNeinFrage JaNeinFrage { get; private set; }

        public ITextAnzeigen TextAnzeigen { get; private set; }

        public void Initialisieren(IJaNeinFrage jaNeinFrage, ITextAnzeigen textAnzeigen)
        {
            JaNeinFrage = jaNeinFrage;
            TextAnzeigen = textAnzeigen;
        }
    }
}
