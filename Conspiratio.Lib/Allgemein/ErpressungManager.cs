using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>Ausgang eines Erpressungsversuchs.</summary>
    public class ErpressungsErgebnis
    {
        public bool Erfolg { get; set; }

        /// <summary>Laufzeit der Erpressung in Jahren (nur bei Erfolg gesetzt).</summary>
        public int Jahre { get; set; }

        /// <summary>
        /// Bleiben die Beweise erhalten? Nur bei einem menschlichen Opfer, das die Erpressung abgelehnt
        /// hat – dann taugen sie weiter für eine Anklage vor Gericht.
        /// </summary>
        public bool BeweiseBleiben { get; set; }

        public string Meldung { get; set; }
    }

    /// <summary>
    /// Erpressung von Amtsträgern (WinForms-Issue #13, Konzept von PommBaer): Wer über seine Spione
    /// genug Belastendes gegen einen Amtsträger gesammelt hat, kann ihn erpressen und für einige Jahre
    /// dessen Amtsprivilegien mitbenutzen.
    ///
    /// Die „Beweispunkte" sind die über <see cref="Gameplay.Hinterzimmer.AktiveSpionagen.GetDelikte"/>
    /// aufsummierte Beweismächtigkeit – jeder Spionagefund steuert 1–4 Punkte bei. Je höher die
    /// Amtsebene des Ziels, desto mehr Punkte sind nötig und desto kürzer wirkt die Erpressung.
    /// </summary>
    public class ErpressungManager
    {
        // Mindestbeweise je Amtsebene (Konzept: Stadt 3, Grafschaft 5, Königreich 7).
        public const int MindestpunkteStadt = 3;
        public const int MindestpunkteLand = 5;
        public const int MindestpunkteReich = 7;

        // Erfolgswahrscheinlichkeit
        private const int Grundchance = 75;
        private const int BonusJeZusatzpunkt = 4;
        private const int MalusJeHoehererTitel = 15;
        private const int BonusJeNiedrigererTitel = 3;
        private const int MalusGleicheReligion = 5;
        private const int MinChance = 5;
        private const int MaxChance = 95;

        // Wirkungsdauer je Amtsebene: Grunddauer plus Zufall. Die Zufallsspanne ist gegenüber dem
        // ursprünglichen Konzept um ein Jahr gekürzt (Abstimmung im Issue), damit eine Erpressung im
        // besten Fall nicht über sieben Jahre trägt.
        private const int GrunddauerStadt = 4;
        private const int GrunddauerLand = 3;
        private const int GrunddauerReich = 2;
        private const int ZufallStadt = 4;   // Rnd-Obergrenze (exklusiv) → 0..3
        private const int ZufallLand = 3;    // → 0..2
        private const int ZufallReich = 2;   // → 0..1

        /// <summary>Beziehungsverlust beim Opfer, wenn die Erpressung scheitert.</summary>
        public const int BeziehungsverlustBeiMisserfolg = 10;

        /// <summary>Beweispunkte, die der aktive Spieler gegen das Ziel gesammelt hat.</summary>
        [PublicAPI]
        public int GetBeweispunkte(int zielId) =>
            SW.Dynamisch.GetAktHum().GetAktiveSpionage(zielId).GetDelikte();

        /// <summary>Für diese Amtsebene nötige Beweispunkte.</summary>
        [PublicAPI]
        public int GetMindestpunkte(int zielId)
        {
            int amtId = SW.Dynamisch.GetSpWithID(zielId).GetAmtID();

            if (amtId < SW.Statisch.GetMaxAmtStadtID())
                return MindestpunkteStadt;
            if (amtId < SW.Statisch.GetMaxAmtLandID())
                return MindestpunkteLand;

            return MindestpunkteReich;
        }

        /// <summary>Prüft, ob der aktive Spieler das Ziel erpressen kann.</summary>
        [PublicAPI]
        public bool KannErpressen(int zielId, out string grund)
        {
            grund = "";

            if (zielId == SW.Dynamisch.GetAktiverSpieler())
            {
                grund = "Ihr könnt Euch nicht selbst erpressen.";
                return false;
            }

            var ziel = SW.Dynamisch.GetSpWithID(zielId);

            if (ziel.GetAmtID() == 0)
            {
                grund = "Nur einen Amtsträger zu erpressen bringt Euch einen Vorteil.";
                return false;
            }

            if (SW.Dynamisch.GetAktHum().ErpresstBereits(zielId))
            {
                grund = ziel.GetName() + " steht bereits unter Eurer Fuchtel.";
                return false;
            }

            if (SW.Dynamisch.GetAktHum().GetAktiveSpionage(zielId).GetKosten() <= 0)
            {
                grund = "Ohne Spione gegen " + ziel.GetName() + " habt Ihr nichts in der Hand.";
                return false;
            }

            int punkte = GetBeweispunkte(zielId);
            int noetig = GetMindestpunkte(zielId);

            if (punkte < noetig)
            {
                grund = "Eure Beweise gegen " + ziel.GetName() + " wiegen zu leicht.\n" +
                        "Für dieses Amt braucht Ihr mindestens " + noetig + " Beweispunkte, Ihr habt " + punkte + ".";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Erfolgsaussicht in Prozent: Grundwert plus Bonus je überzähligem Beweispunkt, abzüglich der
        /// gesellschaftlichen Stellung des Ziels; unter Glaubensbrüdern schickt sich so etwas weniger.
        /// </summary>
        [PublicAPI]
        public int BerechneErfolgschance(int zielId)
        {
            var spieler = SW.Dynamisch.GetAktHum();
            var ziel = SW.Dynamisch.GetSpWithID(zielId);

            int chance = Grundchance;
            chance += (GetBeweispunkte(zielId) - GetMindestpunkte(zielId)) * BonusJeZusatzpunkt;

            int titelUnterschied = ziel.GetTitel() - spieler.GetTitel();

            if (titelUnterschied > 0)
                chance -= titelUnterschied * MalusJeHoehererTitel;
            else
                chance += -titelUnterschied * BonusJeNiedrigererTitel;

            if (ziel.GetReligion() == spieler.GetReligion())
                chance -= MalusGleicheReligion;

            if (chance < MinChance) chance = MinChance;
            if (chance > MaxChance) chance = MaxChance;

            return chance;
        }

        /// <summary>Laufzeit einer erfolgreichen Erpressung in Jahren (Grunddauer plus Zufall).</summary>
        [PublicAPI]
        public int BerechneDauer(int zielId)
        {
            int amtId = SW.Dynamisch.GetSpWithID(zielId).GetAmtID();

            if (amtId < SW.Statisch.GetMaxAmtStadtID())
                return GrunddauerStadt + SW.Statisch.Rnd.Next(0, ZufallStadt);
            if (amtId < SW.Statisch.GetMaxAmtLandID())
                return GrunddauerLand + SW.Statisch.Rnd.Next(0, ZufallLand);

            return GrunddauerReich + SW.Statisch.Rnd.Next(0, ZufallReich);
        }

        /// <summary>
        /// Die konkreten Vorwürfe gegen das Ziel – für die Drohung und für den Dialog, in dem ein
        /// menschliches Opfer entscheidet. Grundlage sind wie vor Gericht die tatsächlich begangenen
        /// Delikte je Gesetz.
        /// </summary>
        [PublicAPI]
        public List<string> GetBeweisliste(int zielId)
        {
            var vorwuerfe = new List<string>();
            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            string[] texte = SW.Statisch.GetGerichtsGesetzesvorwurf();

            for (int i = 0; i < SW.Statisch.GetMaxGesetze() && i < texte.Length; i++)
            {
                if (ziel.GetBegingVerbrechenX(i) > 0)
                    vorwuerfe.Add(texte[i]);
            }

            return vorwuerfe;
        }

        /// <summary>Drohtext mit Beweislage und Erfolgsaussicht – Grundlage der Bestätigungsfrage.</summary>
        [PublicAPI]
        public string GetErpressungsFrage(int zielId)
        {
            var ziel = SW.Dynamisch.GetSpWithID(zielId);

            return "Wollt Ihr " + ziel.GetKompletterName() + " mit Euren Beweisen erpressen?\n\n" +
                   "Beweispunkte: " + GetBeweispunkte(zielId) + " (nötig: " + GetMindestpunkte(zielId) + ")\n" +
                   "Aussicht auf Erfolg: " + BerechneErfolgschance(zielId) + " %\n\n" +
                   "Gelingt es, könnt Ihr einige Jahre lang die Privilegien des Amtes mitnutzen.\n" +
                   "Misslingt es, sind Eure Beweise verloren.";
        }

        /// <summary>Frage an ein menschliches Opfer, ob es sich der Erpressung beugt.</summary>
        [PublicAPI]
        public string GetOpferFrage(int erpresserId, int zielId)
        {
            var erpresser = SW.Dynamisch.GetSpWithID(erpresserId);
            var vorwuerfe = GetBeweisliste(zielId);

            string text = erpresser.GetKompletterName() + " tritt mit Beweisen an Euch heran und fordert,\n" +
                          "dass Ihr Euer Amt fortan nach seinem Willen führt.\n\n";

            if (vorwuerfe.Count > 0)
                text += "Vorgehalten wird Euch:\n" + string.Join("\n", vorwuerfe) + "\n\n";

            return text + "Beugt Ihr Euch der Erpressung?\n" +
                   "Lehnt Ihr ab, behält er die Beweise – und kann Euch damit vor Gericht bringen.";
        }

        /// <summary>
        /// Trägt die Erpressung aus. Bei einem KI-Ziel entscheidet die Erfolgschance; bei einem
        /// menschlichen Ziel hat der Aufrufer dessen Entscheidung bereits eingeholt und übergibt sie in
        /// <paramref name="opferBeugtSich"/>.
        /// </summary>
        [PublicAPI]
        public ErpressungsErgebnis FuehreErpressungDurch(int zielId, bool? opferBeugtSich = null)
        {
            int erpresserId = SW.Dynamisch.GetAktiverSpieler();
            var erpresser = SW.Dynamisch.GetAktHum();
            var ziel = SW.Dynamisch.GetSpWithID(zielId);
            bool zielIstMensch = zielId < SW.Statisch.GetMinKIID();

            bool erfolg = opferBeugtSich ?? SW.Statisch.Rnd.Next(0, 100) < BerechneErfolgschance(zielId);

            // Die Erpressung selbst ist eine Straftat – sie wird dem Erpresser angelastet, gelingt sie
            // oder nicht, und kann später vor Gericht gegen ihn verwendet werden.
            int gesetz = SW.Statisch.GetGesetzErpressung();
            erpresser.SetBegingVerbrechenX(gesetz, erpresser.GetBegingVerbrechenX(gesetz) + 1);

            if (erfolg)
            {
                int jahre = BerechneDauer(zielId);
                erpresser.ErpressungAnlegen(zielId, SW.Dynamisch.GetAktuellesJahr() + jahre);

                // Die Beweise sind verbraucht und stehen nicht mehr vor Gericht zur Verfügung.
                erpresser.GetAktiveSpionage(zielId).SetDelikte(0);

                return new ErpressungsErgebnis
                {
                    Erfolg = true,
                    Jahre = jahre,
                    Meldung = ziel.GetKompletterName() + " muss sich den erdrückenden Beweisen beugen\n" +
                              "und steht nun für " + jahre + " Jahre unter Eurer Fuchtel.\n\n" +
                              "In der Schreibstube könnt Ihr fortan die Privilegien dieses Amtes mitnutzen."
                };
            }

            // Misserfolg: Das Ziel ist verstimmt. Ein menschliches Opfer, das sich weigert, vernichtet
            // die Beweise nicht – sie bleiben dem Erpresser für eine Anklage erhalten (Sonderregel PvP).
            if (!zielIstMensch)
                SW.Dynamisch.GetKIwithID(zielId).ErhoeheBeziehungZuX(erpresserId, -BeziehungsverlustBeiMisserfolg);

            bool beweiseBleiben = zielIstMensch;

            if (!beweiseBleiben)
                erpresser.GetAktiveSpionage(zielId).SetDelikte(0);

            string meldung = zielIstMensch
                ? ziel.GetKompletterName() + " weist Eure Forderung zurück.\n\n" +
                  "Eure Beweise behaltet Ihr – vor Gericht könnten sie noch von Nutzen sein."
                : ziel.GetKompletterName() + " lacht über Eure läppischen Drohungen.\n\n" +
                  "Eure Beweise sind dahin, und Euer Ansehen bei ihm hat gelitten.";

            return new ErpressungsErgebnis
            {
                Erfolg = false,
                BeweiseBleiben = beweiseBleiben,
                Meldung = meldung
            };
        }
    }
}
