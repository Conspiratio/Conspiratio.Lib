using System;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kirche
{
    public class Kupplerin
    {
        public static int ErmittleOptimalenPartnerFuerSpieler(int spielerId)
        {
            int optimalerPartnerId = 0;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                // Wenn sie unterschiedliches Geschlecht vorweisen
                if (SW.Dynamisch.GetHumWithID(spielerId).GetMaennlich() != SW.Dynamisch.GetKIwithID(i).GetMaennlich())
                {
                    // und nicht verheiratet sind
                    if (SW.Dynamisch.GetKIwithID(i).GetVerheiratet() == 0)
                    {
                        // und das Amt nicht höher ist als in der Stadtebene
                        if (SW.Dynamisch.GetKIwithID(i).GetAmtID() < 17)
                        {
                            if (optimalerPartnerId == 0)
                            {
                                optimalerPartnerId = i;
                            }
                            else
                            {
                                int Preis = BerechnePreisFuerKupplerin(i);

                                if (SW.Dynamisch.GetKIwithID(optimalerPartnerId).GetBeziehungZuKIX(SW.Dynamisch.GetAktiverSpieler()) < SW.Dynamisch.GetKIwithID(i).GetBeziehungZuKIX(SW.Dynamisch.GetAktiverSpieler()) + SW.Statisch.Rnd.Next(-15, 16) &&
                                    Preis <= (SW.Dynamisch.GetHumWithID(spielerId).GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler()) * 0.4d))  // Nur die Partner vorschlagen, deren Preis nicht höher liegt als 40 % des Gesamtvermögen des Spielers
                                {
                                    optimalerPartnerId = i;
                                }
                            }
                        }
                    }
                }
            }

            return optimalerPartnerId;
        }

        public static int BerechnePreisFuerKupplerin(int optimalerPartnerId)
        {
            return Convert.ToInt32(SW.Dynamisch.GetKIwithID(optimalerPartnerId).GetTaler() * SW.Statisch.GetKupplerProzente());
        }

        public static void BeginneWerbungUmOptimalenPartner(int spierlerId, int optimalerPartnerId, int preis)
        {
            SW.Dynamisch.GetHumWithID(spierlerId).WirbtUmSpielerID = optimalerPartnerId;
            SW.Dynamisch.GetHumWithID(spierlerId).ErhoeheTaler(-preis);
            SW.Dynamisch.GetKIwithID(optimalerPartnerId).ErhoeheVerliebt(50);
            
            SW.Dynamisch.BelTextAnzeigen("Die Kupplerin leitet alle Vorkehrungen in die Wege...");
        }
    }
}
