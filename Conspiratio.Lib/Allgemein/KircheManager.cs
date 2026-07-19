using System;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die kirchlichen Tätigkeiten aus dem alten WinForms-Client: den Kirchgang (Ablass kaufen,
    /// beichten, Waisenkind adoptieren), das Konvertieren und Austreten sowie den Beitritt zu einer
    /// Konfession für Konfessionslose. Datenabfragen und Mutationen sind von der UI getrennt – die
    /// Bestätigungen und Meldungen zeigt der aufrufende View awaitbar an.
    /// </summary>
    public class KircheManager
    {
        private static Conspiratio.Lib.Gameplay.Personen.HumSpieler AktHum => SW.Dynamisch.GetAktHum();

        #region Konfession

        [PublicAPI]
        public bool IstKonfessionslos()
        {
            return AktHum.GetReligion() == SW.Statisch.GetRelFreiID();
        }

        [PublicAPI]
        public string GetReligionsName()
        {
            return SW.Statisch.GetReligionsNamenX(AktHum.GetReligion());
        }

        /// <summary>
        /// Ein Konfessionsloser nimmt eine Konfession an (katholisch oder evangelisch).
        /// </summary>
        [PublicAPI]
        public void NimmReligionAn(bool katholisch)
        {
            AktHum.SetReligion(katholisch ? SW.Statisch.GetRelKathID() : SW.Statisch.GetRelEvanID());
        }

        #endregion

        [PublicAPI]
        public bool KannBezahlen(int preis)
        {
            return AktHum.GetTaler() >= preis;
        }

        #region Ablass kaufen

        /// <summary>
        /// Berechnet die Deliktpunkte neu und liefert die Kosten für den Ablass (Deliktpunkte × Preis je Punkt).
        /// 0 bedeutet, dass keine Sünden vorliegen.
        /// </summary>
        [PublicAPI]
        public int GetAblassKosten()
        {
            SW.Dynamisch.DeliktpunkteBerechnen();
            return AktHum.GetDeliktpunkte() * SW.Statisch.GetDeliktpunktPreis();
        }

        /// <summary>
        /// Kauft den Ablass: zieht die Kosten ab und halbiert die Sünden (Deliktpunkte).
        /// </summary>
        [PublicAPI]
        public void KaufeAblass(int kosten)
        {
            AktHum.ErhoeheTaler(-kosten);
            AktHum.GetSpielerStatistik().KgekaufteAblaesse++;
            AktHum.HalbiereDelikte();
        }

        #endregion

        #region Beichten

        [PublicAPI]
        public bool HatSchonGebeichtet()
        {
            return AktHum.GetGebeichtet();
        }

        /// <summary>
        /// Die aktuellen Deliktpunkte (nach Neuberechnung).
        /// </summary>
        [PublicAPI]
        public int GetDeliktpunkte()
        {
            SW.Dynamisch.DeliktpunkteBerechnen();
            return AktHum.GetDeliktpunkte();
        }

        /// <summary>
        /// Beichtet: reduziert die Sünden um einen Deliktpunkt und merkt vor, dass dieses Jahr gebeichtet wurde.
        /// </summary>
        [PublicAPI]
        public void Beichte()
        {
            AktHum.GetSpielerStatistik().KabgelegteBeichten++;
            AktHum.SetDeliktpunkte(AktHum.GetDeliktpunkte() - 1);
            AktHum.SetGebeichtet(true);
        }

        #endregion

        #region Waisenkind adoptieren

        [PublicAPI]
        public bool DarfWaisenkindAdoptieren()
        {
            return AktHum.DarfWaisenkindAdoptieren();
        }

        [PublicAPI]
        public int GetWaisenkindPreis()
        {
            return AktHum.ErmittlePreisWaisenkindAdoptieren(SW.Dynamisch.GetAktiverSpieler());
        }

        /// <summary>
        /// Adoptiert ein Waisenkind: zieht den Preis ab, senkt das Ansehen und legt ein zufällig benanntes Kind
        /// (Alter 1) an. (Entspricht HumSpieler.WaisenkindAdoptieren ohne dessen fire-and-forget-Meldungen.)
        /// </summary>
        /// <returns>Der Name des adoptierten Kindes.</returns>
        [PublicAPI]
        public string AdoptiereWaisenkind(int preis)
        {
            AktHum.ErhoeheTaler(-preis);
            AktHum.ErhoehePermaAnsehen(-100);

            bool maennlich = SW.Statisch.Rnd.Next(0, 2) == 0;
            int index = maennlich
                ? SW.Statisch.Rnd.Next(SW.Statisch.GetMinKIID(), SW.Statisch.GetMaennerFrauenGrenze())
                : SW.Statisch.Rnd.Next(SW.Statisch.GetMaennerFrauenGrenze(), SW.Statisch.GetMaxKIID());

            string name = SW.Statisch.GetKINameX(index);

            AktHum.SetKindX(AktHum.GetEmptyKindSlot(), maennlich, name, 1);
            SW.Dynamisch.PrivilegienAktualisieren();

            return name;
        }

        #endregion

        #region Konvertieren

        /// <summary>
        /// Die Konvertierungskosten: mindestens der Basiswert, immer aber 5 % des Gesamtvermögens.
        /// </summary>
        [PublicAPI]
        public int GetKonvertierkosten()
        {
            int basis = SW.Statisch.GetKonvertierkosten();
            int gesamtvermoegen = AktHum.GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler());

            return Math.Max(basis, Convert.ToInt32(gesamtvermoegen * 0.05));
        }

        [PublicAPI]
        public string GetNaechsteReligionName()
        {
            return SW.Statisch.GetReligionsNamenX(GetNaechsteReligionId());
        }

        private static int GetNaechsteReligionId()
        {
            int neu = AktHum.GetReligion() + 1;

            if (neu >= SW.Statisch.GetRelMaxID())
                neu = SW.Statisch.GetRelMinID() + 1;

            return neu;
        }

        /// <summary>
        /// Wechselt zur nächsten Konfession und zieht die Kosten ab.
        /// </summary>
        [PublicAPI]
        public void Konvertiere(int kosten)
        {
            AktHum.SetReligion(GetNaechsteReligionId());
            AktHum.GetSpielerStatistik().KKonvertierungen++;
            AktHum.ErhoeheTaler(-kosten);
        }

        #endregion

        #region Austreten

        /// <summary>
        /// Die Austrittskosten: mindestens der Basiswert, immer aber 20 % des Gesamtvermögens.
        /// </summary>
        [PublicAPI]
        public int GetAustrittskosten()
        {
            int basis = SW.Statisch.GetAustrittskosten();
            int gesamtvermoegen = AktHum.GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler());

            return Math.Max(basis, Convert.ToInt32(gesamtvermoegen * 0.2));
        }

        /// <summary>
        /// Tritt aus der Kirche aus (wird konfessionslos) und zieht die Kosten ab. Ist der Austritt per Gesetz
        /// verboten, gibt es einen Deliktpunkt.
        /// </summary>
        [PublicAPI]
        public void TritteAus(int kosten)
        {
            if (SW.Dynamisch.GetGesetzX(40) != 0)
                AktHum.ErhoeheGesetzXUmEins(40);

            AktHum.SetReligion(SW.Statisch.GetRelFreiID());
            AktHum.ErhoeheTaler(-kosten);
        }

        #endregion
    }
}
