using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Privilegien;
using Conspiratio.Lib.Gameplay.Spielwelt;
using Conspiratio.Lib.Gameplay.Wohnsitz;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die Anwesen-Verwaltung aus dem alten WinForms-Client (HausBauen/HausWaehlen/HausErweiterungen):
    /// Wohnsitze bauen und umbauen, renovieren, erweitern und verkaufen. Das Herunterkommen des Zustands,
    /// der Baufortschritt und der Abschluss einer Renovierung werden bei Rundenende im ZugNachrichtenManager verarbeitet.
    /// </summary>
    public class AnwesenManager
    {
        private static SpHatHaus GetHaus(int stadtId)
        {
            return SW.Dynamisch.GetAktHum().GetSpielerHatHausVonStadtAnArraystelle(stadtId);
        }

        /// <summary>
        /// Der Kostenreduktionsfaktor durch das Sparplan-Privileg (Privileg 15), sonst 1.
        /// </summary>
        private static double GetReduzierungsFaktor()
        {
            if (SW.Dynamisch.GetAktHum().CheckPrivilegX(15))
                return ((PrivSparplan)SW.Statisch.GetPrivX(15)).FaktorReduzierung;

            return 1;
        }

        #region Zustand des Anwesens

        [PublicAPI]
        public bool HatHaus(int stadtId)
        {
            return GetHaus(stadtId).GetHausID() != 0;
        }

        [PublicAPI]
        public bool IstFertig(int stadtId)
        {
            return GetHaus(stadtId).GetRestlicheBauzeit() == 0;
        }

        [PublicAPI]
        public int GetRestlicheBauzeit(int stadtId)
        {
            return GetHaus(stadtId).GetRestlicheBauzeit();
        }

        [PublicAPI]
        public int GetHausID(int stadtId)
        {
            return GetHaus(stadtId).GetHausID();
        }

        [PublicAPI]
        public int GetZustandInProzent(int stadtId)
        {
            return GetHaus(stadtId).ZustandInProzent;
        }

        /// <summary>
        /// Der Name des Wohnsitzes inkl. Pronomen, optional mit Zustand und Erweiterungen (z. B. "Euer prächtiges Bürgerhaus mit Garten").
        /// </summary>
        [PublicAPI]
        public string GetNameInklPronomen(int stadtId, bool mitZustand = true, bool mitErweiterungen = true)
        {
            return GetHaus(stadtId).GetNameInklPronomen(mitZustand, mitErweiterungen);
        }

        /// <summary>
        /// Der Bildname des Hauses (z. B. für die Anzeige des Anwesens), sonst leer.
        /// </summary>
        [PublicAPI]
        public string GetBildname(int stadtId)
        {
            int hausId = GetHaus(stadtId).GetHausID();
            return hausId == 0 ? "" : SW.Statisch.GetHaus(hausId).Bildname;
        }

        #endregion

        /// <summary>
        /// Prüft ohne Nebenwirkung, ob der aktive Spieler den Preis bezahlen kann.
        /// </summary>
        [PublicAPI]
        public bool KannBezahlen(int preis)
        {
            return SW.Dynamisch.GetAktHum().GetTaler() >= preis;
        }

        #region Bauen und Umbauen

        /// <summary>
        /// Liefert alle baubaren Wohnsitze mit Preis. Beim Umbauen (modus 1) wird der halbe aktuelle Wert
        /// des bestehenden Wohnsitzes als Fixpreisreduzierung abgezogen.
        /// </summary>
        /// <param name="modus">0 = neu bauen, 1 = umbauen</param>
        [PublicAPI]
        public List<HausAngebot> GetBaubareHaeuser(int stadtId, int modus)
        {
            var angebote = new List<HausAngebot>();
            double faktor = GetReduzierungsFaktor();

            int fixpreisreduzierung = 0;

            if (modus == 1)
                fixpreisreduzierung = GetHaus(stadtId).GetAktuellerWert() / 2;

            for (int hausId = 1; hausId < SW.Statisch.GetMaxHausID(); hausId++)
            {
                int preis = Convert.ToInt32(SW.Statisch.GetHaus(hausId).Kaufpreis * faktor - fixpreisreduzierung);
                angebote.Add(new HausAngebot(hausId, SW.Statisch.GetHaus(hausId).Name, preis));
            }

            return angebote;
        }

        /// <summary>
        /// True, wenn in dieser Stadt bereits genau dieser Wohnsitz steht (Umbauen auf denselben Typ ist sinnlos).
        /// </summary>
        [PublicAPI]
        public bool IstBereitsVorhanden(int stadtId, int hausId)
        {
            return GetHaus(stadtId).GetHausID() == hausId;
        }

        /// <summary>
        /// Zieht den Preis ab und errichtet den Wohnsitz (Bauzeit gemäß Haustyp, Zustand 100 %, ohne Erweiterungen).
        /// </summary>
        [PublicAPI]
        public void BaueHaus(int stadtId, HausAngebot angebot)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var haus = GetHaus(stadtId);

            spieler.ErhoeheTaler(-angebot.Preis);
            haus.SetHausID(angebot.HausId);
            haus.SetStadtID(stadtId);
            haus.SetRestlicheBauzeit(SW.Statisch.GetHaus(angebot.HausId).Bauzeit);
            haus.HausErweiterungen = null;
            haus.ZustandInProzent = 100;  // Falls an diesem Slot vorher ein heruntergekommener Wohnsitz stand

            spieler.GetSpielerStatistik().SoGebauteHaeuser++;  // Statistik (Issue #19-Erweiterung)
        }

        #endregion

        #region Renovieren

        [PublicAPI]
        public bool BenoetigtRenovierung(int stadtId)
        {
            return GetHaus(stadtId).ZustandInProzent != 100;
        }

        [PublicAPI]
        public bool RenovierungBereitsBeauftragt(int stadtId)
        {
            return GetHaus(stadtId).InDiesemJahrRenovieren;
        }

        /// <summary>
        /// Der Renovierungspreis richtet sich nach dem aktuellen Wert und dem fehlenden Zustand
        /// (voller Wert bei 0 %, nichts bei 100 %), reduziert um den Sparplan-Faktor.
        /// </summary>
        [PublicAPI]
        public int GetRenovierungsPreis(int stadtId)
        {
            var haus = GetHaus(stadtId);
            double faktor = GetReduzierungsFaktor();

            return Convert.ToInt32(haus.GetAktuellerWert() * ((100 - haus.ZustandInProzent) * 0.01) * faktor);
        }

        /// <summary>
        /// Beauftragt die Renovierung: Preis abziehen und für Rundenende vormerken (dort wird der Zustand auf 100 % gesetzt).
        /// </summary>
        [PublicAPI]
        public void Renoviere(int stadtId)
        {
            SW.Dynamisch.GetAktHum().ErhoeheTaler(-GetRenovierungsPreis(stadtId));
            GetHaus(stadtId).InDiesemJahrRenovieren = true;
        }

        #endregion

        #region Erweitern

        [PublicAPI]
        public bool HatVerfuegbareErweiterungen(int stadtId)
        {
            return GetHaus(stadtId).GetFehlendeOderVorhandeneHauserweiterungen().Count > 0;
        }

        /// <summary>
        /// Liefert alle noch fehlenden Hauserweiterungen des Wohnsitzes mit Preis (reduziert um den Sparplan-Faktor).
        /// </summary>
        [PublicAPI]
        public List<ErweiterungsAngebot> GetFehlendeErweiterungen(int stadtId)
        {
            var angebote = new List<ErweiterungsAngebot>();
            double faktor = GetReduzierungsFaktor();

            foreach (HausErweiterung erweiterung in GetHaus(stadtId).GetFehlendeOderVorhandeneHauserweiterungen())
            {
                int preis = Convert.ToInt32(erweiterung.Kaufpreis * faktor);
                angebote.Add(new ErweiterungsAngebot(erweiterung.HausErweiterungID, erweiterung.NameFuerKauf, preis));
            }

            return angebote;
        }

        /// <summary>
        /// Zieht den Preis ab und fügt die Erweiterung dem Wohnsitz hinzu.
        /// </summary>
        [PublicAPI]
        public void BaueErweiterung(int stadtId, ErweiterungsAngebot angebot)
        {
            var haus = GetHaus(stadtId);
            SW.Dynamisch.GetAktHum().ErhoeheTaler(-angebot.Preis);

            if (haus.HausErweiterungen == null)
                haus.HausErweiterungen = new List<int>();

            haus.HausErweiterungen.Add(angebot.ErweiterungId);
        }

        #endregion

        #region Verkaufen

        /// <summary>
        /// Ein Wohnsitz kann nur verkauft werden, wenn der Spieler in dieser Stadt keine Werkstätte mehr besitzt.
        /// </summary>
        [PublicAPI]
        public bool KannVerkaufen(int stadtId)
        {
            for (int i = 1; i <= SW.Statisch.GetMaxWerkstaettenProStadt(); i++)
            {
                if (SW.Dynamisch.GetAktHum().GetSpielerHatInStadtXWerkstaettenY(i, stadtId).GetEnabled())
                    return false;
            }

            return true;
        }

        [PublicAPI]
        public int GetVerkaufswert(int stadtId)
        {
            return GetHaus(stadtId).GetAktuellerWert();
        }

        /// <summary>
        /// Verkauft den Wohnsitz zum aktuellen Wert und entfernt ihn (inkl. Erweiterungen).
        /// </summary>
        [PublicAPI]
        public void VerkaufeHaus(int stadtId)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var haus = GetHaus(stadtId);

            spieler.ErhoeheTaler(haus.GetAktuellerWert());
            haus.SetHausID(0);
            haus.SetStadtID(0);
            haus.HausErweiterungen = null;
        }

        #endregion
    }

    /// <summary>
    /// Ein baubarer Wohnsitz mit seinem (ggf. reduzierten) Preis.
    /// </summary>
    public class HausAngebot
    {
        public HausAngebot(int hausId, string name, int preis)
        {
            HausId = hausId;
            Name = name;
            Preis = preis;
        }

        public int HausId { get; }

        public string Name { get; }

        public int Preis { get; }
    }

    /// <summary>
    /// Eine baubare Hauserweiterung mit ihrem (ggf. reduzierten) Preis.
    /// </summary>
    public class ErweiterungsAngebot
    {
        public ErweiterungsAngebot(int erweiterungId, string name, int preis)
        {
            ErweiterungId = erweiterungId;
            Name = name;
            Preis = preis;
        }

        public int ErweiterungId { get; }

        public string Name { get; }

        public int Preis { get; }
    }
}
