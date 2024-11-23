using System.Threading.Tasks;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kirche
{
    public class Kirchgang
    {
        public async Task<bool> AblassKaufen()
        {
            SW.Dynamisch.DeliktpunkteBerechnen();

            int delpunkte = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetDeliktpunkte();
            int delpunktpreis = SW.Statisch.GetDeliktpunktPreis();

            int kosten = delpunkte * delpunktpreis;

            if (SW.Dynamisch.CheckIfenoughGold(kosten))
            {
                if (kosten != 0)
                {
                    if (await SW.UI.YesNoQuestion.ShowDialogText("Wollt Ihr den Ablass für\n" + kosten.ToStringGeld() + " kaufen?", "Ja", "Nein") == DialogResultGame.Yes)
                    {
                        SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).ErhoeheTaler(-kosten);
                        SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetSpielerStatistik().KgekaufteAblaesse++;

                        //falls verboten
                        if (SW.Dynamisch.GetGesetzX(41) != 0)
                        {
                            SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetBegingVerbrechenX(41);
                        }

                        //Sünden reduzieren
                        SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).HalbiereDelikte();
                        return true;
                    }
                }
                else
                {
                    SW.Dynamisch.BelTextAnzeigen("Ihr habt keine Sünden begangen, die einen Ablasskauf bedürfen.");
                }
            }

            return false;
        }

        public void Beichten()
        {
            if (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetGebeichtet() == true)
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr habt dieses Jahr bereits genug Sünden gebeichtet!");
            }
            else
            {
                int delpunkte = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetDeliktpunkte();

                if (delpunkte > 0)
                {
                    SW.Dynamisch.BelTextAnzeigen("Ihr begebt Euch zu einem Priester, welchem Ihr einen Teil Eurer Sünden gesteht. Entsetzt und widerwillig gewährt Euch dieser die Absolution mit der Aufforderung das Gotteshaus zu verlassen.");
                    SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetSpielerStatistik().KabgelegteBeichten++;

                    //Sünden reduzieren um 1
                    SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).SetDeliktpunkte(SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetDeliktpunkte() - 1);
                    SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).SetGebeichtet(true);
                }
                else
                {
                    SW.Dynamisch.BelTextAnzeigen("Ihr habt keine Sünden begangen, die einer Beichte bedürfen");
                }
            }
        }

        public async Task<bool> WaisenkindAdoptieren()
        {
            if (!SW.Dynamisch.GetAktHum().DarfWaisenkindAdoptieren())
            {
                string vaterMutter = SW.Dynamisch.GetAktHum().GetMaennlich() ? "glücklicher Vater" : "glückliche Mutter";
                SW.Dynamisch.BelTextAnzeigen($"Ihr seid derzeit {vaterMutter} \neines Kindes und könnt daher\n kein Waisenkind adoptieren.");
                return false;
            }

            int preis = SW.Dynamisch.GetAktHum().ErmittlePreisWaisenkindAdoptieren(SW.Dynamisch.GetAktiverSpieler());

            if (await SW.UI.YesNoQuestion.ShowDialogText("Wollt Ihr ein Mündel für\n" + preis.ToStringGeld() + " aus dem \nkirchlichen Waisenhaus adoptieren? \nEuer Ansehen könnte darunter leiden ...",
                                                          "Ja", "Lieber nicht!") != DialogResultGame.Yes)
            {
                return false;
            }

            SW.Dynamisch.GetAktHum().WaisenkindAdoptieren(preis);
            return true;
        }
    }
}
