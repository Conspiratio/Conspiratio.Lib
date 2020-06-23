namespace Conspiratio.Lib.Allgemein
{
    public class UIHelper
    {
        public IJaNeinFrage JaNeinFrage { get; private set; }

        public ITextAnzeigen TextAnzeigen { get; private set; }

        public IBeziehungPflegen BeziehungPflegen { get; private set; }

        public void Initialisieren(IJaNeinFrage jaNeinFrage, ITextAnzeigen textAnzeigen, IBeziehungPflegen beziehungPflegen)
        {
            JaNeinFrage = jaNeinFrage;
            TextAnzeigen = textAnzeigen;
            BeziehungPflegen = beziehungPflegen;
        }
    }
}
