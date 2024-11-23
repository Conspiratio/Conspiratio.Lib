using System.Threading.Tasks;

namespace Conspiratio.Lib.Allgemein
{
    public interface IYesNoQuestion
    {
        /// <summary>
        /// Displays a dialog window with the specified text and two buttons (usually Yes and No). The texts can be freely customized.
        /// The return value is <see cref="DialogResultGame.Yes"/>, <see cref="DialogResultGame.No"/> or <see cref="DialogResultGame.Cancel"/> when closing the dialog (e.g. with right-click).
        /// </summary>
        /// <remarks>When called from the WinForms client, a synchronous variant is used and this internally uses the method "Form.ShowDialog()",
        /// to display a modal dialog. Otherwise, an engine waits for the player to close the dialog via await.</remarks>
        /// <param name="textQuestion">Text of the question</param>
        /// <param name="textYes">OPTIONAL: Labeling of the left “Yes” button</param>
        /// <param name="textNo">OPTIONAL: Labeling of the right “No” button</param>
        /// <returns>Can return <see cref="DialogResultGame.Yes"/>, <see cref="DialogResultGame.No"/> and <see cref="DialogResultGame.Cancel"/> when closing the dialog (e.g. with right-click).</returns>
        Task<DialogResultGame> ShowDialogText(string textQuestion, string textYes = "Ja", string textNo = "Nein");
    }
}