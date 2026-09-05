using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Conspiratio.Lib.Harness
{
    /// <summary>
    /// Messinstrument fuer das Spaetspiel-Balancing. Aufruf:
    ///
    /// <code>
    /// dotnet run --project Conspiratio.Lib.Harness -- --staedte=14 --slots=2 --jahre=30 --seeds=5
    /// </code>
    ///
    /// Schalter: <c>--staedte</c>, <c>--slots</c> (1-2), <c>--jahre</c>, <c>--seeds</c> (Anzahl oder
    /// Liste wie <c>--seeds=1234,5678</c>), <c>--start-taler</c>, <c>--vollstaendig</c> (KI-Jahreswechsel
    /// mitlaufen lassen), <c>--tabelle</c> (Jahrestabelle des ersten Seeds).
    /// </summary>
    internal static class Program
    {
        private static int Zahl(string[] args, string name, int standard)
        {
            string treffer = args.FirstOrDefault(a => a.StartsWith("--" + name + "=", StringComparison.Ordinal));

            return treffer != null && int.TryParse(treffer.Substring(name.Length + 3), out int wert) ? wert : standard;
        }

        private static bool Schalter(string[] args, string name)
            => args.Any(a => a == "--" + name);

        private static List<int> Seeds(string[] args)
        {
            string treffer = args.FirstOrDefault(a => a.StartsWith("--seeds=", StringComparison.Ordinal));

            if (treffer == null)
                return new List<int> { 1234 };

            string wert = treffer.Substring("--seeds=".Length);

            if (wert.Contains(','))
                return wert.Split(',').Select(t => int.Parse(t, CultureInfo.InvariantCulture)).ToList();

            // Eine blosse Zahl bedeutet: so viele feste Seeds, damit Laeufe vergleichbar bleiben.
            int anzahl = int.Parse(wert, CultureInfo.InvariantCulture);
            var basis = new[] { 1234, 5678, 90210, 4711, 31337, 8080, 2468, 13579, 6543, 999 };

            return basis.Take(Math.Min(anzahl, basis.Length)).ToList();
        }

        // InvariantGlobalization ist gesetzt, deutsche Kulturen stehen also nicht zur Verfuegung -
        // der Punkt als Tausendertrenner wird deshalb selbst gesetzt.
        private static string G(int wert) => wert.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".");

        private static void Main(string[] args)
        {
            var aufbau = new Aufbau
            {
                Staedte = Zahl(args, "staedte", 1),
                SlotsProStadt = Zahl(args, "slots", 1),
                StartTaler = Zahl(args, "start-taler", 50_000),
                Lagerausbauten = Zahl(args, "lagerausbauten", 8),
                StaettenJeLinie = Zahl(args, "staetten", 0),
                Vollstaendig = Schalter(args, "vollstaendig"),
                Export = Schalter(args, "export"),
                Adel = Schalter(args, "adel"),
                Marktklug = Schalter(args, "marktklug"),
                KapazitaetsunterhaltJeStaette = Zahl(args, "kapazitaet", 0)
            };

            int jahre = Zahl(args, "jahre", 20);
            var seeds = Seeds(args);
            bool tabelle = Schalter(args, "tabelle");

            Console.WriteLine("Conspiratio-Harness - Wirtschaft ohne Treiberdecke");
            Console.WriteLine("Staedte " + aufbau.Staedte + ", Slots je Stadt " + aufbau.SlotsProStadt +
                              ", Jahre " + jahre + ", Seeds " + seeds.Count +
                              (aufbau.Vollstaendig ? ", mit KI-Jahreswechsel" : ", ohne KI-Jahreswechsel") +
                              (aufbau.Export ? ", Absatz per Export" : ", Absatz vor Ort") +
                              (aufbau.Adel ? ", mit Adelsweg" : "") +
                              (aufbau.Marktklug ? ", marktklug" : ""));
            Console.WriteLine();

            var endstaende = new List<int>();

            foreach (int seed in seeds)
            {
                var spiel = new Harnessspiel(aufbau);
                spiel.Starte(seed);

                var zeilen = new List<Jahreszeile>();

                for (int i = 0; i < jahre; i++)
                    zeilen.Add(spiel.SpieleJahr());

                var letzte = zeilen[zeilen.Count - 1];
                endstaende.Add(letzte.Vermoegen);

                if (tabelle && seed == seeds[0])
                {
                    Console.WriteLine("Aufbau: " + spiel.Staetten + " Staetten, " + spiel.Arbeiter +
                                      " Arbeiter, bindende Grenze: " + spiel.BindendeGrenze);
                    Console.WriteLine();
                    Console.WriteLine("Jahr    Produziert   Verkauft       Erloes    Unterhalt   Hofhaltung   " +
                                      "Gesamtkosten    Kapazitaet       Taler     Vermoegen   Abschlag");
                    Console.WriteLine(new string('-', 125));

                    foreach (var z in zeilen)
                        Console.WriteLine(
                            z.Jahr.ToString(CultureInfo.InvariantCulture).PadLeft(4) + "  " +
                            G(z.Produziert).PadLeft(12) + "  " +
                            G(z.Verkauft).PadLeft(9) + "  " +
                            G(z.Erloes).PadLeft(11) + "  " +
                            G(z.Unterhalt).PadLeft(11) + "  " +
                            G(z.Hofhaltung).PadLeft(11) + "  " +
                            G(z.Gesamtkosten).PadLeft(14) + "  " +
                            G(z.Kapazitaet).PadLeft(10) + "  " +
                            G(z.Taler).PadLeft(10) + "  " +
                            G(z.Vermoegen).PadLeft(12) + "  " +
                            (z.Abschlag + " %").PadLeft(9));

                    Console.WriteLine();
                }

                Console.WriteLine("Seed " + seed.ToString(CultureInfo.InvariantCulture).PadLeft(6) +
                                  ":  Endvermoegen " + G(letzte.Vermoegen).PadLeft(12) +
                                  "   Taler " + G(letzte.Taler).PadLeft(12) +
                                  "   Abschlag " + (letzte.Abschlag + " %").PadLeft(5) +
                                  "   " + letzte.Titel);
            }

            endstaende.Sort();

            Console.WriteLine();
            Console.WriteLine("Band ueber " + endstaende.Count + " Seeds: " + G(endstaende[0]) + " bis " +
                              G(endstaende[endstaende.Count - 1]) +
                              ", Median " + G(endstaende[endstaende.Count / 2]));
            Console.WriteLine();
            Console.WriteLine("Ein einzelner Seed sagt nichts - die Streuung zwischen Seeds dominiert.");
            Console.WriteLine("Vergleiche immer Verteilungen, in derselben Sitzung gemessen.");
        }
    }
}
