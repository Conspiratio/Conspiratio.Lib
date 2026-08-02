using Conspiratio.Lib.Extensions;
using Conspiratio.Lib.Gameplay.Personen;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Bereitet die spielübergreifende Statistik eines <see cref="Profil"/>s für die Anzeige auf – in
    /// demselben Zwei-Spalten-Format wie der <see cref="StatistikManager"/>, aber gespeist aus den
    /// aufsummierten Profilwerten (<see cref="Profil.Gesamt"/>) plus den spielübergreifenden
    /// Kennzahlen (<see cref="ProfilMeta"/>). Das Max-Feld „Höchstes Amt" kommt aus der Meta.
    /// </summary>
    public class ProfilStatistikManager
    {
        /// <summary>Liefert die Profilstatistik in zwei Spalten (Beschriftung + formatierter Wert).</summary>
        public StatistikSeite GetStatistik(Profil profil)
        {
            var stat = profil.Gesamt;
            var meta = profil.Meta;

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
            seite.Links.Add(StatistikEintrag.Leer);
            seite.Links.Add(new StatistikEintrag("Kämpfe gewonnen", stat.MiKaempfeGewonnen.ToString()));
            seite.Links.Add(new StatistikEintrag("Kämpfe verloren", stat.MiKaempfeVerloren.ToString()));
            seite.Links.Add(new StatistikEintrag("Eroberte Stützpunkte", stat.MiEroberteStuetzpunkte.ToString()));
            seite.Links.Add(new StatistikEintrag("Überfallene Karawanen", stat.MiUeberfalleneKarawanen.ToString()));

            seite.Rechts.Add(new StatistikEintrag("Waren verkauft", stat.HaWarenVerkauft.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Waren eingekauft", stat.HaWarenEingekauft.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Entrichtete Steuern", stat.HaentrichteteSteuern.ToStringGeld()));
            seite.Rechts.Add(new StatistikEintrag("Entrichtete Zölle", stat.HaentrichteteZoelle.ToStringGeld()));
            seite.Rechts.Add(StatistikEintrag.Leer);
            seite.Rechts.Add(new StatistikEintrag("Gesetzesverstöße", stat.SogebrocheneGesetze.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Angeklagt", stat.Soangeklagt.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Schuldturmaufenthalte", stat.SoSchuldturmaufenthalte.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Gesamtumsatz", stat.SoGesamtumsatz.ToStringGeld()));
            seite.Rechts.Add(new StatistikEintrag("Gezeugte Kinder", stat.SogezeugteKinder.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Amtseinkommen", stat.SoAmtseinkommen.ToStringGeld()));
            seite.Rechts.Add(new StatistikEintrag("Gebaute Häuser", stat.SoGebauteHaeuser.ToString()));
            seite.Rechts.Add(StatistikEintrag.Leer);
            // Spielübergreifende Kennzahlen (Meta): „Höchstes Amt" als Maximum, nicht aus der Summe.
            seite.Rechts.Add(new StatistikEintrag("Gespielte Spiele", meta.SpieleGesamt.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Gespielte Jahre", meta.GespielteJahre.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Höchstes Amt", meta.HoechstesAmt.ToString()));
            seite.Rechts.Add(new StatistikEintrag("Höchstes Vermögen", meta.HoechstesVermoegen.ToStringGeld()));

            return seite;
        }
    }
}
