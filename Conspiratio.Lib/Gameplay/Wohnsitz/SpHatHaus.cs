using System;
using System.Collections.Generic;
using Conspiratio.Lib.Gameplay.Spielwelt;
using Conspiratio.Lib.Gameplay.Wohnsitz;

namespace Conspiratio.Lib.Gameplay.Wohnsitz
{
    [Serializable]
    public class SpHatHaus
    {
        private int _hausID;
        private int _stadtID;  // Wenn die StadtID != 0 ist, existiert das Haus
        private int _restlicheBauzeit;
        private int _zustandInProzent = 100;
        private double _wertverlustFaktor = 0.5;  // Aktueller Wertverlust

        public SpHatHaus(int hausID, int stadtID, int zustandInProzent, bool inDiesemJahrRenovieren, List<int> hausErweiterungen)
        {
            _hausID = hausID;
            _stadtID = stadtID;
            _zustandInProzent = zustandInProzent;
            InDiesemJahrRenovieren = inDiesemJahrRenovieren;
            HausErweiterungen = hausErweiterungen;
        }

        public int GetHausID()
        {
            return _hausID;
        }

        public int GetStadtID()
        {
            return _stadtID;
        }

        public void SetHausID(int x)
        {
            _hausID = x;
        }

        public void SetStadtID(int x)
        {
            _stadtID = x;
        }

        public int GetRestlicheBauzeit()
        {
            return _restlicheBauzeit;
        }

        public void SetRestlicheBauzeit(int x)
        {
            _restlicheBauzeit = x;
        }

        #region GetNameInklPronomen
        /// <summary>
        /// Dient zur Ermittlung der Bezeichnung des Wohnsitzes inkl. Pronomen (Euer), sowie optional mit dem Umstand und den Erweiterungen.
        /// </summary>
        /// <param name="mitZustand">Optional: Zustand (z.B. prächtiges) anzeigen</param>
        /// <param name="mitErweiterungen">Optional: vorhandene Erweiterungen (z.B Garten) anzeigen</param>
        /// <returns>Bezeichnung des aktuellen Wohnsitzes (anhand HausID)</returns>
        public string GetNameInklPronomen(bool mitZustand = true, bool mitErweiterungen = true)
        {
            int counter = 1;
            int zustandInProzent = -1;
            string hausErweiterungen = "";

            if (mitZustand)
                zustandInProzent = _zustandInProzent;

            if (mitErweiterungen && HausErweiterungen != null)
            {
                if (HausErweiterungen.Count > 0)
                {
                    hausErweiterungen = " mit ";

                    foreach (int iHausErweiterungID in HausErweiterungen)
                    {
                        if (SW.Statisch.GetHaus(_hausID).HausErweiterungen.Count > iHausErweiterungID)  // Sicherheitsabfrage vor Zugriff auf Liste
                        {
                            if (hausErweiterungen != " mit ")
                            {
                                if (HausErweiterungen.Count != counter)
                                    hausErweiterungen += ", ";
                                else
                                    hausErweiterungen += " und ";
                            }   

                            hausErweiterungen += SW.Statisch.GetHaus(_hausID).HausErweiterungen[iHausErweiterungID].Name;

                            counter++;
                        }
                    }
                }
            }

            return SW.Statisch.GetHaus(_hausID).GetNameInklPronomen(zustandInProzent) + hausErweiterungen;
        }
        #endregion

        #region GetFehlendeOderVorhandeneHauserweiterungen
        /// <summary>
        /// Diese Funktion dient zum Auslesen aller noch fehlenden (oder vorhandenen) Hauserweiterungen.
        /// </summary>
        /// <param name="fehlendeErweiterungen">Sollen nur fehlende (true) oder nur vorhandene Hauserweiterungen zurückgegeben werden?</param>
        /// <returns>Alle noch fehlenden (oder vorhandenen) Hauserweiterungen dieses Wohnsitzes anhand der HausID</returns>
        public List<HausErweiterung> GetFehlendeOderVorhandeneHauserweiterungen(bool fehlendeErweiterungen = true)
        {
            List<HausErweiterung> alleHausErweiterungen = SW.Statisch.GetHaus(_hausID).HausErweiterungen;
            List<HausErweiterung> fehlendeAlleHausErweiterungen = new List<HausErweiterung>();

            if (alleHausErweiterungen != null && alleHausErweiterungen.Count > 0)
            {
                bool gueltigeErweiterung = true;

                foreach (HausErweiterung oErweiterung in alleHausErweiterungen)
                {
                    gueltigeErweiterung = true;

                    #region Prüfen, ob die Erweiterung bereits für den Wohnsitz vorhanden ist
                    if (HausErweiterungen != null && HausErweiterungen.Count > 0)
                    {
                        foreach (int iVorhandeneErweiterungID in HausErweiterungen)
                        {
                            if (iVorhandeneErweiterungID == oErweiterung.HausErweiterungID)
                            {
                                gueltigeErweiterung = false;
                                break;
                            }
                        }
                    }
                    #endregion

                    if (gueltigeErweiterung && fehlendeErweiterungen)
                        fehlendeAlleHausErweiterungen.Add(oErweiterung);
                    else if (!gueltigeErweiterung && !fehlendeErweiterungen)
                        fehlendeAlleHausErweiterungen.Add(oErweiterung);
                }
            }

            return fehlendeAlleHausErweiterungen;
        }
        #endregion

        #region GetAktuellerWert
        /// <summary>
        /// Dient zur Ermittlung des aktuellen Werts des Wohnsitzes inkl. Erweiterungen und unter Berücksichtigung des Zustandes.
        /// </summary>
        /// <returns>Aktueller Wert</returns>
        public int GetAktuellerWert()
        {
            int aktuellerWert = SW.Statisch.GetHaus(_hausID).Kaufpreis;

            aktuellerWert += GetKaufpreisAllerErweiterungen();
            aktuellerWert = Convert.ToInt32(aktuellerWert * _wertverlustFaktor);

            if (ZustandInProzent < 100)  // Wertverlust durch Zustand?
                aktuellerWert -= Convert.ToInt32((aktuellerWert * _wertverlustFaktor) - ((ZustandInProzent * 0.01) * (aktuellerWert * _wertverlustFaktor)));

            return aktuellerWert;
        }
        #endregion

        #region GetAnsehensbonus
        /// <summary>
        /// Dient zur Ermittlung des Ansehensbonus des Wohnsitzes inkl. Erweiterungen und unter Berücksichtigung des Zustandes.
        /// </summary>
        /// <returns>Aktueller Ansehensbonus</returns>
        public int GetAnsehensbonus()
        {
            int ansehensbonus = SW.Statisch.GetHaus(_hausID).Ansehensbonus;
            ansehensbonus += GetAnsehensbonusAllerErweiterungen();

            if (ZustandInProzent < 100 && ansehensbonus > 0)  // Ansehensverlust durch Zustand (höchster Ansehensverlust entspricht dWertverlustFaktor) 
            {
                ansehensbonus -= Convert.ToInt32((1 - ZustandInProzent * 0.01) * (ansehensbonus * _wertverlustFaktor));
            }

            return ansehensbonus;
        }
        #endregion

        #region GetGesundheitsbonus
        /// <summary>
        /// Dient zur Ermittlung des Gesundheitsbonus des Wohnsitzes inkl. Erweiterungen und unter Berücksichtigung des Zustandes.
        /// </summary>
        /// <returns>Aktueller Gesundheitsbonus</returns>
        public int GetGesundheitsbonus()
        {
            int gesundheitsbonus = GetGesundheitsbonusAllerErweiterungen();

            if (ZustandInProzent < 100 && gesundheitsbonus > 0)  // Gesundheitsverlust durch Zustand?
                gesundheitsbonus -= Convert.ToInt32((gesundheitsbonus * _wertverlustFaktor) - ((ZustandInProzent * 0.01) * (gesundheitsbonus * _wertverlustFaktor)));

            return gesundheitsbonus;
        }
        #endregion


        #region GetKaufpreisAllerErweiterungen
        /// <summary>
        /// Dient zur Ermittlung des Gesamtpreises aller vorhandenen Erweiterungen.
        /// </summary>
        /// <returns>Kaufpreis aller vorhandenen Erweiterungen in Summe</returns>
        private int GetKaufpreisAllerErweiterungen()
        {
            int kaufpreis = 0;

            foreach (HausErweiterung oErweiterung in GetFehlendeOderVorhandeneHauserweiterungen(false))
                kaufpreis += oErweiterung.Kaufpreis;

            return kaufpreis;
        }
        #endregion

        #region GetAnsehensbonusAllerErweiterungen
        /// <summary>
        /// Dient zur Ermittlung des Ansehensbonus aller vorhandenen Erweiterungen.
        /// </summary>
        /// <returns>Ansehensbonus aller vorhandenen Erweiterungen in Summe</returns>
        private int GetAnsehensbonusAllerErweiterungen()
        {
            int ansehensbonus = 0;

            foreach (HausErweiterung oErweiterung in GetFehlendeOderVorhandeneHauserweiterungen(false))
                ansehensbonus += oErweiterung.Ansehensbonus;

            return ansehensbonus;
        }
        #endregion

        #region GetGesundheitsbonusAllerErweiterungen
        /// <summary>
        /// Dient zur Ermittlung des Gesundheitsbonus aller vorhandenen Erweiterungen.
        /// </summary>
        /// <returns>Gesundheitsbonus aller vorhandenen Erweiterungen in Summe</returns>
        private int GetGesundheitsbonusAllerErweiterungen()
        {
            int gesundheitsbonus = 0;

            foreach (HausErweiterung oErweiterung in GetFehlendeOderVorhandeneHauserweiterungen(false))
                gesundheitsbonus += oErweiterung.Gesundheitsbonus;

            return gesundheitsbonus;
        }
        #endregion


        #region Properties

        /// <summary>
        /// ZustandInProzent: Gibt an, wie gut der Zustand des Wohnsitzes in Prozent ist (von 0 - 100).
        /// </summary>
        public int ZustandInProzent
        {
            get
            {
                return _zustandInProzent;
            }

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
        /// InDiesemJahrRenovieren: Dient zur Zwischenspeicherung und gibt an, ob der Wohnsitz in diesem Jahr renoviert werden soll. Wird bei Rundenende abfragt.
        /// </summary>
        public bool InDiesemJahrRenovieren { get; set; }

        /// <summary>
        /// HausErweiterungen: Stellt eine Liste der vorhandenen HausErweiterungs-IDs dar.
        /// </summary>
        public List<int> HausErweiterungen { get; set; }

        #endregion
    }
}