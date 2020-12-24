using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Einstellungen;
using Conspiratio.Lib.Gameplay.Gebiete;
using Conspiratio.Lib.Gameplay.Justiz;
using Conspiratio.Lib.Gameplay.Kampf;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Personen;
using Conspiratio.Lib.Gameplay.Privilegien;
using Conspiratio.Lib.Gameplay.Rohstoffe;
using Conspiratio.Lib.Gameplay.Schreibstube;

namespace Conspiratio.Lib.Gameplay.Spielwelt
{
    /// <summary>
    /// Variablen usw. die sich während des Spiels verändern können aber nicht auf den Benutzer beziehen. Enthält außerdem verschiedene allgemeine Gameplay Logiken.
    /// </summary>
    public class DynamischeSpieldaten
    {
        #region Variablen

        public Spielstand Spielstand { get; set; }

        #endregion

        #region Konstuktor
        public DynamischeSpieldaten()
        {
            //NeuInitialisieren();
        }
        #endregion

        #region Methoden

        #region NeuInitialisieren
        public void NeuInitialisieren()
        {
            if (Spielstand == null)
                Spielstand = new Spielstand();

            AktiveSpielerAnzahl = 0;
            AktiverSpielerID = 1;
            AktuellesJahr = SW.Statisch.StartJahr;
            Wahlen = new WahlAbhalten[SW.Statisch.GetMaxAnzahlWahlen()];

            #region Gerichtsverhandlungen
            Gerichtshandlungen = new Gerichtsverhandlung[SW.Statisch.GetmaxAnzahlGerichtsverhandlungen()];
            for (int i = 0; i < SW.Statisch.GetmaxAnzahlGerichtsverhandlungen(); i++)
            {
                Gerichtshandlungen[i] = new Gerichtsverhandlung();
            }
            #endregion

            #region Amtsenthebungen
            Amtsenthebungen = new Amtsenthebung[SW.Statisch.GetMaxAnzahlAmtsenthebungen()];
            for (int i = 1; i < SW.Statisch.GetMaxAnzahlAmtsenthebungen(); i++)
            {
                Amtsenthebungen[i] = new Amtsenthebung(0, 0, 0, 0);
            }
            #endregion

            #region Rohstoffe
            Rohstoffe = new Rohstoff[SW.Statisch.GetMaxRohID()];

            Rohstoffe[1] = new Rohstoff(7, 8, 20, "Korn", "Erntet Korn;auf:Feld.Feldern", 5, 1, 1, "Die Getreideernte fiel {0} aus");
            Rohstoffe[2] = new Rohstoff(7, 9, 20, "Wolle", "Schert Wolle;an:Schaf.Schafen", 1, 2, 1, "Die Schafschur verlief {0}");
            Rohstoffe[3] = new Rohstoff(7, 8, 20, "Obst", "Pflück Obst;in:Garten.Gärten", 4, 1, 1, "Die Obsternte fiel {0} aus");
            Rohstoffe[4] = new Rohstoff(7, 8, 20, "Bier", "Braut Bier;an:Kessel.Kesseln", 2, 1, 1, "Das Bierbrauen verlief {0}");
            Rohstoffe[5] = new Rohstoff(7, 8, 20, "Holz", "Schlagt Holz;in:Wald.Wäldern", 7, 1, 1, "Der Holzschlag war {0}");
            Rohstoffe[6] = new Rohstoff(7, 8, 20, "Fisch", "Fangt Fisch;in:Boot.Booten", 3, 1, 1, "Der Fischfang verlief {0}");
            Rohstoffe[7] = new Rohstoff(7, 8, 20, "Ziegel", "Brennt Ziegel;in:Ziegelei.Ziegeleien", 6, 1, 1, "Die Ziegelbrennerei verlief {0}");

            Rohstoffe[8] = new Rohstoff(10, 14, 30, "Glas", "Blast Glas;in:Ofen.Öfen", 3, 1, 2, "Die Glasproduktion war {0}");
            Rohstoffe[9] = new Rohstoff(10, 14, 30, "Wein", "Keltert Wein;in:Fass.Fässern", 1, 1, 2, "Die Weinherstellung verlief {0}");
            Rohstoffe[10] = new Rohstoff(10, 14, 30, "Rind", "Züchtet Rind;auf:Weide.Weiden", 1, 1, 2, "Die Rinderzucht verlief {0}");
            Rohstoffe[11] = new Rohstoff(10, 14, 30, "Fell", "Gerbt Fell;in:Bottich.Bottichen", 1, 2, 2, "Die Fellgerbung verlief {0}");
            Rohstoffe[12] = new Rohstoff(10, 14, 30, "Rum", "Brennt Rum;in:Destille.Destillerien", 1, 2, 2, "Die Rumdestillation verlief {0}");
            Rohstoffe[13] = new Rohstoff(10, 14, 30, "Erz", "Fördert Erz;aus:Grube.Gruben", 5, 1, 2, "Die Erzausbeute war {0}");
            Rohstoffe[14] = new Rohstoff(10, 14, 30, "Kupfer", "Schmelzt Kupfer;in:Schmelze.Schmelzen", 4, 1, 2, "Der Kupferertrag war {0}");

            Rohstoffe[15] = new Rohstoff(13, 19, 40, "Pfeffer", "Holt Pfeffer;aus:Land.Ländern", 9, 1, 3, "Die Pfeffersuche verlief {0}");
            Rohstoffe[16] = new Rohstoff(13, 19, 40, "Salz", "Fördert Salz;in:Mine.Minen", 7, 1, 3, "Die Salzförderung war {0}");
            Rohstoffe[17] = new Rohstoff(13, 19, 40, "Waffen", "Baut Waffen;in:Schmiede.Schmieden", 4, 1, 3, "Die Waffenproduktion verlief {0}");
            Rohstoffe[18] = new Rohstoff(13, 19, 40, "Diamant", "Fördert Diamant;in:Bergwerk.Bergwerken", 6, 1, 3, "Die Diamantförderung verlief {0}");
            Rohstoffe[19] = new Rohstoff(13, 19, 40, "Gold", "Fördert Gold;in:Schacht.Schächten", 5, 1, 3, "Der Goldertrag war {0}");
            Rohstoffe[20] = new Rohstoff(13, 19, 40, "Medizin", "Fertigt Medizin;in:Labor.Labore", 2, 1, 3, "Die Medizinfertigung verlief {0}");
            Rohstoffe[21] = new Rohstoff(13, 19, 40, "Silber", "Fördert Silber;in:Stollen.Stollen", 8, 1, 3, "Der Silberertrag war {0}");
            #endregion

            #region Staedte anlegen
            Staedte = new Stadt[SW.Statisch.GetMaxStadtID()];

            //Leere Katastrophen werden mit 0 gefüllt:

            Staedte[1] = new Stadt("Frozen Castle", 5, 6, 11, 13, 19, 20, 1.1, 1.0, 1.1, 1.0, 1.0, 1.1, 3, 4, 0, 0, 75, 75, 0, 5000, 1, 16, 12, 15, 14, 7, 8, 12, 23, 25, 24, 13, 22, 15, 21, 35, 31, 33, 33, 22, 19, 29);
            Staedte[2] = new Stadt("Icepike", 2, 6, 8, 11, 20, 21, 1.1, 1.0, 1.1, 1.0, 1.0, 1.1, 5, 5, 0, 0, 50, 75, 75, 2500, 1, 15, 9, 16, 14, 14, 8, 15, 13, 24, 25, 15, 21, 21, 24, 35, 31, 33, 31, 31, 22, 19);
            Staedte[3] = new Stadt("Hilligen", 2, 7, 12, 13, 16, 21, 1.1, 1.0, 1.0, 1.1, 1.1, 1.0, 1, 2, 75, 75, 0, 0, 0, 2500, 1, 16, 9, 14, 15, 12, 13, 8, 22, 25, 23, 21, 15, 13, 23, 33, 19, 35, 33, 31, 29, 22);

            Staedte[4] = new Stadt("Nightmine", 2, 7, 9, 13, 18, 19, 1.0, 1.1, 1.0, 1.1, 1.1, 1.0, 2, 5, 0, 0, 75, 50, 100, 2500, 2, 15, 9, 15, 16, 12, 14, 7, 21, 15, 22, 21, 25, 13, 24, 33, 33, 35, 19, 22, 33, 29);
            Staedte[5] = new Stadt("Dragonrock", 5, 7, 8, 13, 18, 21, 1.1, 1.0, 1.1, 1.0, 1.0, 1.1, 3, 1, 0, 0, 75, 50, 0, 5000, 2, 14, 13, 16, 12, 7, 15, 8, 13, 24, 22, 22, 22, 21, 25, 31, 35, 31, 22, 29, 33, 19);
            Staedte[6] = new Stadt("Crowbrigde", 4, 7, 8, 10, 17, 21, 1.0, 1.1, 1.0, 1.1, 1.1, 1.0, 2, 1, 0, 0, 0, 100, 0, 2500, 2, 13, 13, 14, 8, 13, 16, 7, 15, 23, 13, 23, 24, 22, 25, 29, 33, 19, 29, 29, 35, 22);
            Staedte[7] = new Stadt("Altenfield", 1, 5, 11, 14, 15, 19, 1.1, 1.0, 1.1, 1.0, 1.0, 1.1, 2, 1, 100, 25, 0, 0, 0, 2500, 2, 7, 12, 13, 16, 8, 15, 14, 21, 22, 21, 13, 25, 23, 15, 22, 35, 29, 31, 19, 33, 29);

            Staedte[8] = new Stadt("Outpost", 2, 3, 10, 14, 15, 17, 1.0, 1.1, 1.0, 1.1, 1.1, 1.0, 5, 3, 0, 0, 50, 50, 100, 2500, 3, 14, 9, 7, 12, 15, 13, 16, 22, 22, 15, 24, 22, 25, 13, 19, 29, 22, 31, 31, 35, 31);
            Staedte[9] = new Stadt("Kingsguard", 1, 4, 10, 12, 17, 18, 1.0, 1.1, 1.1, 1.0, 1.0, 1.1, 3, 2, 50, 50, 0, 0, 0, 7500, 3, 8, 14, 13, 8, 16, 12, 15, 23, 23, 13, 24, 15, 25, 21, 31, 29, 22, 19, 31, 33, 35);
            Staedte[10] = new Stadt("Ellmau", 1, 3, 9, 10, 16, 19, 1.1, 1.0, 1.1, 1.0, 1.0, 1.1, 4, 6, 100, 0, 50, 50, 50, 2500, 3, 7, 15, 8, 13, 16, 14, 12, 24, 13, 15, 25, 21, 23, 23, 33, 22, 29, 31, 19, 33, 35);
            Staedte[11] = new Stadt("Bellington", 4, 6, 8, 12, 16, 17, 1.0, 1.1, 1.0, 1.1, 1.0, 1.1, 5, 7, 100, 50, 0, 0, 50, 2500, 3, 13, 14, 12, 8, 15, 7, 16, 15, 21, 21, 25, 13, 22, 24, 29, 22, 19, 35, 33, 33, 31);

            Staedte[12] = new Stadt("Thornhall", 1, 6, 12, 14, 15, 16, 1.0, 1.1, 1.1, 1.0, 1.0, 1.1, 1, 5, 50, 0, 50, 50, 50, 2500, 4, 8, 16, 12, 15, 14, 7, 13, 25, 21, 23, 22, 13, 24, 15, 22, 19, 33, 35, 35, 29, 33);
            Staedte[13] = new Stadt("Shadowbay", 3, 4, 9, 11, 18, 20, 1.0, 1.1, 1.1, 1.0, 1.0, 1.1, 2, 4, 100, 50, 0, 0, 0, 2500, 4, 12, 15, 8, 8, 13, 16, 15, 25, 13, 24, 15, 23, 24, 21, 29, 29, 31, 22, 35, 19, 33);
            Staedte[14] = new Stadt("Janon", 3, 5, 9, 14, 15, 20, 1.1, 1.0, 1.0, 1.1, 1.1, 1.0, 3, 1, 0, 0, 100, 75, 0, 5000, 4, 12, 16, 7, 13, 8, 12, 14, 24, 15, 25, 23, 23, 21, 13, 19, 29, 33, 31, 35, 22, 33);
            #endregion

            #region Laender anlegen
            Laender = new Land[SW.Statisch.GetMaxLandID()];
            Laender[1] = new Land("Wattern", 1, 2, 3, 0, 1, 5);
            Laender[2] = new Land("Graniteland", 4, 5, 6, 7, 2, 6);
            Laender[3] = new Land("Meadowvalley", 8, 9, 10, 11, 3, 7);
            Laender[4] = new Land("Redcoast", 12, 13, 14, 0, 4, 8);
            #endregion

            #region Reich anlegen
            Reiche = new Reich[SW.Statisch.GetMaxReichID()];
            Reiche[1] = new Reich("Lottringen");
            #endregion

            #region Zollburgen und Räuberlager
            Stuetzpunkte = new Stuetzpunkt[8];

            // Zollburgen
            Stuetzpunkte[0] = new Zollburg(1, "Zattingham", 0, 1380, 80000, 1, 60, 50, 200, 30, 50, 0.05);
            Stuetzpunkte[1] = new Zollburg(2, "Ullentowers", 0, 1520, 60000, 2, 45, 50, 170, 30, 50, 0.05);
            Stuetzpunkte[2] = new Zollburg(3, "Valdon", 0, 1608, 70000, 3, 50, 50, 200, 30, 50, 0.05);
            Stuetzpunkte[3] = new Zollburg(4, "Redcastle", 0, 1404, 70000, 4, 50, 50, 200, 30, 50, 0.05);
            // Räuberlager
            Stuetzpunkte[4] = new Raeuberlager(5, "Downfall Hills", 0, 1680, 70000, 1, 60, 50, 150, 25, 50);
            Stuetzpunkte[5] = new Raeuberlager(6, "Bandit Moor", 0, 1504, 50000, 2, 45, 50, 120, 25, 50);
            Stuetzpunkte[6] = new Raeuberlager(7, "Dark Forest", 0, 1635, 60000, 3, 50, 50, 150, 25, 50);
            Stuetzpunkte[7] = new Raeuberlager(8, "Red Hideout", 0, 1650, 60000, 4, 50, 50, 150, 25, 50);

            LandsicherheitenInitialisieren();
            #endregion

            #region KI Spieler anlegen
            KSpieler = new KISpieler[SW.Statisch.GetMaxKIID()];

            //jeweilige counter
            int cstadt = 1;
            int cland = 1;
            int creich = 1;

            //Amt IDs
            int camt = 1;
            int camt1 = 17;
            int camt2 = 34;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                bool mann = false;
                int verheir = 0;
                if (i < SW.Statisch.GetMaennerFrauenGrenze())
                {
                    mann = true;
                    if (SW.Statisch.Rnd.Next(0, 2) == 0) //50% das er verheiratet ist
                    {
                        verheir = i + SW.Statisch.GetMaennerFrauenGrenze();
                    }
                }

                KSpieler[i] = new KISpieler(SW.Statisch.Rnd.Next(10000, 100000), SW.Statisch.GetKINameX(i), mann, SW.Statisch.Rnd.Next(0, 101), verheir, SW.Statisch.Rnd.Next(0, SW.Statisch.GetKImaxVerblJahre()));
                int randalt = SW.Statisch.Rnd.Next(20, 40);
                KSpieler[i].SetAlter(randalt);

                int religio = SW.Statisch.GetRelKathID();
                if (SW.Statisch.Rnd.Next(SW.Statisch.GetRelMinID() + 1, SW.Statisch.GetRelMaxID()) == 0) //Gehört zu Religion 1 oder 2
                {
                    religio = SW.Statisch.GetRelEvanID();
                }
                KSpieler[i].SetReligion(religio);

                KSpieler[i].SetDeliktpunkte(Convert.ToInt32(KSpieler[i].GetBosheit() / 10));
            }


            //Jetzt sind schon alle angelegt
            int[] RandShuffle = new int[SW.Statisch.GetMaxKIID() - SW.Statisch.GetMinKIID()];
            for (int i = 0; i < SW.Statisch.GetMaxKIID() - SW.Statisch.GetMinKIID(); i++)
            {
                RandShuffle[i] = SW.Statisch.GetMinKIID() + i;
            }

            ShuffleList(RandShuffle);

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                int j = RandShuffle[i - SW.Statisch.GetMinKIID()];
                KSpieler[j].SetTitel(1); //Jeder is mindestens Buerger

                if (i < SW.Statisch.GetMinKIID() + 224) //14Ämter pro Stadt * 16Ämter = 224 Ämter
                {
                    AmtAufStufeXGebietYidZanWvergeben(0, cstadt, camt, j);

                    //KI auch noch Ansehen verhältnismäßig zu seinem Amt verleihen
                    int amt_ans_bonus = SW.Statisch.GetAmtwithID(camt).GetBonusAnsehen();
                    KSpieler[j].SetAnsehen(amt_ans_bonus * 5);

                    //zufälligen Titel verleihen
                    int randtit = SW.Statisch.Rnd.Next(1, 3);
                    KSpieler[j].SetTitel(randtit);

                    camt++;
                    if (camt >= 17)
                    {
                        camt = 1;
                        cstadt++;
                    }
                }
                else if (i < SW.Statisch.GetMinKIID() + 292) //17Ämter pro Land * 4Länder = 68 Ländliche Ämter
                {
                    AmtAufStufeXGebietYidZanWvergeben(1, cland, camt1, j);

                    //KI auch noch Ansehen verhältnismäßig zu seinem Amt verleihen
                    int amt_ans_bonus = SW.Statisch.GetAmtwithID(camt1).GetBonusAnsehen();
                    KSpieler[j].SetAnsehen(amt_ans_bonus * 5);

                    //zufälligen Titel verleihen
                    int randtit = SW.Statisch.Rnd.Next(3, 6);
                    KSpieler[j].SetTitel(randtit);

                    camt1++;
                    if (camt1 >= 34)
                    {
                        camt1 = 17;
                        cland++;
                    }
                }
                else if (i < SW.Statisch.GetMinKIID() + 307) //1Reich * 15 Ämter = 15 Reichsämter
                {
                    AmtAufStufeXGebietYidZanWvergeben(2, creich, camt2, j);

                    //KI auch noch Ansehen verhältnismäßig zu seinem Amt verleihen
                    int amt_ans_bonus = SW.Statisch.GetAmtwithID(camt2).GetBonusAnsehen();
                    KSpieler[j].SetAnsehen(amt_ans_bonus * 5);

                    //zufälligen Titel verleihen
                    int randtit = SW.Statisch.Rnd.Next(6, 8);
                    KSpieler[j].SetTitel(randtit);

                    //Regent, Marschall und Erzbischof sollen maxtitel bekommen
                    if (camt2 == 39 || camt2 == 42 || camt2 == 48)
                    {
                        KSpieler[j].SetTitel(SW.Statisch.GetMaxTitelID() - 1);
                    }

                    camt2++;
                }

                //Weitere Taler fuer Ansehen verleihen
                int akt_goldd = KSpieler[j].GetTaler();
                int fakt = Convert.ToInt32(KSpieler[j].GetAnsehen() / 100);
                KSpieler[j].ErhoeheTaler(fakt * akt_goldd);

                KSpieler[j].ErhoeheAnsehen(SW.Statisch.Rnd.Next(-10, 5));

                KSpieler[j].CreateRndBeziehungen(i);
            }

            // Acht KI Spielern jeweils als Besitzer für die Stützpunkte (Zollburgen und Räuberlager) setzen
            for (int i = 0; i < GetStuetzpunkte().Length; i++)
            {
                GetStuetzpunkte()[i].BesitzerStuetzpunktZufaelligSetzen(creich);
            }

            //KI Sympatien random ändern
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                for (int j = 1; j < SW.Statisch.GetMaxKIID(); j++)
                {
                    GetKIwithID(i).ErhoeheBeziehungZuX(j, SW.Statisch.Rnd.Next(-20, 21));
                }
            }

            //Bei den Frauen das verheiratet setzen
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaennerFrauenGrenze(); i++)
            {
                if (KSpieler[i].GetVerheiratet() != 0)
                {
                    KSpieler[KSpieler[i].GetVerheiratet()].SetVerheiratet(i);
                }
            }
            #endregion

            #region Gesetze
            Gesetze = new int[SW.Statisch.GetMaxGesetze()];


            GesetzesTexte = new string[SW.Statisch.GetMaxGesetze()];

            //0 = erlaubt
            //1 = verboten

            //0 = Kredite
            //1 = Bestechungen
            //2 = Hoechstzahl Anwesen
            //3 = Maximale Taler
            //4 = Gluecksspiel

            //20 = Spionage
            //21 = Sabotage
            //22 = Anschwaerzen
            //23 = Ermordung
            //24 = Waffenhandel

            //40 = Religionsfreiheit
            //41 = Ablass
            //42 = Schloss
            //43 = Gold
            //44 = Silber



            #region Finanz
            //Kredite
            Gesetze[0] = 0;
            //Bestechungen
            Gesetze[1] = 1;
            //Höchstzahl Niederlassungen
            Gesetze[2] = 3;
            //Maxmimale Taler
            Gesetze[3] = 10;
            //Gluecksspiel
            Gesetze[4] = 0;
            #endregion

            #region Justiz
            //Spionage
            Gesetze[20] = 1;
            //Sabotage
            Gesetze[21] = 1;
            //Anschwärzen
            Gesetze[22] = 0;
            //Ermordung
            Gesetze[23] = 1;
            //Waffenhandel
            Gesetze[24] = 1;
            #endregion

            #region Kirche
            //Rel-Freiheit
            Gesetze[40] = 1;
            //Ablaß
            Gesetze[41] = 0;
            //Schloesser
            Gesetze[42] = 1;
            //Gold
            Gesetze[43] = 0;
            //Silber
            Gesetze[44] = 0;
            #endregion

            #endregion

            #region Wahlen anlegen
            for (int i = 1; i < SW.Statisch.GetMaxAnzahlWahlen(); i++)
            {
                Wahlen[i] = new WahlAbhalten(0, 0, 0, 0, 0, 0);
            }
            #endregion

            #region HumSpieler Anlegen
            HSpieler = new HumSpieler[SW.Statisch.GetMinKIID()];
            for (int i = 1; i < SW.Statisch.GetMinKIID(); i++)
            {
                CreateSpielerX(i, 0, "", true, 0, 0);
            }
            #endregion
        }
        #endregion

        #region Verändern nur an VW etwas
        public void SetHumX(int x, HumSpieler hs)
        {
            HSpieler[x] = hs;
        }

        public void CreateSpielerX(int x, int goldd, string nam, bool maenl, int verh, int verblJahre)
        {
            HSpieler[x] = new HumSpieler(goldd, nam, maenl, verh, verblJahre);
        }

        public void SetAktivSpielerAnzahl(int aktspAnz)
        {
            AktiveSpielerAnzahl = aktspAnz;
        }

        public void SetAktiverSpieler(int wert)
        {
            AktiverSpielerID = wert;
        }

        public void KIXneuAnlegen(int X)
        {
            double fakt = 1.2;

            KSpieler[X] = null;

            KSpieler[X] = new KISpieler(Convert.ToInt32(SW.Statisch.Rnd.Next(10000, 100000) * fakt), SW.Statisch.GetKINameX(X), GetTrueFalseRandom(), SW.Statisch.Rnd.Next(0, 101), 0, SW.Statisch.Rnd.Next(SW.Statisch.GetKIminVerblJahre(), SW.Statisch.GetKImaxVerblJahre()));
            KSpieler[X].SetAlter(SW.Statisch.GetStartAlter() + SW.Statisch.Rnd.Next(0, 4));
        }

        public void SetAmtsenthebungVonID(int OpferID)
        {
            int aid = GetLeereAmtsenthebungsID();

            if (GetSpWithID(OpferID).GetAmtID() != 0)
            {
                //Ermitteln ob gegen dieses Opfer bereits eine Enthebung im Gange ist
                bool done = false;
                for (int i = 1; i < SW.Statisch.GetMaxAnzahlAmtsenthebungen(); i++)
                {
                    if (GetAmtsenthebungX(i).OpferID == OpferID)
                    {
                        done = true;
                        break;
                    }
                }

                if (done == false)
                {
                    //Die 3 Waehler herausfinden
                    int[] waehlerAmtIDS = new int[3];
                    waehlerAmtIDS[0] = SW.Statisch.GetAmtwithID(GetSpWithID(OpferID).GetAmtID()).GetWaehler1AmtID();
                    waehlerAmtIDS[1] = SW.Statisch.GetAmtwithID(GetSpWithID(OpferID).GetAmtID()).GetWaehler2AmtID();
                    waehlerAmtIDS[2] = SW.Statisch.GetAmtwithID(GetSpWithID(OpferID).GetAmtID()).GetWaehler3AmtID();

                    int amtid = GetSpWithID(OpferID).GetAmtID();
                    int gebid = GetSpWithID(OpferID).GetAmtGebiet();

                    int[] wid = new int[3];

                    for (int i = 0; i < 3; i++)
                    {
                        if (waehlerAmtIDS[i] < SW.Statisch.GetMaxAmtStadtID())
                        {
                            wid[i] = GetStadtwithID(gebid).GetAmtX(waehlerAmtIDS[i]);
                        }
                        else if (waehlerAmtIDS[i] < SW.Statisch.GetMaxAmtLandID())
                        {
                            if (amtid < SW.Statisch.GetMaxAmtStadtID())
                            {
                                //Schaun zu welchem Land die Stadt gehört
                                int lid = GetLandIDzuStadtX(gebid);
                                wid[i] = GetLandWithID(lid).GetAmtX(waehlerAmtIDS[i]);
                            }
                            else
                            {
                                wid[i] = GetLandWithID(gebid).GetAmtX(waehlerAmtIDS[i]);
                            }
                        }
                        else
                        {
                            wid[i] = GetReichWithID(1).GetAmtX(waehlerAmtIDS[i]);
                        }
                    }

                    SetAmtsenthebungDaten(aid, OpferID, wid[0], wid[1], wid[2]);
                }
            }
            else
            {
                SetAmtsenthebungDaten(aid, 0, 0, 0, 0);
            }
        }

        public void SetAmtsenthebungDaten(int x, int oid, int w1, int w2, int w3)
        {
            Amtsenthebungen[x].OpferID = oid;
            Amtsenthebungen[x].Waehler1 = w1;
            Amtsenthebungen[x].Waehler2 = w2;
            Amtsenthebungen[x].Waehler3 = w3;
        }

        public void SetAktuellesJahr(int j)
        {
            AktuellesJahr = j;
        }

        public void ErhoehAktuellesJahrUmEins()
        {
            AktuellesJahr++;
        }

        public void VerheirateXundY(int X, int Y)
        {
            GetSpWithID(X).SetVerheiratet(Y);
            GetSpWithID(Y).SetVerheiratet(X);

            int titelX = GetSpWithID(X).GetTitel();
            int titelY = GetSpWithID(Y).GetTitel();

            if (titelX > titelY)
            {
                GetSpWithID(Y).SetTitel(GetSpWithID(X).GetTitel());
            }
            else
            {
                GetSpWithID(X).SetTitel(GetSpWithID(Y).GetTitel());

            }
        }

        public void CheckGesetzesVerstossMitRohIDx(int x)
        {
            switch (x)
            {
                case 17: //Waffenhandel
                    if (GetGesetzX(24) != 0)
                    {
                        GetHumWithID(GetAktiverSpieler()).ErhoeheGesetzXUmEins(24);
                    }
                    break;
                case 19: //Goldabbau
                    if (GetGesetzX(43) != 0)
                    {
                        GetHumWithID(GetAktiverSpieler()).ErhoeheGesetzXUmEins(43);
                    }
                    break;
                case 21: //Silberabbau
                    if (GetGesetzX(44) != 0)
                    {
                        GetHumWithID(GetAktiverSpieler()).ErhoeheGesetzXUmEins(44);
                    }
                    break;
            }
        }

        public void SetGesetzXtoY(int x, int y)
        {
            Gesetze[x] = y;
        }

        public void GesetzXschaltenUmY(int x, int y)
        {
            Gesetze[x] += y;
            if (Gesetze[x] > SW.Statisch.GetGesetzXDefObergrenze(x))
            {
                Gesetze[x] = SW.Statisch.GetGesetzXDefObergrenze(x);
            }
            if (Gesetze[x] < SW.Statisch.GetGesetzXDefUntergrenze(x))
            {
                Gesetze[x] = SW.Statisch.GetGesetzXDefUntergrenze(x);
            }
        }

        public void AmtVonXfreigeben(int id)
        {
            int gebiet = GetSpWithID(id).GetAmtGebiet();
            int amtid = GetSpWithID(id).GetAmtID();

            if (amtid != 0)
            {
                GetGebietwithID(gebiet, GetStufeVonAmtmitIDx(amtid)).SetAmtXtoY(amtid, 0);
                GetSpWithID(id).SetAmt(0, 0);

                WahlAnlegen(amtid, gebiet, GetStufeVonAmtmitIDx(amtid));
            }
        }

        public void AmtAufStufeXGebietYidZanWvergeben(int x, int y, int z, int w)
        {
            GetSpWithID(w).SetAmt(z, y);
            if (y != 0)
            {
                GetGebietwithID(y, x).SetAmtXtoY(z, w);
            }
        }

        public void RohBedarfAktRundenEnde()
        {
            //Die von den menschlichen Spielern verkauften Rohstoffe
            for (int j = 1; j < SW.Statisch.GetMaxStadtID(); j++)
            {
                for (int k = 1; k < SW.Statisch.GetMaxRohID(); k++)
                {
                    for (int i = 1; i < GetAktivSpielerAnzahl(); i++)
                    {
                        int temp = GetHumWithID(i).GetEinVerkaeufeInStadtXVonRohstoffIDY(j, k);
                        GetStadtwithID(j).ErhoeheRohstoffVorratWithIDXByY(k, temp);
                        GetHumWithID(i).SetEinVerkaeufeInStadtXVonRohstoffIDYAufZ(j, k, 0);
                    }
                    //Und jene auch etwas verbrauchen
                    GetStadtwithID(j).ErhoeheRohstoffVorratWithIDXByY(k, -GetStadtwithID(j).GetEinwohner() / 4);
                }
            }
        }

        public void RohPreiseRandomSchwanken()
        {
            int value = 0;
            if (GetAktuellesJahr() > SW.Statisch.StartJahr + 1)
            {
                //alle Staedte
                for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
                {
                    //alle Rohstoffe
                    for (int j = 1; j < SW.Statisch.GetMaxRohID(); j++)
                    {
                        value = SW.Statisch.Rnd.Next(-1, 2);
                        GetStadtwithID(i).ErhoeheRohstoffPreisVonIDXByY(j, value);
                    }
                }
            }
        }

        // Private Methoden
        private void WahlAnlegen(int AmtID, int RegID, int StufID)
        {
            //Die Wähler für das Amt werden ermittelt
            int wahlid = GetLeereWahlID();
            int w1 = SW.Statisch.GetAmtwithID(AmtID).GetWaehler1AmtID();
            int w2 = SW.Statisch.GetAmtwithID(AmtID).GetWaehler2AmtID();
            int w3 = SW.Statisch.GetAmtwithID(AmtID).GetWaehler3AmtID();

            //Wähler dürfen nicht mittig leer sein
            if (w1 == 0 && w2 == 0 && w3 != 0)
            {
                w1 = w3;
                w3 = 0;
            }
            if (w1 == 0 && w2 != 0 && w3 == 0)
            {
                w1 = w2;
                w2 = 0;
            }
            if (w1 == 0 && w2 != 0 && w3 != 0)
            {
                w1 = w2;
                w2 = w3;
                w3 = 0;
            }
            if (w1 != 0 && w2 == 0 && w3 != 0)
            {
                w2 = w3;
                w3 = 0;
            }

            //KI Kandidaten für das Amt werden ermittelt
            int[] kandx = new int[SW.Statisch.GetMaxWahlKandidaten()];

            int startki = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());
            bool go = true;

            int gocounter = 0;

            while (go)
            {
                gocounter++;

                //Die KI soll an keiner anderen Wahl teilnehmen
                while (GetKIwithID(startki).GetNimmtAnWahlTeil() == true)
                {
                    startki = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());
                }

                //Wenn die KI durch ihr Amt an der Wahl teilnehmen darf
                if (gocounter < 100)
                {
                    if (CheckBewerbAmtStufenDifferenz(GetKIwithID(startki).GetAmtID(), AmtID))
                    {
                        if (kandx[0] == 0)
                        {
                            kandx[0] = startki;
                            GetKIwithID(startki).SetNimmtAnWahlTeil(true);
                        }
                        else if (kandx[1] == 0)
                        {
                            if (startki != kandx[0])
                            {
                                kandx[1] = startki;
                                GetKIwithID(startki).SetNimmtAnWahlTeil(true);
                            }
                        }

                        if (kandx[1] != 0)
                        {
                            go = false;
                            break;
                        }
                    }

                    if (go == false)
                    {
                        break;
                    }

                    startki = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaxKIID());
                }
                //Dann abkürzen und die Voraussetzungen ignorieren
                else
                {
                    if (kandx[0] == 0)
                    {
                        kandx[0] = startki;
                        GetKIwithID(startki).SetNimmtAnWahlTeil(true);
                    }
                    else if (kandx[1] == 0)
                    {
                        if (startki != kandx[0])
                        {
                            kandx[1] = startki;
                            GetKIwithID(startki).SetNimmtAnWahlTeil(true);
                        }
                    }

                    if (kandx[1] != 0)
                    {
                        go = false;
                    }
                }

                if (go == false)
                {
                    break;
                }

                SetWahlX(wahlid, AmtID, RegID, StufID, w1, w2, w3, kandx, false);
            }
        }

        private void GesetzeAktualisieren()
        {
            string[] kircheBewertung;
            string[] finanzJustizBewertung;
            kircheBewertung = new string[2];
            kircheBewertung[0] = "gottgefällig";
            kircheBewertung[1] = "lästerlich";
            finanzJustizBewertung = new string[2];
            finanzJustizBewertung[0] = "erlaubt";
            finanzJustizBewertung[1] = "verboten";

            GesetzesTexte[0] = "Kredite sind " + finanzJustizBewertung[Gesetze[0]] + ".";
            GesetzesTexte[1] = "Bestechungen sind " + finanzJustizBewertung[Gesetze[1]] + ".";
            GesetzesTexte[2] = "Maximale Anwesen: " + Gesetze[2].ToString() + ".";
            GesetzesTexte[3] = "Maximale Taler: " + (Gesetze[3] * 100000).ToStringGeld(false) + ".";
            GesetzesTexte[4] = "Glücksspiel ist " + finanzJustizBewertung[Gesetze[4]] + ".";

            GesetzesTexte[20] = "Spionage ist " + finanzJustizBewertung[Gesetze[20]] + ".";
            GesetzesTexte[21] = "Sabotage ist " + finanzJustizBewertung[Gesetze[21]] + ".";
            GesetzesTexte[22] = "Anschwärzen ist " + finanzJustizBewertung[Gesetze[22]] + ".";
            GesetzesTexte[23] = "Mord ist " + finanzJustizBewertung[Gesetze[23]] + ".";
            GesetzesTexte[24] = "Waffenhandel ist " + finanzJustizBewertung[Gesetze[24]] + ".";

            GesetzesTexte[40] = "Religionsfreiheit ist " + kircheBewertung[Gesetze[40]] + ".";
            GesetzesTexte[41] = "Das Ablaßwesen ist " + kircheBewertung[Gesetze[41]] + ".";
            GesetzesTexte[42] = "Schlösser sind " + kircheBewertung[Gesetze[42]] + ".";
            GesetzesTexte[43] = "Goldabbau ist " + kircheBewertung[Gesetze[43]] + ".";
            GesetzesTexte[44] = "Silbernes Glanzwerk ist " + kircheBewertung[Gesetze[44]] + ".";
        }

        private void SetWahlX(int x, int aid, int gid, int stu, int w1, int w2, int w3, int[] k, bool sp)
        {
            Wahlen[x].AmtID = aid;
            Wahlen[x].GebietID = gid;
            Wahlen[x].Stufe = stu;

            Wahlen[x].Waehler1 = w1;
            Wahlen[x].Waehler2 = w2;
            Wahlen[x].Waehler3 = w3;

            Wahlen[x].SetKandidaten(k);
        }

        public void KIsVonWahlenAbmelden()
        {
            // Alle KIs sollen nicht mehr an Wahlen teilnehmen
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                GetKIwithID(i).SetNimmtAnWahlTeil(false);
            }
        }

        public void KlagenAbgewickelt()
        {
            // Alle Klagen usw. sollen abgewickelt sein
            for (int i = 1; i <= GetAktivSpielerAnzahl(); i++)
            {
                GetHumWithID(i).SetWirdBereitsVerklagt(false);
                GetHumWithID(i).SetKlagtSpielerMitIDXAn(0);
                GetHumWithID(i).SetHenkersHand(false);
            }
        }
        #endregion

        #region Public Getter (einzeilig)
        public KISpieler GetKIwithID(int id) { return KSpieler[id]; }
        public HumSpieler GetHumWithID(int id) { return HSpieler[id]; }
        public HumSpieler GetAktHum() { return GetHumWithID(GetAktiverSpieler()); }
        public Stuetzpunkt[] GetStuetzpunkte() { return Stuetzpunkte; }
        public Zollburg GetZollburgWithIDx(int x) { return (Zollburg)Stuetzpunkte[x]; }
        public Raeuberlager GetRaeuberlagerWithIDx(int x) { return (Raeuberlager)Stuetzpunkte[x]; }
        public Gerichtsverhandlung GetGerichtsverhandlungX(int x) { return Gerichtshandlungen[x]; }
        public Rohstoff GetRohstoffwithID(int id) { return Rohstoffe[id]; }
        public Stadt GetStadtwithID(int id) { return Staedte[id]; }
        public Land GetLandWithID(int id) { return Laender[id]; }
        public Reich GetReichWithID(int id) { return Reiche[id]; }
        public WahlAbhalten[] GetWahlen() { return Wahlen; }
        public WahlAbhalten GetWahlX(int x) { return Wahlen[x]; }
        public Amtsenthebung GetAmtsenthebungX(int x) { return Amtsenthebungen[x]; }
        public int GetGesetzX(int x) { return Gesetze[x]; }
        public int GetAktuellesJahr() { return AktuellesJahr; }
        public int GetAktivSpielerAnzahl() { return AktiveSpielerAnzahl; }
        public int GetAktiverSpieler() { return AktiverSpielerID; }
        public string GetAmtsnameVonSPIDx(int x) { return SW.Statisch.GetAmtwithID(GetSpWithID(x).GetAmtID()).GetAmtsname(GetSpWithID(x).GetMaennlich()); }
        #endregion

        #region Gameplay Logik und Berechnungen

        #region GetAnzahlFreieAemterFuerSpX
        public int GetAnzahlFreieAemterFuerSpX(int X)
        {
            // TODO: Hier den BUg finden
            // Es gibt gelegentlich einen viel zu hohen amtcounter zurück
            // speziell wenn sabotiert wird
            int[] wahlids = GetFreieAemterFuerSpX(X);

            int amtcounter = 0;

            for (int i = 0; i < wahlids.Length; i++)
            {
                if (wahlids[i] != 0)
                {
                    amtcounter++;
                }
            }
            return amtcounter;
        }
        #endregion

        #region GetFreieAemterFuerSpX
        public int[] GetFreieAemterFuerSpX(int X)
        {
            WahlAbhalten[] wahlab = GetWahlen();
            int maxWahlen = SW.Statisch.GetMaxAnzahlWahlen();
            int[] wahlids = new int[50];
            int amtcounter = 0;

            // Alle Wahlen durchgehen
            for (int i = 1; i < maxWahlen; i++)
            {
                // prüfen ob Wahl i existiert
                if (wahlab[i].GebietID != 0)
                {
                    // Wahl i existiert
                    // prüfen ob Spieler sich bewerben kann:
                    if (CheckBewerbAmtStufenDifferenz(GetHumWithID(X).GetAmtID(), wahlab[i].AmtID))
                    {
                        // Spieler kann sich bewerben

                        // Falls das Amt "staedtlich" ist
                        if (wahlab[i].AmtID < SW.Statisch.GetMaxAmtStadtID())
                        {
                            if (GetHumWithID(X).GetSpielerHatHausVonStadtAnArraystelle(wahlab[i].GebietID).GetHausID() != 0)
                            {
                                // Der Spieler muss mindestens den Titel x führen
                                if (GetHumWithID(X).GetTitel() >= SW.Statisch.GetMinTitelStadtEbene())
                                {
                                    // Wenn es sich um ein kirchliches Amt hat, muss der Spieler einer Religion angehören
                                    if (GetHumWithID(X).GetReligion() != SW.Statisch.GetRelFreiID())
                                    {
                                        wahlids[amtcounter] = i;
                                        amtcounter++;
                                    }
                                }
                            }
                        }

                        // Falls das Amt "laendlich" ist
                        else if (wahlab[i].AmtID < SW.Statisch.GetMaxAmtLandID())
                        {
                            // Der Spieler muss mindestens den Titel x führen
                            if (GetHumWithID(X).GetTitel() >= SW.Statisch.GetMinTitelLandEbene())
                            {
                                // Spieler kann sich bewerben wenn er in diesem Land mindestens 2 Häuser besitzt
                                int LandStaedte = GetLandWithID(wahlab[i].GebietID).GetAnzahlStaedte();
                                int hauscounter = 0;

                                for (int j = 0; j < LandStaedte; j++)
                                {
                                    if (GetHumWithID(X).GetSpielerHatHausVonStadtAnArraystelle(GetLandWithID(wahlab[i].GebietID).GetStadtX(j)).GetHausID() != 0)
                                    {
                                        hauscounter++;
                                    }
                                }

                                if (hauscounter >= 2)
                                {
                                    wahlids[amtcounter] = i;
                                    amtcounter++;
                                }
                            }
                        }

                        // Falls das Amt im Koenigreich ist
                        else
                        {
                            // Der Spieler muss mindestens den Titel x führen
                            if (GetHumWithID(X).GetTitel() >= SW.Statisch.GetMinTitelReichsEbene())
                            {
                                wahlids[amtcounter] = i;
                                amtcounter++;
                            }
                        }
                    }
                }
            }

            return wahlids;
        }
        #endregion

        #region GetEmptyGerichtsverhandlung
        public int GetEmptyGerichtsverhandlung()
        {
            for (int i = 0; i < SW.Statisch.GetmaxAnzahlGerichtsverhandlungen(); i++)
            {
                if (Gerichtshandlungen[i].IsEmpty() == true)
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion

        #region GetSpWithID
        public Spieler GetSpWithID(int id)
        {
            if (id < SW.Statisch.GetMinKIID())
            {
                return GetHumWithID(id);
            }
            return GetKIwithID(id);
        }
        #endregion

        #region GetGebietwithID
        public Gebiet GetGebietwithID(int id, int stufe)
        {
            if (stufe == 0)
            {
                return Staedte[id];
            }
            else if (stufe == 1)
            {
                return Laender[id];
            }
            else
            {
                return Reiche[id];
            }
        }
        #endregion

        #region GetLandIDzuStadtX
        public int GetLandIDzuStadtX(int x)
        {
            int landid = 0;

            //Jedes Land
            for (int i = 1; i < SW.Statisch.GetMaxLandID(); i++)
            {
                //Jede Stadt
                for (int j = 0; j < GetLandWithID(i).GetAnzahlStaedte(); j++)
                {
                    if (x == GetLandWithID(i).GetStadtX(j))
                    {
                        return i;
                    }
                }
            }

            return landid;
        }
        #endregion

        #region GetUntergebene
        public int[] GetUntergebene(int bossID)
        {
            //Alle Untergebenen herausfinden
            int amtid = GetSpWithID(bossID).GetAmtID();
            int gebid = GetSpWithID(bossID).GetAmtGebiet();
            int[] untergebeneAmtids = new int[7];

            int[] ufinalID = new int[7];
            int ucounter = 0;

            if (amtid == 0)
            {
                return ufinalID;
            }

            //Aemter
            for (int i = 1; i < SW.Statisch.GetMaxAmtID(); i++)
            {
                if (SW.Statisch.GetAmtwithID(i).GetWaehler1AmtID() == amtid || SW.Statisch.GetAmtwithID(i).GetWaehler2AmtID() == amtid || SW.Statisch.GetAmtwithID(i).GetWaehler3AmtID() == amtid)
                {
                    //Dann ist i die AmtsID eines Untergebenen
                    untergebeneAmtids[ucounter] = i;
                    ucounter++;
                }
            }

            if (ucounter == 0)
            {
                return ufinalID;
            }
            else
            {
                int b = 0;

                for (int k = 0; k < ucounter; k++)
                {
                    if (amtid < SW.Statisch.GetMaxAmtStadtID())
                    {
                        //Dann kann der Untergebene nur von derselben Stufe sein
                        ufinalID[b] = GetStadtwithID(gebid).GetAmtX(untergebeneAmtids[k]);
                        if (ufinalID[b] != 0)
                        {
                            b++;
                        }
                    }
                    else if (amtid < SW.Statisch.GetMaxAmtLandID())
                    {
                        //Dann kann der Untergebene von der unteren Stufe sein
                        if (untergebeneAmtids[k] < SW.Statisch.GetMaxAmtStadtID())
                        {
                            //Dann sind aus allen Städten dieses Landes die Leute untergebene
                            for (int l = 0; l < GetLandWithID(gebid).GetAnzahlStaedte(); l++)
                            {
                                ufinalID[b] = GetStadtwithID(GetLandWithID(gebid).GetStadtX(l)).GetAmtX(untergebeneAmtids[k]);
                                if (ufinalID[b] != 0)
                                {
                                    b++;
                                }
                            }
                        }
                        //oder derselben
                        else
                        {
                            ufinalID[b] = GetLandWithID(gebid).GetAmtX(untergebeneAmtids[k]);
                            if (ufinalID[b] != 0)
                            {
                                b++;
                            }
                        }
                    }
                    else
                    {
                        //Dann kann der Untergebene von der unteren Stufe sein
                        if (untergebeneAmtids[k] < SW.Statisch.GetMaxAmtLandID())
                        {
                            //Dann sind aus allen Laendern dieses Landes die Leute untergebene
                            for (int l = 1; l < SW.Statisch.GetMaxLandID(); l++)
                            {
                                ufinalID[b] = GetLandWithID(l).GetAmtX(untergebeneAmtids[k]);
                                if (ufinalID[b] != 0)
                                {
                                    b++;
                                }
                            }
                        }
                        //oder derselben
                        else
                        {
                            ufinalID[b] = GetReichWithID(gebid).GetAmtX(untergebeneAmtids[k]);
                            if (ufinalID[b] != 0)
                            {
                                b++;
                            }
                        }
                    }
                }
                return ufinalID;
            }
        }
        #endregion

        #region GetKIthatDislikesHumX
        public int GetKIthatDislikesHumX(int x, int minamtid, int maxamtid)
        {
            int aktuelle = 10;
            int dislike = 100;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (GetKIwithID(i).GetAmtID() >= minamtid)
                {
                    if (GetKIwithID(i).GetAmtID() <= maxamtid)
                    {
                        if (GetKIwithID(i).GetBeziehungZuKIX(x) < dislike)
                        {
                            dislike = GetKIwithID(i).GetBeziehungZuKIX(x);
                            aktuelle = i;
                        }
                    }
                }
            }

            return aktuelle;
        }
        #endregion

        #region GetMinGegnerAmtID
        public int GetMinGegnerAmtID(int humid)
        {
            int minamtid;

            if (GetHumWithID(humid).GetAmtID() < SW.Statisch.GetMaxAmtStadtID())
            {
                minamtid = 0;
            }
            else if (GetHumWithID(humid).GetAmtID() < SW.Statisch.GetMaxLandID())
            {
                minamtid = SW.Statisch.GetMaxAmtStadtID() + 1;
            }
            else
            {
                minamtid = SW.Statisch.GetMaxAmtLandID() + 1;
            }

            return minamtid;
        }
        #endregion

        #region GetMaxGegnerAmtID
        public int GetMaxGegnerAmtID(int humid)
        {
            int maxamtid;

            if (GetHumWithID(humid).GetAmtID() < SW.Statisch.GetMaxAmtStadtID())
            {
                maxamtid = 0;
            }
            else if (GetHumWithID(humid).GetAmtID() < SW.Statisch.GetMaxLandID())
            {
                maxamtid = SW.Statisch.GetMaxAmtStadtID() + 1;
            }
            else
            {
                maxamtid = SW.Statisch.GetMaxAmtLandID() + 1;
            }

            return maxamtid;
        }
        #endregion

        #region GetRelSympathieVonXzuY
        public int GetRelSympathieVonXzuY(int x, int y)
        {
            int value = 0;

            if (GetSpWithID(y).GetReligion() == SW.Statisch.GetRelFreiID())
            {
                return value;
            }

            if (GetSpWithID(x).GetReligion() == GetSpWithID(y).GetReligion())
            {
                value = 5;
            }
            else
            {
                value = -5;
            }

            return value;
        }
        #endregion

        #region GetWerkposInStadtXzuRohIDy
        public int GetWerkposInStadtXzuRohIDy(int x, int y)
        {
            //Zu einem Rohstoff schalten, der in der Stadt vorhanden ist
            for (int i = 1; i <= SW.Statisch.GetMaxWerkstaettenProStadt(); i++)
            {
                if (y == GetStadtwithID(x).GetSingleRohstoff(i))
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion

        #region GetGesetzXinText
        public string GetGesetzXinText(int x)
        {
            GesetzeAktualisieren();
            return GesetzesTexte[x];
        }
        #endregion

        #region GetUeberfallOpferInLand
        /// <summary>
        /// Ermittelt das Überfallopfer mit einer Karawane in einem bestimmten Land.
        /// </summary>
        /// <remarks>
        /// Es werden zuerst menschliche Spieler mit Verkaufs-Aktionen in den Städten des Landes berücksichtigt und erst danach wird nach einem zufällig KI-Amtsinhaber in einer der Städte gesucht.
        /// Bei KI-Spielern wird die Ware und der Warenwert zufällig ermittelt. Der Warenwert richtet sich grob nach einem Maximalwert von 10% des durchschnittlichen Gesamtvermögens aller menschlichen Spieler.
        /// </remarks>
        /// <param name="landID">ID des Landes, in dem das Opfer mit Karawane ermittelt werden soll</param>
        /// <param name="spielerIDAusnahme">Der menschliche Spieler mit dieser ID wird nicht ermittelt, da er der Angreifer ist</param>
        /// <returns>Das Objekt der KampfKarawane mit allen nötigen Angaben</returns>
        public KampfKarawane GetUeberfallOpferInLand(int landID, int spielerIDAusnahme)
        {
            List<KampfKarawane> karawanen = new List<KampfKarawane>();
            int stadtID;
            int wuerfel;
            int warenanzahl;
            int warenwert;

            // Zuerst nach einem menschlichen Spieler mit Stützpunkt in dieser Stadt suchen, der Waren mit Karawane exportiert
            // Der Spieler darf nicht der Ausnahme entsprechend, da dies der Angreifer ist

            for (int SpCounter = 1; SpCounter <= GetAktivSpielerAnzahl(); SpCounter++)  // Alle menschlichen Spieler durchgehen
            {
                if (spielerIDAusnahme == SpCounter)
                    continue;

                for (int StCounter = 0; StCounter < GetLandWithID(landID).GetAnzahlStaedte(); StCounter++)  // Alle Städte in dem Land durchgehen
                {
                    stadtID = GetLandWithID(landID).GetStadtX(StCounter);

                    for (int WsCounter = 1; WsCounter <= SW.Statisch.GetMaxWerkstaettenProStadt(); WsCounter++)  // Alle Werkstätten durchgehen
                    {
                        if (!GetHumWithID(SpCounter).GetSpielerHatInStadtXWerkstaettenY(WsCounter, stadtID).GetEnabled())  // WS vorhanden?
                            continue;

                        for (int ProdSlotCounter = 0; ProdSlotCounter < SW.Statisch.GetMaxProdSlots(); ProdSlotCounter++)  // Prüfen, ob ein Produktionsslot auf 'verkaufen' steht
                        {
                            Produktionsslot oProduktionsslot = GetHumWithID(SpCounter).GetProduktionsslot(stadtID, ProdSlotCounter);

                            if ((oProduktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Verkaufen || oProduktionsslot.GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.PermanentVerkaufen) &&
                                (oProduktionsslot.GetVerkaufAnzahl() > 0))
                            {
                                // Auswürfeln, ob der Sicherheitsbonus der Karawane vor dem Überfall schützt
                                wuerfel = SW.Statisch.Rnd.Next(1, 101);

                                if (wuerfel <= SW.Statisch.GetKarawane(GetHumWithID(SpCounter).GetKarawaneInStadtX(stadtID)).Sicherheit)
                                    continue;  // Kein Angriff, die Karawane konnte entgehen

                                // Treffer! Ermittle Warenanzahl und Warenwert der Waren, die gestohlen werden können, abhängig von der Sicherheit der Karawane
                                // Beispiel: kleiner gleich 20 % Sicherheit = 50 % möglicher Verlust, 30 % Sicherheit = 40 % möglicher Verlust usw.
                                double prozentsatzWarenanzahl = Convert.ToDouble(70 - SW.Statisch.GetKarawane(GetHumWithID(SpCounter).GetKarawaneInStadtX(stadtID)).Sicherheit) / 100d;
                                if (prozentsatzWarenanzahl > 0.5)
                                    prozentsatzWarenanzahl = 0.5;   // Maximaler Verlust: 50 % der mitgeführten Waren

                                warenanzahl = Convert.ToInt32(prozentsatzWarenanzahl * Convert.ToDouble(oProduktionsslot.GetVerkaufAnzahl()));
                                warenwert = GetStadtwithID(stadtID).GetRohstoffPreisVonIDX(oProduktionsslot.GetVerkaufRohstoff()) * warenanzahl;

                                // Die Karawane als Objekt 'KampfKarawane' der Liste hinzufügen
                                karawanen.Add(new KampfKarawane(SpCounter, stadtID, GetHumWithID(SpCounter).GetKarawaneInStadtX(stadtID), ProdSlotCounter,
                                                                   oProduktionsslot.GetVerkaufRohstoff(), warenanzahl, warenwert));
                            }
                        }
                    }
                }
            }

            if (karawanen.Count == 1)
                return karawanen[0];

            // Wenn in der Liste mehr als ein Treffer existiert, wird zufällig einer ausgewählt
            if (karawanen.Count > 1)
            {
                wuerfel = SW.Statisch.Rnd.Next(0, karawanen.Count);
                return karawanen[wuerfel];
            }
            else
            {
                // Gibt es keine Einträge in der Liste, wird also kein menschlicher Spieler gefunden, dann wird ein zufälliger KI-Amtsinhaber in einer der Städte genommen
                wuerfel = SW.Statisch.Rnd.Next(0, GetLandWithID(landID).GetAnzahlStaedte());  // Stadt ermitteln
                stadtID = GetLandWithID(landID).GetStadtX(wuerfel);

                wuerfel = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxAmtStadtID());  // Amt ermitteln
                int spielerID = GetStadtwithID(stadtID).GetAmtX(wuerfel);

                if (spielerID == 0)
                {
                    for (int i = 1; i < SW.Statisch.GetMaxAmtStadtID(); i++)
                    {
                        spielerID = GetStadtwithID(stadtID).GetAmtX(wuerfel);

                        if (spielerID != 0)
                            break;
                    }
                }

                if (spielerID == 0)
                    spielerID = SW.Statisch.GetMaxKIID() - 1;

                int karawaneID = SW.Statisch.Rnd.Next(0, SW.Statisch.GetMaxKarawane());  // Karawane ermitteln
                int gesamtvermoegen = 0;

                // Warenwert ermitteln
                for (int SpCounter = 1; SpCounter <= GetAktivSpielerAnzahl(); SpCounter++)  // Alle menschlichen Spieler durchgehen
                    gesamtvermoegen += GetHumWithID(SpCounter).GetGesamtVermoegen(SpCounter);

                wuerfel = SW.Statisch.Rnd.Next(1, 7);  // Prozentualen Anteil ermitteln
                double dFaktor = Convert.ToDouble(wuerfel) / Convert.ToDouble(100);

                warenwert = Convert.ToInt32((Convert.ToDouble(gesamtvermoegen) / Convert.ToDouble(GetAktivSpielerAnzahl())) * dFaktor);

                return new KampfKarawane(spielerID, stadtID, karawaneID, 0, 0, 0, warenwert);
            }
        }
        #endregion

        #region GetStufeVonAmtmitIDx
        public int GetStufeVonAmtmitIDx(int x)
        {
            int amtstufe = SW.Statisch.GetAmtwithID(x).GetAmtsStufe();

            if (amtstufe < 6)
            {
                return 0;
            }
            if (amtstufe < 11)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        #endregion

        #region GetSpXlebtNochSoVielJahre
        public int GetSpXlebtNochSoVielJahre(int SpID)
        {
            int verblJahre = GetSpWithID(SpID).GetVerbleibendeJahre();
            int gesundheit = GetSpWithID(SpID).GetGesundheit();
            int fehlendeGesundheit = SW.Statisch.GetMaxGesundheit() - gesundheit;

            int minusGesundJahre = 0;

            while (fehlendeGesundheit > 0)
            {
                fehlendeGesundheit -= 10;
                minusGesundJahre++;
            }

            int gesamtjahre = verblJahre - minusGesundJahre;
            return gesamtjahre;
        }
        #endregion


        #region BerechneAnteilRohstoffvorratImLand
        /// <summary>
        /// Berechnet den Anteil des Rohstoffvorrats einer bestimmten Stadt am Gesamtvorrat im Land. Wird z.B. für die Stadtinformation verwendet.
        /// </summary>
        /// <param name="StadtID">Gewünschte StadtID</param>
        /// <returns>Array mit allen Rohstoffen und einem Anteil in Prozent von 0 bis 100</returns>
        public int[] BerechneAnteilRohstoffvorratImLand(int StadtID)
        {
            int[] RohstoffvorratAnteilStadt = new int[SW.Statisch.GetMaxRohID()];  // in Prozent
            int[] RohstoffvorratSummeLand = new int[SW.Statisch.GetMaxRohID()];
            // Alle Rohstoffvorräte aller Städte im Land summieren
            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                for (int j = 1; j < SW.Statisch.GetMaxRohID(); j++)
                    RohstoffvorratSummeLand[j] += GetStadtwithID(i).GetRohstoffIDXVorrat(j);
            }
            // Anteil des Rohstoffvorrats der gewünschten Stadt berechnen
            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                if (GetStadtwithID(StadtID).GetRohstoffIDXVorrat(i) != 0)
                    RohstoffvorratAnteilStadt[i] = Convert.ToInt32(Math.Round((Convert.ToDouble(GetStadtwithID(StadtID).GetRohstoffIDXVorrat(i)) / Convert.ToDouble(RohstoffvorratSummeLand[i])) * 100d, 0));
                else
                    RohstoffvorratAnteilStadt[i] = 0;
            }

            return RohstoffvorratAnteilStadt;
        }
        #endregion

        #region CheckIfenoughGold
        public bool CheckIfenoughGold(int preis)
        {
            if (GetHumWithID(GetAktiverSpieler()).GetTaler() >= preis)
                return true;

            BelTextAnzeigen("Ihr besitzt die " + preis.ToStringGeld(false) + "\n Taler für dieses\nVorhaben nicht.");
            return false;
        }
        #endregion

        #region LandsicherheitenInitialisieren
        /// <summary>
        /// Diese Funktion intialisiert die Landsicherheiten auf die Standardwerte (wird vor jedem Rundenende aufgerufen).
        /// </summary>
        public void LandsicherheitenInitialisieren()
        {
            List<StuetzpunktAktion> aktionen;

            Landsicherheiten = new Landsicherheit[SW.Statisch.GetMaxLandID() - 1];

            for (int i = 1; i < SW.Statisch.GetMaxLandID(); i++)
            {
                aktionen = new List<StuetzpunktAktion>();

                foreach (Stuetzpunkt stuetzpunkt in Stuetzpunkte)
                {
                    if (stuetzpunkt.Aktionen != null)
                    {
                        foreach (StuetzpunktAktion aktion in stuetzpunkt.Aktionen)
                        {
                            if (aktion == null)
                                continue;

                            if (aktion.ZielLandID == i)
                                aktionen.Add(aktion);
                        }
                    }
                }

                Landsicherheiten[i - 1] = new Landsicherheit(i, 40, aktionen);
            }
        }
        #endregion

        #region KIstirbt
        public void KIstirbt(int kiid)
        {
            GetKIwithID(kiid).SetStirbt(false);

            // Falls der Tote ein Amt hatte
            if (GetKIwithID(kiid).GetAmtID() != 0)
                AmtVonXfreigeben(kiid);  // Tote KI soll sein Amt verlieren

            // Und wieder jung sein
            GetKIwithID(kiid).SetAlter(SW.Statisch.Rnd.Next(20, 30));
            GetKIwithID(kiid).SetVerbleibendeJahre(SW.Statisch.Rnd.Next(10, 30));

            // Die Delikte verfallen bis auf eins...
            GetKIwithID(kiid).SetDeliktpunkte(1);

            // Und nicht mehr verheiratet sein falls der Tote verheiratet war
            if (GetKIwithID(kiid).GetVerheiratet() != 0)
            {
                // Falls er mit einem Humspieler verheiratet war
                if (GetKIwithID(kiid).GetVerheiratet() < SW.Statisch.GetMinKIID())
                {
                    for (int i = 1; i <= GetAktivSpielerAnzahl(); i++)
                    {
                        if (GetHumWithID(i).GetErbeSpielerID() == kiid)
                        {
                            BelTextAnzeigen("Euer geliebter Ehepartner ist dieses Jahr von uns gegangen!");

                            // Zur Zeit einfach Erzbistum als Erben setzen
                            if (GetHumWithID(i).GetErbeSpielerID() == kiid)
                            {
                                GetHumWithID(i).SetErbeSpielerID(0);
                                BelTextAnzeigen("Da Euer im Testament erwähnter Erbe verstorben ist,\nist aktuell das Erzbistum der Erbe Eures gesamten Vermögens!");
                            }
                            GetHumWithID(i).SetVerheiratet(0);
                        }
                    }
                }
                else
                {
                    GetKIwithID(GetKIwithID(kiid).GetVerheiratet()).SetVerheiratet(0);
                }

                GetKIwithID(kiid).SetVerheiratet(0);
            }
        }
        #endregion

        #region AktivenSpielerEntfernen
        public bool? AktivenSpielerEntfernen()
        {
            if (SW.UI.JaNeinFrage.ShowDialogText(textFrage: "Wollt Ihr wirklich all' Euer Geld aus dem Fenster\n werfen und das Spiel verlassen?", "Ja", "Lieber nicht") != DialogResult.Yes)
            {
                return null;
            }

            // Ein möglicher Ehepartner soll nicht mehr verheiratet sein
            if (GetHumWithID(GetAktiverSpieler()).GetVerheiratet() != 0)
            {
                GetKIwithID(GetHumWithID(GetAktiverSpieler()).GetVerheiratet()).SetVerheiratet(0);
            }

            string name = GetHumWithID(GetAktiverSpieler()).GetName();

            bool last = false;
            if (GetAktiverSpieler() == GetAktivSpielerAnzahl())
            {
                last = true;
            }

            // Amt freigeben
            AmtVonXfreigeben(GetAktiverSpieler());

            // Von Wahl abmelden
            if (GetHumWithID(GetAktiverSpieler()).GetWahlTeilnahme() != 0)
            {
                // Position suchen
                int u = 0;
                while (true)
                {
                    if (GetWahlX(GetHumWithID(GetAktiverSpieler()).GetWahlTeilnahme()).GetKandidaten()[u] == GetAktiverSpieler())
                    {
                        GetWahlX(GetHumWithID(GetAktiverSpieler()).GetWahlTeilnahme()).SetKandidatenXAufY(u, 0);
                        break;
                    }
                    u++;
                }

                GetHumWithID(GetAktiverSpieler()).SetWahlTeilnahme(0);  // Teilnahme zurücksetzen
            }

            CreateSpielerX(GetAktiverSpieler(), 0, "", true, 0, 0);  // Aktuelles Spieler Objekt initialisieren (auf null setzen führt ansonsten z.B. in der Statistik zu Problemen beim Zugriff: NullReference Exception)
            SetAktivSpielerAnzahl(GetAktivSpielerAnzahl() - 1);

            // Ist der Spieler der letzte in der Liste? Dann wird nicht geordnet und einfach ein neues Jahr gestartet
            if (last)
            {
                SetAktiverSpieler(1);
                ErhoehAktuellesJahrUmEins();
            }
            else
            {
                // Ist der Spieler nicht der letzte, so muss die Liste neu geordnet werden
                int i = GetAktiverSpieler();
                while (i + 1 <= GetAktivSpielerAnzahl() + 1)
                {
                    SetHumX(i, GetHumWithID(i + 1));
                    i++;
                }
            }

            if (GetAktivSpielerAnzahl() != 0)
            {
                BelTextAnzeigen($"Der Spieler {name} wurde aus dem Spiel entfernt.");
                return false;
            }
            else
            {
                BelTextAnzeigen($"Der Spieler {name} wurde aus dem Spiel entfernt.\n Es befinden sich keine weiteren Mitstreiter in diesem Spiel.\n Das Spiel wird daher beendet.");
                return true;
            }
        }
        #endregion

        #region Ermordung
        public void Ermordung(int id)
        {
            if (id >= SW.Statisch.GetMinKIID())
            {
                //Es ist noch keine Ermordung in Auftrag
                if (GetHumWithID(GetAktiverSpieler()).GetErmordetKISpielerID() == 0)
                {
                    int kosten = Convert.ToInt32(GetKIwithID(id).GetTaler() * SW.Statisch.GetErmordungProzentsatz());

                    if (SW.UI.JaNeinFrage.ShowDialogText(textFrage: "Wollt Ihr wirklich die Ermordung von\n" + GetKIwithID(id).GetKompletterName() + " für " + kosten.ToStringGeld() + "\nin Auftrag geben?") == DialogResult.Yes)
                    {
                        if (CheckIfenoughGold(kosten))  // Wenn man auch genügend Taler besitzt
                        {
                            if (GetGesetzX(23) != 0)  // Wenn es verboten ist
                                GetHumWithID(GetAktiverSpieler()).ErhoeheGesetzXUmEins(23);

                            GetHumWithID(GetAktiverSpieler()).GetSpielerStatistik().HiVersuchteErmordungen++;
                            GetHumWithID(GetAktiverSpieler()).ErhoeheTaler(-kosten);
                            GetHumWithID(GetAktiverSpieler()).SetErmordetKISpielerID(id);
                        }
                    }
                }
                //Es ist schon eine Ermordung in Auftrag
                else
                {
                    BelTextAnzeigen("Ihr habt dieses Jahr bereits eine Ermordung in Auftrag gegeben...");
                }
            }
            else
            {
                BelTextAnzeigen("Ihr könnt keinen menschlichen Mitspieler ermorden lassen");
            }
        }
        #endregion

        #region Erpressen
        public void Erpressen(int id)
        {
            if (id >= SW.Statisch.GetMinKIID())
            {
                int Delikte = GetHumWithID(GetAktiverSpieler()).GetAktiveSpionage(id).GetDelikte();
                string Name = GetKIwithID(id).GetKompletterName();

                if ((Delikte * 10) > 69)
                {
                    //lässt sich erpressen
                    BelTextAnzeigen(Name + " muss sich den erdrückenden Beweisen beugen und steht nun unter Eurer Fuchtel.");
                }
                else
                {
                    //lässt sich nicht erpressen
                    BelTextAnzeigen(Name + " lacht über Eure läppischen Drohungen.");
                    GetKIwithID(id).ErhoeheBeziehungZuX(GetAktiverSpieler(), -20);
                }
            }
            else
            {
                BelTextAnzeigen("Ihr könnt keinen menschlichen Mitspieler erpressen");
            }
        }
        #endregion

        #region PartnerSuchen
        public void PartnerSuchen(int id)
        {
            if (id >= SW.Statisch.GetMinKIID())
            {
                if (GetKIwithID(id).GetMaennlich() == GetHumWithID(GetAktiverSpieler()).GetMaennlich())
                {
                    BelTextAnzeigen("Die Person ist vom gleichen\nGeschlecht, wie Ihr es seid.");
                }
                else
                {
                    if (GetKIwithID(id).GetVerheiratet() != 0)
                    {
                        BelTextAnzeigen("Diese Person ist bereits verheiratet.");
                    }
                    else
                    {
                        GetHumWithID(GetAktiverSpieler()).WirbtUmSpielerID = id;
                        GetKIwithID(id).ErhoeheVerliebt(-20);
                        BelTextAnzeigen("Ihr habt Euch entschlossen, um " + GetKIwithID(id).GetKompletterName() + " zu werben.");
                    }
                }
            }
            else
            {
                BelTextAnzeigen("Ihr könnt keinen menschlichen Mitspieler heiraten.");
            }
        }
        #endregion

        #region ProzessInitiieren
        public void ProzessInitiieren(int id)
        {

            if (SW.UI.JaNeinFrage.ShowDialogText(textFrage: "Wollt Ihr wirklich gegen " + GetSpWithID(id).GetKompletterName() + "\neinen Prozess initiieren?") == DialogResult.Yes)
            {
                GetHumWithID(GetAktiverSpieler()).SetKlagtSpielerMitIDXAn(id);
                SpielerKlagtXAn(id);
                BelTextAnzeigen("Ihr leitet einen Prozess gegen " + GetSpWithID(id).GetName() + " in die Wege.");
            }
        }
        #endregion

        #region WeinVergiften
        public void WeinVergiften(int id)
        {
            int kosten = 5000;

            // Es ist noch keine Vergiftung in Vorbereitung
            if (GetHumWithID(GetAktiverSpieler()).GetVergiftetWeinVonKISpielerID() == 0)
            {
                if (SW.UI.JaNeinFrage.ShowDialogText(textFrage: "Wollt Ihr wirklich für " + kosten.ToStringGeld() + "\n einen Trank von " + GetSpWithID(id).GetName() + " vergiften?") == DialogResult.Yes)
                {
                    if (CheckIfenoughGold(kosten))
                    {
                        GetHumWithID(GetAktiverSpieler()).ErhoeheTaler(-kosten);
                        GetHumWithID(GetAktiverSpieler()).SetVergiftetWeinVonKISpielerID(id);
                    }
                }
            }
            // Es ist schon eine Vergiftung in Vorbereitung
            else
            {
                BelTextAnzeigen("Ihr habt in diesem Jahr bereits Vorbereitungen für einen anderen Anschlag getroffen.");
            }
        }
        #endregion

        #region HenkersHand
        public void HenkersHand(int id)
        {
            //Es ist noch keine 'Henkers Hand' in Vorbereitung
            if (GetHumWithID(GetAktiverSpieler()).GetHenkersHand() == false)
            {
                GetHumWithID(GetAktiverSpieler()).SetHenkersHand(true);

                GetSpWithID(id).ErhoeheAnsehen(-50);
                BelTextAnzeigen("Auf dem Marktplatz erblickt Ihr " + GetSpWithID(id).GetName() + ". Ihr nähert Euch, täuscht es vor zu stolpern und berührt dabei " + GetSpWithID(id).GetName() + ". " + GetSpWithID(id).GetSeinIhr() + " Ansehen leidet stark.");
            }
            //Henkers Hand bereits benutzt
            else
            {
                BelTextAnzeigen("Ihr habt dieses Jahr bereits einmal Euer Amt missbraucht");
            }
        }
        #endregion

        #region RundenBestechungenAbwickeln
        public void RundenBestechungenAbwickeln()
        {
            for (int i = 1; i <= GetAktivSpielerAnzahl(); i++)
            {
                for (int j = 1; j < SW.Statisch.GetMaxKIID(); j++)
                {
                    if (GetHumWithID(i).GetBestechungVonSpielerMitIDX(j) != 0)
                    {
                        int summe = GetHumWithID(i).GetBestechungVonSpielerMitIDX(j);

                        if (j >= SW.Statisch.GetMinKIID())
                        {
                            // Wenn eine KI Geld bekommen sollte
                            int oamtid = GetKIwithID(j).GetAmtID();
                            int oamtstufe = 0;

                            if (oamtid != 0)
                            {
                                oamtstufe = SW.Statisch.GetAmtwithID(oamtid).GetAmtsStufe();
                            }

                            int produkt;
                            if (oamtstufe > 0)
                            {
                                produkt = summe / (oamtstufe * 2);
                            }
                            else
                            {
                                produkt = summe;
                            }

                            if (oamtstufe > 5)
                            {
                                produkt = produkt / 2;
                            }

                            if (oamtstufe > 10)
                            {
                                produkt = produkt / 2;
                            }

                            int verbesserung = Convert.ToInt32(produkt / 10);

                            GetKIwithID(j).ErhoeheBeziehungZuX(i, verbesserung);
                        }

                        GetHumWithID(i).SetBestechungVonSpielerMitIDXAufY(i, 0);
                        GetSpWithID(j).ErhoeheTaler(summe);
                    }
                }
            }
        }
        #endregion

        #region DeliktpunkteBerechnen
        public void DeliktpunkteBerechnen()
        {
            for (int i = 1; i <= GetAktivSpielerAnzahl(); i++)
            {
                int value = 0;
                var spieler = GetHumWithID(i);

                value += spieler.GetBegingVerbrechenX(22) * 1;
                value += spieler.GetBegingVerbrechenX(1) * 2;
                value += spieler.GetBegingVerbrechenX(23) * 5;
                value += spieler.GetBegingVerbrechenX(21) * 2;
                value += spieler.GetBegingVerbrechenX(20) * 2;

                spieler.SetDeliktpunkte(value);
            }
        }
        #endregion

        #region KIAktionenDurchfuehren
        public void KIAktionenDurchfuehren()
        {
            // Jede KI
            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                // Altern
                GetSpWithID(i).AlterPlusEins();

                // Gesundheit random etwas ändern
                int rand_nr = SW.Statisch.Rnd.Next(-5, 6);
                GetSpWithID(i).ErhoeheGesundheit(rand_nr);

                // Neue Deliktpunkte
                int boese = GetKIwithID(i).GetBosheit();
                int rndneuerpunkt = SW.Statisch.Rnd.Next(0, 100);
                int rndplus = SW.Statisch.Rnd.Next(1, 4);
                if (rndneuerpunkt < boese)
                {
                    GetKIwithID(i).SetDeliktpunkte(GetKIwithID(i).GetDeliktpunkte() + rndplus);
                }
                else
                {
                    GetKIwithID(i).SetDeliktpunkte(GetKIwithID(i).GetDeliktpunkte() - 1);
                }

                // Verfall
                int rndverfall = SW.Statisch.Rnd.Next(0, 10); // Damit kann eine KI nie mehr als 20 Deliktpunkte haben
                if (GetKIwithID(i).GetDeliktpunkte() > rndverfall)
                {
                    GetKIwithID(i).SetDeliktpunkte(Convert.ToInt32((GetKIwithID(i).GetDeliktpunkte() * 2) / 2));
                }

                #region Amtsentehungen
                // Wenn sie zu einem Untergebenen eine schlechte Beziehung hat
                int[] untergebene = GetUntergebene(i);
                int u_len = 0;

                int z = 0;
                while (z < untergebene.Length)
                {
                    if (untergebene[z] != 0)
                    {
                        u_len++;
                    }
                    z++;
                }

                int maxAbsetzSympathie = SW.Statisch.GetMaxAbsetzSympathie();

                switch (SW.Dynamisch.Spielstand.Einstellungen.AggressivitaetKISpieler)
                {
                    case EnumSchwierigkeitsgrad.Niedrig:
                        maxAbsetzSympathie -= 2;
                        break;
                    case EnumSchwierigkeitsgrad.Mittel:
                        maxAbsetzSympathie += 10;
                        break;
                    case EnumSchwierigkeitsgrad.Hoch:
                        maxAbsetzSympathie += 20;
                        break;
                }

                for (int j = 1; j < u_len; j++)
                {
                    if (GetKIwithID(i).GetBeziehungZuKIX(untergebene[j]) < maxAbsetzSympathie)
                    {
                        // KI fordert Absetzung
                        SetAmtsenthebungVonID(untergebene[j]);
                    }
                }
                #endregion

                #region Beziehungen schwanken
                // Bei Spieler soll es abhängig sein vom grad, den sie einen mögen
                for (int k = 1; k <= GetAktivSpielerAnzahl(); k++)
                {
                    int rand;
                    if (GetKIwithID(i).GetBeziehungZuKIX(k) > 80)
                    {
                        rand = SW.Statisch.Rnd.Next(-10, -5);
                    }
                    else if (GetKIwithID(i).GetBeziehungZuKIX(k) > 60)
                    {
                        rand = SW.Statisch.Rnd.Next(-5, 0);
                    }
                    else
                    {
                        rand = SW.Statisch.Rnd.Next(-5, 5);
                    }

                    int jackpot = SW.Statisch.Rnd.Next(0, 400);
                    if (jackpot == 100)
                    {
                        rand += 30;
                    }
                    if (jackpot == 200)
                    {
                        rand -= 30;
                    }
                    if (jackpot == 150)
                    {
                        rand += 15;
                    }
                    if (jackpot == 250)
                    {
                        rand -= 15;
                    }

                    GetKIwithID(i).ErhoeheBeziehungZuX(k, rand);
                }

                // KIs unter sich
                for (int k = SW.Statisch.GetMinKIID(); k < SW.Statisch.GetMaxKIID(); k++)
                {
                    int rand = SW.Statisch.Rnd.Next(-5, 6);
                    int jackpot = SW.Statisch.Rnd.Next(0, 400);
                    if (jackpot == 100)
                    {
                        rand += 30;
                    }
                    if (jackpot == 200)
                    {
                        rand -= 30;
                    }
                    if (jackpot == 150)
                    {
                        rand += 15;
                    }
                    if (jackpot == 250)
                    {
                        rand -= 15;
                    }

                    GetKIwithID(i).ErhoeheBeziehungZuX(k, rand);
                }
                #endregion
            }
        }
        #endregion

        #region KIVerheiraten
        public void KIVerheiraten()
        {
            int[] unverhMaenner = new int[SW.Statisch.GetMaxKIID()];
            int[] unverhFrauen = new int[SW.Statisch.GetMaxKIID()];
            int mcounter = 0;
            int fcounter = 0;

            for (int i = SW.Statisch.GetMinKIID(); i < SW.Statisch.GetMaxKIID(); i++)
            {
                if (GetKIwithID(i).GetVerheiratet() == 0)
                {
                    if (i < SW.Statisch.GetMaennerFrauenGrenze())
                    {
                        unverhMaenner[mcounter] = i;
                        mcounter++;
                    }
                    else
                    {
                        unverhFrauen[fcounter] = i;
                        fcounter++;
                    }
                }
            }

            if (mcounter >= (SW.Statisch.GetMaennerFrauenGrenze() / 2) && fcounter >= ((SW.Statisch.GetMaennerFrauenGrenze() / 2) - 10))
            {
                for (int j = 0; j < 10; j++)
                {
                    VerheirateXundY(unverhMaenner[j], unverhFrauen[j]);
                }
            }
        }
        #endregion

        #region BerechneProdKosten
        public int BerechneProdKosten(int stadtid, int slot0oder1)
        {
            #region Produktion
            if (GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Produzieren)
            {
                int rohid = GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetProduktionRohstoff();
                int arbeiterpreis = GetRohstoffwithID(rohid).GetWSArbeiterpreis();
                int arbeiterkosten = GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetProduktionArbeiter() * arbeiterpreis;  //VERMERK: getAnzahl1 könnte auch setAnzahl1 gewesen sein

                int prodstaettepreis = GetRohstoffwithID(rohid).GetWSEinzelpreis();
                int prodstaettenkosten = GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetProduktionStaetten() * prodstaettepreis;

                return arbeiterkosten + prodstaettenkosten;
            }
            #endregion

            #region Verkauf
            if (GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.Verkaufen || GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetTaetigkeit() == (int)EnumProduktionsslotAktionsart.PermanentVerkaufen)
            {
                int kara_id = GetHumWithID(GetAktiverSpieler()).GetKarawaneInStadtX(stadtid);
                int karaPreisPro100Stueck = SW.Statisch.GetKarawane(kara_id).PreisProStueck;
                int karaGrundPreis = SW.Statisch.GetKarawane(kara_id).Fixpreis;

                int anz_exp = GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtid, slot0oder1).GetVerkaufAnzahl();

                if (anz_exp == 0)
                {
                    return 0;
                }

                int anz_fuhren = 0;

                while (anz_exp > 0)
                {
                    anz_fuhren++;
                    anz_exp -= 100;
                }

                return Convert.ToInt32(karaGrundPreis + karaPreisPro100Stueck * anz_fuhren);
            }
            #endregion

            return 0;
        }
        #endregion

        #region ProdVerhaeltnisAnzeigen
        public void ProdVerhaeltnisAnzeigen(int stadtID, int slot)
        {
            if (GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtID, slot).GetTaetigkeit() == 1)
            {
                int rohid = GetHumWithID(GetAktiverSpieler()).GetProduktionsslot(stadtID, slot).GetProduktionRohstoff();
                int arbeiter = GetRohstoffwithID(rohid).GetArbeiter();
                int werkst = GetRohstoffwithID(rohid).GetWerkstaetten();

                BelTextAnzeigen("Das Produktionsverhältnis zwischen Arbeitern und Werkstätten bei " + GetRohstoffwithID(rohid).GetRohName() + " ist " + arbeiter.ToString() + " zu " + werkst.ToString());
            }
        }
        #endregion

        #region PrivilegienAktualisieren
        public void PrivilegienAktualisieren()
        {
            #region 1 - Medikus
            GetAktHum().SetPrivilegX(1, true);
            #endregion

            #region 2 - Amt niederlegen
            if (GetAktHum().GetAmtID() == 0)
            {
                GetAktHum().SetPrivilegX(2, false);
            }
            else
            {
                GetAktHum().SetPrivilegX(2, true);
                GetAktHum().HandelszertifikatVerleihen(2, 1, 8);
            }
            #endregion

            #region 3 - Testament machen
            if (GetAktHum().GetVerheiratet() == 0)
            {
                GetAktHum().SetPrivilegX(3, false);
                for (int i = SW.Statisch.GetMinKindSlotNr(); i < SW.Statisch.GetMaxKinderAnzahl(); i++)
                {
                    if (GetAktHum().GetKindX(i).GetKindName() != "")
                    {
                        GetAktHum().SetPrivilegX(3, true);
                        break;
                    }
                }
            }
            else
            {
                GetAktHum().SetPrivilegX(3, true);
            }
            #endregion

            #region 4 - Handelszertifikate
            GetAktHum().SetPrivilegX(4, true);
            #endregion

            #region 5 - Einkommen
            if (GetAktHum().GetAmtID() != 0)
            {
                GetAktHum().SetPrivilegX(5, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(5, false);
            }
            #endregion

            #region 6 - Untergebene
            if (GetAktHum().GetAmtID() != 0)
            {
                //Wenn auch noch Untergebene vorhanden sind...
                int[] uid = SW.Dynamisch.GetUntergebene(SW.Dynamisch.GetAktiverSpieler());
                if (uid[0] != 0)
                {
                    GetAktHum().SetPrivilegX(6, true);
                }
            }
            else
            {
                GetAktHum().SetPrivilegX(6, false);
            }
            #endregion

            #region 7 - Kerkerklatsch
            if (GetAktHum().GetAmtID() == 15)
            {
                GetAktHum().SetPrivilegX(7, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(7, false);
            }
            #endregion

            #region 8 - Confessio
            if (GetAktHum().GetAmtID() == 8 || GetAktHum().GetAmtID() == 9)
            {
                GetAktHum().SetPrivilegX(8, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(8, false);
            }
            #endregion

            #region 9 - Prozess initiieren
            GetAktHum().SetPrivilegX(9, true);
            #endregion

            #region 10 - Bauwerk stiften
            if (GetAktHum().GetAmtID() >= SW.Statisch.GetMaxAmtStadtID())
            {
                GetAktHum().SetPrivilegX(10, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(10, false);
            }
            #endregion

            #region 11 - Händler (AUSKOMMENTIERT)
            //if (GetAktHum().getAmtID() == 6)
            //{
            //    GetAktHum().setPrivX(11, true);
            //}
            //else
            //{
            //    GetAktHum().setPrivX(11, false);
            //}
            #endregion

            #region 12 - Kaufmann (AUSKOMMENTIERT)
            //if (GetAktHum().getAmtID() == 21)
            //{
            //    GetAktHum().setPrivX(12, true);
            //}
            //else
            //{
            //    GetAktHum().setPrivX(12, false);
            //}
            #endregion

            #region 13 - Großkaufmann (AUSKOMMENTIERT)
            //if (GetAktHum().getAmtID() == 38)
            //{
            //    GetAktHum().setPrivX(13, true);
            //}
            //else
            //{
            //    GetAktHum().setPrivX(13, false);
            //}
            #endregion

            #region 14 - Umsatzsteuer festlegen
            if (GetAktHum().GetAmtID() == 7)
            {
                GetAktHum().SetPrivilegX(14, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(14, false);
            }
            #endregion

            #region 15 - Sparplan
            if (GetAktHum().GetAmtID() == 4)
            {
                GetAktHum().SetPrivilegX(15, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(15, false);
            }
            #endregion

            #region 16 - Kein Kirchenzehnt
            if ((GetAktHum().GetAmtID() >= 10 && GetAktHum().GetAmtID() <= 10) || (GetAktHum().GetAmtID() >= 23 && GetAktHum().GetAmtID() <= 27) || (GetAktHum().GetAmtID() >= 40 && GetAktHum().GetAmtID() <= 42))
            {
                GetAktHum().SetPrivilegX(16, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(16, false);
            }
            #endregion

            #region 17 - Vergifteter Wein
            if (GetAktHum().GetAmtID() == 23)
            {
                GetAktHum().SetPrivilegX(17, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(17, false);
            }
            #endregion

            #region 18 - Wachen
            if (GetAktHum().GetAmtID() == 22 || GetAktHum().GetAmtID() == 27 || GetAktHum().GetAmtID() == 33 || GetAktHum().GetAmtID() >= 34)
            {
                GetAktHum().SetPrivilegX(18, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(18, false);
            }
            #endregion

            #region 19 - Leibgarde
            if (GetAktHum().GetAmtID() == 39 || GetAktHum().GetAmtID() == 42 || GetAktHum().GetAmtID() == 48)
            {
                GetAktHum().SetPrivilegX(19, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(19, false);
            }
            #endregion

            #region 20 - HenkersHand
            if (GetAktHum().GetAmtID() == 13)
            {
                GetAktHum().SetPrivilegX(20, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(20, false);
            }
            #endregion

            #region 21 - Korruptionsgelder
            if (GetAktHum().GetAmtID() == 11)
            {
                GetAktHum().SetPrivilegX(21, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(21, false);
            }
            #endregion

            #region 22 - Schmuggel
            if (GetAktHum().GetAmtID() == 29 || GetAktHum().GetAmtID() == 30 || GetAktHum().GetAmtID() == 32)
            {
                GetAktHum().SetPrivilegX(22, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(22, false);
            }
            #endregion

            #region 23 - Zollkartell
            if (GetAktHum().GetAmtID() == 32)
            {
                GetAktHum().SetPrivilegX(23, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(23, false);
            }
            #endregion

            #region 24 - Kirchengesetze festlegen
            if (GetAktHum().GetAmtID() == 42)
            {
                GetAktHum().SetPrivilegX(24, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(24, false);
            }
            #endregion

            #region 25 - Finanzgesetze festlegen
            if (GetAktHum().GetAmtID() == 38)
            {
                GetAktHum().SetPrivilegX(25, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(25, false);
            }
            #endregion

            #region 26 - Justizgesetze festlegen
            if (GetAktHum().GetAmtID() == 37)
            {
                GetAktHum().SetPrivilegX(26, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(26, false);
            }
            #endregion

            #region 27 - Steuerhinterziehung A
            if (GetAktHum().GetAmtID() == 1 || GetAktHum().GetAmtID() == 2 || GetAktHum().GetAmtID() == 3)
            {
                GetAktHum().SetPrivilegX(27, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(27, false);
            }
            #endregion

            #region 28 - Steuerhinterziehung B
            if (GetAktHum().GetAmtID() == 17 || GetAktHum().GetAmtID() == 18 || GetAktHum().GetAmtID() == 19)
            {
                GetAktHum().SetPrivilegX(28, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(28, false);
            }
            #endregion

            #region 29 - Steuerhinterziehung C
            if (GetAktHum().GetAmtID() == 34 || GetAktHum().GetAmtID() == 35 || GetAktHum().GetAmtID() == 36)
            {
                GetAktHum().SetPrivilegX(29, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(29, false);
            }
            #endregion

            #region 30 - Günstige Kredite
            if (GetAktHum().GetAmtID() == 22)
            {
                GetAktHum().SetPrivilegX(30, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(30, false);
            }
            #endregion

            #region 31 - Zollfrei
            if (GetAktHum().GetAmtID() == 29 || GetAktHum().GetAmtID() == 30)
            {
                GetAktHum().SetPrivilegX(31, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(31, false);
            }
            #endregion

            #region 32 - Prediger
            if (GetAktHum().GetAmtID() == 8 || GetAktHum().GetAmtID() == 9)
            {
                GetAktHum().SetPrivilegX(32, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(32, false);
            }
            #endregion

            #region 33 - Ein Fest geben
            // Der Spieler benötigt einen Wohnsitz und muss mind. Bürger sein
            if (GetAktHum().GetAnzahlHaeuser() > 0 && GetAktHum().GetTitel() > 0)
            {
                GetAktHum().SetPrivilegX(33, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(33, false);
            }
            #endregion

            #region 34 - Jurist aufsuchen
            // Der Spieler muss mind. Bürger sein
            if (GetAktHum().GetTitel() > 0)
            {
                GetAktHum().SetPrivilegX(34, true);
            }
            else
            {
                GetAktHum().SetPrivilegX(34, false);
            }
            #endregion
        }
        #endregion

        #region AnklagenVonKISpielernErstellen
        /// <summary>
        /// Anklagen von KI-Spielern für den aktiven menschlichen Spieler erstellen.
        /// </summary>
        public void AnklagenVonKISpielernErstellen()
        {
            DeliktpunkteBerechnen();

            // Die Chance beruht auf den eigenen Deliktpunkten und auf der Einstellung "Aggressivität der KI-Spieler"
            int deliktpunkte = GetHumWithID(GetAktiverSpieler()).GetDeliktpunkte();

            if (deliktpunkte <= 0)  // Man wird nie ohne Deliktpunkte von KI-Spielern angeklagt
                return;

            int faktor = 10;

            switch (SW.Dynamisch.Spielstand.Einstellungen.AggressivitaetKISpieler)
            {
                case EnumSchwierigkeitsgrad.Niedrig:
                    faktor = 4;
                    break;
                case EnumSchwierigkeitsgrad.Mittel:
                    faktor = 8;
                    break;
                case EnumSchwierigkeitsgrad.Hoch:
                    faktor = 12;
                    break;
            }

            int chanceAufAnklageInProzent = deliktpunkte * faktor;

            if (chanceAufAnklageInProzent < 100)
            {
                if (chanceAufAnklageInProzent < SW.Statisch.Rnd.Next(0, 99))
                    return;
            }

            int minamtID = GetMinGegnerAmtID(GetAktiverSpieler());
            int maxamtID = GetMaxGegnerAmtID(GetAktiverSpieler());

            int klaegerID = GetKIthatDislikesHumX(GetAktiverSpieler(), minamtID, maxamtID);
            int klageID = GetEmptyGerichtsverhandlung();

            List<int> validStaedteIDs = new List<int>();

            // In Frage kommende Städte für das Gerichtsverfahren ermitteln
            for (int currentStadtID = 1; currentStadtID < SW.Statisch.GetMaxStadtID(); currentStadtID++)
            {
                // Wenn der angeklagte Spieler selber Richter ist, fällt die Stadt, in der er Richter ist, weg
                if (GetHumWithID(GetAktiverSpieler()).GetAmtID() == 5 &&
                    GetHumWithID(GetAktiverSpieler()).GetAmtGebiet() == currentStadtID)
                {
                    continue;
                }

                // Es sind nur die Städte gültig, bei denen der Richterposten besetzt ist
                if (GetStadtwithID(currentStadtID).GetRichter() == 0)
                {
                    continue;
                }

                validStaedteIDs.Add(currentStadtID);
            }

            // Es muss mind. 3 Städte mit Richtern geben, von denen niemand der Angeklagte ist
            if (validStaedteIDs.Count < 3)
            {
                return;  // Es kann keine Klage stattfinden
            }

            int stadtID = validStaedteIDs[SW.Statisch.Rnd.Next(1, validStaedteIDs.Count) - 1];

            // Richter ermitteln
            int richter1 = GetStadtwithID(stadtID).GetRichter();
            validStaedteIDs.Remove(stadtID);

            int randomStadtID = validStaedteIDs[SW.Statisch.Rnd.Next(1, validStaedteIDs.Count) - 1];
            int richter2 = GetStadtwithID(randomStadtID).GetRichter();
            validStaedteIDs.Remove(randomStadtID);

            randomStadtID = validStaedteIDs[SW.Statisch.Rnd.Next(1, validStaedteIDs.Count) - 1];
            int richter3 = GetStadtwithID(randomStadtID).GetRichter();
            validStaedteIDs.Remove(randomStadtID);

            GetGerichtsverhandlungX(klageID).SetAll(richter1, richter2, richter3, stadtID, 0, GetAktiverSpieler(), klaegerID);
        }
        #endregion

        // Private Methoden
        #region GetLeereWahlID
        private int GetLeereWahlID()
                {
                    for (int i = 1; i < SW.Statisch.GetMaxAnzahlWahlen(); i++)
                    {
                        if (Wahlen[i].Waehler1 == 0 &&
                            Wahlen[i].Waehler2 == 0 &&
                            Wahlen[i].Waehler3 == 0 &&
                            Wahlen[i].GetKandidaten()[0] == 0 &&
                            Wahlen[i].GetKandidaten()[1] == 0 &&
                            Wahlen[i].GetKandidaten()[2] == 0 &&
                            Wahlen[i].GetKandidaten()[3] == 0 &&
                            Wahlen[i].GetKandidaten()[4] == 0 &&
                            Wahlen[i].GetKandidaten()[5] == 0 &&
                            Wahlen[i].GetKandidaten()[6] == 0 &&
                            Wahlen[i].GetKandidaten()[7] == 0 &&
                            Wahlen[i].GetKandidaten()[8] == 0 &&
                            Wahlen[i].GetKandidaten()[9] == 0 &&
                            Wahlen[i].GetKandidaten()[10] == 0 &&
                            Wahlen[i].AmtID == 0 &&
                            Wahlen[i].GebietID == 0 &&
                            Wahlen[i].Stufe == 0)
                        {
                            return i;
                        }
                    }

                    BelTextAnzeigen("Diese Meldung sollte nicht erscheinen können. Bitte melde dich im Forum, am besten mit Savegame. Fehlercode 7");
                    return 700;
                }
                #endregion

        #region GetLeereAmtsenthebungsID
        private int GetLeereAmtsenthebungsID()
        {
            for (int i = 1; i < SW.Statisch.GetMaxAnzahlAmtsenthebungen(); i++)
            {
                if (Amtsenthebungen[i].Waehler1 == 0 && Amtsenthebungen[i].Waehler2 == 0 && Amtsenthebungen[i].Waehler3 == 0 && Amtsenthebungen[i].OpferID == 0)
                {
                    return i;
                }
            }
            return 1;
        }
        #endregion

        #region CheckBewerbAmtStufenDifferenz
        private bool CheckBewerbAmtStufenDifferenz(int amtIDKlein, int amtIDGross)
        {
            int amtstgr = SW.Statisch.GetAmtwithID(amtIDGross).GetAmtsStufe();
            int amtstkl = SW.Statisch.GetAmtwithID(amtIDKlein).GetAmtsStufe();

            if (amtstgr - amtstkl == 1 || amtstgr - amtstkl == 2)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region SpielerKlagtXAn
        private void SpielerKlagtXAn(int iAngeklagter)
        {
            int iGebietsstufe = 0;
            int iKlageID = GetEmptyGerichtsverhandlung();
            int iGebietsID;

            // Ort der Verfahrens ermitteln
            if (GetSpWithID(iAngeklagter).GetAmtID() != 0 && GetSpWithID(iAngeklagter).GetAmtID() != 5)
            {
                // Hat der Angeklagte ein Amt und ist er kein Richter, findet der Prozess in seiner Stadt statt
                iGebietsID = GetSpWithID(iAngeklagter).GetAmtGebiet();
            }
            else
            {
                iGebietsID = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());

                // Falls der Angeklagte zufälligerweise in dieser Stadt Richter ist
                if (GetSpWithID(iAngeklagter).GetAmtID() == 5)
                {
                    while (GetSpWithID(iAngeklagter).GetAmtGebiet() == iGebietsID)
                    {
                        iGebietsID = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
                    }
                }
            }

            // Richter ermitteln
            int r1 = GetStadtwithID(iGebietsID).GetRichter();

            if (r1 == 0) //Ersatzrichter finden
            {
                while (r1 == 0)
                {
                    int randstd = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
                    if (GetStadtwithID(randstd).GetRichter() != 0 && GetStadtwithID(randstd).GetRichter() != iAngeklagter)
                    {
                        r1 = GetStadtwithID(randstd).GetRichter();
                    }
                }
            }

            int r2 = 0;
            int loopcounter = 0;

            while (r2 == 0)
            {
                int randstd = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
                if (GetStadtwithID(randstd).GetRichter() != 0 && GetStadtwithID(randstd).GetRichter() != iAngeklagter && GetStadtwithID(randstd).GetRichter() != r1)
                {
                    r2 = GetStadtwithID(randstd).GetRichter();
                }
                loopcounter++;

                if (loopcounter > 1000)
                {
                    BelTextAnzeigen("Fehlercode 19. Bitte melde mir diesen Fehler");
                    break;
                }
            }

            loopcounter = 0;

            int r3 = 0;
            while (r3 == 0)
            {
                int randstd = SW.Statisch.Rnd.Next(1, SW.Statisch.GetMaxStadtID());
                if (GetStadtwithID(randstd).GetRichter() != 0 && GetStadtwithID(randstd).GetRichter() != iAngeklagter && GetStadtwithID(randstd).GetRichter() != r1 && GetStadtwithID(randstd).GetRichter() != r2)
                {
                    r3 = GetStadtwithID(randstd).GetRichter();
                }

                loopcounter++;
                if (loopcounter > 1000)
                {
                    BelTextAnzeigen("Fehlercode 55. Bitte melde mir diesen Fehler dringend");
                    break;
                }
            }

            GetGerichtsverhandlungX(iKlageID).SetAll(r1, r2, r3, iGebietsID, iGebietsstufe, iAngeklagter, GetAktiverSpieler());
        }
        #endregion

        #endregion

        #region Sonstige Hilfsfunktionen

        #region BelTextAnzeigen
        public void BelTextAnzeigen(string text)
        {
            SW.UI.TextAnzeigen.ShowDialog(text);
        }
        #endregion

        private bool GetTrueFalseRandom()
        {
            bool tr = true;

            if (SW.Statisch.Rnd.Next(0, 2) == 1)
                tr = false;

            return tr;
        }

        private int[] ShuffleList(int[] inputList)
        {
            for (int t = 0; t < inputList.Length; t++)
            {
                int tmp = inputList[t];
                int r = SW.Statisch.Rnd.Next(t, inputList.Length);
                inputList[t] = inputList[r];
                inputList[r] = tmp;
            }
            return inputList;
        }

        #endregion

        #endregion

        #region Properties

        private int AktiverSpielerID
        {
            get { return Spielstand.AktiverSpielerID; }
            set { Spielstand.AktiverSpielerID = value; }
        }

        private int AktiveSpielerAnzahl
        {
            get { return Spielstand.AktiveSpielerAnzahl; }
            set { Spielstand.AktiveSpielerAnzahl = value; }
        }

        private int AktuellesJahr
        {
            get { return Spielstand.AktuellesJahr; }
            set { Spielstand.AktuellesJahr = value; }
        }

        private int[] Gesetze
        {
            get { return Spielstand.Gesetze; }
            set { Spielstand.Gesetze = value; }
        }

        private string[] GesetzesTexte
        {
            get { return Spielstand.GesetzesTexte; }
            set { Spielstand.GesetzesTexte = value; }
        }

        private Gerichtsverhandlung[] Gerichtshandlungen
        {
            get { return Spielstand.Gerichtshandlungen; }
            set { Spielstand.Gerichtshandlungen = value; }
        }

        private HumSpieler[] HSpieler
        {
            get { return Spielstand.HSpieler; }
            set { Spielstand.HSpieler = value; }
        }

        private KISpieler[] KSpieler
        {
            get { return Spielstand.KSpieler; }
            set { Spielstand.KSpieler = value; }
        }

        private Stadt[] Staedte
        {
            get { return Spielstand.Staedte; }
            set { Spielstand.Staedte = value; }
        }

        private Land[] Laender
        {
            get { return Spielstand.Laender; }
            set { Spielstand.Laender = value; }
        }

        private Reich[] Reiche
        {
            get { return Spielstand.Reiche; }
            set { Spielstand.Reiche = value; }
        }

        private Stuetzpunkt[] Stuetzpunkte
        {
            get { return Spielstand.Stuetzpunkte; }
            set { Spielstand.Stuetzpunkte = value; }
        }

        private WahlAbhalten[] Wahlen
        {
            get { return Spielstand.Wahlen; }
            set { Spielstand.Wahlen = value; }
        }

        private Amtsenthebung[] Amtsenthebungen
        {
            get { return Spielstand.Amtsenthebungen; }
            set { Spielstand.Amtsenthebungen = value; }
        }

        private Rohstoff[] Rohstoffe
        {
            get { return Spielstand.Rohstoffe; }
            set { Spielstand.Rohstoffe = value; }
        }

        // Public Properties

        public Landsicherheit[] Landsicherheiten
        {
            get { return Spielstand.Landsicherheiten; }
            set { Spielstand.Landsicherheiten = value; }
        }

        public string SpielName
        {
            get { return Spielstand.SpielName; }
            set { Spielstand.SpielName = value; }
        }

        public bool TodesfaelleAnzeigen
        {
            get { return Spielstand.TodesfaelleAnzeigen; }
            set { Spielstand.TodesfaelleAnzeigen = value; }
        }

        public bool Testmodus
        {
            get { return Spielstand.Testmodus; }
            set { Spielstand.Testmodus = value; }
        }

        public bool Cheatmodus
        {
            get { return Spielstand.Cheatmodus; }
            set { Spielstand.Cheatmodus = value; }
        }

        #endregion
    }
}
