using System.Windows.Forms;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivJurist: Privileg
    {
        public PrivJurist() : base("Jurist aufsuchen", 34)
        {
        }

        public override void PrivExecute()
        {
            int preis = 1000;

            if (SW.UI.JaNeinFrage.ShowDialogText("Beim Jurist erhaltet Ihr Einblicke\nin Eure bisherigen Verbrechen und deren Bewertung.\n" +
                                                $"Wollt Ihr diese Dienste für\n{preis.ToStringGeld()} in Anspruch nehmen?") != DialogResult.Yes)
            {
                return;
            }

            if (!SW.Dynamisch.CheckIfenoughGold(preis))
            {
                return;
            }

            SW.Dynamisch.GetAktHum().ErhoeheTaler(-preis);

            int kirchenvergehen = 0, finanzvergehen = 0, strafvergehen = 0;

            for (int i = 0; i < SW.Statisch.GetMaxGesetze(); i++)
            {
                if (SW.Dynamisch.GetAktHum().GetBegingVerbrechenX(i) > 0)
                {
                    if (i >= 40)
                        kirchenvergehen++;
                    else if (i >= 20)
                        strafvergehen++;
                    else
                        finanzvergehen++;
                }
            }

            string verstossText, meldung = "Der Jurist meint leicht erstaunt,\ndass derzeit keine Beweise für\nStraftaten von Euch bekannt sind.";

            if (kirchenvergehen > 0 || finanzvergehen > 0 || strafvergehen > 0)
            {
                meldung = "Der Jurist räuspert sich und legt Euch vor:\n";

                if (kirchenvergehen > 0)
                {
                    if (kirchenvergehen == 1)
                        verstossText = "Verstoß";
                    else
                        verstossText = "Verstöße";

                    meldung += $"\n - {kirchenvergehen} {verstossText} gegen Kirchengesetze";
                }

                if (finanzvergehen > 0)
                {
                    if (finanzvergehen == 1)
                        verstossText = "Verstoß";
                    else
                        verstossText = "Verstöße";

                    meldung += $"\n - {finanzvergehen} {verstossText} gegen Finanzgesetze";
                }

                if (strafvergehen > 0)
                {
                    if (strafvergehen == 1)
                        verstossText = "Verstoß";
                    else
                        verstossText = "Verstöße";

                    meldung += $"\n - {strafvergehen} {verstossText} gegen Strafgesetze";
                }

                meldung += "\n\nZur Schwere Eurer Schuld meint er:\n";
                int deliktpunkte = SW.Dynamisch.GetAktHum().GetDeliktpunkte();

                if (deliktpunkte > 9)
                    meldung += "\"Eure Kapitalverbrechen werden Euch\n eines Tage des Kopf kosten!\"";
                else if (deliktpunkte > 5)
                    meldung += "\"Ihr werdet allmählich zum Berufsverbrecher!\"";
                else if (deliktpunkte > 2)
                    meldung += "\"Eure Gaunereien könnten Euch\nteuer zu stehen kommen!\"";
                else
                    meldung += "\"Ein paar Anschuldigungen, weiter nichts.\"";
            }

            SW.Dynamisch.BelTextAnzeigen(meldung);
        }
    }
}
