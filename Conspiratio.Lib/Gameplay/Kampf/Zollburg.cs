using System;

using Conspiratio.Kampf;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Gameplay.Kampf
{
    /// <summary>
    /// Diese Klasse stellt eine Zollburg dar
    /// </summary>
    [Serializable]
    public class Zollburg: Stuetzpunkt
    {
        #region Variablen und Properties

        private double _zoll;

        public double Zoll
        {
            get { return _zoll; }
            set
            {
                if (value < 0d)
                    _zoll = 0d;
                else
                    _zoll = value;
            }
        }

        #endregion

        #region Funktionen

        #region Konstruktor
        /// <summary>
        /// Dient zur Intitialisierung des Objekts
        /// </summary>
        /// <param name="id">Gewünschte interne, eindeutige Nummer des Stützpunktes (laufend, beginnt mit 1)</param>
        /// <param name="name">Gewünschter Name (z.B. Hochfels)</param>
        /// <param name="besitzer">Gewünschte ID des Besitzers (Spieler oder KI), 0 bei keinem Besitzer</param>
        /// <param name="baujahr">Gewünschtes Baujahr (kein Gameplay-Nutzen)</param>
        /// <param name="basispreis">Gewünschter Basispreis</param>
        /// <param name="landID">ID des Landes, in dem sich dieser Stützpunkt befindet.</param>
        /// <param name="zustandInProzent">Gewünschter Zustand in Prozent (ca. 60 sind Standard-Startwert)</param>
        /// <param name="sicherheitTarnungInProzent">Gewünschte Sicherheit/Tarnung in Prozent</param>
        /// <param name="maximaleKapazitaet">Gewünschte maximale Kapazität des Stützpunktes im Hinblick auf Anzahl der Truppen</param>
        /// <param name="kapazitaet">Gewünschte aktuelle Kapazität des Stützpunktes im Hinblick auf Anzahl der Truppen</param>
        /// <param name="moralTruppeInProzent">Gewünschte Moral der Truppen (ca. 50 sind Standard-Startwert)</param>
        /// <param name="zoll">Gewünschter Zollsatz (z.B. 0.15)</param>
        /// <param name="aktionen">OPTIONAL: Liste der Aktionen dieses Stützpunktes</param>
        public Zollburg(int id, string name, int besitzer, int baujahr, int basispreis, int landID, int zustandInProzent, int sicherheitTarnungInProzent, int maximaleKapazitaet, int kapazitaet, 
                        int moralTruppeInProzent, double zoll, StuetzpunktAktion[] aktionen = null) : 
                   base(id, name, besitzer, baujahr, basispreis, "Kasematten", landID, EnumStuetzpunktArt.Zollburg, zustandInProzent, sicherheitTarnungInProzent, maximaleKapazitaet, 
                        kapazitaet, moralTruppeInProzent, aktionen)
        {
            Zoll = zoll;
        }
        #endregion

        #region AktionenInitialisieren
        public void AktionenInitialisieren()
        {
            if (Aktionen == null)
                Aktionen = new ZollburgAktion[2];
        }
        #endregion

        #region GetLandID
        public int GetLandID()
        {
            for (int i = 1; i < SW.Statisch.GetMaxLandID(); i++)
            {
                if (SW.Dynamisch.GetLandWithID(i).GetZollburgIndex() == ID - 1)
                    return i;
            }

            return 0;
        }
        #endregion

        #region RundenendeKIAktionenDurchfuehren
        public string RundenendeKIAktionenDurchfuehren()
        {
            string text = "";
            string result;
            double kiAktivitaetsfaktor = 1d;  // 1.00 = normal (50 %), von 0.02 (1 %) bis 2.00 (100 %) möglich
            int wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100
            Type truppeneinheit = null;

            switch (SW.Dynamisch.Spielstand.Einstellungen.AggressivitaetKISpieler)
            {
                case Einstellungen.EnumSchwierigkeitsgrad.Niedrig:
                    kiAktivitaetsfaktor = 0.5d;  // 25 %
                    break;
                case Einstellungen.EnumSchwierigkeitsgrad.Mittel:
                    kiAktivitaetsfaktor = 1d;  // 50 %
                    break;
                case Einstellungen.EnumSchwierigkeitsgrad.Hoch:
                    kiAktivitaetsfaktor = 1.7d;  // 85 %
                    break;
            }

            if (wuerfel <= Convert.ToInt32(Math.Round(50 * kiAktivitaetsfaktor, 0)))  // Auswürfeln, ob generell in diesem Zug etwas passieren soll
            {
                // Kapazität erhöhen
                wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                if (wuerfel <= Convert.ToInt32(Math.Round(30 * kiAktivitaetsfaktor, 0)))  // Soll ausgebaut werden?
                {
                    result = KapazitaetErhoehen(2);

                    if (string.IsNullOrEmpty(result))
                        text += $"{Name} wird ausgebaut und um neue {KapazitaetBezeichnung} erweitert. ";
                }

                // Sicherheit erhöhen
                wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                if (wuerfel <= Convert.ToInt32(Math.Round(40 * kiAktivitaetsfaktor, 0)))  // Soll ausgebaut werden?
                {
                    if (SicherheitTarnungInProzent < 100)
                    {
                        SicherheitTarnungInProzent++;
                        text += $"Karren mit Baumaterial sind auf dem Weg nach {Name}, es wird die {SicherheitTarnungAlsString()} verbessert. ";
                    }
                }

                // Reparieren
                wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                if (wuerfel <= Convert.ToInt32(Math.Round(50 * kiAktivitaetsfaktor, 0)))  // Soll ausgebaut werden?
                {
                    if (ZustandInProzent < 100)
                    {
                        ZustandInProzent++;
                        text += $"Baumeister wurden nahe {Name} gesichtet, es werden kleinere Schäden repariert. ";
                    }
                }

                // Rekrutierung von neuen Truppen
                wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                if (wuerfel <= Convert.ToInt32(Math.Round(50 * kiAktivitaetsfaktor, 0)))  // Soll rekrutiert werden?
                {
                    /*
                    Verteilung in Prozent:
                    35 % = Söldner
                    30 % = Musketiere
                    20 % = Kanoniere
                    15 % = Offiziere
                    */

                    if (wuerfel <= 35)
                        truppeneinheit = typeof(ZollSoeldner);
                    else if (wuerfel <= 65)
                        truppeneinheit = typeof(ZollMusketier);
                    else if (wuerfel <= 85)
                        truppeneinheit = typeof(ZollKanonier);
                    else if (wuerfel <= 100)
                        truppeneinheit = typeof(ZollSoeldner);

                    result = ErhoeheTruppen(1, truppeneinheit);

                    if (string.IsNullOrEmpty(result))
                        text += $"Für {Name} werden neue Rekruten angeheuert. ";
                }

                // Manöver durchführen
                wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                if (wuerfel <= Convert.ToInt32(Math.Round(40 * kiAktivitaetsfaktor, 0)))  // Soll Manöver durchgeführt werden?
                {
                    result = ManoeverDurchfuehrenKISpieler();

                    if (string.IsNullOrEmpty(result))
                        text += result;
                }

                if (Einheiten.Count > 4)  // Nur bei mehr als 4 Einheiten
                {
                    if (Aktionen == null || Aktionen?.Length == 0)  // Müsste eine neue Aktion angelegt werden?
                    {
                        wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                        // Neue Aktion: Überwachen
                        if (wuerfel <= Convert.ToInt32(Math.Round(90 * kiAktivitaetsfaktor, 0)))  // Soll eine neue Aktion Überwachen angelegt werden?
                        {
                            AktionenInitialisieren();

                            Aktionen[0] = new ZollburgAktion(EnumAktionsartZollburg.Überwachen, GetLandID(), 0, ID, 0);
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollSoeldner))) / 2d, 0)), typeof(ZollSoeldner));
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollMusketier))) / 2d, 0)), typeof(ZollMusketier));
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollKanonier))) / 2d, 0)), typeof(ZollKanonier));
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollOffizier))) / 2d, 0)), typeof(ZollOffizier));
                        }

                        // TODO: Neue Aktion: Truppen schicken
                    }
                    else
                    {
                        wuerfel = SW.Statisch.Rnd.Next(1, 101);  // 1 bis 100

                        // Aktion aktualisieren (Art "Überwachen" und 50 % der Truppen)
                        if (wuerfel <= Convert.ToInt32(Math.Round(50 * kiAktivitaetsfaktor, 0)))  // Soll die erste Aktion aktualisiert werden?
                        {
                            Aktionen[0] = new ZollburgAktion(EnumAktionsartZollburg.Überwachen, GetLandID(), 0, ID, 0);
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollSoeldner))) / 2d, 0)), typeof(ZollSoeldner));
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollMusketier))) / 2d, 0)), typeof(ZollMusketier));
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollKanonier))) / 2d, 0)), typeof(ZollKanonier));
                            Aktionen[0].ErhoeheTruppen(Convert.ToInt32(Math.Round(Convert.ToDouble(GetAnzahlTruppen(typeof(ZollOffizier))) / 2d, 0)), typeof(ZollOffizier));
                        }

                        // TODO: Aktion entfernen (z.B. Truppen schicken)
                    }
                }
            }

            return text;
        }
        #endregion

        #endregion
    }
}
