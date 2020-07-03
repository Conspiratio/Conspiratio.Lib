using System;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien
{
    public class PrivMedikus : Privileg
    {
        private string[] _alterFazit;
        private string[] _gesundFazit;

        public PrivMedikus() : base("Medikus konsultieren", 1)
        {
            _alterFazit = new string[10];
            _alterFazit[0] = "Macht Euer Testament...";
            _alterFazit[1] = "Ich gebe Euch höchstens noch 5 Jahre...";
            _alterFazit[2] = "Ihr solltet keine größeren Reisen mehr unternehmen.";
            _alterFazit[3] = "Der Zahn der Zeit nagt immer kräftiger.";
            _alterFazit[4] = "Die Götter sind Euch wohlgesonnen.";
            _alterFazit[5] = "Ihr habt noch ein langes Leben vor Euch!";
            _alterFazit[6] = "Genießt Eure besten Jahre!";

            _gesundFazit = new string[10];
            _gesundFazit[0] = "Um Himmels Willen! Ihr müsst etwas für Eure Gesundheit tun!";
            _gesundFazit[1] = "Ihr seid in sehr schlechter Verfassung!";
            _gesundFazit[2] = "Ihr solltet wesentlich mehr auf Eure Gesundheit achten.";
            _gesundFazit[3] = "Ihr kränkelt ein wenig.";
            _gesundFazit[4] = "Es könnte Euch besser gehen.";
            _gesundFazit[5] = "Alles in allem seid Ihr gesund.";
        }

        public override void PrivExecute()
        {
            int verbleibendeJahre = SW.Dynamisch.GetSpXlebtNochSoVielJahre(SW.Dynamisch.GetAktiverSpieler());
            int afaz;

            if (verbleibendeJahre < 2)
            {
                afaz = 0;
            }
            else if (verbleibendeJahre < 5)
            {
                afaz = 1;
            }
            else if (verbleibendeJahre < 8)
            {
                afaz = 2;
            }
            else if (verbleibendeJahre < 11)
            {
                afaz = 3;
            }
            else if (verbleibendeJahre < 14)
            {
                afaz = 4;
            }
            else if (verbleibendeJahre < 18)
            {
                afaz = 5;
            }
            else
            {
                afaz = 6;
            }

            int gfaz = SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetAktiverSpieler()).BeurteileGesundheitIntWert();
            SW.Dynamisch.BelTextAnzeigen("Der Arzt untersucht Euch gründlich und meint schließlich:\n\"" + _gesundFazit[gfaz] + "\"" + "\n\nZu Eurem Alter meint er:\n\"" + _alterFazit[afaz] + "\"");

            if (SW.Dynamisch.Testmodus)
            {
                SW.Dynamisch.BelTextAnzeigen("Testmodus" + Environment.NewLine + "Alter: " + SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetAlter().ToString() + "/" + (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetVerbleibendeJahre() + SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetAlter()).ToString()  + Environment.NewLine + "Gesundheit: " + SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetAktiverSpieler()).GetGesundheit().ToString() + "/" + SW.Statisch.GetMaxGesundheit().ToString() + Environment.NewLine + "Mit der derzeitigen Gesundheit lebt Ihr noch genau " + verbleibendeJahre + " Jahre");
            }
        }
    }
}
