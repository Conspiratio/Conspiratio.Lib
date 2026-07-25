using System.Collections.Generic;

using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Stellt die Spielerstatistik für die Anzeige bereit (Migration von FormStatistik): liefert die
    /// beteiligten menschlichen Spieler samt Banner sowie – je Spieler – die Statistikwerte in zwei
    /// Spalten (Beschriftung und formatierter Wert), in derselben Reihenfolge wie im Original.
    /// </summary>
    public class StatistikManager
    {
        /// <summary>Die IDs der menschlichen Spieler (mit vergebenem Namen), zwischen denen umgeschaltet werden kann.</summary>
        public IReadOnlyList<int> GetSpielerIds()
        {
            var ids = new List<int>();

            for (int i = 1; i < SW.Statisch.GetMinKIID(); i++)
            {
                if (SW.Dynamisch.GetHumWithID(i).GetName() != "")
                    ids.Add(i);
            }

            return ids;
        }

        public string GetName(int spielerId) => SW.Dynamisch.GetHumWithID(spielerId).GetName();

        public int GetBanner(int spielerId) => SW.Dynamisch.GetHumWithID(spielerId).GetBanner();

        /// <summary>Liefert die Statistikwerte des Spielers in zwei Spalten (Beschriftung + formatierter Wert).</summary>
        public StatistikSeite GetStatistik(int spielerId)
        {
            var spieler = SW.Dynamisch.GetHumWithID(spielerId);
            var stat = spieler.GetSpielerStatistik();

            var seite = new StatistikSeite();

            seite.Links.Add(new StatistikEintrag("Spionagen", stat.HiSpionagen.ToString()));
            seite.Links.Add(new StatistikEintrag("Sabotagen", stat.HiSabotagen.ToString()));
            seite.Links.Add(new StatistikEintrag("Anschläge", stat.HiVersuchteErmordungen.ToString()));
            seite.Links.Add(new StatistikEintrag("Erfolgreiche Anschläge", stat.HiErfolgreicheErmordungen.ToString()));
            seite.Links.Add(new StatistikEintrag("Bestechungen", stat.HiBestechungen.ToString()));
            seite.Links.Add(new StatistikEintrag("Bestechungssumme", stat.HiBestechungssumme.ToStringGeld()));
            seite.Links.Add(new StatistikEintrag("Glücksspiele", stat.HiKartenSpielen.ToString()));
            seite.Links.Add(new StatistikEintrag("Anschwärzungen", stat.HiAnschwaerzungen.ToString()));
            seite.Links.Add(StatistikEintrag.Leer);
            seite.Links.Add(new StatistikEintrag("Gekaufte Ablässe", stat.KgekaufteAblaesse.ToString()));
            seite.Links.Add(new StatistikEintrag("Abgelegte Beichten", stat.KabgelegteBeichten.ToString()));
            seite.Links.Add(new StatistikEintrag("Hochzeiten", stat.KHochzeiten.ToString()));
            seite.Links.Add(new StatistikEintrag("Konvertierungen", stat.KKonvertierungen.ToString()));
            seite.Links.Add(StatistikEintrag.Leer);
            seite.Links.Add(new StatistikEintrag("Kredite genommen", stat.SgenommeneKredite.ToString()));
            seite.Links.Add(new StatistikEintrag("Wahlen teilgenommen", stat.SWahlenTeilgenommen.ToString()));
            seite.Links.Add(new StatistikEintrag("Wahlen gewonnen", stat.SWahlenGewonnen.ToString()));

            seite.Rechts.Add(new StatistikEintrag("Waren verkauft", stat.HaWarenVerkauft.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Waren eingekauft", stat.HaWarenEingekauft.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Entrichtete Steuern", stat.HaentrichteteSteuern.ToStringGeld()));
            seite.Rechts.Add(new StatistikEintrag("Entrichtete Zölle", stat.HaentrichteteZoelle.ToStringGeld()));
            seite.Rechts.Add(StatistikEintrag.Leer);
            seite.Rechts.Add(new StatistikEintrag("Gesetzesverstöße", stat.SogebrocheneGesetze.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Angeklagt", stat.Soangeklagt.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Schuldturmaufenthalte", stat.SoSchuldturmaufenthalte.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Höchstes Amt", stat.SoHoechstesAmt.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Gesamtumsatz", stat.SoGesamtumsatz.ToStringGeld()));
            seite.Rechts.Add(new StatistikEintrag("Gezeugte Kinder", stat.SogezeugteKinder.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Amtseinkommen", stat.SoAmtseinkommen.ToStringGeld()));
            seite.Rechts.Add(StatistikEintrag.Leer);
            seite.Rechts.Add(new StatistikEintrag("Gesamtvermögen", spieler.GetGesamtVermoegen(spielerId).ToStringGeld()));
            seite.Rechts.Add(new StatistikEintrag("Taler", spieler.GetTaler().ToStringGeld()));

            return seite;
        }
    }

    /// <summary>Ein einzelner Statistik-Eintrag (Beschriftung und formatierter Wert). Ein leerer Eintrag dient als Abstand.</summary>
    public class StatistikEintrag
    {
        public static readonly StatistikEintrag Leer = new StatistikEintrag("", "");

        public StatistikEintrag(string beschriftung, string wert)
        {
            Beschriftung = beschriftung;
            Wert = wert;
        }

        public string Beschriftung { get; }

        public string Wert { get; }
    }

    /// <summary>Die Statistik eines Spielers in zwei Spalten.</summary>
    public class StatistikSeite
    {
        public List<StatistikEintrag> Links { get; } = new List<StatistikEintrag>();

        public List<StatistikEintrag> Rechts { get; } = new List<StatistikEintrag>();
    }
}
