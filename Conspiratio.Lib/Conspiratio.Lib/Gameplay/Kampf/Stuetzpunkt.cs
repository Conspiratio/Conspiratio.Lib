using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Kampf.Einheiten;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Basisklasse zur Abbildung eines Militärstützpunktes (z.B. Zollburg oder Räuberlager)
    /// </summary>
    [Serializable]
    public class Stuetzpunkt
    {
        #region Variablen und Properties

        private int _zustandInProzent;
        private int _sicherheitTarnungInProzent;
        private int _maximaleKapazitaet;
        private int _kapazitaet;
        private int _moralTruppeInProzent;

        /// <summary>
        /// Interne, eindeutige Nummer des Stützpunktes (laufend, beginnt mit 1)
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Name des Stützpunktes
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// ID des Besitzers (Spieler oder KI)
        /// </summary>
        public int Besitzer { get; set; }

        /// <summary>
        /// Baujahr des Stützpunktes (nur ein Infowert ohne Gameplay-Nutzen, dient einfach der Atmosphäre)
        /// </summary>
        public int Baujahr { get; }

        /// <summary>
        /// Basispreis des Stützpunkts, dient als Grundlage zur Berechnung des aktuellen Werts und Kauf/Verkaufspreises
        /// </summary>
        public int Basispreis { get; }

        /// <summary>
        /// Bezeichnung der Kapazität (z.B. Strohmatten)
        /// </summary>
        public string KapazitaetBezeichnung { get; }

        /// <summary>
        /// ID des Landes, in dem sich dieser Stützpunkt befindet.
        /// </summary>
        public int LandID { get; }

        /// <summary>
        /// Liste aller Einheiten in diesem Stützpunkt
        /// </summary>
        public List<Einheit> Einheiten { get; }

        /// <summary>
        /// Art des Stützpunktes (z.B. Zollburg)
        /// </summary>
        public EnumStuetzpunktArt Art { get; }

        /// <summary>
        /// Aktionen des Stützpunktes (z.B. Überwachen von ...)
        /// </summary>
        public StuetzpunktAktion[] Aktionen { get; set;  }

        /// <summary>
        /// Zustand des Stützpunktes in Prozent (0 - 100), hat Einfluss auf die Verteidung bei einem Angriff.
        /// </summary>
        public int ZustandInProzent
        {
            get { return _zustandInProzent; }
            set
            {
                if (value < 0)
                    _zustandInProzent = 0;
                else if (value > 100)
                    _zustandInProzent = 100;
                else
                    _zustandInProzent = value;
            }
        }

        /// <summary>
        /// Sicherheit oder Tarnung des Stützpunktes in Prozent (0 - 100), hat Einfluss auf die Verteidung bei einem Angriff sowie auf die Moral der Truppe.
        /// </summary>
        public int SicherheitTarnungInProzent
        {
            get { return _sicherheitTarnungInProzent; }
            set
            {
                if (value < 0)
                    _sicherheitTarnungInProzent = 0;
                else if (value > 100)
                    _sicherheitTarnungInProzent = 100;
                else
                    _sicherheitTarnungInProzent = value;
            }
        }

        /// <summary>
        /// Maximale Kapazität des Stützpunktes (Strohmatten oder Kasematten) im Hinblick auf die Anzahl der Truppen, die untergebracht werden können.
        /// </summary>
        public int MaximaleKapazitaet
        {
            get { return _maximaleKapazitaet; }
            set
            {
                if (value < 0)
                    _maximaleKapazitaet = 0;
                else
                    _maximaleKapazitaet = value;
            }
        }

        /// <summary>
        /// Aktuelle Kapazität des Stützpunktes (Strohmatten oder Kasematten) im Hinblick auf die Anzahl der Truppen, die untergebracht werden können.
        /// Kann die maximale Kapazität nicht überschreiten.
        /// </summary>
        public int Kapazitaet
        {
            get { return _kapazitaet; }
            set
            {
                if (value < 0)
                    _kapazitaet = 0;
                else if (value > MaximaleKapazitaet)
                    _kapazitaet = MaximaleKapazitaet;
                else
                    _kapazitaet = value;
            }
        }

        /// <summary>
        /// Aktuelle Moral der Truppe in Prozent (0 - 100). Hat Einfluss auf die Kampfstärke.
        /// </summary>
        public int MoralTruppeInProzent
        {
            get { return _moralTruppeInProzent; }
            set
            {
                if (value < 0)
                    _moralTruppeInProzent = 0;
                else if (value > 100)
                    _moralTruppeInProzent = 100;
                else
                    _moralTruppeInProzent = value;
            }
        }
        #endregion

        #region Konstruktor
        /// <summary>
        /// Dient zur Intitialisierung des Objekts
        /// </summary>
        /// <param name="id">Gewünschte interne, eindeutige Nummer des Stützpunktes (laufend, beginnt mit 1)</param>
        /// <param name="name">Gewünschter Name (z.B. Hochfels)</param>
        /// <param name="besitzer">Gewünschte ID des Besitzers (Spieler oder KI), 0 bei keinem Besitzer</param>
        /// <param name="baujahr">Gewünschtes Baujahr (kein Gameplay-Nutzen)</param>
        /// <param name="basispreis">Gewünschter Basispreis</param>
        /// <param name="kapazitaetBezeichnung">Gewünschte Bezeichnung der Kapazität (z.B. Strohmatten)</param>
        /// <param name="landID">ID des Landes, in dem sich dieser Stützpunkt befindet.</param>
        /// <param name="art">Gewünschte Art des Stützpunktes</param>
        /// <param name="zustandInProzent">Gewünschter Zustand in Prozent (ca. 60 sind Standard-Startwert)</param>
        /// <param name="sicherheitTarnungInProzent">Gewünschte Sicherheit/Tarnung in Prozent</param>
        /// <param name="maximaleKapazitaet">Gewünschte maximale Kapazität des Stützpunktes im Hinblick auf Anzahl der Truppen</param>
        /// <param name="kapazitaet">Gewünschte aktuelle Kapazität des Stützpunktes im Hinblick auf Anzahl der Truppen</param>
        /// <param name="moralTruppeInProzent">Gewünschte Moral der Truppen (ca. 50 sind Standard-Startwert)</param>
        /// <param name="aktionen">OPTIONAL: Liste der Aktionen dieses Stützpunktes</param>
        /// <param name="einheiten">OPTIONAL: Liste aller Einheiten dieses Stützpunktes</param>
        public Stuetzpunkt(int id, string name, int besitzer, int baujahr, int basispreis, string kapazitaetBezeichnung, int landID, EnumStuetzpunktArt art, int zustandInProzent, 
                           int sicherheitTarnungInProzent, int maximaleKapazitaet, int kapazitaet, int moralTruppeInProzent, StuetzpunktAktion[] aktionen = null, List<Einheit> einheiten = null)
        {
            ID = id;
            Name = name;
            Besitzer = Besitzer;
            Baujahr = baujahr;
            Basispreis = basispreis;
            KapazitaetBezeichnung = kapazitaetBezeichnung;
            LandID = landID;
            Einheiten = einheiten;
            Art = art;
            Aktionen = aktionen;
            ZustandInProzent = zustandInProzent;
            SicherheitTarnungInProzent = sicherheitTarnungInProzent;
            MaximaleKapazitaet = maximaleKapazitaet;
            Kapazitaet = kapazitaet;
            MoralTruppeInProzent = moralTruppeInProzent;

            Einheiten = new List<Einheit>();
            Aktionen = new StuetzpunktAktion[2];
        }
        #endregion

        #region Funktionen

        #region ErhoeheTruppen
        /// <summary>
        /// Erhöht die Truppen des Stützpunktes um die angegebene Anzahl. Bei der Erhöhung kommt es im Standardfall zu einem Moralverlust der Truppe.
        /// Wenn die Erhöhung nicht möglich ist, wird der Grund als String zurückgegeben, ansonsten null.
        /// </summary>
        /// <param name="Anzahl">Gewünschte Anzahl der Einheiten, die hinzugefügt werden sollen</param>
        /// <param name="TypeEinheit">Gewünschte Einheit, die hinzugefügt werden soll (Klasse abgeleitet von Einheit z.B. ZollSoeldner)</param>
        /// <param name="bMoralveraenderung">OPTIONAL: Gibt an, ob die Moral beim Hinzufügen von neuen Truppen verändert werden soll</param>
        /// <returns>Gibt im Falle von Erfolg null zurück, ansonsten einen String mit der Meldung, warum das Hinzufügen gescheitert ist</returns>
        public string ErhoeheTruppen(int Anzahl, Type TypeEinheit, bool bMoralveraenderung = true)
        {
            // Plausiprüfungen
            if (!TypeEinheit.IsSubclassOf(typeof(Einheit)))
                return $"Systemfehler: Ungültiger Typ '{TypeEinheit.ToString()}' für Einheit!";

            Einheit Truppeneinheit = (Einheit)Activator.CreateInstance(TypeEinheit);

            if (Truppeneinheit.StuetzpunktArt != Art)
                return $"Ungültige Art der Einheit '{Truppeneinheit.Name}'\n für diesen Stützpunkt.";

            if ((Einheiten?.Count + Anzahl) > Kapazitaet)
            {
                if (Anzahl == 1)
                    return $"Es gibt nicht genügend \nUnterkünfte für {Anzahl} neuen Rekrut.";
                else
                    return $"Es gibt nicht genügend \nUnterkünfte für {Anzahl} neue Rekruten.";
            }

            for (int i = 0; i < Anzahl; i++)
            {
                // Eine neue Instanz der übergebenen Truppeneinheit erstellen und der Liste der Einheiten des Stützpunktes hinzufügen
                Einheiten.Add((Einheit)Activator.CreateInstance(TypeEinheit));

                if (bMoralveraenderung)
                    MoralTruppeInProzent -= 1;  // Pro neuer Einheit 1% Moralverlust
            }

            return null;
        }
        #endregion

        #region VerringereTruppen
        /// <summary>
        /// Verringert die Truppen des Stützpunktes um die angegebene Anzahl. Bei der Verringerung kommt es im Standardfall zu einem Moralverlust der Truppe.
        /// Wenn die Verringerung nicht möglich ist, wird der Grund als String zurückgegeben, ansonsten null.
        /// </summary>
        /// <param name="Anzahl">Gewünschte Anzahl der Einheiten, die entfernt werden sollen</param>
        /// <param name="TypeEinheit">Gewünschte Einheit, die entfernt werden soll (Klasse abgeleitet von Einheit z.B. ZollSoeldner)</param>
        /// <param name="bMoralveraenderung">OPTIONAL: Gibt an, ob die Moral beim Entfernen von neuen Truppen verändert werden soll</param>
        /// <returns>Gibt im Falle von Erfolg null zurück, ansonsten einen String mit der Meldung, warum das Entfernen gescheitert ist</returns>
        public string VerringereTruppen(int Anzahl, Type TypeEinheit, bool bMoralveraenderung = true)
        {
            // Plausiprüfungen
            if (!TypeEinheit.IsSubclassOf(typeof(Einheit)))
                return $"Systemfehler: Ungültiger Typ '{TypeEinheit.ToString()}' für Einheit!";

            Einheit Truppeneinheit = (Einheit)Activator.CreateInstance(TypeEinheit);

            if (Truppeneinheit.StuetzpunktArt != Art)
                return $"Ungültige Art der Einheit '{Truppeneinheit.Name}' für diesen Stützpunkt.";

            if (GetAnzahlTruppen(TypeEinheit) < Anzahl)
                return $"Ihr habt nicht so viele Truppen.";

            for (int i = 0; i < Anzahl; i++)
            {
                for (int j = 0; j < Einheiten?.Count; j++)
                {
                    if (Einheiten[j].GetType() == TypeEinheit)
                    {
                        Einheiten.RemoveAt(j);

                        if (bMoralveraenderung)
                            MoralTruppeInProzent -= 1;  // Pro entfernter Einheit 1% Moralverlust
                        break;
                    }
                }
            }

            return null;
        }
        #endregion

        #region GetAnzahlTruppen
        /// <summary>
        /// Gibt die Anzahl der Truppen einer bestimmten Einheit des Stützpunktes zurück.
        /// </summary>
        /// <param name="TypeEinheit">Gewünschte Einheit, die gezählt werden soll (Klasse abgeleitet von Einheit z.B. ZollSoeldner)</param>
        /// <returns>Gibt gibt die Anzahl der Truppen zurück</returns>
        public int GetAnzahlTruppen(Type TypeEinheit)
        {
            return Einheiten.Count(x => x.GetType() == TypeEinheit);
        }
        #endregion

        #region BerechneWert
        /// <summary>
        /// Dient zur Berechnung des aktuellen Werts des Stützpunktes. Es wird der Zustand, Sicherheit/Tarnung sowie die Kapazität und ein grundsätzlicher Wertverlust nach Kauf berücksichtigt.
        /// </summary>
        /// <returns>Aktueller Wert des Stützpunktes</returns>
        public int BerechneWert()
        {
            double dWertverlustFaktor = 0.8d;  // Wertverlust durch Kauf
            int Wert = Convert.ToInt32(Basispreis * dWertverlustFaktor);
            
            if (ZustandInProzent < 60)  // Wertverlust durch Zustand?
                Wert -= Convert.ToInt32(((100 - ZustandInProzent) * 0.003) * Wert);
            else // Wertgewinn durch Zustand
                Wert += Convert.ToInt32((ZustandInProzent * 0.002) * Wert);

            if (SicherheitTarnungInProzent < 40)  // Wertverlust durch Sicherheit/Tarnung?
                Wert -= Convert.ToInt32(((100 - SicherheitTarnungInProzent) * 0.004) * Wert);
            else  // Wertgewinn durch Sicherheit/Tarnung
                Wert += Convert.ToInt32((SicherheitTarnungInProzent * 0.003) * Wert);

            if (Kapazitaet < 30)  // Wertverlust durch Kapazität?
                Wert -= (30 - Kapazitaet) * 400;
            else  // Wertgewinn durch Kapazität
                Wert += (Kapazitaet - 30) * 200;

            return Wert;
        }
        #endregion

        #region BerechneKostenSicherheitTarnung
        /// <summary>
        /// Dient zur Berechnung von Kosten einer Erhöhung von Sicherheit/Tarnung in %.
        /// </summary>
        /// <returns>Aktuelle Kosten für die Erhöhung</returns>
        public int BerechneKostenSicherheitTarnung(int ErhoehungInProzent)
        {
            return ErhoehungInProzent * (BerechneWert() / 100);  // Kosten je % = 1 % vom aktuellen Gesamtwert
        }
        #endregion

        #region BerechneKostenZustand
        /// <summary>
        /// Dient zur Berechnung von Kosten einer Erhöhung vom Zustand in %.
        /// </summary>
        /// <param name="ErhoehungInProzent"></param>
        /// <returns>Aktuelle Kosten für die Erhöhung</returns>
        public int BerechneKostenZustand(int ErhoehungInProzent)
        {
            return ErhoehungInProzent * ((BerechneWert() / 100) / 2);  // Kosten je % = 0,5 % vom aktuellen Gesamtwert
        }
        #endregion

        #region BerechneKostenKapazitaet
        /// <summary>
        /// Dient zur Berechnung von Kosten einer Erhöhung vom der Kapazität.
        /// </summary>
        /// <param name="AnzahlErhoehung"></param>
        /// <returns>Aktuelle Kosten für die Erhöhung</returns>
        public int BerechneKostenKapazitaet(int AnzahlErhoehung)
        {
            return AnzahlErhoehung * (BerechneWert() / 100);  // Kosten je % = 1 % vom aktuellen Gesamtwert
        }
        #endregion

        #region BerechneKostenManoever
        /// <summary>
        /// Dient zur Berechnung der Kosten eines Truppen Manövers.
        /// </summary>
        /// <returns>Aktuelle Kosten für ein Manöver.</returns>
        public int BerechneKostenManoever()
        {
            double Prozentsatz = Convert.ToDouble(MoralTruppeInProzent) / 100d;

            if (MoralTruppeInProzent < 40)
                Prozentsatz = 0.4d;

            return Convert.ToInt32(Einheiten.Count * 100 * Prozentsatz);  // Bei z.B. 50 % Moral: Pro Truppeneinheit 50 Taler
        }
        #endregion

        #region ManoeverDurchfuehrenSpieler
        /// <summary>
        /// Dient zur Durchführung eines Manövers auf dem Stützpunkt des aktiven menschlichen Spielers.
        /// </summary>
        /// <returns>Manöver erfolgreich (wurde die Moral erhöht)?</returns>
        public bool ManoeverDurchfuehrenSpieler()
        {
            int Wuerfel = SW.Statisch.Rnd.Next(1, 21); // 1 - 20 würfeln
            int KostenManoever = BerechneKostenManoever();

            if (KostenManoever <= 0)
            {
                SW.Dynamisch.BelTextAnzeigen($"Ihr habt derzeit keine Truppen\n in {Name} stationiert und könnt\n kein Manöver durchführen lassen.");
                return false;
            }

            if (SW.UI.JaNeinFrage.ShowDialogText($"Aktuelle Moral: {MoralTruppeInProzent} %\nWollt Ihr mit Euren Truppen\n in {Name} wirklich ein Manöver\n für {KostenManoever.ToStringGeld()} durchführen lassen?", "Ja", "Lieber nicht!") != DialogResult.Yes)
                return false;

            if (!SW.Dynamisch.CheckIfenoughGold(KostenManoever))
                return false;

            SW.Dynamisch.GetSpWithID(Besitzer).ErhoeheTaler(-KostenManoever);

            if (Wuerfel == 1)
            {
                MoralTruppeInProzent -= 1;  // nicht erfolgreich, Moral sinkt
                SW.Dynamisch.BelTextAnzeigen($"Das Manöver verlief äußerst erfolglos.\n Die Moral Eurer Truppe sank auf\n {MoralTruppeInProzent} %.");
                return false;
            }
            else if (Wuerfel <= 3)
            {
                SW.Dynamisch.BelTextAnzeigen($"Das Manöver verlief nicht erfolgreich.\n Die Moral Eurer Truppe verbesserte sich nicht.");
                return false;  // nicht erfolgreich, Moral bleibt gleich
            }
            else
            {
                MoralTruppeInProzent += 2;  // erfolgreich, Moral steigt
                SW.Dynamisch.BelTextAnzeigen($"Das Manöver verlief erfolgreich.\n Die Moral Eurer Truppe stieg auf\n {MoralTruppeInProzent} %.");
                return true;
            }
        }
        #endregion

        #region ManoeverDurchfuehrenKISpieler
        /// <summary>
        /// Dient zur Durchführung eines Manövers auf dem Stützpunkt eines KI-Spielers.
        /// </summary>
        /// <returns>Ergebnis des Manövers als Text</returns>
        public string ManoeverDurchfuehrenKISpieler()
        {
            int Wuerfel = SW.Statisch.Rnd.Next(1, 21); // 1 - 20 würfeln

            if (Einheiten.Count == 0)
                return null;

            // SW.Dynamisch.getSpWithID(Besitzer).veraenderTaler(-KostenManoever);   // Kosten erst berücksichtigen, wenn mir klar ist, ob und wie KI-Spieler an Geld kommen

            if (Wuerfel == 1)
            {
                MoralTruppeInProzent -= 1;  // nicht erfolgreich, Moral sinkt
                return $"{SW.Dynamisch.GetSpWithID(Besitzer).GetName()} führte mit " + SW.Dynamisch.GetSpWithID(Besitzer).GetSeinenIhren() + " Truppen ein äußerst erfolgloses Manöver in der Umgebung von {Name} durch.\n";
            }
            else if (Wuerfel <= 3)
            {
                return $"{SW.Dynamisch.GetSpWithID(Besitzer).GetName()} führte mit " + SW.Dynamisch.GetSpWithID(Besitzer).GetSeinenIhren() + " Truppen ein erfolgloses Manöver in der Umgebung von {Name} durch.\n";
            }
            else
            {
                MoralTruppeInProzent += 2;  // erfolgreich, Moral steigt
                return $"{SW.Dynamisch.GetSpWithID(Besitzer).GetName()} führte mit " + SW.Dynamisch.GetSpWithID(Besitzer).GetSeinenIhren() + " Truppen ein Manöver in der Umgebung von {Name} durch. Es verlief erfolgreich.\n";
            }
        }
        #endregion

        #region StuetzpunktArtAlsString
        /// <summary>
        /// Gibt den Bezeichnung der Art des Stützpunktes als String zurürck (z.B. Zollburg).
        /// </summary>
        /// <returns>Art als String  (z.B. Zollburg)</returns>
        public string StuetzpunktArtAlsString()
        {
            if (Art == EnumStuetzpunktArt.Zollburg)
                return "Zollburg";
            else if (Art == EnumStuetzpunktArt.Raeuberlager)
                return "Räuberlager";
            else
                return "";
        }
        #endregion

        #region SicherheitTarnungAlsString
        /// <summary>
        /// Gibt je nach Art des Stützpunktes die Bezeichnung  der Ausbaustufe als String zurürck (z.B. Sicherheit).
        /// </summary>
        /// <returns>Ausbaustufe als String  (z.B. Sicherheit)</returns>
        public string SicherheitTarnungAlsString()
        {
            if (Art == EnumStuetzpunktArt.Zollburg)
                return "Sicherheit";
            else if (Art == EnumStuetzpunktArt.Raeuberlager)
                return "Tarnung";
            else
                return "";
        }
        #endregion

        #region KaufangebotAbgeben
        /// <summary>
        /// Dient zur Unterbreitung eines Kaufangebots an den Besitzer eines Stützpunktes.
        /// </summary>
        /// <param name="Preis">Gewünschte Preis des Angebots</param>
        /// <returns>Kauf erfolgreich?</returns>
        public bool KaufangebotAbgeben(int Preis)
        {
            if (SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).HatAngebotFuerStuetzpunktAbgegeben)
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr habt in diesem Jahr bereits ein Angebot abgegeben.");
                return false;
            }

            if (Preis <= 0)
            {
                SW.Dynamisch.BelTextAnzeigen("Ihr müsst einen Kaufpreis angeben.");
                return false;
            }

            if (!SW.Dynamisch.CheckIfenoughGold(Preis))
                return false;

            string NameBesitzer = "";

            if (Besitzer >= SW.Statisch.GetMinKIID())
                NameBesitzer = SW.Dynamisch.GetKIwithID(Besitzer).GetName();
            else
                NameBesitzer = SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).GetName();

            if (SW.UI.JaNeinFrage.ShowDialogText("Wollt Ihr " + NameBesitzer + " wirklich ein Angebot\nüber " + Preis.ToStringGeld() +
                                                 " unterbreiten?\nIhr könnt pro Jahr nur einmal ein Angebot abgeben.", "Ja", "Lieber nicht!") == DialogResult.Yes)
            {
                if (Besitzer >= SW.Statisch.GetMinKIID())
                {
                    SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).HatAngebotFuerStuetzpunktAbgegeben = true;

                    // Prüfen, ob das Angebot angenommen wurde
                    int bezieh = SW.Dynamisch.GetKIwithID(Besitzer).GetBeziehungZuKIX(SW.Dynamisch.GetAktiverSpieler());
                    int ansehbon = Convert.ToInt32(SW.Dynamisch.GetSpWithID(SW.Dynamisch.GetAktiverSpieler()).GetAnsehen() / 10);
                    int relsympathie = SW.Dynamisch.GetRelSympathieVonXzuY(Besitzer, SW.Dynamisch.GetAktiverSpieler());
                    int value = bezieh + ansehbon + relsympathie;

                    int AktuellerWert = BerechneWert();

                    if (Preis >= (AktuellerWert + 1000))
                        value += (Preis - AktuellerWert) / 1000;

                    int zufall = 0;

                    if (value > 120)
                        zufall = SW.Statisch.Rnd.Next(0, value);
                    else
                        zufall = SW.Statisch.Rnd.Next(0, 120);

                    if (zufall >= 100)
                    {
                        SW.Dynamisch.GetHumWithID(SW.Dynamisch.GetAktiverSpieler()).ErhoeheTaler(-Preis);
                        SW.Dynamisch.GetKIwithID(Besitzer).SetTaler(SW.Dynamisch.GetKIwithID(Besitzer).GetTaler() + Preis);
                        Besitzer = SW.Dynamisch.GetAktiverSpieler();
                        SW.Dynamisch.BelTextAnzeigen($"Euer Angebot wurde angenommen, Ihr seid nun stolzer Besitzer von {Name}.");
                        return true;
                    }
                    else
                    {
                        SW.Dynamisch.BelTextAnzeigen($"Leider konntet Ihr {NameBesitzer} mit Eurem Angebot nicht überzeugen.");
                        return false;
                    }
                }
                else
                {
                    // TODO: Kauf von anderem Spieler
                    SW.Dynamisch.BelTextAnzeigen("Ein Kaufangebot an einen Mitspieler ist leider noch nicht möglich.");
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region BesitzerStuetzpunktZufaelligSetzen
        /// <summary>
        /// Setzt den Besitzer eines Stützpunktes (Zollburgen und Räuberlager) auf einen zufälligen KI-Amtsinhaber eines Amtes der Stufe 2 (Reichsebene).
        /// Wenn auf der Amtsebene 2 kein KI-Spieler ermittelt werden konnte, greift ein Fallback und setzt den ersten KI-Spieler als Besitzer.
        /// </summary>
        /// <param name="ReichID">OPTIONAL: Gewünschte ID des Reiches (zur Ermittlung des Amtsträgers)</param>
        /// <param name="NurStuetzpunkteOhneBesitzer">OPTIONAL: Gibt an, ob der Besitzer nur für Stützpunkte ohne Besitzer oder für alle Stützpunkte gesetzt werden soll</param>
        public void BesitzerStuetzpunktZufaelligSetzen(int ReichID = 1, bool NurOhneBesitzer = false)
        {
            int AmtsinhaberID = 0;
            int Zaehler = 0;

            if ((!NurOhneBesitzer) || (NurOhneBesitzer && Besitzer <= 0))
            {
                while (AmtsinhaberID < SW.Statisch.GetMinKIID())
                {
                    // Nur Amtsinhaber von Stufe 2 Ämtern auswählen (Reichsebene, ab ID 34)
                    AmtsinhaberID = SW.Dynamisch.GetGebietwithID(ReichID, 2).GetAmtX(SW.Statisch.Rnd.Next(34, 49));

                    if (AmtsinhaberID >= SW.Statisch.GetMinKIID())  // Handelt es sich beim Amtsinhaber um einen KI-Spieler?
                        Besitzer = AmtsinhaberID;

                    Zaehler++;

                    if (Zaehler >= 50)  // Nach 50 erfolglosen Durchläufen greift ein Fallback: Es wird der erste KI Spieler als Besitzer gesetzt
                    {
                        Besitzer = SW.Statisch.GetMinKIID();
                        break;
                    }
                }
            }
        }
        #endregion

        #region TruppenAnheuern
        /// <summary>
        /// Dient zum Anheuern von Truppen zu einem Stützpunktes eines menschlichen Spielers.
        /// </summary>
        /// <param name="Anzahl">Gewünschte Anzahl</param>
        /// <param name="TypeEinheit">Gewünschte Einheit (Klasse abgeleitet von Einheit)</param>
        /// <returns>Anheuern erfolgreich?</returns>
        public bool TruppenAnheuern(int Anzahl, Type TypeEinheit)
        {
            Einheit Truppeneinheit = (Einheit)Activator.CreateInstance(TypeEinheit);
            int Kosten = Truppeneinheit.Basispreis * Anzahl;
            string Meldung;
            string NameEinheiten = Truppeneinheit.NamePlural;

            if (Kosten <= 0)
                return false;

            if (Anzahl == 1)
                NameEinheiten = Truppeneinheit.Name;

            if (SW.UI.JaNeinFrage.ShowDialogText($"Wollt Ihr {Anzahl} {NameEinheiten}\n für {Kosten.ToStringGeld()} Handgeld anheuern?", "Ja", "Lieber nicht!") != DialogResult.Yes)
                return false;

            if (!SW.Dynamisch.CheckIfenoughGold(Kosten))
                return false;

            Meldung = ErhoeheTruppen(Anzahl, TypeEinheit);

            if (Meldung != null)
            {
                SW.Dynamisch.BelTextAnzeigen(Meldung);
                return false;
            }

            SW.Dynamisch.GetSpWithID(Besitzer).ErhoeheTaler(-Kosten);
            return true;
        }
        #endregion

        #region TruppenEntlassen
        /// <summary>
        /// Dient zum Entlassen von Truppen zu einem Stützpunktes eines menschlichen Spielers.
        /// </summary>
        /// <param name="Anzahl">Gewünschte Anzahl</param>
        /// <param name="TypeEinheit">Gewünschte Einheit (Klasse abgeleitet von Einheit)</param>
        /// <returns>Entlassen erfolgreich?</returns>
        public bool TruppenEntlassen(int Anzahl, Type TypeEinheit)
        {
            Einheit Truppeneinheit = (Einheit)Activator.CreateInstance(TypeEinheit);
            string Meldung;
            string NameEinheiten = Truppeneinheit.NamePlural;

            if (Anzahl == 1)
                NameEinheiten = Truppeneinheit.Name;

            if (SW.UI.JaNeinFrage.ShowDialogText($"Wollt Ihr wirklich\n{Anzahl} {NameEinheiten} entlassen?", "Ja", "Lieber nicht!") != DialogResult.Yes)
                return false;

            Meldung = VerringereTruppen(Anzahl, TypeEinheit);

            if (Meldung != null)
            {
                SW.Dynamisch.BelTextAnzeigen(Meldung);
                return false;
            }

            return true;
        }
        #endregion

        #region RundenendeAlleAktionenDurchfuehren
        /// <summary>
        /// Diese Funktion dient am Rundenende dazu, alle aktuellen Aktionen dieses Stützpunktes durchzuführen (z.B. Truppen verschicken).
        /// </summary>
        /// <returns>Protokoll der Aktionen, die durchgeführt wurden (zur Anzeige)</returns>
        public string RundenendeAlleAktionenDurchfuehren()
        {
            string sText = "";

            if (Aktionen == null)
                return "";

            if (Besitzer >= SW.Statisch.GetMinKIID())  // Ist Besitzer ein KI-Spieler?
            {
                if (Art == EnumStuetzpunktArt.Zollburg)
                    sText += ((Zollburg)this).RundenendeKIAktionenDurchfuehren();
                else if (Art == EnumStuetzpunktArt.Raeuberlager)
                    sText += ((Raeuberlager)this).RundenendeKIAktionenDurchfuehren();
            }

            foreach (StuetzpunktAktion oAktion in Aktionen)
            {
                string sResult = oAktion.AktionAusfuehren(ID);

                if (!string.IsNullOrEmpty(sResult))
                    sText += sResult + "\n\n";
            }

            return sText;
        }
        #endregion

        #region KapazitaetErhoehen
        /// <summary>
        /// Funktion zur Erhöhung der Kapazität des Stützpunktes.
        /// </summary>
        /// <param name="Anzahl">Die gewünschte Anzahl, um die die Kapazität ausgebaut werden soll</param>
        /// <returns>Gibt null zurück, wenn der Ausbau erfolgreich war, einen leerer String, wenn die Anzahl ungültig (kleiner als 0) war und eine Fehlermeldung, wenn der Ausbau nicht möglich war</returns>
        public string KapazitaetErhoehen(int Anzahl)
        {
            if (Anzahl <= 0)
                return "";

            if ((Kapazitaet + Anzahl) > MaximaleKapazitaet)
                return "Der Ausbau ist nicht möglich, da die maximale Kapazität überschritten werden würde.";

            Kapazitaet += Anzahl;
            return null;
        }
        #endregion

        #endregion
    }
}