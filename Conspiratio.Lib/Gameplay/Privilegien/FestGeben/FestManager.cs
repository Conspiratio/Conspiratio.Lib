using System;
using System.Linq;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Privilegien.FestGeben
{
    public class FestManager
    {
        private EnumFestGroesse _startGroesse = EnumFestGroesse.kleines;
        private EnumFestMusiker _startMusiker = EnumFestMusiker.schlechten;

        public int StadtID { get; set; }
        public EnumFestGroesse Groesse { get; set; }
        public EnumFestMusiker Musiker { get; set; }
        public int Jahr { get; set; }

        public FestManager()
        {
            StadtID = SW.Dynamisch.GetAktHum().GetFirstStadtIDMitWohnsitz();
            Groesse = _startGroesse;
            Musiker = _startMusiker;
            Jahr = SW.Dynamisch.GetAktuellesJahr() + 1;
        }

        public string GetStadtName(int stadtID = 0)
        {
            if (stadtID == 0)
                stadtID = StadtID;

            return SW.Dynamisch.GetStadtwithID(stadtID).GetGebietsName();
        }

        public int GetMaxJahr()
        {
            return Jahr + 9;
        }

        public void SetNextStadtID()
        {
            StadtID = SW.Dynamisch.GetAktHum().GetNextStadtIDMitWohnsitz(StadtID);
        }

        public void SetNextGroesse()
        {
            Array values = Enum.GetValues(typeof(EnumFestGroesse));

            if ((int)Groesse + 1 >= values.Length)
                Groesse = _startGroesse;
            else
                Groesse++;
        }

        public void SetNextMusiker()
        {
            Array values = Enum.GetValues(typeof(EnumFestMusiker));

            if ((int)Musiker + 1 >= values.Length)
                Musiker = _startMusiker;
            else
                Musiker++;
        }

        public string ErstelleNeuesFest(int stadtID, EnumFestGroesse groesse, EnumFestMusiker musiker, int jahr, int spielerID = 0)
        {
            if (spielerID == 0)
                spielerID = SW.Dynamisch.GetAktiverSpieler();

            if ((stadtID <= 0) || (stadtID > SW.Statisch.GetMaxStadtID()))
                throw new ArgumentOutOfRangeException(nameof(stadtID));

            if ((jahr <= SW.Dynamisch.GetAktuellesJahr()) || (jahr > GetMaxJahr()))
                throw new ArgumentOutOfRangeException(nameof(jahr));

            if (SW.Dynamisch.Spielstand.Feste.Where(x => x.SpielerID == spielerID && x.Jahr == jahr).Any())
                throw new Exception("In diesem Jahr habt Ihr bereits ein Fest geplant!");

            double faktorGroesse;

            switch (groesse)
            {
                case EnumFestGroesse.kleines:
                    faktorGroesse = 1d;
                    break;
                case EnumFestGroesse.normales:
                    faktorGroesse = 3d;
                    break;
                case EnumFestGroesse.großes:
                    faktorGroesse = 5d;
                    break;
                case EnumFestGroesse.riesiges:
                    faktorGroesse = 10d;
                    break;
                default:
                    throw new NotSupportedException(groesse.ToString() + " wird nicht unterstützt.");
            }

            double faktorMusiker;

            switch (musiker)
            {
                case EnumFestMusiker.schlechten:
                    faktorMusiker = 0.4d;
                    break;
                case EnumFestMusiker.mittelmäßigen:
                    faktorMusiker = 0.8d;
                    break;
                case EnumFestMusiker.guten:
                    faktorMusiker = 1.2d;
                    break;
                default:
                    throw new NotSupportedException(musiker.ToString() + " wird nicht unterstützt.");
            }

            double faktorVergangeneSpielzeit = 1d;

            if (((SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr) > 10) && ((SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr) <= 40))
            {
                faktorVergangeneSpielzeit = (Convert.ToDouble(SW.Dynamisch.GetAktuellesJahr()) - Convert.ToDouble(SW.Statisch.StartJahr)) / 10d;
            }
            else if ((SW.Dynamisch.GetAktuellesJahr() - SW.Statisch.StartJahr) > 40)
            {
                faktorVergangeneSpielzeit = (Convert.ToDouble(SW.Dynamisch.GetAktuellesJahr()) - Convert.ToDouble(SW.Statisch.StartJahr)) / 5d;
            }

            int anteilGesamtvermoegen = SW.Dynamisch.GetAktHum().GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler()) / 200;  // 0,5 % des Gesamtvermögens

            int grundpreis = 800;
            int geplanteKosten = Convert.ToInt32((grundpreis * faktorGroesse + (grundpreis * faktorMusiker)) * faktorVergangeneSpielzeit) + anteilGesamtvermoegen;

            if (geplanteKosten > 500000)  // Kosten deckeln bei sehr weit fortgeschrittenen Spielen
                geplanteKosten = 500000 + anteilGesamtvermoegen;

            Fest fest = new Fest(spielerID, stadtID, groesse, musiker, jahr, geplanteKosten);
            SW.Dynamisch.Spielstand.Feste.Add(fest);

            string message = $"Denkt daran, dass Eure Niederlassung in {GetStadtName(stadtID)} im Jahr {jahr} mit kulinarischen Köstlichkeiten gut gefüllt sein sollte ...{Environment.NewLine}" +
                             $"Die Musiker werden Euch etwa {geplanteKosten.ToStringGeld()} kosten.";

            return message;
        }

        /// <summary>
        /// TODO: Diese Methode ist aus Zeitgründen noch sehr quick & dirty. Sollte überarbeitet (entzerrt und ausgelagert) werden.
        /// </summary>
        /// <param name="fest"></param>
        /// <returns></returns>
        public string FestFeiern(Fest fest)
        {
            if (SW.Dynamisch.GetAktuellesJahr() != fest.Jahr)
                throw new ArgumentOutOfRangeException(nameof(fest), "Das Jahr des Festes ist nicht das aktuelle Jahr.");

            int lagerstandObst = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 3);
            int lagerstandBier = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 4);
            int lagerstandFisch = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 6);
            int lagerstandWein = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 9);
            int lagerstandRind = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 10);
            int lagerstandRum = SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 12);

            int verbrauchProWareKlein = 100;
            int verbrauchProWareNormal = 200;
            int verbrauchProWareGross = 300;
            int verbrauchProWareRiesig = 400;

            int verbrauchObst = 0;
            int verbrauchBier = 0;
            int verbrauchFisch = 0;
            int verbrauchWein = 0;
            int verbrauchRind = 0;
            int verbrauchRum = 0;

            int verbrauchSollObst = 0;
            int verbrauchSollBier = 0;
            int verbrauchSollFisch = 0;
            int verbrauchSollWein = 0;
            int verbrauchSollRind = 0;
            int verbrauchSollRum = 0;

            int anzahlBenoetigteWaren;
            double permaAnsehenFaktor;

            #region Anzahl der benötigten Ware und der Verbräuche ermitteln

            switch (fest.Groesse)
            {
                case EnumFestGroesse.kleines:
                    {
                        // Bier oder Fisch muss vorhanden sein
                        anzahlBenoetigteWaren = 1;
                        permaAnsehenFaktor = 1d;

                        if ((lagerstandBier > 0) && (lagerstandBier > lagerstandFisch))
                        {
                            verbrauchSollBier = verbrauchProWareKlein;

                            if (lagerstandBier >= verbrauchSollBier)
                                verbrauchBier = verbrauchSollBier;
                            else
                                verbrauchBier = lagerstandBier;
                        }

                        if ((verbrauchBier == verbrauchSollBier) && (verbrauchBier != 0))
                            break;

                        if (lagerstandFisch > 0)
                        {
                            verbrauchSollFisch = verbrauchProWareKlein;

                            if (lagerstandFisch >= verbrauchSollFisch)
                                verbrauchFisch = verbrauchSollFisch;
                            else
                                verbrauchFisch = lagerstandFisch;
                        }
                        else
                            verbrauchSollBier = verbrauchProWareKlein;

                        break;
                    }
                case EnumFestGroesse.normales:
                    {
                        // Bier und Fisch oder Obst muss vorhanden sein
                        anzahlBenoetigteWaren = 2;
                        permaAnsehenFaktor = 1.2d;

                        verbrauchSollBier = verbrauchProWareNormal;

                        if (lagerstandBier > 0)
                        {
                            if (lagerstandBier >= verbrauchSollBier)
                                verbrauchBier = verbrauchSollBier;
                            else
                                verbrauchBier = lagerstandBier;
                        }

                        if ((lagerstandFisch > 0) && (lagerstandFisch > lagerstandObst))
                        {
                            verbrauchSollFisch = verbrauchProWareNormal;

                            if (lagerstandFisch >= verbrauchSollFisch)
                                verbrauchFisch = verbrauchSollFisch;
                            else
                                verbrauchFisch = lagerstandFisch;
                        }

                        if ((verbrauchFisch == verbrauchSollFisch) && (verbrauchFisch != 0))
                            break;

                        if (lagerstandObst > 0)
                        {
                            verbrauchSollObst = verbrauchProWareNormal;

                            if (lagerstandObst >= verbrauchSollObst)
                                verbrauchObst = verbrauchSollObst;
                            else
                                verbrauchObst = lagerstandObst;
                        }
                        else
                            verbrauchSollFisch = verbrauchProWareNormal;

                        break;
                    }
                case EnumFestGroesse.großes:
                    {
                        // Bier, Fisch und Obst oder Wein muss vorhanden sein
                        anzahlBenoetigteWaren = 3;
                        permaAnsehenFaktor = 1.6d;

                        verbrauchSollBier = verbrauchProWareGross;
                        verbrauchSollFisch = verbrauchProWareGross;

                        if (lagerstandBier > 0)
                        {
                            if (lagerstandBier >= verbrauchSollBier)
                                verbrauchBier = verbrauchSollBier;
                            else
                                verbrauchBier = lagerstandBier;
                        }

                        if (lagerstandFisch > 0)
                        {
                            if (lagerstandFisch >= verbrauchSollFisch)
                                verbrauchFisch = verbrauchSollFisch;
                            else
                                verbrauchFisch = lagerstandFisch;
                        }

                        if ((lagerstandObst > 0) && (lagerstandObst > lagerstandWein))
                        {
                            verbrauchSollObst = verbrauchProWareGross;

                            if (lagerstandObst >= verbrauchSollObst)
                                verbrauchObst = verbrauchSollObst;
                            else
                                verbrauchObst = lagerstandObst;
                        }

                        if ((verbrauchObst == verbrauchSollObst) && (verbrauchObst != 0))
                            break;

                        if (lagerstandWein > 0)
                        {
                            verbrauchSollWein = verbrauchProWareGross;

                            if (lagerstandWein >= verbrauchSollWein)
                                verbrauchWein = verbrauchSollWein;
                            else
                                verbrauchWein = lagerstandWein;
                        }
                        else
                            verbrauchSollObst = verbrauchProWareGross;

                        break;
                    }
                case EnumFestGroesse.riesiges:
                    {
                        // Bier, Fisch, Obst, Wein und Rum oder Rind muss vorhanden sein
                        anzahlBenoetigteWaren = 5;
                        permaAnsehenFaktor = 2d;

                        verbrauchSollBier = verbrauchProWareRiesig;
                        verbrauchSollFisch = verbrauchProWareRiesig;
                        verbrauchSollObst = verbrauchProWareRiesig;
                        verbrauchSollWein = verbrauchProWareRiesig;

                        if (lagerstandBier > 0)
                        {
                            if (lagerstandBier >= verbrauchSollBier)
                                verbrauchBier = verbrauchSollBier;
                            else
                                verbrauchBier = lagerstandBier;
                        }

                        if (lagerstandFisch > 0)
                        {
                            if (lagerstandFisch >= verbrauchSollFisch)
                                verbrauchFisch = verbrauchSollFisch;
                            else
                                verbrauchFisch = lagerstandFisch;
                        }

                        if (lagerstandObst > 0)
                        {
                            if (lagerstandObst >= verbrauchSollObst)
                                verbrauchObst = verbrauchSollObst;
                            else
                                verbrauchObst = lagerstandObst;
                        }

                        if (lagerstandWein > 0)
                        {
                            if (lagerstandWein >= verbrauchSollWein)
                                verbrauchWein = verbrauchSollWein;
                            else
                                verbrauchWein = lagerstandWein;
                        }

                        if ((lagerstandRum > 0) && (lagerstandRum > lagerstandRind))
                        {
                            verbrauchSollRum = verbrauchProWareRiesig;

                            if (lagerstandRum >= verbrauchSollRum)
                                verbrauchRum = verbrauchSollRum;
                            else
                                verbrauchRum = lagerstandRum;
                        }

                        if ((verbrauchRum == verbrauchSollRum) && (verbrauchRum != 0))
                            break;

                        if (lagerstandRind > 0)
                        {
                            verbrauchSollRind = verbrauchProWareRiesig;

                            if (lagerstandRind >= verbrauchSollRind)
                                verbrauchRind = verbrauchSollRind;
                            else
                                verbrauchRind = lagerstandRind;
                        }
                        else
                            verbrauchSollRum = verbrauchProWareRiesig;

                        break;
                    }
                default:
                    throw new NotSupportedException(fest.Groesse.ToString() + " wird nicht unterstützt.");
            }

            #endregion

            int erfolgWarenInProzent = 0;
            string messageWareFehlt = "";

            #region  Waren verbrauchen und Erfolgschance ermitteln

            if (verbrauchSollBier > 0)
            {
                if (verbrauchBier == verbrauchSollBier)  // Ware vollständig vorhanden?
                {
                    if (anzahlBenoetigteWaren == 1)
                        erfolgWarenInProzent = 100;
                    else
                        erfolgWarenInProzent = 100 / anzahlBenoetigteWaren;
                }
                else if (verbrauchBier >= (Convert.ToDouble(verbrauchSollBier) * 0.3d))  // Ware zu mind. 30 % vorhanden?
                {
                    if (anzahlBenoetigteWaren == 1)
                        erfolgWarenInProzent = 100 / 2;
                    else
                        erfolgWarenInProzent = (100 / anzahlBenoetigteWaren) / 2;

                    messageWareFehlt += "nicht ganz ausreichendes Bier";
                }
                else  // Weniger als 30 % der Ware vorhanden, bringt keinen Erfolgsvorteil
                {
                    if (verbrauchBier == 0)
                        messageWareFehlt += "komplett fehlendes Bier";
                    else
                        messageWareFehlt += "viel zu wenig Bier";
                }

                // Ware verbrauchen
                if (verbrauchBier > 0)
                    SW.Dynamisch.GetAktHum().SetStadtRohstoffAnzahl(fest.StadtID, 4, SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 4) - verbrauchBier);
            }

            if ((verbrauchSollFisch > 0) && (erfolgWarenInProzent < 100))
            {
                if (verbrauchFisch == verbrauchSollFisch)  // Ware vollständig vorhanden?
                {
                    if (anzahlBenoetigteWaren == 1)
                        erfolgWarenInProzent = 100;
                    else
                        erfolgWarenInProzent += 100 / anzahlBenoetigteWaren;
                }
                else if (verbrauchFisch >= (Convert.ToDouble(verbrauchSollFisch) * 0.3d))  // Ware zu mind. 30 % vorhanden?
                {
                    if (anzahlBenoetigteWaren == 1)
                        erfolgWarenInProzent = 100 / 2;
                    else
                        erfolgWarenInProzent += (100 / anzahlBenoetigteWaren) / 2;

                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    messageWareFehlt += "nicht ganz ausreichenden Fisch";
                }
                else  // Weniger als 30 % der Ware vorhanden, bringt keinen Erfolgsvorteil
                {
                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    if (verbrauchFisch == 0)
                        messageWareFehlt += "komplett fehlender Fisch";
                    else
                        messageWareFehlt += "viel zu wenig Fisch";
                }

                // Ware verbrauchen
                if (verbrauchFisch > 0)
                    SW.Dynamisch.GetAktHum().SetStadtRohstoffAnzahl(fest.StadtID, 6, SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 6) - verbrauchFisch);
            }

            if ((verbrauchSollObst > 0) && (erfolgWarenInProzent < 100))
            {
                if (verbrauchObst == verbrauchSollObst)  // Ware vollständig vorhanden?
                {
                    erfolgWarenInProzent += 100 / anzahlBenoetigteWaren;
                }
                else if (verbrauchObst >= (Convert.ToDouble(verbrauchSollObst) * 0.3d))  // Ware zu mind. 30 % vorhanden?
                {
                    erfolgWarenInProzent += (100 / anzahlBenoetigteWaren) / 2;

                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    messageWareFehlt += "nicht ganz ausreichendes Obst";
                }
                else  // Weniger als 30 % der Ware vorhanden, bringt keinen Erfolgsvorteil
                {
                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    if (verbrauchObst == 0)
                        messageWareFehlt += "komplett fehlendes Obst";
                    else
                        messageWareFehlt += "viel zu wenig Obst";
                }

                // Ware verbrauchen
                if (verbrauchObst > 0)
                    SW.Dynamisch.GetAktHum().SetStadtRohstoffAnzahl(fest.StadtID, 3, SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 3) - verbrauchObst);
            }

            if ((verbrauchSollWein > 0) && (erfolgWarenInProzent < 100))
            {
                if (verbrauchWein == verbrauchSollWein)  // Ware vollständig vorhanden?
                {
                    erfolgWarenInProzent += 100 / anzahlBenoetigteWaren;
                }
                else if (verbrauchWein >= (Convert.ToDouble(verbrauchSollWein) * 0.3d))  // Ware zu mind. 30 % vorhanden?
                {
                    erfolgWarenInProzent += (100 / anzahlBenoetigteWaren) / 2;

                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    messageWareFehlt += "nicht ganz ausreichenden Wein";
                }
                else  // Weniger als 30 % der Ware vorhanden, bringt keinen Erfolgsvorteil
                {
                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    if (verbrauchObst == 0)
                        messageWareFehlt += "komplett fehlenden Wein";
                    else
                        messageWareFehlt += "viel zu wenig Wein";
                }

                // Ware verbrauchen
                if (verbrauchWein > 0)
                    SW.Dynamisch.GetAktHum().SetStadtRohstoffAnzahl(fest.StadtID, 9, SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 9) - verbrauchWein);
            }

            if ((verbrauchSollRum > 0) && (erfolgWarenInProzent < 100))
            {
                if (verbrauchRum == verbrauchSollRum)  // Ware vollständig vorhanden?
                {
                    erfolgWarenInProzent += 100 / anzahlBenoetigteWaren;
                }
                else if (verbrauchRum >= (Convert.ToDouble(verbrauchSollRum) * 0.3d))  // Ware zu mind. 30 % vorhanden?
                {
                    erfolgWarenInProzent += (100 / anzahlBenoetigteWaren) / 2;

                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    messageWareFehlt += "nicht ganz ausreichenden Rum";
                }
                else  // Weniger als 30 % der Ware vorhanden, bringt keinen Erfolgsvorteil
                {
                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    if (verbrauchObst == 0)
                        messageWareFehlt += "komplett fehlenden Rum";
                    else
                        messageWareFehlt += "viel zu wenig Rum";
                }

                // Ware verbrauchen
                if (verbrauchRum > 0)
                    SW.Dynamisch.GetAktHum().SetStadtRohstoffAnzahl(fest.StadtID, 12, SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 12) - verbrauchRum);
            }

            if ((verbrauchSollRind > 0) && (erfolgWarenInProzent < 100))
            {
                if (verbrauchRind == verbrauchSollRind)  // Ware vollständig vorhanden?
                {
                    erfolgWarenInProzent += 100 / anzahlBenoetigteWaren;
                }
                else if (verbrauchRind >= (Convert.ToDouble(verbrauchSollRind) * 0.3d))  // Ware zu mind. 30 % vorhanden?
                {
                    erfolgWarenInProzent += (100 / anzahlBenoetigteWaren) / 2;

                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    messageWareFehlt += "nicht ganz ausreichendes Rind";
                }
                else  // Weniger als 30 % der Ware vorhanden, bringt keinen Erfolgsvorteil
                {
                    if (!string.IsNullOrEmpty(messageWareFehlt))
                        messageWareFehlt += ", ";

                    if (verbrauchObst == 0)
                        messageWareFehlt += "komplett fehlendes Rind";
                    else
                        messageWareFehlt += "viel zu wenig Rind";
                }

                // Ware verbrauchen
                if (verbrauchRind > 0)
                    SW.Dynamisch.GetAktHum().SetStadtRohstoffAnzahl(fest.StadtID, 10, SW.Dynamisch.GetAktHum().GetStadtRohstoffAnzahl(fest.StadtID, 10) - verbrauchRind);
            }

            #endregion

            // Geld für die Musiker abziehen
            SW.Dynamisch.GetAktHum().ErhoeheTaler(-fest.GeplanteKosten);

            // Ermitteln wie hoch der Erfolg war
            int erfolgMusikerInProzent;
            string messageMusiker = "";

            switch (fest.Musiker)
            {
                case EnumFestMusiker.schlechten:
                    erfolgMusikerInProzent = 10;
                    messageMusiker = "schlechte Musiker";
                    break;
                case EnumFestMusiker.mittelmäßigen:
                    erfolgMusikerInProzent = 25;
                    messageMusiker = "mittelmäßige Musiker";
                    break;
                case EnumFestMusiker.guten:
                    erfolgMusikerInProzent = 50;
                    break;
                default:
                    throw new NotSupportedException(fest.Musiker.ToString() + " wird nicht unterstützt.");
            }

            int gesamterfolgInProzent = erfolgMusikerInProzent + (int)(Convert.ToDouble(erfolgWarenInProzent) * 0.5d);

            // Meldung erstellen und ggf. Ansehen verändern
            string message;

            if (gesamterfolgInProzent == 100)
            {
                message = $"Ihr gabt ein {fest.Groesse} Fest in {GetStadtName(fest.StadtID)}. Euer rauschende Fest war ein voller Erfolg! Die Sause kostete Euch {fest.GeplanteKosten.ToStringGeld()}.";
                SW.Dynamisch.GetAktHum().ErhoehePermaAnsehen((int)(9d * permaAnsehenFaktor));
            }
            else if (gesamterfolgInProzent >= 80)
            {
                message = $"Ihr gabt ein {fest.Groesse} Fest in {GetStadtName(fest.StadtID)}. Euer angenehmes Fest war ein leidlicher Erfolg! Die Sause kostete Euch {fest.GeplanteKosten.ToStringGeld()}.";
                SW.Dynamisch.GetAktHum().ErhoehePermaAnsehen((int)(7d * permaAnsehenFaktor));
            }
            else if (gesamterfolgInProzent >= 60)
            {
                message = $"Ihr gabt ein {fest.Groesse} Fest in {GetStadtName(fest.StadtID)}. Euer mittelmäßiges Fest war teilweise ein Erfolg. Die Sause kostete Euch {fest.GeplanteKosten.ToStringGeld()}.";
                SW.Dynamisch.GetAktHum().ErhoehePermaAnsehen((int)(5d * permaAnsehenFaktor));
            }
            else if (gesamterfolgInProzent >= 40)
            {
                message = $"Ihr gabt ein {fest.Groesse} Fest in {GetStadtName(fest.StadtID)}. Euer langweiliges Fest war gerade noch ein kleiner Erfolg. Die Sause kostete Euch {fest.GeplanteKosten.ToStringGeld()}.";
                SW.Dynamisch.GetAktHum().ErhoehePermaAnsehen((int)(2d * permaAnsehenFaktor));
            }
            else if (gesamterfolgInProzent >= 20)
            {
                message = $"Ihr gabt ein {fest.Groesse} Fest in {GetStadtName(fest.StadtID)}. Euer niveauloses Fest war leider kein Erfolg. Die Sause kostete Euch {fest.GeplanteKosten.ToStringGeld()}.";
            }
            else
            {
                message = $"Ihr gabt ein {fest.Groesse} Fest in {GetStadtName(fest.StadtID)}. Euer unterirdisches Fest war kein Erfolg. Ihr wurdet von einigen Gästen verspottet. Die Sause kostete Euch {fest.GeplanteKosten.ToStringGeld()}.";
                SW.Dynamisch.GetAktHum().ErhoehePermaAnsehen(-(int)(3d * permaAnsehenFaktor));
            }

            if (!string.IsNullOrEmpty(messageWareFehlt))
                message += $"{Environment.NewLine}{Environment.NewLine}Die Gäste beschwerten sich über {messageWareFehlt}";

            if (!string.IsNullOrEmpty(messageMusiker))
            {
                if (string.IsNullOrEmpty(messageWareFehlt))
                    message += $"{Environment.NewLine}{Environment.NewLine}Die Gäste beschwerten sich über {messageMusiker}.";
                else
                    message += $" und {messageMusiker}.";
            }
            else if (!string.IsNullOrEmpty(messageWareFehlt))
            {
                message += ".";
            }

            return message;
        }
    }
}
