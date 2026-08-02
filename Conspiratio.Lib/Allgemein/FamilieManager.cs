using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Kirche;
using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt das Familiensystem aus dem alten WinForms-Client: die Partnersuche über die Kupplerin,
    /// die jährliche Brautwerbung mit Geschenken, die Hochzeit, den Nachwuchs (Geburt und Kindestod)
    /// sowie das Testament (Erbe bestimmen und Erbfolge beim Tod). Die zugrundeliegende Spiellogik
    /// (Kupplerin, VerheirateXundY, TestamentVollstrecken) liegt bereits in der Lib.
    /// </summary>
    public class FamilieManager
    {
        private static Conspiratio.Lib.Gameplay.Personen.HumSpieler AktHum => SW.Dynamisch.GetAktHum();

        #region Status

        [PublicAPI]
        public bool IstVerheiratet()
        {
            return AktHum.GetVerheiratet() != 0;
        }

        [PublicAPI]
        public string GetEhepartnerName()
        {
            return IstVerheiratet() ? SW.Dynamisch.GetKIwithID(AktHum.GetVerheiratet()).GetKompletterName() : "";
        }

        [PublicAPI]
        public bool WirbtGerade()
        {
            return AktHum.WirbtUmSpielerID != 0;
        }

        [PublicAPI]
        public string GetUmworbenerName()
        {
            return WirbtGerade() ? SW.Dynamisch.GetKIwithID(AktHum.WirbtUmSpielerID).GetKompletterName() : "";
        }

        #endregion

        #region Kupplerin / Partnersuche

        /// <summary>
        /// Prüft, ob der Spieler eine Partnersuche beginnen kann (nicht verheiratet, wirbt nicht bereits).
        /// </summary>
        [PublicAPI]
        public bool KannPartnerSuchen(out string hinweis)
        {
            if (IstVerheiratet())
            {
                hinweis = "Ihr seid bereits verheiratet. Nehmt Euch doch eine Mätresse";
                return false;
            }

            if (WirbtGerade())
            {
                hinweis = "Ihr werbt bereits um " + GetUmworbenerName();
                return false;
            }

            hinweis = "";
            return true;
        }

        /// <summary>
        /// Ermittelt den von der Kupplerin vorgeschlagenen optimalen Partner (oder null, wenn keiner passt).
        /// </summary>
        [PublicAPI]
        public KupplerinVorschlag ErmittleKupplerinVorschlag()
        {
            int partnerId = Kupplerin.ErmittleOptimalenPartnerFuerSpieler(SW.Dynamisch.GetAktiverSpieler());

            if (partnerId == 0)
                return null;

            int preis = Kupplerin.BerechnePreisFuerKupplerin(partnerId);
            return new KupplerinVorschlag(partnerId, SW.Dynamisch.GetKIwithID(partnerId).GetKompletterName(), preis);
        }

        [PublicAPI]
        public bool KannBezahlen(int preis)
        {
            return AktHum.GetTaler() >= preis;
        }

        /// <summary>
        /// Beginnt die Werbung um den vorgeschlagenen Partner: Preis abziehen, Verliebtheit auf 50 setzen.
        /// (Entspricht Kupplerin.BeginneWerbungUmOptimalenPartner ohne dessen fire-and-forget-Meldung – die
        /// zeigt der aufrufende View selbst awaitbar an.)
        /// </summary>
        [PublicAPI]
        public void BeginneWerbung(KupplerinVorschlag vorschlag)
        {
            AktHum.WirbtUmSpielerID = vorschlag.PartnerId;
            AktHum.ErhoeheTaler(-vorschlag.Preis);
            SW.Dynamisch.GetKIwithID(vorschlag.PartnerId).ErhoeheVerliebt(50);
        }

        #endregion

        #region Brautwerbung (jährliches Ereignis)

        [PublicAPI]
        public bool StehtBrautwerbungAn()
        {
            return WirbtGerade();
        }

        /// <summary>
        /// Erzeugt die drei zufälligen Werbegeschenke (billig, mittelteuer, teuer) samt Preisen für dieses Jahr.
        /// </summary>
        [PublicAPI]
        public GeschenkAuswahl ErstelleGeschenkAuswahl()
        {
            var partner = SW.Dynamisch.GetKIwithID(AktHum.WirbtUmSpielerID);
            int kiTaler = partner.GetTaler();
            int zuschlag = Convert.ToInt32(0.001 * AktHum.GetGesamtVermoegen(SW.Dynamisch.GetAktiverSpieler()));

            int[] geschenkIds = new int[4];
            geschenkIds[1] = SW.Statisch.Rnd.Next(1, SW.Statisch.GetWerbegeschenkGrenzeBillig());
            geschenkIds[2] = SW.Statisch.Rnd.Next(SW.Statisch.GetWerbegeschenkGrenzeBillig(), SW.Statisch.GetWerbegeschenkGrenzeMittelteuer());
            geschenkIds[3] = SW.Statisch.Rnd.Next(SW.Statisch.GetWerbegeschenkGrenzeMittelteuer(), SW.Statisch.GetMaxWerbegeschenke());

            var geschenke = new List<WerbeGeschenk>();

            for (int k = 1; k <= 3; k++)
            {
                var wg = SW.Statisch.GetWerbegeschenk(geschenkIds[k]);
                int preis = Convert.ToInt32(wg.Basispreis + wg.Vermoegensfaktor * kiTaler) + zuschlag;
                geschenke.Add(new WerbeGeschenk(geschenkIds[k], wg.Text, preis));
            }

            return new GeschenkAuswahl(partner.GetKompletterName(), geschenke, SW.Statisch.GetWerbegeschenk(0).Text);
        }

        /// <summary>
        /// Schenkt dem umworbenen Partner das gewählte Geschenk. Wie sehr es gefällt, hängt von den Boni des
        /// Geschenks (Preis, Romantik, Bosheit) und der Bosheit des Partners ab; entsprechend steigt die Verliebtheit.
        /// </summary>
        [PublicAPI]
        public WerbungsErgebnis GibGeschenk(WerbeGeschenk gewaehlt)
        {
            var partner = SW.Dynamisch.GetKIwithID(AktHum.WirbtUmSpielerID);
            var wg = SW.Statisch.GetWerbegeschenk(gewaehlt.GeschenkId);

            int gefallenMin = 0;
            int gefallenMax = 10;
            const int oSchranke = 75;
            const int uSchranke = 25;
            const int oRandomschranke = 5;
            const int uRandomschranke = 5;

            int boniPreis = wg.BonusPreis;
            int boniBoese = wg.BonusBoese;
            int boniRoman = wg.BonusRomantik;
            int bosheit = partner.GetBosheit();

            if (boniBoese > 0)
            {
                if (bosheit > oSchranke)
                    gefallenMin = uRandomschranke;
                else if (bosheit < uSchranke)
                    gefallenMax = oRandomschranke;
            }

            if (boniRoman > 0)
            {
                if (bosheit < uSchranke)
                    gefallenMin = uRandomschranke;
                else if (bosheit > oSchranke)
                    gefallenMax = oRandomschranke;
            }

            int gefallen = SW.Statisch.Rnd.Next(gefallenMin, gefallenMax + 1);
            int plusVerliebt = 6 + Convert.ToInt32((gefallen * (boniPreis + boniBoese + boniRoman)) / 10);

            partner.ErhoeheVerliebt(plusVerliebt);
            AktHum.ErhoeheTaler(-gewaehlt.Preis);

            return new WerbungsErgebnis(partner.GetName() + SW.Statisch.GetWerbereaktion(gefallen), partner.GetVerliebt());
        }

        /// <summary>
        /// Der Spieler macht dieses Jahr kein Geschenk – der Partner fühlt sich vernachlässigt (Verliebtheit −10).
        /// </summary>
        [PublicAPI]
        public WerbungsErgebnis GibKeinGeschenk()
        {
            var partner = SW.Dynamisch.GetKIwithID(AktHum.WirbtUmSpielerID);
            partner.ErhoeheVerliebt(-10);

            return new WerbungsErgebnis(partner.GetName() + " fühlt sich vernachlässigt...", partner.GetVerliebt());
        }

        #endregion

        #region Hochzeit

        /// <summary>
        /// True, wenn der umworbene Partner voll verliebt (100) ist und die Hochzeit ansteht.
        /// </summary>
        [PublicAPI]
        public bool StehtHochzeitAn()
        {
            int brautId = AktHum.WirbtUmSpielerID;
            return brautId != 0 && SW.Dynamisch.GetKIwithID(brautId).GetVerliebt() >= 100;
        }

        /// <summary>
        /// Führt die Hochzeit durch: beendet die Werbung, verheiratet Spieler und Partner (inkl. Titelangleich).
        /// </summary>
        [PublicAPI]
        public HochzeitErgebnis FuehreHochzeitDurch()
        {
            int brautId = AktHum.WirbtUmSpielerID;
            var braut = SW.Dynamisch.GetKIwithID(brautId);

            braut.SetVerliebt(0);
            AktHum.WirbtUmSpielerID = 0;

            string partnerName = braut.GetKompletterName();
            bool partnerMaennlich = braut.GetMaennlich();

            AktHum.GetSpielerStatistik().KHochzeiten++;
            SW.Dynamisch.VerheirateXundY(SW.Dynamisch.GetAktiverSpieler(), brautId);

            return new HochzeitErgebnis(partnerName, partnerMaennlich);
        }

        #endregion

        #region Kinder

        /// <summary>
        /// Prüft (und würfelt), ob der Spieler dieses Jahr ein Kind bekommt: nur mit freiem Kind-Slot,
        /// entweder per Cheat-Flag oder – wenn verheiratet – mit der Zufallschance des Originals.
        /// </summary>
        [PublicAPI]
        public bool StehtGeburtAn()
        {
            if (AktHum.GetEmptyKindSlot() == SW.Statisch.GetMaxKinderAnzahl())
                return false;

            if (AktHum.GetKindBekommen())
                return true;

            return IstVerheiratet() && SW.Statisch.Rnd.Next(0, SW.Statisch.GetChanceFuerKind()) == 0;
        }

        /// <summary>
        /// Würfelt das Geschlecht des Neugeborenen (true = Sohn).
        /// </summary>
        [PublicAPI]
        public bool ErmittleGeburtGeschlecht()
        {
            return SW.Statisch.Rnd.Next(0, 2) == 0;
        }

        /// <summary>
        /// Legt das Kind mit gewähltem Geschlecht und Namen an und setzt das Kind-bekommen-Flag zurück.
        /// </summary>
        [PublicAPI]
        public void FuehreGeburtDurch(bool maennlich, string name)
        {
            AktHum.SetKindX(AktHum.GetEmptyKindSlot(), maennlich, name);
            AktHum.SetKindBekommen(false);
            AktHum.GetSpielerStatistik().SogezeugteKinder++;  // Statistik (Issue #19)
        }

        /// <summary>
        /// Prüft für jedes Kind den frühen Kindestod (Zufallschance). Ein gestorbenes Kind wird entfernt;
        /// war es der Erbe, fällt das Erbe zurück ans Erzbistum.
        /// </summary>
        /// <returns>Je gestorbenem Kind eine Meldung; die erste Meldung nennt ggf. den Erbwechsel.</returns>
        [PublicAPI]
        public List<string> PruefeKindestode()
        {
            var meldungen = new List<string>();

            // Hinweis: Das Original verwendet hier versehentlich GetMinKIID() als Startindex, wodurch die
            // Schleife nie läuft (Kinder sterben nie). Hier wird der korrekte Kind-Slot-Bereich genutzt.
            for (int i = SW.Statisch.GetMinKindSlotNr(); i < SW.Statisch.GetMaxKinderAnzahl(); i++)
            {
                var kind = AktHum.GetKindX(i);

                if (kind.GetKindName() == "")
                    continue;

                if (SW.Statisch.Rnd.Next(0, SW.Statisch.GetChanceFuerKindStirbt()) != 0)
                    continue;

                if (AktHum.GetErbeSpielerID() == i)
                {
                    AktHum.SetErbeSpielerID(0);
                    meldungen.Add("Da Euer im Testament erwähnter Erbe verstorben ist, ist aktuell das Erzbistum der Erbe Eures gesamten Vermögens!");
                }

                string sohnTochter = kind.GetMaennlich() ? "Euer Sohn " : "Eure Tochter ";
                meldungen.Add(sohnTochter + kind.GetKindName() + " erlitt im Alter von " + kind.GetAlter() + " Jahren den frühen Kindestod.");

                kind.SetName("");
            }

            return meldungen;
        }

        [PublicAPI]
        public List<KindInfo> GetKinder()
        {
            var kinder = new List<KindInfo>();

            for (int i = SW.Statisch.GetMinKindSlotNr(); i < SW.Statisch.GetMaxKinderAnzahl(); i++)
            {
                var kind = AktHum.GetKindX(i);

                if (kind.GetKindName() != "")
                    kinder.Add(new KindInfo(i, kind.GetKindName(), kind.GetMaennlich(), kind.GetAlter()));
            }

            return kinder;
        }

        #endregion

        #region Testament

        /// <summary>
        /// Die möglichen Erben: das Erzbistum (0), der Ehepartner und alle lebenden Kinder.
        /// </summary>
        [PublicAPI]
        public List<ErbeOption> GetErbeOptionen()
        {
            var optionen = new List<ErbeOption> { new ErbeOption(0, GetErbeBezeichnung(0)) };

            if (IstVerheiratet())
                optionen.Add(new ErbeOption(AktHum.GetVerheiratet(), GetErbeBezeichnung(AktHum.GetVerheiratet())));

            foreach (var kind in GetKinder())
                optionen.Add(new ErbeOption(kind.Slot, GetErbeBezeichnung(kind.Slot)));

            return optionen;
        }

        [PublicAPI]
        public int GetAktuellerErbeId()
        {
            return AktHum.GetErbeSpielerID();
        }

        [PublicAPI]
        public void SetzeErbe(int erbeId)
        {
            AktHum.SetErbeSpielerID(erbeId);
        }

        /// <summary>
        /// Die Testamentsformulierung für einen Erben (Erzbistum, Gatte/Gattin oder Sohn/Tochter).
        /// </summary>
        [PublicAPI]
        public string GetErbeBezeichnung(int erbeId)
        {
            if (erbeId == 0)
                return "Das Erzbistum";

            if (erbeId >= SW.Statisch.GetMinKIID())
            {
                var gatte = SW.Dynamisch.GetKIwithID(erbeId);
                return (gatte.GetMaennlich() ? "Meinen Gatten " : "Meine Gattin ") + gatte.GetName();
            }

            var kind = AktHum.GetKindX(erbeId);
            return (kind.GetMaennlich() ? "Meinen Sohn " : "Meine Tochter ") + kind.GetKindName();
        }

        /// <summary>
        /// Vollstreckt das Testament beim Tod des Spielers. Ohne Erben (Erzbistum) scheidet der Spieler aus
        /// (sein Amt und seine Wahlteilnahme werden freigegeben); mit Erben übernimmt dieser die Identität und
        /// führt die Dynastie fort. Das Amt wird dabei nicht vererbt: Es wird – wie bei jedem Todesfall –
        /// freigegeben und steht damit im nächsten Jahr zur Wahl.
        /// </summary>
        [PublicAPI]
        public TestamentErgebnis FuehreTestamentAus()
        {
            int erbe = AktHum.GetErbeSpielerID();
            string bezeichnung = GetErbeBezeichnung(erbe);

            if (erbe == 0)
            {
                bool spielVorbei = SW.Dynamisch.EntferneAktivenSpielerAusDemSpiel();
                return new TestamentErgebnis(false, spielVorbei, bezeichnung);
            }

            // Das Amt des Verstorbenen wird nicht vererbt: vor der Erbübernahme freigeben (erzeugt eine Wahl).
            if (AktHum.GetAmtID() != 0)
                SW.Dynamisch.AmtVonXfreigeben(SW.Dynamisch.GetAktiverSpieler());

            SW.Dynamisch.TestamentVollstrecken();

            // Erbt der Ehepartner, übernimmt der Erbe in TestamentVollstrecken dessen Amt – auch dieses wird
            // freigegeben, damit der Erbe ohne geerbtes Amt in die Dynastie startet.
            if (AktHum.GetAmtID() != 0)
                SW.Dynamisch.AmtVonXfreigeben(SW.Dynamisch.GetAktiverSpieler());

            return new TestamentErgebnis(true, false, bezeichnung);
        }

        #endregion

        #region KI-Hochzeiten (Rundenende)

        /// <summary>
        /// Verheiratet am Jahresende die KI-Spieler untereinander (wie im Original).
        /// </summary>
        [PublicAPI]
        public void VerheirateKis()
        {
            SW.Dynamisch.KIVerheiraten();
        }

        #endregion
    }

    /// <summary>Ein von der Kupplerin vorgeschlagener Partner samt Preis.</summary>
    public class KupplerinVorschlag
    {
        public KupplerinVorschlag(int partnerId, string partnerName, int preis)
        {
            PartnerId = partnerId;
            PartnerName = partnerName;
            Preis = preis;
        }

        public int PartnerId { get; }

        public string PartnerName { get; }

        public int Preis { get; }
    }

    /// <summary>Ein einzelnes Werbegeschenk zur Auswahl.</summary>
    public class WerbeGeschenk
    {
        public WerbeGeschenk(int geschenkId, string text, int preis)
        {
            GeschenkId = geschenkId;
            Text = text;
            Preis = preis;
        }

        public int GeschenkId { get; }

        public string Text { get; }

        public int Preis { get; }
    }

    /// <summary>Die Geschenkauswahl eines Werbungsjahres.</summary>
    public class GeschenkAuswahl
    {
        public GeschenkAuswahl(string partnerName, List<WerbeGeschenk> geschenke, string keinGeschenkText)
        {
            PartnerName = partnerName;
            Geschenke = geschenke;
            KeinGeschenkText = keinGeschenkText;
        }

        public string PartnerName { get; }

        public List<WerbeGeschenk> Geschenke { get; }

        public string KeinGeschenkText { get; }
    }

    /// <summary>Das Ergebnis einer Geschenkübergabe.</summary>
    public class WerbungsErgebnis
    {
        public WerbungsErgebnis(string reaktionsText, int verliebtheit)
        {
            ReaktionsText = reaktionsText;
            Verliebtheit = verliebtheit;
        }

        public string ReaktionsText { get; }

        /// <summary>Die Verliebtheit des Partners nach der Übergabe (Ziel: 100).</summary>
        public int Verliebtheit { get; }
    }

    /// <summary>Das Ergebnis einer Hochzeit.</summary>
    public class HochzeitErgebnis
    {
        public HochzeitErgebnis(string partnerName, bool partnerMaennlich)
        {
            PartnerName = partnerName;
            PartnerMaennlich = partnerMaennlich;
        }

        public string PartnerName { get; }

        public bool PartnerMaennlich { get; }
    }

    /// <summary>Ein Kind des Spielers.</summary>
    public class KindInfo
    {
        public KindInfo(int slot, string name, bool maennlich, int alter)
        {
            Slot = slot;
            Name = name;
            Maennlich = maennlich;
            Alter = alter;
        }

        public int Slot { get; }

        public string Name { get; }

        public bool Maennlich { get; }

        public int Alter { get; }
    }

    /// <summary>Eine Erbe-Option im Testament.</summary>
    public class ErbeOption
    {
        public ErbeOption(int erbeId, string bezeichnung)
        {
            ErbeId = erbeId;
            Bezeichnung = bezeichnung;
        }

        public int ErbeId { get; }

        public string Bezeichnung { get; }
    }

    /// <summary>Das Ergebnis der Testamentsvollstreckung beim Tod.</summary>
    public class TestamentErgebnis
    {
        public TestamentErgebnis(bool erbeUebernahm, bool spielVorbei, string erbeBezeichnung)
        {
            ErbeUebernahm = erbeUebernahm;
            SpielVorbei = spielVorbei;
            ErbeBezeichnung = erbeBezeichnung;
        }

        /// <summary>True, wenn ein Erbe die Dynastie fortführt; false, wenn der Spieler ausscheidet.</summary>
        public bool ErbeUebernahm { get; }

        /// <summary>True, wenn danach kein menschlicher Spieler mehr im Spiel ist.</summary>
        public bool SpielVorbei { get; }

        public string ErbeBezeichnung { get; }
    }
}
