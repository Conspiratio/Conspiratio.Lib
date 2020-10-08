using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Ereignisse;
using Conspiratio.Lib.Gameplay.Hinterzimmer;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Schreibstube;
using Conspiratio.Lib.Gameplay.Spielwelt;
using Conspiratio.Lib.Gameplay.Wohnsitz;

namespace Conspiratio.Lib.Gameplay.Personen
{
    [Serializable]
    public class HumSpieler : Spieler
    {
        #region Variablen

        public int WirbtUmSpielerID { get; set; }
        public bool HatAngebotFuerStuetzpunktAbgegeben { get; set; }
        public List<Ereigniszeitpunkt> EreignisseZuletztPassiert { get; set; }

        private int _bannerID;
        private bool _sitztImKerker;
        private int _bekamHandelszertifikatX;
        private int _bekamTitelX;
        private int _permaAnsehen;
        private bool _kindBekommen;  // nur mit Cheats
        private int[] _umsatzInStadt;
        private int _klagtSpielerMitIDXAn;
        private bool _gebeichtet;
        private int[,] _rohstoffeEinVerkaeufeInStadt;
        private int[] _bestechungen = new int[SW.Statisch.GetMaxKIID()];
        private bool _henkersHand;
        private int[,] _hatInStadtXMengeYRohstoffe;
        private int _spieltKartenGegenSpielerID;
        private int _ermordetKISpielerID;
        private int _vergiftetWeinVonKISpielerID;
        private int _erbeSpielerID;
        private bool[] _rohstoffrechte = new bool[SW.Statisch.GetMaxRohID()];  // Welche Handelsrechte der Spieler von welchem Rohstoff schon besitzt
        private int[] _karawaneInStadt = new int[SW.Statisch.GetMaxStadtID()];
        private int[] _begingVerbrechenX;
        private bool _privilegKaufmannBenutzt;

        private SpielerStatistik _spielerStatistik;
        private Produktionsslot[,] _produktionsslotsInStadtX;
        private AktiveSabotagen[] _aktiveSabotagen = new AktiveSabotagen[SW.Statisch.GetMaxKIID()];
        private AktiveSpionagen[] _aktiveSpionagen = new AktiveSpionagen[SW.Statisch.GetMaxKIID()];
        private Kredit[] _aktiveKredite = new Kredit[SW.Statisch.GetMaxKredite()+1];
        private Kind[] _kinder = new Kind[SW.Statisch.GetMaxKinderAnzahl()];
        private SpHatHaus[] _spielerHatHausVonStadtAnArraystelle = new SpHatHaus[SW.Statisch.GetMaxStadtID()];
        private SpHatWerkstaetten[,] _spielerHatInStadtXWerkstaettenY = new SpHatWerkstaetten[SW.Statisch.GetMaxStadtID(), SW.Statisch.GetMaxWerkstaettenProStadt()];  // Das sind jene Werkstätten, die der Spieler besitzt

        #endregion

        #region Konstruktor
        public HumSpieler(int taler, string name, bool maennlich, int verheiratetMitSpielerID, int verbleibendeJahre): base(taler, name, maennlich, verheiratetMitSpielerID, verbleibendeJahre)
        {
            this.Name = name;
            this.Taler = taler;
            this.Maennlich = maennlich;
            this.Alter = SW.Statisch.GetStartAlter();
            this.VerheiratetMit = verheiratetMitSpielerID;

            HatAngebotFuerStuetzpunktAbgegeben = false;

            _erbeSpielerID = 0;
            _bannerID = 0;
            _spielerStatistik = new SpielerStatistik();
            _begingVerbrechenX = new int[SW.Statisch.GetMaxGesetze()];
            _hatInStadtXMengeYRohstoffe = new int[SW.Statisch.GetMaxStadtID(), SW.Statisch.GetMaxRohID()];
            _umsatzInStadt = new int[SW.Statisch.GetMaxStadtID()];  // Umsatz pro Stadt
            _rohstoffeEinVerkaeufeInStadt = new int[SW.Statisch.GetMaxStadtID(), SW.Statisch.GetMaxRohID()];
            EreignisseZuletztPassiert = new List<Ereigniszeitpunkt>();

            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                _spielerHatHausVonStadtAnArraystelle[i] = new SpHatHaus(0, 0, 100, false, null);

                for (int j = 1; j < SW.Statisch.GetMaxRohID(); j++)
                    _hatInStadtXMengeYRohstoffe[i, j] = 0;
            }

            _produktionsslotsInStadtX = new Produktionsslot[15, 2];
            for (int i = SW.Statisch.GetMinStadtID(); i < SW.Statisch.GetMaxStadtID(); i++)
            {
                for (int j = 0; j < SW.Statisch.GetMaxProdSlots(); j++)
                    _produktionsslotsInStadtX[i, j] = new Produktionsslot(0, SW.Dynamisch.GetStadtwithID(i).GetSingleRohstoff(1), 0, 0, SW.Dynamisch.GetStadtwithID(i).GetSingleRohstoff(1), 0, 1);
            }

            // Sabotagen und Spionagen anlegen
            for (int i = 0; i < SW.Statisch.GetMaxKIID(); i++)
            {
                _aktiveSabotagen[i] = new AktiveSabotagen(0, 0);
                _aktiveSpionagen[i] = new AktiveSpionagen(0);
            }

            // Kredite anlegen
            for (int i = 0; i <= SW.Statisch.GetMaxKredite(); i++)
                _aktiveKredite[i] = new Kredit(0, 0, 0);

            // Kinder anlegen
            for (int i = 1; i < SW.Statisch.GetMaxKinderAnzahl(); i++)
                _kinder[i] = new Kind(false, "", 0); 

            // Werkstätten anlegen
            for (int i = 0; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                for (int j = 0; j < SW.Statisch.GetMaxWerkstaettenProStadt();  j++)
                    _spielerHatInStadtXWerkstaettenY[i, j] = new SpHatWerkstaetten();
            }
        }
        #endregion

        #region Rohstoffe
        public int GetStadtRohstoffAnzahl(int stadtID, int rohID)
        {
            return _hatInStadtXMengeYRohstoffe[stadtID, rohID];
        }

        public int SetStadtRohstoffAnzahl(int stadtID, int rohID, int anzahl)
        {
            int belegterLagerplatz = SW.Dynamisch.GetRohstoffwithID(rohID).ErmittleBenoetigtenLagerplatz(_hatInStadtXMengeYRohstoffe[stadtID, rohID]);
            int verfuegbarerLagerplatz = ErmittleLagerplatzInStadt(stadtID, rohID);
            int benoetigterLagerplatz = SW.Dynamisch.GetRohstoffwithID(rohID).ErmittleBenoetigtenLagerplatz(anzahl);
            int anzahlMoeglich;

            if (benoetigterLagerplatz > verfuegbarerLagerplatz)  // Kein ausreichender Lagerplatz?
            {
                if ((verfuegbarerLagerplatz - belegterLagerplatz) > 0)  // ist für eine Teilmenge Platz?
                {
                    anzahlMoeglich = (verfuegbarerLagerplatz - belegterLagerplatz) * SW.Dynamisch.GetRohstoffwithID(rohID).GetLagermengeProQMeter();

                    if ((anzahl - anzahlMoeglich) <= 4)   // Toleranz von bis zu 4 Einheiten, diese passen immer
                        anzahlMoeglich = anzahl;

                    anzahl = GetStadtRohstoffAnzahl(stadtID, rohID) + anzahlMoeglich;
                }
                else
                    anzahlMoeglich = 0;  // Lager voll
            }
            else
                anzahlMoeglich = anzahl;  // ausreichend Platz vorhanden

            if ((anzahlMoeglich > 0) || (anzahl == 0))
                _hatInStadtXMengeYRohstoffe[stadtID, rohID] = anzahl;

            return anzahlMoeglich;
        }

        public int VeraenderStadtRohstoffAnzahl(int stadtID, int rohID, int anzahl)
        {
            return SetStadtRohstoffAnzahl(stadtID, rohID, GetStadtRohstoffAnzahl(stadtID, rohID) + anzahl);
        }
        #endregion

        #region ErmittleLagerplatzInStadt
        public int ErmittleLagerplatzInStadt(int stadtID, int rohID)
        {
            int lagerplatz = 0;

            for (int i = 0; i < SW.Statisch.GetMaxWerkstaettenProStadt(); i++)
            {
                if ((_spielerHatInStadtXWerkstaettenY[stadtID, i].GetEnabled()) && ((_spielerHatInStadtXWerkstaettenY[stadtID, i].GetRohstoffID() == rohID) || SW.Dynamisch.GetStadtwithID(stadtID).GetSingleRohstoff(i + 1) == rohID))
                {
                    lagerplatz += _spielerHatInStadtXWerkstaettenY[stadtID, i].GetSKillX(1);
                    break;
                }
            }

            return lagerplatz;
        }
        #endregion

        #region Produktionsslot
        public Produktionsslot GetProduktionsslot(int stadtID, int nr0Oder1)
        {
            return _produktionsslotsInStadtX[stadtID, nr0Oder1];
        }

        public void SetProduktionsslot(int stadtID, int nr1Oder2, int arbeiter, int prodstaetten, int rohNr, int taetigkeit)
        {
            _produktionsslotsInStadtX[stadtID, nr1Oder2].SetProduktionArbeiter(arbeiter);
            _produktionsslotsInStadtX[stadtID, nr1Oder2].SetProduktionStaetten(prodstaetten);
            _produktionsslotsInStadtX[stadtID, nr1Oder2].SetProduktionRohstoff(rohNr);
            _produktionsslotsInStadtX[stadtID, nr1Oder2].SetTaetigkeit(taetigkeit);
        }
        #endregion

        #region Sabotage
        public AktiveSabotagen GetAktiveSabotage(int sabotageID)
        {
            return _aktiveSabotagen[sabotageID];
        }

        public void AktiveSabotageEntfernen(int sabotageID)
        {
            _aktiveSabotagen[sabotageID].SetDauer(0);
            _aktiveSabotagen[sabotageID].SetKosten(0);
        }
        #endregion

        #region Spionage
        public AktiveSpionagen GetAktiveSpionage(int spionageID)
        {
            return _aktiveSpionagen[spionageID];
        }

        public void AktiveSpionageEntfernen(int spionageID)
        {
            _aktiveSpionagen[spionageID].SetKosten(0);
            _aktiveSpionagen[spionageID].SetDauer(0);
        }
        #endregion

        #region Karten spielen
        public void SetSpieltKartenGegenSpielerID(int spielerID)
        {
            _spieltKartenGegenSpielerID = spielerID;
        }

        public int GetSpieltKartenGegenSpielerID()
        {
            return _spieltKartenGegenSpielerID;
        }
        #endregion

        #region Kredite
        public Kredit GetKreditMitID(int kreditID)
        {
            return _aktiveKredite[kreditID];
        }

        public int GetEmptyKreditID()
        {
            for (int i = 0; i < SW.Statisch.GetMaxKredite(); i++)
            {
                if (_aktiveKredite[i].GetDauer() == 0)
                    return i;
            }

            return 5;
        }
        #endregion

        #region Getter und Setter

        public int GetBestechungVonSpielerMitIDX(int X)
        {
            return _bestechungen[X];
        }

        public int[] GetBestechungVonAllen()
        {
            return _bestechungen;
        }

        public void SetBestechungVonSpielerMitIDXAufY(int X, int Y)
        {
            _bestechungen[X] = Y;
        }

        public void ErhoeheBestechungVonSpielerMitIDXUmY(int X, int Y)
        {
            _bestechungen[X] += Y;
        }

        public int GetEinVerkaeufeInStadtXVonRohstoffIDY(int X, int Y)
        {
            return _rohstoffeEinVerkaeufeInStadt[X, Y];
        }

        public void SetEinVerkaeufeInStadtXVonRohstoffIDYAufZ(int X, int Y, int Z)
        {
            _rohstoffeEinVerkaeufeInStadt[X, Y] = Z;
        }

        public void ErhoeheEinVerkaeufeInStadtXVonRohstoffIDYUmZ(int X, int Y, int Z)
        {
            _rohstoffeEinVerkaeufeInStadt[X, Y] += Z;
        }

        public SpielerStatistik GetSpielerStatistik()
        {
            return _spielerStatistik;
        }

        public bool GetHenkersHand()
        {
            return _henkersHand;
        }

        public override int GetGesamtVermoegen(int spielerID)
        {
            int gesamtVermoegen = 0;
            double factor = 0.7;

            gesamtVermoegen += Taler;  // Bargeld

            // Wohnsitze, Rohstoffe, Werkstätten
            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                // Wohnsitze
                gesamtVermoegen += _spielerHatHausVonStadtAnArraystelle[i].GetAktuellerWert();

                // Werkstätten und Rohstoffe
                for (int j = 1; j < SW.Statisch.GetMaxWerkstaettenProStadt(); j++)
                {
                    // Werkstätten
                    if (_spielerHatInStadtXWerkstaettenY[i, j].GetEnabled() == true)
                    {
                        gesamtVermoegen += Convert.ToInt32(SW.Dynamisch.GetRohstoffwithID(SW.Dynamisch.GetStadtwithID(i).GetRohstoffe()[j]).GetWSKaufpreis() * factor);
                    }

                    // Rohstoffe
                    int rohid = SW.Dynamisch.GetStadtwithID(i).GetRohstoffe()[j];
                    gesamtVermoegen += SW.Dynamisch.GetRohstoffwithID(rohid).GetPreisStd() * _hatInStadtXMengeYRohstoffe[i, j];
                }
            }

            // Stützpunkte
            for (int i = 0; i < SW.Dynamisch.GetStuetzpunkte().Length; i++)
            {
                if (SW.Dynamisch.GetStuetzpunkte()[i].Besitzer == spielerID)
                    gesamtVermoegen += SW.Dynamisch.GetStuetzpunkte()[i].BerechneWert();
            }

            if (gesamtVermoegen < 0)
                gesamtVermoegen = 0;

            return gesamtVermoegen;
        }

        public int GetAnzahlHaeuser()
        {
            int counter = 0;

            for (int i = SW.Statisch.GetMinStadtID(); i < SW.Statisch.GetMaxStadtID(); i++)
            {
                if (GetSpielerHatHausVonStadtAnArraystelle(i).GetHausID() != 0)
                    counter++;
            }

            return counter;
        }

        public void SetHenkersHand(bool x)
        {
            _henkersHand = x;
        }

        public bool GetGebeichtet()
        {
            return _gebeichtet;
        }

        public void SetGebeichtet(bool x)
        {
            _gebeichtet = x;
        }

        public bool GetPrivilegKaufmannBenutzt()
        {
            return _privilegKaufmannBenutzt;
        }

        public void SetPrivilegKaufmannBenutzt(bool x)
        {
            _privilegKaufmannBenutzt = x;
        }

        public void SetKlagtSpielerMitIDXAn(int X)
        {
            _klagtSpielerMitIDXAn = X;
        }

        public int GetKlagtSpielerMitIDXAn()
        {
            return _klagtSpielerMitIDXAn;
        }

        public bool GetKindBekommen()
        {
            return _kindBekommen;
        }

        public int GetUmsatzInStadtX(int X)
        {
            return _umsatzInStadt[X];
        }

        public void SetUmsatzInStadtX(int value, int X)
        {
            _umsatzInStadt[X] = value;
        }

        public void ErhoeheUmsatzInStadtX(int value, int X)
        {
            _umsatzInStadt[X] += value;
        }

        public void SetKindBekommen(bool value)
        {
            _kindBekommen = value;
        }

        public int GetEmptyKindSlot()
        {
            for (int i = SW.Statisch.GetMinKindSlotNr(); i < SW.Statisch.GetMaxKinderAnzahl(); i++)
            {
                if (_kinder[i].GetKindName() == "")
                    return i;
            }

            return SW.Statisch.GetMaxKinderAnzahl();
        }

        public void SetKindX(int x, bool maennlich, string name, int alter = 0)
        {
            _kinder[x].SetName(name);
            _kinder[x].SetMaennlich(maennlich);
            _kinder[x].SetAlter(alter);
            _kinder[x].Geburtsjahr = SW.Dynamisch.GetAktuellesJahr() - alter;
        }

        public void KinderAltern()
        {
            for (int i = 1; i < SW.Statisch.GetMaxKinderAnzahl(); i++)
            {
                if (_kinder[i].GetKindName() != "")
                    _kinder[i].AlterPlusEins();
            }
        }

        public Kind GetKindX(int x)
        {
            return _kinder[x];
        }

        public int GetErbeSpielerID()
        {
            return _erbeSpielerID;
        }

        public void SetErbeSpielerID(int x)
        {
            _erbeSpielerID = x;
        }

        public SpHatHaus GetSpielerHatHausVonStadtAnArraystelle(int x)
        {
            return _spielerHatHausVonStadtAnArraystelle[x];
        }

        public SpHatWerkstaetten GetSpielerHatInStadtXWerkstaettenY(int werkstaettenNr, int stadtID)
        {
            return _spielerHatInStadtXWerkstaettenY[stadtID, werkstaettenNr - 1];
        }

        public int GetBanner()
        {
            return _bannerID;
        }

        public void SetBanner(int x)
        {
            _bannerID = x;
        }

        public int GetKarawaneInStadtX(int x)
        {
            return _karawaneInStadt[x];
        }

        public void SetKarawaneInStadtXzuY(int x, int y)
        {
            _karawaneInStadt[x] = y;
        }

        public bool GetRohstoffrechteX(int x)
        {
            return _rohstoffrechte[x];
        }

        public void SetRohstoffrechteXZuY(int x, bool y)
        {
            _rohstoffrechte[x] = y;
        }

        public void SetBekamHandelszertifikatX(int rohstoff)
        {
            _bekamHandelszertifikatX = rohstoff;
        }

        public int GetBekamHandeslzertifikatX()
        {
            return _bekamHandelszertifikatX;
        }

        public void SetBekamTitelX(int titel)
        {
            _bekamTitelX = titel;
        }

        public int GetBekamTitelX()
        {
            return _bekamTitelX;
        }

        public int GetErmordetKISpielerID()
        {
            return _ermordetKISpielerID;
        }

        public void SetErmordetKISpielerID(int spielerID)
        {
            _ermordetKISpielerID = spielerID;
        }

        public int GetVergiftetWeinVonKISpielerID()
        {
            return _vergiftetWeinVonKISpielerID;
        }

        public void SetVergiftetWeinVonKISpielerID(int id)
        {
            _vergiftetWeinVonKISpielerID = id;
        }

        public int GetPermaAnsehen()
        {
            return _permaAnsehen;
        }

        public void ErhoehePermaAnsehen(int wert)
        {
            _permaAnsehen += wert;
        }

        public void SetPermaAnsehen(int wert)
        {
            _permaAnsehen = wert;
        }

        public void SetSitztImKerker(bool value)
        {
            _sitztImKerker = value;
        }

        public bool GetSitztImKerker()
        {
            return _sitztImKerker;
        }

        public int GetBegingVerbrechenX(int x)
        {
            return _begingVerbrechenX[x];
        }

        public void SetBegingVerbrechenX(int x, int y)
        {
            _begingVerbrechenX[x] = y;
        }

        public void ErhoeheGesetzXUmEins(int x)
        {
            _begingVerbrechenX[x]++;
        }

        public void HalbiereDelikte()
        {
            for (int i = 0; i < SW.Statisch.GetMaxGesetze(); i++)
                _begingVerbrechenX[i] /= 2;
        }

        public int[] GetBegingVerbrechenX()
        {
            return _begingVerbrechenX;
        }
        #endregion

        #region GetFirstStadtIDMitWohnsitz
        public int GetFirstStadtIDMitWohnsitz()
        {
            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                if (SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(i).GetHausID() != 0)
                    return i;
            }

            return 0;  // Kein Wohnsitz vorhanden
        }
        #endregion

        #region GetNextStadtIDMitWohnsitz
        public int GetNextStadtIDMitWohnsitz(int aktuelleStadtID)
        {
            int firstStadtIDMitWohnsitz = 0;

            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                if (SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(i).GetHausID() != 0)
                {
                    if (firstStadtIDMitWohnsitz == 0)
                        firstStadtIDMitWohnsitz = i;

                    if (i > aktuelleStadtID)
                        return i;
                }
            }

            return firstStadtIDMitWohnsitz;
        }
        #endregion

        #region VersuchHandelszertifikatVerleihen
        public void VersuchHandelszertifikatVerleihen()
        {
            // Falls nicht schon eines diese Runde verliehen wird
            if (GetBekamHandeslzertifikatX() == 0)
            {
                int handzert = 0;

                // Handelszertifikate zählen
                for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
                {
                    if (GetRohstoffrechteX(i))
                    {
                        handzert++;
                    }
                }

                // Falls er bereits 2 Rohstoffe besaß
                if (handzert >= 2)
                {
                    if (GetTaler() >= 1000000)
                    {
                        HandelszertifikatVerleihen(5, 15, SW.Statisch.GetMaxRohID());
                    }
                    else if (GetTaler() >= 500000)
                    {
                        HandelszertifikatVerleihen(4, 8, 19);
                    }
                    else if (GetTaler() >= 100000)
                    {
                        HandelszertifikatVerleihen(3, 8, 15);
                    }
                }
            }
        }
        #endregion

        #region HandelszertifikatVerleihen
        public void HandelszertifikatVerleihen(int anzahlRohstoffrechte, int minRohID, int maxRohID)
        {
            int aktuelleAnzahlRohstoffrechte = 0;
            // Rohstoffrecht verleihen
            for (int i = 1; i < SW.Statisch.GetMaxRohID(); i++)
            {
                if (GetRohstoffrechteX(i))
                {
                    aktuelleAnzahlRohstoffrechte++;
                }
            }

            // Wenn der Spieler weniger als x Rechte besitzt, bekommt er ein neues verliehen
            if (aktuelleAnzahlRohstoffrechte < anzahlRohstoffrechte)
            {
                int neuesRecht = SW.Statisch.Rnd.Next(minRohID, maxRohID);

                // Solange der Spieler das neue Recht aber schon hat, soll ein anderes gewählt werden
                while (GetRohstoffrechteX(neuesRecht) == true)
                {
                    neuesRecht = SW.Statisch.Rnd.Next(minRohID, maxRohID);
                }

                SetRohstoffrechteXZuY(neuesRecht, true);
                SetBekamHandelszertifikatX(neuesRecht);
            }
        }
        #endregion

        #region AnsehenAktualisieren
        public void AnsehenAktualisieren()
        {
            int perm_ans = GetPermaAnsehen();
            int ans_plus = 0;

            // Geldansehen
            ans_plus += Convert.ToInt32(GetTaler() / SW.Statisch.GetAnsehenProTaler());

            // Amtansehen
            int amt_id = GetAmtID();
            if (amt_id != 0)
            {
                ans_plus += SW.Statisch.GetAmtwithID(amt_id).GetBonusAnsehen();
            }

            // Häuser Ansehen
            for (int i = 1; i < SW.Statisch.GetMaxStadtID(); i++)
            {
                if (GetSpielerHatHausVonStadtAnArraystelle(i).GetHausID() != 0 &&
                    GetSpielerHatHausVonStadtAnArraystelle(i).GetStadtID() != 0)   // Haus vorhanden?
                {
                    ans_plus += GetSpielerHatHausVonStadtAnArraystelle(i).GetAnsehensbonus();

                    // Gesundheit berücksichtigen
                    ErhoeheGesundheit(GetSpielerHatHausVonStadtAnArraystelle(i).GetGesundheitsbonus());
                }
            }

            SetAnsehen(perm_ans + ans_plus);
        }
        #endregion

        #region DarfWaisenkindAdoptieren
        public bool DarfWaisenkindAdoptieren()
        {
            // Hat der Spieler noch kein eigenes Kind?
            for (int j = 1; j < SW.Statisch.GetMaxKinderAnzahl(); j++)
            {
                if (!string.IsNullOrEmpty(GetKindX(j).GetKindName()))
                    return false;
            }
            
            return true;
        }
        #endregion

        #region WaisenkindAdoptieren
        public void WaisenkindAdoptieren(int preis)
        {
            if (!SW.Dynamisch.CheckIfenoughGold(preis))
                return;

            int random = SW.Statisch.Rnd.Next(0, 2);
            bool maennlich = random == 0;

            if (maennlich)
                random = SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaennerFrauenGrenze());
            else
                random = SW.Statisch.Rnd.Next(SW.Statisch.GetMaennerFrauenGrenze(), SW.Statisch.GetMaxKIID());

            string name = SW.Statisch.GetKINameX(random);

            SetKindX(SW.Dynamisch.GetAktHum().GetEmptyKindSlot(), maennlich, name, 1);

            SW.Dynamisch.BelTextAnzeigen($"Dank Eurer großzügigen Spende \nkonntet Ihr das Kind {name} \naus dem Waisenhaus adoptieren.");
        }
        #endregion

        #region ErmittlePreisWaisenkindAdoptieren
        public int ErmittlePreisWaisenkindAdoptieren(int spielerID)
        {
            int zufallswert = SW.Statisch.Rnd.Next(15, 25);
            int gesamtvermoegen = GetGesamtVermoegen(spielerID);

            if (gesamtvermoegen <= 0)
                gesamtvermoegen = SW.Statisch.GetStartgold();  // falls kein Vermögen vorhanden ist (oder Schulden) wird vom Startkapital ausgegangen

            return Convert.ToInt32((zufallswert * gesamtvermoegen) / 100);
        }
        #endregion
    }
}
