using System.Windows.Forms;

namespace Conspiratio.Lib.Allgemein
{
    public interface IJaNeinFrage
    {
        DialogResult ShowDialogText(string textFrage, string textJa = "Ja", string textNein = "Nein");
    }
}