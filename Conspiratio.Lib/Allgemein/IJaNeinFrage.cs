namespace Conspiratio.Lib.Allgemein
{
    public interface IJaNeinFrage
    {
        DialogResultGame ShowDialogText(string textFrage, string textJa = "Ja", string textNein = "Nein");
    }
}