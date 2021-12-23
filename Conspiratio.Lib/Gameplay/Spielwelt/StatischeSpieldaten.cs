using System;
using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Aemter;
using Conspiratio.Lib.Gameplay.Ereignisse;
using Conspiratio.Lib.Gameplay.Justiz;
using Conspiratio.Lib.Gameplay.Kirche;
using Conspiratio.Lib.Gameplay.Niederlassung;
using Conspiratio.Lib.Gameplay.Privilegien;
using Conspiratio.Lib.Gameplay.Titel;
using Conspiratio.Lib.Gameplay.Wohnsitz;

namespace Conspiratio.Lib.Gameplay.Spielwelt
{
    /// <summary>
    /// Enthält alle Daten zum Spiel und zur Spielwelt, die sich während des Spiels nicht verändern.
    /// Nur beim Start von Conspiratio findet eine Initialisierung mit Standardwerten durch die Methode <see cref="Initialisieren"/> statt.
    /// </summary>
    public class StatischeSpieldaten
    {
        #region Variablen

        #region Allgemein

        /// <summary>
        /// Gibt die Jahreszahl zu Beginn eines neuen Spiels an.
        /// </summary>
        public int StartJahr { get; private set; }

        /// <summary>
        /// Enthält die Texte aller Tipps.
        /// </summary>
        public string[] Tipps { get; private set; }

        private int TippsMaxIndex = 0;

        #endregion

        #region KIs
        private int KIminVerblJahre;
        private int KImaxVerblJahre;

        private int MaennerFrauenGrenze; //Ab welcher Nummer die weiblichen namen beginnen
        private int minKIID;
        private int maxKIID;
        private string[] KINames; //Die Namen der KI-Spieler
        #endregion

        #region Gesetze
        private int maxKorruptionsGelder;
        private int KreditZinsMin;
        private int KreditZinsMax;

        private int maxAnzahlStrafen = 4;
        private IStrafe[] Strafarten;

        private int[] GesetzDefUntergrenze;
        private int[] GesetzDefObergrenze;
        private int maxGesetze;

        private string[] GerichtsGesetzesvorwurf;
        private int GesetzAnzahlFinanz;
        private int GesetzAnzahlStraf;
        private int GesetzAnzahlKirch;

        private int GesetzgrenzeFinanz;
        private int GesetzgrenzeStraf;
        private int GesetzgrenzeKirche;

        private double maxUmsatzsteuer;
        private double minUmsatzsteuer;

        private string[] ReligionsNamen;
        private int RelFreiID;
        private int RelMinID;
        private int RelMaxID;
        private int RelKathID;
        private int RelEvanID;
        private int maxAnzahlGerichtsverhandlungen;

        private int GerichtsKlagepunkte;
        #endregion

        #region Weltkarte
        private int[,] StadtRechtecke;
        private int[,] StuetzpunktRechtecke;

        private int minStadtID;
        private int maxStadtID;
        private int minLandID;
        private int maxLandID;
        private int minReichID;
        private int maxReichID;
        #endregion

        #region Politik
        private int maxStufeID;
        private string[] StufenNamen;

        private int MaxAbsetzSympathie;
        private int MaxWahlKandidaten;
        private int MaxWahlWaehler;
        private int MaxTitelID;

        private int MaxAmtID;
        private int MaxAmtStadtID;
        private int MaxAmtLandID;
        private Amt[] Aemter;

        private int KITeilnehmerProWahl;

        private int MinTitelStadtEbene;
        private int MinTitelLandEbene;
        private int MinTitelReichsEbene;

        private Adelstitel[] Tit;
        private int maxAnzahlWahlen;
        private int maxAnzahlAmtsenthebungen;
        #endregion

        #region HumSpieler
        private int minNameLength;
        private int maxNameLength;

        private int NeuerSpielerRohwahlkosten;
        private int NeuerSpielerStadtwahlkosten;

        private int HumMinVerblJahre;
        private int HumMaxVerblJahre;
        private int maxAlter; //Maximal erreichbares Alter
        private int StartAlter;

        private int minSterbeAlter;
        private int MinGesundheit;

        private int maxSchulden;
        private int KerkerGesundheit;
        private int KerkerAnsehen;

        private int ChanceFuerKind;
        private int ChanceFuerKindStirbt;
        private int MaxHumSpielerAnzahl; //Wie viele Menschliche Spieler es maximal geben kann
        private int StartGold; //Startgold für Spieler
        private int Startlagerraum; //Lagerraum den jeder Spieler zu Beginn zugeweisen bekommt
        private int maxGesundheit; //Maximale Gesundheit

        private double Werbegeschenkfaktor;
        private Werbegeschenk[] WG; //Array der Werbegeschenke
        private int maxWerbegeschenke;
        private int WerbegeschenkGrenzeBillig;
        private int WerbegeschenkGrenzeMittelteuer;
        private string[] Werbereaktionen;

        private int maxKrediteAnzahl;
        private int maxKinderAnzahl;
        private int minKindSlotNr;

        private double Kirchenzehnt;
        private int Konvertierkosten;
        private int Austrittskosten;
        private int DeliktpunktPreis;
        #endregion

        #region Regionen und Wirtschaft
        private int MinRohGrundPreis;
        private int MaxRohGrundPreis;
        private double StandardUmsatzSteuer;
        private int maxStaedteProLand;

        private int maxProdSlots; //Maximale Anzahl der Produktionsslots pro Stadt
        private int maxWerkstaettenProStadt;
        private int maxArbeiterAnzahl; //
        private int maxAnzahlVonEinemRohstoff; //Die maximale Obergrenze von einem Rohstoff in einer Stadt

        private int StartHausID; //Die ID des Starthauses für Einsteiger
        private int MaxHausID; //Wie viele Häuser es maximal gibt
        private Haus[] Haeuser;
        private int DefaultKarawane; //Die ID der Karawane, die standardmäßig eingestellt ist
        private int minKarawane; //Die minimale ID von den Karawanen
        private int maxKarawane; //Wie viele Karawanen es zur Auswahl gibt
        private Karawane[] Kara;

        private int LagerraumBasisPreis;
        private int MaxReichtum;
        private int MaxCrime;

        private int maxRohID; //Die höchste ID der Rohstoffe
        private int maxRohStufe1ID;
        private int maxRohStufe2ID;

        private int maxAnzahlSkills;
        private int MaxKatastrophen;
        private int EinkaufspreisZuschlag;

        private double minZollsatz;
        private double maxZollsatz;
        #endregion

        #region Privilegien und Beziehungen
        private int KartenSpielenMinTaler;
        private double kartenSpielenProzentsatz;

        private double ErmordungProzentsatz;
        private int ErmordungsChance;
        private int VergifteterWeinChance;

        private int maxPrivilegien = 100;
        private IPrivileg[] Privilegien;

        private double KupplerProzente;
        private int AnsehenProTaler;
        #endregion

        #region Ereignisse

        private int maxTexteTodesursachen;
        private string[] TexteTodesursachen;

        public List<Datumsereignis> Datumsereignisse { get; private set; }

        #endregion

        #endregion

        #region Initialisieren
        public void Initialisieren(IStrafe[] strafarten, IPrivileg[] privilegien)
        {
            #region Gesetze
            GesetzAnzahlFinanz = 5;
            GesetzAnzahlStraf = 5;
            GesetzAnzahlKirch = 5;

            GesetzgrenzeFinanz = 20;
            GesetzgrenzeStraf = 40;
            GesetzgrenzeKirche = 60;

            maxGesetze = 100;
            GesetzDefObergrenze = new int[maxGesetze];
            GesetzDefUntergrenze = new int[maxGesetze];

            //Finanzgesetze
            //Kredite
            GesetzDefUntergrenze[0] = 0;
            GesetzDefObergrenze[0] = 1;
            //Bestechungen
            GesetzDefUntergrenze[1] = 0;
            GesetzDefObergrenze[1] = 1;
            //Hoechstzahl Anwesen
            GesetzDefUntergrenze[2] = 4;
            GesetzDefObergrenze[2] = 14;
            //Maxmimale Taler
            GesetzDefUntergrenze[3] = 5;
            GesetzDefObergrenze[3] = 30;
            //Gluecksspiel
            GesetzDefUntergrenze[4] = 0;
            GesetzDefObergrenze[4] = 1;

            //Strafgesetze
            //Spionage
            GesetzDefUntergrenze[20] = 0;
            GesetzDefObergrenze[20] = 1;
            //Sabotage
            GesetzDefUntergrenze[21] = 0;
            GesetzDefObergrenze[21] = 1;
            //Anschwärzen
            GesetzDefUntergrenze[22] = 0;
            GesetzDefObergrenze[22] = 1;
            //Ermordung
            GesetzDefUntergrenze[23] = 1;
            GesetzDefObergrenze[23] = 1;
            //Waffenhandel
            GesetzDefUntergrenze[24] = 0;
            GesetzDefObergrenze[24] = 1;

            //Kirchengesetze
            //Rel-Freiheit
            GesetzDefUntergrenze[40] = 0;
            GesetzDefObergrenze[40] = 1;
            //Ablaß
            GesetzDefUntergrenze[41] = 0;
            GesetzDefObergrenze[41] = 1;
            //Schloesser
            GesetzDefUntergrenze[42] = 0;
            GesetzDefObergrenze[42] = 1;
            //Gold
            GesetzDefUntergrenze[43] = 0;
            GesetzDefObergrenze[43] = 1;
            //Silber
            GesetzDefUntergrenze[44] = 0;
            GesetzDefObergrenze[44] = 1;
            #endregion

            #region Gebiete
            minStadtID = 1;
            maxStadtID = 15; //In Wirklichkeit 1 weniger, da Arrayverschub
            minLandID = 1;
            maxLandID = 5;
            minReichID = 1;
            maxReichID = 2;
            maxStaedteProLand = 4;

            maxStufeID = 3;
            StufenNamen = new string[maxStufeID];
            StufenNamen[0] = "Stadt";
            StufenNamen[1] = "Land";
            StufenNamen[2] = "Reich";

            StadtRechtecke = new int[maxStadtID, 4]; //[x,0] = Left, [x,1] = right, [x,2] = top, [x,3] = bot
            StadtRechtecke[1, 0] = 418;
            StadtRechtecke[1, 1] = 467;
            StadtRechtecke[1, 2] = 8;
            StadtRechtecke[1, 3] = 78;

            StadtRechtecke[2, 0] = 677;
            StadtRechtecke[2, 1] = 718;
            StadtRechtecke[2, 2] = 105;
            StadtRechtecke[2, 3] = 182;

            StadtRechtecke[3, 0] = 82;
            StadtRechtecke[3, 1] = 131;
            StadtRechtecke[3, 2] = 39;
            StadtRechtecke[3, 3] = 78;

            StadtRechtecke[4, 0] = 946;
            StadtRechtecke[4, 1] = 988;
            StadtRechtecke[4, 2] = 149;
            StadtRechtecke[4, 3] = 183;

            StadtRechtecke[5, 0] = 1073;
            StadtRechtecke[5, 1] = 1125;
            StadtRechtecke[5, 2] = 137;
            StadtRechtecke[5, 3] = 172;

            StadtRechtecke[6, 0] = 998;
            StadtRechtecke[6, 1] = 1039;
            StadtRechtecke[6, 2] = 237;
            StadtRechtecke[6, 3] = 275;

            StadtRechtecke[7, 0] = 938;
            StadtRechtecke[7, 1] = 991;
            StadtRechtecke[7, 2] = 315;
            StadtRechtecke[7, 3] = 351;

            StadtRechtecke[8, 0] = 59;
            StadtRechtecke[8, 1] = 122;
            StadtRechtecke[8, 2] = 612;
            StadtRechtecke[8, 3] = 658;

            StadtRechtecke[9, 0] = 300;
            StadtRechtecke[9, 1] = 375;
            StadtRechtecke[9, 2] = 424;
            StadtRechtecke[9, 3] = 490;

            StadtRechtecke[10, 0] = 597;
            StadtRechtecke[10, 1] = 658;
            StadtRechtecke[10, 2] = 396;
            StadtRechtecke[10, 3] = 442;

            StadtRechtecke[11, 0] = 676;
            StadtRechtecke[11, 1] = 720;
            StadtRechtecke[11, 2] = 297;
            StadtRechtecke[11, 3] = 328;

            StadtRechtecke[12, 0] = 605;
            StadtRechtecke[12, 1] = 665;
            StadtRechtecke[12, 2] = 563;
            StadtRechtecke[12, 3] = 606;

            StadtRechtecke[13, 0] = 1235;
            StadtRechtecke[13, 1] = 1302;
            StadtRechtecke[13, 2] = 511;
            StadtRechtecke[13, 3] = 545;

            StadtRechtecke[14, 0] = 1251;
            StadtRechtecke[14, 1] = 1315;
            StadtRechtecke[14, 2] = 680;
            StadtRechtecke[14, 3] = 745;

            StuetzpunktRechtecke = new int[8, 4]; //[x,0] = Left, [x,1] = right, [x,2] = top, [x,3] = bot

            // Zollburgen
            StuetzpunktRechtecke[0, 0] = 195;
            StuetzpunktRechtecke[0, 1] = 345;
            StuetzpunktRechtecke[0, 2] = 225;
            StuetzpunktRechtecke[0, 3] = 325;

            StuetzpunktRechtecke[1, 0] = 1510;
            StuetzpunktRechtecke[1, 1] = 1650;
            StuetzpunktRechtecke[1, 2] = 340;
            StuetzpunktRechtecke[1, 3] = 410;

            StuetzpunktRechtecke[2, 0] = 140;
            StuetzpunktRechtecke[2, 1] = 300;
            StuetzpunktRechtecke[2, 2] = 610;
            StuetzpunktRechtecke[2, 3] = 720;

            StuetzpunktRechtecke[3, 0] = 600;
            StuetzpunktRechtecke[3, 1] = 740;
            StuetzpunktRechtecke[3, 2] = 845;
            StuetzpunktRechtecke[3, 3] = 950;

            // Räuberlager
            StuetzpunktRechtecke[4, 0] = 960;
            StuetzpunktRechtecke[4, 1] = 1100;
            StuetzpunktRechtecke[4, 2] = 120;
            StuetzpunktRechtecke[4, 3] = 230;

            StuetzpunktRechtecke[5, 0] = 2300;
            StuetzpunktRechtecke[5, 1] = 2440;
            StuetzpunktRechtecke[5, 2] = 460;
            StuetzpunktRechtecke[5, 3] = 570;

            StuetzpunktRechtecke[6, 0] = 700;
            StuetzpunktRechtecke[6, 1] = 840;
            StuetzpunktRechtecke[6, 2] = 500;
            StuetzpunktRechtecke[6, 3] = 610;

            StuetzpunktRechtecke[7, 0] = 1740;
            StuetzpunktRechtecke[7, 1] = 1870;
            StuetzpunktRechtecke[7, 2] = 700;
            StuetzpunktRechtecke[7, 3] = 800;

            minZollsatz = 0.05;
            maxZollsatz = 0.15;
            #endregion

            #region Gericht
            maxAnzahlGerichtsverhandlungen = 400;
            GerichtsKlagepunkte = 10;

            GerichtsGesetzesvorwurf = new string[maxGesetze];
            GerichtsGesetzesvorwurf[0] = "Ihr habt hinterrücks mit steuerfreien Krediten gewirtschaftet";
            GerichtsGesetzesvorwurf[1] = "Ihr habt es versucht, Euch andere Personen mit Talern gefügig zu machen";
            GerichtsGesetzesvorwurf[2] = "Mit Euren zahlreichen Faktoreien habt Ihr die Wirtschaft gefährdet";
            GerichtsGesetzesvorwurf[3] = "Eure angehäuften Taler schaden der Wirtschaft!";
            GerichtsGesetzesvorwurf[4] = "Euer Glücksspiel hat manch einen an den Bettelstab gebracht";

            GerichtsGesetzesvorwurf[20] = "Ihr habt spioniert!";
            GerichtsGesetzesvorwurf[21] = "Ihr habt Sabotage betrieben!";
            GerichtsGesetzesvorwurf[22] = "Mittels politischen Intrigen habt Ihr Euch einen Vorteil verschafft";
            GerichtsGesetzesvorwurf[23] = "Es gibt Zeugen, die behaupten Ihr habt eine Ermordung in Auftrag gegeben!";
            GerichtsGesetzesvorwurf[24] = "Euer Waffenhandel ist nichts anderes als Kriegstreiberei!";

            GerichtsGesetzesvorwurf[40] = "Ihr habt Euch gegen Gottes Willen von allen Religionen losgesagt!";
            GerichtsGesetzesvorwurf[41] = "Ihr habt es versucht, Euch von Euren Missetaten freizukaufen!";
            GerichtsGesetzesvorwurf[42] = "Eure prunkvollen Schlösser sind verschwenderisch und eine Beleidigung für Gott!";
            GerichtsGesetzesvorwurf[43] = "Für die vielen Taler, die Ihr besitzt, habt Ihr bestimmt Eure Seele an den Teufel verkauft!";
            GerichtsGesetzesvorwurf[44] = "Für das viele Silber, das Ihr besitzt, habt Ihr bestimmt Eure Seele an den Teufel verkauft!";
            #endregion

            #region Tipps
            Tipps = new string[100];

            Tipps[0] = "Große eindrucksvolle Gebäude in Eurem Besitz verschaffen Euch Ansehen, während schäbige Gebäude dieses schmälern.";
            Tipps[1] = "Um neue Handelszertifikate zu erlangen müsst Ihr höhere Ämter bekleiden, genügend Taler besitzen und einen dementsprechend hohen Titel besitzen.";
            Tipps[2] = "Ein in einer Stadt als Hauptproduktion markierter Rohstoff wird schneller produziert, als die anderen Rohstoffe in dieser Stadt. Dafür ist der Verkaufspreis in dieser Stadt aber etwas geringer.";
            Tipps[3] = "Das Innehaben von mehreren Wohnsitzen und das damit verbundene Reisen wirkt sich negativ auf Eure Gesundheit aus.";
            Tipps[4] = "Durch einen Rechtsklick auf den Rohstoff in Produktion wird Euch das empfohlene Produktionsverhältnis angezeigt.";
            Tipps[5] = "Ein höheres Ansehen verschafft Euch nicht nur bei Wahlen einen Vorteil. Es erlaubt Euch auch auf mächtigere Personen einzuwirken.";
            Tipps[6] = "Der Kauf eines Ablassbriefes erleichtert Euch um einen Teil Eurer Sünden und kann Euch dadurch den ein oder anderen Gerichtsprozess ersparen.";
            Tipps[7] = "Gleich und gleich gesellt sich gern. Ob Ihr katholisch oder evangelisch seid, kann sich bei Wahlen positiv oder negativ auswirken.";
            Tipps[8] = "Ein Austritt aus der Kirche verwehrt Euch auch das Ausführen von kirchlichen Ämtern.";
            Tipps[9] = "Die Kupplerin hat Ihren Preis. Allerdings erleichtert Ihre Hilfe es deutlich, den Partner zu umgarnen.";
            Tipps[10] = "Jeder Verstoß gegen ein Gesetz, erhöht die Wahrscheinlichkeit, dass Ihr für Eure Vergehen zur Rechenschaft gezogen werdet.";
            Tipps[11] = "Ein Jahr im Kerker zu verbringen raubt Euch nicht nur wertvolle Zeit. Es schadet auch Eurer Gesundheit und Eurem Ansehen.";
            Tipps[12] = "Nutzt in Eurer Schreibstube rechtzeitig das Privileg, ein Testament zu machen. Habt Ihr bei Eurem Tod keinen Erben eingesetzt, fällt all' Euer Hab und Gut dem Erzbistum zu.";
            Tipps[13] = "Bevor Ihr Eure Konkurrenten anschwärzt, solltet Ihr sicherstellen, dass Ihr über ausreichend Beweise für Eure Behauptungen verfügt.";
            Tipps[14] = "Mit einem Rechtsklick auf eine Werkstatt in einer Stadt kann die Werkstatt verkauft werden.";
            Tipps[15] = "Ein regelmäßiger Blick in die Gesetzesbücher kann Euch den ein oder anderen Prozess ersparen.";
            Tipps[16] = "Wenn Ihr einen bestimmten Rohstoff immer wieder in derselben Stadt verkauft, so wird der Verkaufserlös für diesen Rohstoff in dieser Stadt abnehmen.";
            Tipps[16] = "Ein Kredit kann Eure Liquiditätsprobleme im Handumdrehen lösen. Doch Vorsicht! Die Zinsen können Euch in den Ruin treiben.";
            Tipps[17] = "Damit Ihr bei Amtswahlen als Sieger hervorgeht, müsst Ihr um die Gunst der Wähler buhlen. Dies könnt Ihr mittels Bestechung, Kartenspielen oder Ähnlichem erreichen.";
            Tipps[18] = "Falls Ihr über die Landesgrenzen hinaus Eure Waren exportiert, so fallen zusätzlich zu den Karawanenkosten auch noch Zölle an.";
            Tipps[19] = "Es lohnt sich nur dorthin zu exportieren, wo der Rohstoff auch benötigt wird.";
            Tipps[20] = "Es kann sich auch lohnen, in Städten gewisse Rohstoffe einzukaufen und woanders hin zu exportieren.";
            Tipps[21] = "Erweitert Euren Wohnsitz, um Euch zusätzliches Ansehen zu verschaffen. Manche Erweiterungen wirken sich auch positiv oder sogar negativ auf Eure Gesundheit aus.";
            Tipps[22] = "Denkt regelmäßig an die Renovierung Eures Wohnsitzes. So verhindert Ihr einen Wertverlust und sorgt dafür, dass Euer Ansehen nicht leidet.";
            Tipps[23] = "Der Verkauf Eurer Ware in einer anderen Stadt beschert Euch sehr häufig einen höheren Erlös. Bedenkt bei Euren Plänen aber auch, dass Eure Karawane überfallen werden kann!";
            Tipps[24] = "Überlegt Euch gut, ob Ihr wirklich an den Kosten der Karawane für Eure Waren sparen wollt. Gut bezahlte Karawanen können durch Schleichwege die Chance auf Überfälle verringern.";
            Tipps[25] = "Die Sicherheit bzw. Tarnung einer Söldnerburg oder eines Räuberlagers gibt Auskunft darüber, wie hoch der Schutz gegen eine Eroberung ausfällt. Der Zustand hat ebenfalls großen Einfluss darauf.";
            Tipps[26] = "Wollt Ihr eine schlagkräftige Truppe in Eurer Burg oder Eurem Räuberlager aufstellen, so achtet darauf, eine ausgewogene Mischung von Kämpfern anzuheuern. Sie haben alle unterschiedliche Stärken und Schwächen.";
            Tipps[27] = "Werden die Räuberbanden bei ihren Überfällen auf die Karawanen regelmäßig von Söldnern besiegt, sinkt ihre Moral und damit auch die Chance auf weitere Überfälle.";
            Tipps[28] = "Habt Ihr Interesse am Kauf einer Söldnerburg oder eines Räuberlagers, so schadet es nicht, sich mit dem Eigentümer gut zu stellen.";
            Tipps[29] = "Achtet während der Umwerbung auf die Reaktion Eures (hoffentlich zukünftigen) Partners auf Eure Geschenke. Neben dem Preis spielt auch sein Charakter eine Rolle!";

            TippsMaxIndex = 29;

            #endregion

            #region Religion
            RelFreiID = 0;
            RelMinID = 0;
            RelMaxID = 3;
            RelKathID = 1;
            RelEvanID = 2;
            ReligionsNamen = new string[RelMaxID];
            ReligionsNamen[1] = "katholisch";
            ReligionsNamen[2] = "evangelisch";
            #endregion

            #region Ämter
            MaxAmtStadtID = 17;
            MaxAmtLandID = 34;
            MaxAmtID = 49;
            Aemter = new Amt[MaxAmtID];

            //Die Basisklasse, wenn jemand kein Amt trägt:
            Aemter[0] = new Amt(0, "Ohne Amt;Ohne Amt", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            //Stufe 0 - Politische Ämter
            Aemter[1] = new AmtRatsherr(1, "Ratsherr;Ratsherrin", 100, 20, 5, 40, 4, 5, 6, 1, 3, 700);
            Aemter[2] = new AmtRatsherr(2, "Ratsherr;Ratsherrin", 100, 20, 5, 40, 4, 5, 6, 1, 3, 700);
            Aemter[3] = new AmtRatsherr(3, "Ratsherr;Ratsherrin", 100, 20, 5, 40, 4, 5, 6, 1, 3, 700);
            Aemter[4] = new AmtBaumeister(4, "Baumeister;Baumeisterin", 50, 5, 10, 10, 7, 0, 0, 3, 7, 1000);
            Aemter[5] = new AmtRichter(5, "Richter;Richterin", 50, 5, 10, 5, 7, 0, 0, 3, 8, 900);
            Aemter[6] = new AmtKaemmerer(6, "Kämmerer;Kämmerin", 50, 10, 10, 20, 7, 0, 0, 3, 8, 800);
            Aemter[7] = new AmtBuergermeister(7, "Bürgermeister;Bürgermeisterin", 25, 30, 20, 30, 22, 0, 0, 5, 12, 1500);
            //Stufe 0 - Kirchliche Ämter
            Aemter[8] = new AmtPriester(8, "Priester;Priesterin", 50, -10, 10, -10, 10, 0, 0, 2, 4, 200);
            Aemter[9] = new AmtPriester(9, "Priester;Priesterin", 50, -10, 10, -10, 10, 0, 0, 2, 4, 200);
            Aemter[10] = new AmtDomherr(10, "Domherr;Domherrin", 25, 0, 25, 10, 27, 0, 0, 4, 10, 700);
            //Stufe 0 - Militärische Ämter
            Aemter[11] = new AmtStadtwache(11, "Stadtwache;Stadtwache", 100, 50, 3, 30, 14, 15, 0, 1, 2, 150);
            Aemter[12] = new AmtFolterknecht(12, "Folterknecht;Foltermagd", 100, 50, 3, 30, 14, 15, 0, 1, 2, 150);
            Aemter[13] = new AmtHenker(13, "Henker;Henkerin", 100, 50, 3, 30, 14, 15, 0, 1, 2, 150);
            Aemter[14] = new AmtWachkommandant(14, "Wachkommandant;Wachkommandantin", 100, 60, 8, 40, 16, 0, 0, 3, 6, 350);
            Aemter[15] = new AmtKerkermeister(15, "Kerkermeister;Kerkermeisterin", 100, 60, 8, 40, 16, 0, 0, 3, 6, 350);
            Aemter[16] = new AmtStadtkommandant(16, "Stadtkommandant;Stadtkommandantin", 50, 50, 15, 30, 33, 0, 0, 5, 10, 800);
            //Stufe 1 - Politische Ämter
            Aemter[17] = new AmtGeheimrat(17, "Geheimrat;Geheimrätin", 20, 40, 30, 50, 20, 21, 0, 6, 15, 2000);
            Aemter[18] = new AmtGeheimrat(18, "Geheimrat;Geheimrätin", 20, 40, 30, 50, 20, 21, 0, 6, 15, 2000);
            Aemter[19] = new AmtGeheimrat(19, "Geheimrat;Geheimrätin", 20, 40, 30, 50, 20, 21, 0, 6, 15, 200);
            Aemter[20] = new AmtJustizberater(20, "Justizberater;Justizberaterin", 10, 40, 50, 20, 22, 0, 0, 8, 25, 5000);
            Aemter[21] = new AmtFinanzberater(21, "Finanzberater;Finanzberaterin", 10, 40, 50, 20, 22, 0, 0, 8, 28, 5000);
            Aemter[22] = new AmtVogt(22, "Vogt;Vögtin", 5, 40, 65, 35, 39, 0, 0, 10, 40, 8000);
            //Stufe 1 - Kirchliche Ämter
            Aemter[23] = new AmtKellermeister(23, "Kellermeister;Kellermeisterin", 12, 0, 20, 20, 25, 26, 0, 6, 12, 500);
            Aemter[24] = new AmtSakristan(24, "Sakristan;Sakristanin", 12, 0, 20, 20, 25, 26, 0, 6, 14, 700);
            Aemter[25] = new AmtDiakon(25, "Diakon;Diakonin", 6, -10, 35, -10, 0, 0, 0, 8, 24, 1600);
            Aemter[26] = new AmtAbt(26, "Abt;Äbtissin", 6, 0, 35, -10, 0, 0, 0, 8, 24, 1300);
            Aemter[27] = new AmtBischof(27, "Bischof;Bischöfin", 3, -20, 50, -15, 42, 0, 0, 10, 36, 4000);
            //Stufe 1 - Militärische Ämter
            Aemter[28] = new AmtstvBefehlshaber(28, "Stv. Befehlshaber;Stv. Befehlshaberin", 30, 50, 20, 50, 31, 32, 0, 6, 10, 1000);
            Aemter[29] = new AmtZoellner(29, "Zöllner;Zöllnerin", 30, 50, 20, 50, 31, 32, 0, 6, 10, 1000);
            Aemter[30] = new AmtZoellner(30, "Zöllner;Zöllnerin", 30, 50, 20, 50, 31, 32, 0, 6, 10, 1000);
            Aemter[31] = new AmtBefehlshaber(31, "Befehlshaber;Befehlshaberin", 15, 55, 30, 55, 33, 0, 0, 8, 20, 2000);
            Aemter[32] = new AmtZollmeister(32, "Zollmeister;Zollmeisterin", 15, 55, 30, 55, 33, 0, 0, 8, 20, 2000);
            Aemter[33] = new AmtHauptmann(33, "Hauptmann;Hauptfrau", 8, 65, 40, 65, 48, 0, 0, 10, 30, 3000);
            //Stufe 2 - Politische Ämter
            Aemter[34] = new AmtHofrat(34, "Hofrat;Hofrätin", 8, 70, 65, 70, 37, 38, 0, 11, 35, 10000);
            Aemter[35] = new AmtHofrat(35, "Hofrat;Hofrätin", 8, 70, 65, 70, 37, 38, 0, 11, 35, 10000);
            Aemter[36] = new AmtHofrat(36, "Hofrat;Hofrätin", 8, 70, 65, 70, 37, 38, 0, 11, 35, 10000);
            Aemter[37] = new AmtJustizminister(37, "Justizminister;Justizministerin", 3, 60, 80, 50, 39, 0, 0, 13, 60, 15000);
            Aemter[38] = new AmtFinanzminister(38, "Finanzminister;Finanzministerin", 3, 70, 80, 60, 39, 0, 0, 13, 60, 20000);
            Aemter[39] = new AmtRegent(39, "Regent;Regentin", 1, 50, 100, 60, 0, 0, 0, 15, 100, 50000);
            //Stufe 2 - Kirchliche Ämter
            Aemter[40] = new AmtInquisitor(40, "Inquisitor;Inquisitorin", 2, 50, 85, 60, 42, 0, 0, 12, 55, 5000);
            Aemter[41] = new AmtErzdiakon(41, "Erzdiakon;Erzdiakonin", 2, 50, 85, 60, 42, 0, 0, 12, 65, 8000);
            Aemter[42] = new AmtErzbischof(42, "Erzbischof;Erzbischöfin", 1, -50, 95, -50, 0, 0, 0, 14, 90, 20000);
            //Stufe 2 - Militärische Ämter
            Aemter[43] = new AmtOffizier(43, "Offizier;Offizierin", 8, 80, 60, 70, 46, 47, 0, 11, 40, 4000);
            Aemter[44] = new AmtOffizier(44, "Offizier;Offizierin", 8, 80, 60, 70, 46, 47, 0, 11, 40, 4000);
            Aemter[45] = new AmtOffizier(45, "Offizier;Offizierin", 8, 80, 60, 70, 46, 47, 0, 11, 40, 4000);
            Aemter[46] = new AmtMarschall(46, "Marschall;Marschallin", 4, 90, 80, 75, 48, 0, 0, 13, 55, 8000);
            Aemter[47] = new AmtMarschall(47, "Marschall;Marschallin", 4, 90, 80, 75, 48, 0, 0, 13, 55, 8000);
            Aemter[48] = new AmtFeldmarschall(48, "Feldmarschall;Feldmarschallin", 2, 100, 90, 80, 0, 0, 0, 15, 80, 16000);
            #endregion

            #region Spieler
            KIminVerblJahre = 30;
            KImaxVerblJahre = 40;
            maxAlter = 60;
            StartAlter = 15;

            HumMinVerblJahre = KIminVerblJahre; // maxAlter - StartAlter - 10;
            HumMaxVerblJahre = KImaxVerblJahre; // maxAlter - StartAlter;

            minSterbeAlter = 35;
            MinTitelStadtEbene = 1;
            MinTitelLandEbene = 3;
            MinTitelReichsEbene = 5;

            MaxHumSpielerAnzahl = 9;
            MinGesundheit = 10;
            maxGesundheit = 100;

            minKindSlotNr = 1;
            maxKinderAnzahl = 5;
            maxKrediteAnzahl = 4;

            minNameLength = 3;
            maxNameLength = 12;
            NeuerSpielerRohwahlkosten = 100;
            NeuerSpielerStadtwahlkosten = 150;

            ChanceFuerKind = 3;
            ChanceFuerKindStirbt = 10;

            StartGold = 1000;
            Startlagerraum = 30;

            MaennerFrauenGrenze = 200;
            minKIID = 10;
            maxKIID = 400;

            ErmordungProzentsatz = 0.5;
            ErmordungsChance = 3;
            VergifteterWeinChance = 5;

            maxAnzahlSkills = 11;

            AnsehenProTaler = 2500;

            Kirchenzehnt = 0.1;
            Konvertierkosten = 2000;
            Austrittskosten = 10000;
            DeliktpunktPreis = 1000;
            #endregion

            #region KINamen
            KINames = new string[maxKIID];

            #region Männernamen
            KINames[10] = "Aaron";
            KINames[11] = "Adalbert";
            KINames[12] = "Adam";
            KINames[13] = "Albertus";
            KINames[14] = "Albin";
            KINames[15] = "Alexander";
            KINames[16] = "Andreas";
            KINames[17] = "Angus";
            KINames[18] = "Anselm";
            KINames[19] = "Anton";
            KINames[20] = "Arnd";
            KINames[21] = "Arnulf";
            KINames[22] = "Attila";
            KINames[23] = "Auberlin";
            KINames[24] = "Balthasar";
            KINames[25] = "Baratheon";
            KINames[26] = "Barbossa";
            KINames[27] = "Benjen";
            KINames[28] = "Benno";
            KINames[29] = "Bernhard";
            KINames[30] = "Berthold";
            KINames[31] = "Bertram";
            KINames[32] = "Blanko";
            KINames[33] = "Boris";
            KINames[34] = "Bran";
            KINames[35] = "Brutus";
            KINames[36] = "Burchart";
            KINames[37] = "Burckhart";
            KINames[38] = "Christopher";
            KINames[39] = "Conan";
            KINames[40] = "Daniel";
            KINames[41] = "Dante";
            KINames[42] = "Deckard";
            KINames[43] = "Dennis";
            KINames[44] = "Dermod";
            KINames[45] = "Diether";
            KINames[46] = "Dietmar";
            KINames[47] = "Dietrich";
            KINames[48] = "Douglas";
            KINames[49] = "Drogo";
            KINames[50] = "Earl";
            KINames[51] = "Edelbert";
            KINames[52] = "Edmund";
            KINames[53] = "Eduard";
            KINames[54] = "Elmar";
            KINames[55] = "Emanuel";
            KINames[56] = "Endres";
            KINames[57] = "Erasmus";
            KINames[58] = "Erhardt";
            KINames[59] = "Erich";
            KINames[60] = "Ernel";
            KINames[61] = "Errol";
            KINames[62] = "Erwin";
            KINames[63] = "Ewalt";
            KINames[64] = "Felix";
            KINames[65] = "Fletscher";
            KINames[66] = "Franz";
            KINames[67] = "Friedbert";
            KINames[68] = "Friedhelm";
            KINames[69] = "Friedjof";
            KINames[70] = "Friedrich";
            KINames[71] = "Fritz";
            KINames[72] = "Gannone";
            KINames[73] = "Garret";
            KINames[74] = "Gavin";
            KINames[75] = "Geoffrey";
            KINames[76] = "Georg";
            KINames[77] = "Gerhart";
            KINames[78] = "Gernot";
            KINames[79] = "Gisbert";
            KINames[80] = "Goethe";
            KINames[81] = "Grantelo";
            KINames[82] = "Gregor";
            KINames[83] = "Guntram";
            KINames[84] = "Gustav";
            KINames[85] = "Hagen";
            KINames[86] = "Halmar";
            KINames[87] = "Hans";
            KINames[88] = "Harald";
            KINames[89] = "Heiko";
            KINames[90] = "Heinrich";
            KINames[91] = "Heldenmuth";
            KINames[92] = "Henry";
            KINames[93] = "Herman";
            KINames[94] = "Honker";
            KINames[95] = "Honn";
            KINames[96] = "Horace";
            KINames[97] = "Hoscher";
            KINames[98] = "Hubert";
            KINames[99] = "Humbert";
            KINames[100] = "Igantius";
            KINames[101] = "Iterer";
            KINames[102] = "Ivan";
            KINames[103] = "Jaime";
            KINames[104] = "Jakob";
            KINames[105] = "Jasper";
            KINames[106] = "Jeronimus";
            KINames[107] = "Jezarus";
            KINames[108] = "Johannes";
            KINames[109] = "Jonas";
            KINames[110] = "Jonathan";
            KINames[111] = "Josef";
            KINames[112] = "Julian";
            KINames[113] = "Julius";
            KINames[114] = "Kalcher";
            KINames[115] = "Karl";
            KINames[116] = "Karl";
            KINames[117] = "Karlmann";
            KINames[118] = "Kasper";
            KINames[119] = "Kirbo";
            KINames[120] = "Kisamo";
            KINames[121] = "Klaus";
            KINames[122] = "Klemens";
            KINames[123] = "Konrad";
            KINames[124] = "Kunibert";
            KINames[125] = "Kunibert";
            KINames[126] = "Leibniz";
            KINames[127] = "Leo";
            KINames[128] = "Leonhart";
            KINames[129] = "Leonidas";
            KINames[130] = "Linker";
            KINames[131] = "Liudgar";
            KINames[132] = "Loras";
            KINames[133] = "Louis";
            KINames[134] = "Luciano";
            KINames[135] = "Ludwig";
            KINames[136] = "Lukas";
            KINames[137] = "Makarius";
            KINames[138] = "Manuel";
            KINames[139] = "Marco";
            KINames[140] = "Mario";
            KINames[141] = "Martin";
            KINames[142] = "Marx";
            KINames[143] = "Maximin";
            KINames[144] = "Meinhard";
            KINames[145] = "Michael";
            KINames[146] = "Mortimer";
            KINames[147] = "Napoleon";
            KINames[148] = "Nelson";
            KINames[149] = "Nepomuk";
            KINames[150] = "Neville";
            KINames[151] = "Oliver";
            KINames[152] = "Otfried";
            KINames[153] = "Otis";
            KINames[154] = "Otto";
            KINames[155] = "Ottokar";
            KINames[156] = "Paul";
            KINames[157] = "Peiner";
            KINames[158] = "Peter";
            KINames[159] = "Pluto";
            KINames[160] = "Raphael";
            KINames[161] = "Reinald";
            KINames[162] = "Reinhold";
            KINames[163] = "Rene";
            KINames[164] = "Richard";
            KINames[165] = "Robin";
            KINames[166] = "Rodrik";
            KINames[167] = "Ronald";
            KINames[168] = "Rudolf";
            KINames[169] = "Ruprecht";
            KINames[170] = "Russel";
            KINames[171] = "Ruthart";
            KINames[172] = "Samuel";
            KINames[173] = "Sebastian";
            KINames[174] = "Sigmund";
            KINames[175] = "Silvester";
            KINames[176] = "Sixtus";
            KINames[177] = "Stanis";
            KINames[178] = "Stefan";
            KINames[179] = "Sumpfer";
            KINames[180] = "Sven";
            KINames[181] = "Tadelo";
            KINames[182] = "Theon";
            KINames[183] = "Thomas";
            KINames[184] = "Timon";
            KINames[185] = "Tyrion";
            KINames[186] = "Ulrich";
            KINames[187] = "Volker";
            KINames[188] = "Walter";
            KINames[189] = "Wart";
            KINames[190] = "Wentzel";
            KINames[191] = "Wernhart";
            KINames[192] = "Willhelm";
            KINames[193] = "William";
            KINames[194] = "Wolfgang";
            KINames[195] = "Wolfger";
            KINames[196] = "Wolfram";
            KINames[197] = "Wolfram";
            KINames[198] = "Wulf";
            KINames[199] = "Zacharias";
            #endregion

            #region Frauennamen
            KINames[200] = "Adela";
            KINames[201] = "Adelhaid";
            KINames[202] = "Adelice";
            KINames[203] = "Adey";
            KINames[204] = "Adora";
            KINames[205] = "Adorne";
            KINames[206] = "Adrea";
            KINames[207] = "Aeldra";
            KINames[208] = "Agate";
            KINames[209] = "Agnes";
            KINames[210] = "Ailyn";
            KINames[211] = "Aindrea";
            KINames[212] = "Alawa";
            KINames[213] = "Alethea";
            KINames[214] = "Aletta";
            KINames[215] = "Alexia";
            KINames[216] = "Alfreda";
            KINames[217] = "Alliser";
            KINames[218] = "Allison";
            KINames[219] = "Anatola";
            KINames[220] = "Angalla";
            KINames[221] = "Ange";
            KINames[222] = "Anja";
            KINames[223] = "Anna";
            KINames[224] = "Annabel";
            KINames[225] = "Anne";
            KINames[226] = "Annelise ";
            KINames[227] = "Annice";
            KINames[228] = "Aponi";
            KINames[229] = "Ariane";
            KINames[230] = "Arya";
            KINames[231] = "Asena";
            KINames[232] = "Babbette";
            KINames[233] = "Barbara";
            KINames[234] = "Barbette";
            KINames[235] = "Beate";
            KINames[236] = "Beatrix";
            KINames[237] = "Bena";
            KINames[238] = "Bende";
            KINames[239] = "Bessy";
            KINames[240] = "Beth";
            KINames[241] = "Betteann";
            KINames[242] = "Bianca";
            KINames[243] = "Brianne";
            KINames[244] = "Brienne";
            KINames[245] = "Brigitte";
            KINames[246] = "Brinn";
            KINames[247] = "Brita";
            KINames[248] = "Caren";
            KINames[249] = "Caressa";
            KINames[250] = "Caroline";
            KINames[251] = "Cathelyn";
            KINames[252] = "Celine";
            KINames[253] = "Christine";
            KINames[254] = "Concettina";
            KINames[255] = "Constantia";
            KINames[256] = "Dalenna";
            KINames[257] = "Darlleen";
            KINames[258] = "Dasha";
            KINames[259] = "Deike";
            KINames[260] = "Deloris";
            KINames[261] = "Dena";
            KINames[262] = "Diana";
            KINames[263] = "Didem";
            KINames[264] = "Doris";
            KINames[265] = "Dorothea";
            KINames[266] = "Duretta";
            KINames[267] = "Earwine";
            KINames[268] = "Edeltraut";
            KINames[269] = "Elisabeth";
            KINames[270] = "Elke";
            KINames[271] = "Elliot";
            KINames[272] = "Elvira";
            KINames[273] = "Engel";
            KINames[274] = "Engelberhta";
            KINames[275] = "Eva";
            KINames[276] = "Felicita";
            KINames[277] = "Flavia";
            KINames[278] = "Franziska";
            KINames[279] = "Frauke";
            KINames[280] = "Frederica";
            KINames[281] = "Gerlinde";
            KINames[282] = "Gertrude";
            KINames[283] = "Gertrude";
            KINames[284] = "Gestada";
            KINames[285] = "Gisela";
            KINames[286] = "Gretchen";
            KINames[287] = "Hande";
            KINames[288] = "Hannah";
            KINames[289] = "Helena";
            KINames[290] = "Helene";
            KINames[291] = "Herthe";
            KINames[292] = "Hilde";
            KINames[293] = "Ilona";
            KINames[294] = "Ingred";
            KINames[295] = "Irene";
            KINames[296] = "Jana";
            KINames[297] = "Januara";
            KINames[298] = "Jella";
            KINames[299] = "Jenna";
            KINames[300] = "Johanna";
            KINames[301] = "Jonata";
            KINames[302] = "Juliana";
            KINames[303] = "Kanti";
            KINames[304] = "Karoline";
            KINames[305] = "Kasa";
            KINames[306] = "Katherina";
            KINames[307] = "Kungundt";
            KINames[308] = "Kunigunde";
            KINames[309] = "Laguna";
            KINames[310] = "Landia";
            KINames[311] = "Laura";
            KINames[312] = "Lea";
            KINames[313] = "Lilias";
            KINames[314] = "Lisi";
            KINames[315] = "Lola";
            KINames[316] = "Lotta";
            KINames[317] = "Lotte";
            KINames[318] = "Lucia";
            KINames[319] = "Luise";
            KINames[320] = "Lyla";
            KINames[321] = "Lysa";
            KINames[322] = "Madeleine";
            KINames[323] = "Magdalena";
            KINames[324] = "Mara";
            KINames[325] = "Margot";
            KINames[326] = "Margret";
            KINames[327] = "Maria";
            KINames[328] = "Marika";
            KINames[329] = "Marion";
            KINames[330] = "Martha";
            KINames[331] = "Mathilde";
            KINames[332] = "Maureen";
            KINames[333] = "Maya";
            KINames[334] = "Mechtild";
            KINames[335] = "Melanie";
            KINames[336] = "Melisa";
            KINames[337] = "Melisandre";
            KINames[338] = "Mia";
            KINames[339] = "Miranda";
            KINames[340] = "Mirijam";
            KINames[341] = "Misae";
            KINames[342] = "Monika";
            KINames[343] = "Nadine";
            KINames[344] = "Nathalie";
            KINames[345] = "Nicole";
            KINames[346] = "Niki";
            KINames[347] = "Nina";
            KINames[348] = "Nira";
            KINames[349] = "Nopricht";
            KINames[350] = "Nuna";
            KINames[351] = "Olga";
            KINames[352] = "Ortrud";
            KINames[353] = "Otilia";
            KINames[354] = "Palma";
            KINames[355] = "Paola";
            KINames[356] = "Peternella";
            KINames[357] = "Petra";
            KINames[358] = "Philine";
            KINames[359] = "Ramona";
            KINames[360] = "Randa";
            KINames[361] = "Ranita";
            KINames[362] = "Resi";
            KINames[363] = "Reusin";
            KINames[364] = "Sabine";
            KINames[365] = "Sabrina";
            KINames[366] = "Sandra";
            KINames[367] = "Sansa";
            KINames[368] = "Sarah";
            KINames[369] = "Segrima";
            KINames[370] = "Semerine";
            KINames[371] = "Shaka";
            KINames[372] = "Siegesmunde";
            KINames[373] = "Sigrid";
            KINames[374] = "Silvana";
            KINames[375] = "Silvia";
            KINames[376] = "Simona";
            KINames[377] = "Sophie";
            KINames[378] = "Sorcha";
            KINames[379] = "Sosanna";
            KINames[380] = "Stina";
            KINames[381] = "Susanne";
            KINames[382] = "Sybille";
            KINames[383] = "Tamara";
            KINames[384] = "Tina";
            KINames[385] = "Tiva";
            KINames[386] = "Truda";
            KINames[387] = "Ursel";
            KINames[388] = "Valerie";
            KINames[389] = "Vanessa";
            KINames[390] = "Veronika";
            KINames[391] = "Viola";
            KINames[392] = "Violante";
            KINames[393] = "Walburga";
            KINames[394] = "Waltraud";
            KINames[395] = "Wanda";
            KINames[396] = "Winifrid";
            KINames[397] = "Xanthippe";
            KINames[398] = "Xebrima";
            KINames[399] = "Zihna";
            #endregion
            #endregion

            #region Karawanen
            DefaultKarawane = 1;
            maxKarawane = 3;
            minKarawane = 0;

            Kara = new Karawane[maxKarawane];
            Kara[0] = new Karawane(0, 100, 100, 100, 20, 100, "Billiger geht es nicht!");
            Kara[1] = new Karawane(1, 500, 75, 200, 30, 100, "Eure Waren verschwinden,\nwährend Euer Geldbeutel wächst.");
            Kara[2] = new Karawane(2, 1000, 50, 300, 40, 100, "Unser Angebot ist unübertroffen,\nwir kennen alle Schleichwege!");
            #endregion

            #region Anwesen
            StartHausID = 1;
            MaxHausID = 10;

            Haeuser = new Haus[MaxHausID];

            #region Zustandsbezeichnungen
            HausZustandsbezeichnung[] aHauszustandBezeichnungenDer;
            HausZustandsbezeichnung[] aHauszustandBezeichnungenDie;
            HausZustandsbezeichnung[] aHauszustandBezeichnungenDas;
            int iMaxZustandsbezeichnungen = 5;

            aHauszustandBezeichnungenDer = new HausZustandsbezeichnung[iMaxZustandsbezeichnungen];
            aHauszustandBezeichnungenDer[0] = new HausZustandsbezeichnung("prächtiger", 80, 100);
            aHauszustandBezeichnungenDer[1] = new HausZustandsbezeichnung("schöner", 60, 79);
            aHauszustandBezeichnungenDer[2] = new HausZustandsbezeichnung("gut erhaltener", 40, 59);
            aHauszustandBezeichnungenDer[3] = new HausZustandsbezeichnung("verwahrloster", 20, 39);
            aHauszustandBezeichnungenDer[4] = new HausZustandsbezeichnung("verfallener", 0, 19);

            aHauszustandBezeichnungenDie = new HausZustandsbezeichnung[iMaxZustandsbezeichnungen];
            aHauszustandBezeichnungenDie[0] = new HausZustandsbezeichnung("prächtige", 80, 100);
            aHauszustandBezeichnungenDie[1] = new HausZustandsbezeichnung("schöne", 60, 79);
            aHauszustandBezeichnungenDie[2] = new HausZustandsbezeichnung("gut erhaltene", 40, 59);
            aHauszustandBezeichnungenDie[3] = new HausZustandsbezeichnung("verwahrloste", 20, 39);
            aHauszustandBezeichnungenDie[4] = new HausZustandsbezeichnung("verfallene", 0, 19);

            aHauszustandBezeichnungenDas = new HausZustandsbezeichnung[iMaxZustandsbezeichnungen];
            aHauszustandBezeichnungenDas[0] = new HausZustandsbezeichnung("prächtiges", 80, 100);
            aHauszustandBezeichnungenDas[1] = new HausZustandsbezeichnung("schönes", 60, 79);
            aHauszustandBezeichnungenDas[2] = new HausZustandsbezeichnung("gut erhaltenes", 40, 59);
            aHauszustandBezeichnungenDas[3] = new HausZustandsbezeichnung("verwahrlostes", 20, 39);
            aHauszustandBezeichnungenDas[4] = new HausZustandsbezeichnung("verfallenes", 0, 19);
            #endregion

            #region Hauserweiterungen
            List<HausErweiterung> lstHausErweiterungenKate = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenHuette = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenHaus = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenBuergerhaus = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenLandhaus = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenVilla = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenLandsitz = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenBurg = new List<HausErweiterung>();
            List<HausErweiterung> lstHausErweiterungenSchloss = new List<HausErweiterung>();

            //Idee fürs Balancing: 
            //Die folgende Gleichung muss erfüllt sein: x + 2y = i wobei: 
            //i... ID des Hauses, x... Ansehensboni von Erw., y... Gesundheitsboni von Erw.
            //Für nähere Details siehe unter Dokumentation im Excel "Anwesen"
            lstHausErweiterungenKate.Add(new HausErweiterung(1, 0, "kleinem Gemüsegarten", "einen kleinen Gemüsegarten", 0, 1));
            lstHausErweiterungenKate.Add(new HausErweiterung(1, 1, "Sitzbank", "eine Sitzbank", 1, 0));
            lstHausErweiterungenKate.Add(new HausErweiterung(1, 2, "Blumenbeet", "ein Blumenbeet", 2, -1));

            lstHausErweiterungenHuette.Add(new HausErweiterung(2, 0, "kleinem Gärtchen", "ein kleines Gärtchen", 1, 1));
            lstHausErweiterungenHuette.Add(new HausErweiterung(2, 1, "Werkzeugschuppen", "einen Werkzeugschuppen", 2, 0));
            lstHausErweiterungenHuette.Add(new HausErweiterung(2, 2, "Gartenteich", "einen Gartenteich", 3, -1));

            lstHausErweiterungenHaus.Add(new HausErweiterung(3, 0, "Garten", "einen Garten", 1, 2));
            lstHausErweiterungenHaus.Add(new HausErweiterung(3, 1, "kleiner Laube", "eine kleine Laube", 3, 0));
            lstHausErweiterungenHaus.Add(new HausErweiterung(3, 2, "Teich", "einen Teich", 4, -1));

            lstHausErweiterungenBuergerhaus.Add(new HausErweiterung(4, 0, "Vorgarten", "einen Vorgarten", 2, 2));
            lstHausErweiterungenBuergerhaus.Add(new HausErweiterung(4, 1, "Nebengebäude", "ein Nebengebäude", 4, 0));
            lstHausErweiterungenBuergerhaus.Add(new HausErweiterung(4, 2, "Weiher", "einen Weiher", 6, -1));

            lstHausErweiterungenLandhaus.Add(new HausErweiterung(5, 0, "großem Gemüsegarten", "einen großen Gemüsegarten", 2, 3));
            lstHausErweiterungenLandhaus.Add(new HausErweiterung(5, 1, "Nebengebäuden", "mehrere Nebengebäude", 5, 0));
            lstHausErweiterungenLandhaus.Add(new HausErweiterung(5, 2, "Fischteich", "einen Fischteich", 7, -2));
            //lstHausErweiterungenLandhaus.Add(new HausErweiterung(5, 3, "Ländereien", "mehrere Ländereien"                           , 4,  0));

            lstHausErweiterungenVilla.Add(new HausErweiterung(6, 0, "Park", "einen Park", 3, 3));
            lstHausErweiterungenVilla.Add(new HausErweiterung(6, 1, "Nebengebäuden", "mehrere Nebengebäude", 6, 0));
            lstHausErweiterungenVilla.Add(new HausErweiterung(6, 2, "Springbrunnen", "einen Springbrunnen", 8, -2));

            lstHausErweiterungenLandsitz.Add(new HausErweiterung(7, 0, "weitläufigen Ländereien", "mehrere weitläufige Ländereien", 3, 4));
            lstHausErweiterungenLandsitz.Add(new HausErweiterung(7, 1, "Nebengebäuden", "mehrere Nebengebäude", 7, 0));
            lstHausErweiterungenLandsitz.Add(new HausErweiterung(7, 2, "Gehöft", "ein Gehöft", 10, -3));
            //lstHausErweiterungenLandsitz.Add(new HausErweiterung(7, 3, "Wäldchen", "ein Wäldchen"                                 , 1,  1));

            lstHausErweiterungenBurg.Add(new HausErweiterung(8, 0, "großem Park", "einen großen Park", 4, 4));
            lstHausErweiterungenBurg.Add(new HausErweiterung(8, 1, "Nebengebäuden", "mehrere Nebengebäude", 8, 0));
            lstHausErweiterungenBurg.Add(new HausErweiterung(8, 2, "Burggraben", "einen Burggraben", 11, -3));
            //lstHausErweiterungenBurg.Add(new HausErweiterung(8, 3, "weitläufigen Ländereien", "mehrere weitläufige Ländereien"      , 1,  0));
            //lstHausErweiterungenBurg.Add(new HausErweiterung(8, 4,                                , 1,  0));

            lstHausErweiterungenSchloss.Add(new HausErweiterung(9, 0, "herrschaftlichem Park", "einen herrschaftlichen Park", 4, 5));
            lstHausErweiterungenSchloss.Add(new HausErweiterung(9, 1, "Nebengebäuden", "mehrere Nebengebäude", 9, 0));
            lstHausErweiterungenSchloss.Add(new HausErweiterung(9, 2, "Stallungen", "mehrere Stallungen", 12, -3));
            //lstHausErweiterungenSchloss.Add(new HausErweiterung(9, 3, "weitläufigen Ländereien", "mehrere weitläufige Ländereien"   , 1,  0));
            //lstHausErweiterungenSchloss.Add(new HausErweiterung(9, 4, "moderne Skulpturen", "mehrere moderne Skulpturen"            , 4,  0));

            #endregion

            Haeuser[0] = new Haus(0, "Kein Haus", 0, 0, 0, "Kein Haus", "", null, null);
            Haeuser[1] = new Haus(1, "Kate", -10, 1000, 1, "Kate", "Eure", aHauszustandBezeichnungenDie, lstHausErweiterungenKate);
            Haeuser[2] = new Haus(2, "Hütte", -5, 2000, 2, "Huette", "Eure", aHauszustandBezeichnungenDie, lstHausErweiterungenHuette);
            Haeuser[3] = new Haus(3, "Haus", 0, 3500, 3, "Haus", "Euer", aHauszustandBezeichnungenDas, lstHausErweiterungenHaus);
            Haeuser[4] = new Haus(4, "Bürgerhaus", 3, 6000, 4, "Buergerhaus", "Euer", aHauszustandBezeichnungenDas, lstHausErweiterungenBuergerhaus);
            Haeuser[5] = new Haus(5, "Landhaus", 5, 10000, 5, "Landhaus", "Euer", aHauszustandBezeichnungenDas, lstHausErweiterungenLandhaus);
            Haeuser[6] = new Haus(6, "Villa", 10, 20000, 6, "Villa", "Eure", aHauszustandBezeichnungenDie, lstHausErweiterungenVilla);
            Haeuser[7] = new Haus(7, "Landsitz", 17, 50000, 7, "Landsitz", "Euer", aHauszustandBezeichnungenDer, lstHausErweiterungenLandsitz);
            Haeuser[8] = new Haus(8, "Burg", 25, 100000, 8, "Burg", "Eure", aHauszustandBezeichnungenDie, lstHausErweiterungenBurg);
            Haeuser[9] = new Haus(9, "Schloss", 50, 250000, 9, "Schloss", "Euer", aHauszustandBezeichnungenDas, lstHausErweiterungenSchloss);
            #endregion

            #region Privilegien anlegen

            Privilegien = privilegien;  // Von außen als Interface übergeben, da die eigentliche Implementierung der einzelnen Privilegien im jeweiligen Client und nicht in der Lib existiert

            #endregion

            #region Rohstoffe anlegen
            maxRohID = 22;
            maxRohStufe1ID = 8;
            maxRohStufe2ID = 15;
            EinkaufspreisZuschlag = 3;
            MinRohGrundPreis = 5;
            MaxRohGrundPreis = 40;

            #endregion

            #region Sonstiges
            KreditZinsMin = 10;
            KreditZinsMax = 20;
            KupplerProzente = 0.05;
            maxKorruptionsGelder = 700;
            MaxKatastrophen = 5;
            Werbegeschenkfaktor = 0.005;
            KITeilnehmerProWahl = 2;

            maxTexteTodesursachen = 10;
            TexteTodesursachen = new string[maxTexteTodesursachen];
            TexteTodesursachen[0] = "Ihr wurdet vom Blitz erschlagen! Das Glück war Euch noch nie hold!";
            TexteTodesursachen[1] = "Ihr seid über eine Treppe gestolpert und erlagt den Verletzungen!";
            TexteTodesursachen[2] = "Die Pest hat Euch dahingerafft!";
            TexteTodesursachen[3] = "In Hektik habt Ihr eine vorbeifahrende Droschke übersehen und wurdet überfahren!";
            TexteTodesursachen[4] = "Die Tuberkulose brachte Euch diesen Dezember den Tod!";
            TexteTodesursachen[5] = "Euer abenteuerliches Leben wurde von einer fortgeschrittenen Syphilis nun beendet!";
            TexteTodesursachen[6] = "Eure unbehandelte Cholera hat Euch Geizhals den Tod gebracht!";
            TexteTodesursachen[7] = "Ein böser Virus hat Euch Stück für Stück unter die Erde gebracht!";
            TexteTodesursachen[8] = "Nach einem Festmahl zu Gast habt Ihr am Lokus einem qualvollen Gifttod!";
            TexteTodesursachen[9] = "In einer kalten Februarnacht erlagt Ihr einer Grippe!";

            MaxAbsetzSympathie = 10;
            MaxWahlKandidaten = KITeilnehmerProWahl + MaxHumSpielerAnzahl;
            MaxWahlWaehler = 3;

            maxSchulden = -500;
            KerkerGesundheit = 10;
            KerkerAnsehen = 10;

            maxArbeiterAnzahl = 99;
            maxProdSlots = 2;
            maxWerkstaettenProStadt = 6;
            StartJahr = 1600;
            maxAnzahlVonEinemRohstoff = 999999;

            maxAnzahlWahlen = 200;
            maxAnzahlAmtsenthebungen = 120;

            KartenSpielenMinTaler = 100;
            kartenSpielenProzentsatz = 0.1;

            Rnd = new Random();
            #endregion

            #region Stadt
            LagerraumBasisPreis = 10;
            MaxReichtum = 14;
            MaxCrime = 5;

            StandardUmsatzSteuer = 0.15;
            maxUmsatzsteuer = 0.25;
            minUmsatzsteuer = 0.05;
            #endregion

            #region Strafarten

            Strafarten = strafarten;  // Von außen als Interface übergeben, da die eigentliche Implementierung der einzelnen Strafen im jeweiligen Client und nicht in der Lib existiert

            #endregion

            #region Titel
            MaxTitelID = 10;
            Tit = new Adelstitel[MaxTitelID];

            Tit[0] = new Herr(0, "Herr", "Frau", 0);
            Tit[1] = new Buerger(1, "Bürger", "Bürgerin", 5);
            Tit[2] = new Edelmann(2, "Edelmann", "Edelfrau", 10);
            Tit[3] = new Ritter(3, "Ritter", "Hofdame", 20);
            Tit[4] = new Landherr(4, "Landherr", "Landfrau", 35);
            Tit[5] = new Freiherr(5, "Freiherr", "Freifrau", 55);
            Tit[6] = new Baron(6, "Baron", "Baronin", 75);
            Tit[7] = new Graf(7, "Graf", "Gräfin", 100);
            Tit[8] = new Herzog(8, "Herzog", "Herzogin", 125);
            Tit[9] = new Fuerst(9, "Fürst", "Fürstin", 150);
            #endregion

            #region Werbegeschenke anlegen

            WerbegeschenkGrenzeBillig = 11;
            WerbegeschenkGrenzeMittelteuer = 21;
            maxWerbegeschenke = 31;

            WG = new Werbegeschenk[maxWerbegeschenke];

            //Kein Geschenk
            WG[0] = new Werbegeschenk(0, 0, 0, 0, 0.0000, 0, "Das ist mir alles zu teuer!");

            //Billige Geschenke
            WG[1] = new Werbegeschenk(1, 4, 0, 1, 0.0000, 0, "Einen Spaziergang durch den Park");
            WG[2] = new Werbegeschenk(2, 5, 0, 2, 0.0103, 10, "Eine wohlduftende Rose");
            WG[3] = new Werbegeschenk(3, 0, 0, 4, 0.0105, 20, "Einen Kochtopf");
            WG[4] = new Werbegeschenk(4, 0, 3, 4, 0.0105, 30, "Eine Axt");
            WG[5] = new Werbegeschenk(5, 0, 0, 5, 0.0105, 40, "Einen geflochtenen Korb");
            WG[6] = new Werbegeschenk(6, 0, 5, 4, 0.0107, 50, "Eine Gerte");
            WG[7] = new Werbegeschenk(7, 0, 0, 6, 0.0110, 100, "Einen Kerzenleuchter");
            WG[8] = new Werbegeschenk(8, 2, 0, 6, 0.0110, 200, "Eine Theatervorführung");
            WG[9] = new Werbegeschenk(9, 6, 0, 6, 0.0115, 300, "Eine bronzene Halskette");
            WG[10] = new Werbegeschenk(10, 4, 0, 8, 0.0115, 400, "Ein Parfüm");

            //Mittelteure Geschenke
            WG[11] = new Werbegeschenk(11, 6, 0, 8, 0.0120, 500, "Einen Degen");
            WG[12] = new Werbegeschenk(12, 1, 0, 9, 0.0125, 550, "Ein Gemälde von Euch");
            WG[13] = new Werbegeschenk(13, 6, 0, 9, 0.0125, 600, "Eine kleine Reise");
            WG[14] = new Werbegeschenk(14, 4, 0, 10, 0.0125, 650, "Einen gravierten Silberring");
            WG[15] = new Werbegeschenk(15, 0, 0, 11, 0.0125, 700, "Ein maßgeschneidertes Gewand");
            WG[16] = new Werbegeschenk(16, 4, 0, 12, 0.0130, 750, "Ein Reitpferd");
            WG[17] = new Werbegeschenk(17, 0, 2, 13, 0.0135, 800, "Einige Leibeigene");
            WG[18] = new Werbegeschenk(18, 0, 0, 14, 0.0135, 850, "Einen Kupferstich von Euch");
            WG[19] = new Werbegeschenk(19, 0, 6, 14, 0.0140, 900, "Eine Kanone");
            WG[20] = new Werbegeschenk(20, 0, 9, 14, 0.0145, 950, "Ein paar Folterwerkzeuge");

            //Teure Geschenke
            WG[21] = new Werbegeschenk(21, 0, 0, 15, 0.0150, 1000, "Ein Paar goldbestickte Pantoffeln");
            WG[22] = new Werbegeschenk(22, 6, 0, 15, 0.0155, 1250, "Ein Waffenarsenal");
            WG[23] = new Werbegeschenk(23, 6, 0, 16, 0.0160, 1500, "Eine große Reise");
            WG[24] = new Werbegeschenk(24, 7, 2, 17, 0.0165, 2000, "Eine maßgefertigte Rüstung");
            WG[25] = new Werbegeschenk(25, 0, 0, 18, 0.0170, 2500, "Eine verzierte Kutsche");
            WG[26] = new Werbegeschenk(26, 0, 9, 18, 0.0175, 3000, "Eine Folterkammer");
            WG[27] = new Werbegeschenk(27, 0, 0, 19, 0.0180, 4000, "Eine Statue von Euch");
            WG[28] = new Werbegeschenk(28, 0, 0, 19, 0.0185, 5000, "Einen prächtigen Diamanten");
            WG[29] = new Werbegeschenk(29, 0, 0, 20, 0.0190, 7500, "Ein Schiff");
            WG[30] = new Werbegeschenk(30, 9, 0, 25, 0.0200, 10000, "Ein königliches Diadem");
            #endregion

            #region Werbereaktionen anlegen
            Werbereaktionen = new string[11];
            Werbereaktionen[0] = " ist schockiert.";
            Werbereaktionen[1] = " ist entsetzt.";
            Werbereaktionen[2] = " ist überrascht.";
            Werbereaktionen[3] = " sieht Euch fragend an.";
            Werbereaktionen[4] = " ist dankbar.";
            Werbereaktionen[5] = " lächelt verlegen den Boden an.";
            Werbereaktionen[6] = " freut sich.";
            Werbereaktionen[7] = " lächelt Euch lieblich an.";
            Werbereaktionen[8] = " ist glücklich.";
            Werbereaktionen[9] = " haucht Euch einen Kuss zu.";
            Werbereaktionen[10] = " ist verliebt.";
            #endregion

            #region Ereignisse

            var AktuellesDatum = DateTime.Now;

            Datumsereignisse = new List<Datumsereignis>
            {
                new Datumsereignis
                {
                    ID = 1,
                    Ueberschrift = "Weihnachten",
                    Nachricht = "Bei der diesjährigen Christmette gebt Ihr Euch besonders großherzig und füllt den Klingelbeutel reichlich mit {0}n. Nach der Messe winkt Euch der Priester herbei und dankt Euch überschwänglich vor einigen umstehenden Würdenträgern, in deren Gunst Ihr steigt.",
                    NurGueltigReligion = EnumGueltigkeitReligion.NurGueltigMitReligion,
                    TalerMultiplikator = -1,
                    AnsehenMultiplikator = 2,
                    GesundheitMultiplikator = 0,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 12, 24),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 12, 26, 23, 59, 59, 999)
                },
                new Datumsereignis
                {
                    ID = 1,
                    Ueberschrift = "Weihnachten",
                    Nachricht = "Es ist Heilig Abend! Langsam schlendert Ihr bei leichtem Schneefall an den Häusern Eures Heimatortes vorbei und lukt verstohlen in die festlich geschmückten Fenster. In einem spontanen Anfall von Großzügigkeit beschließt Ihr, Euren Arbeitern heute freizugeben und jedem ein wenig Extralohn in die Hand zu drücken. Ihr verliert {0}.",
                    NurGueltigReligion = EnumGueltigkeitReligion.NurGueltigOhneReligion,
                    TalerMultiplikator = -2,
                    AnsehenMultiplikator = 1,
                    GesundheitMultiplikator = 0,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 12, 24),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 12, 26, 23, 59, 59, 999)
                },
                new Datumsereignis
                {
                    ID = 2,
                    Ueberschrift = "Silvester",
                    Nachricht = "In diesem Jahr verbringt Ihr den letzten Tag des Jahres in Eurem Anwesen im Kreise Eurer Lieben und vertreibt Euch die Zeit am gemütlichem Kaminfeuer mit Bleigießen und gutem Essen. Als vorgezogenen Neujahrsvorsatz verzichtet Ihr dabei vollständig auf Alkohol.",
                    NurGueltigReligion = EnumGueltigkeitReligion.ReligionIstEgal,
                    TalerMultiplikator = 0,
                    AnsehenMultiplikator = 0,
                    GesundheitMultiplikator = 1,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 12, 31),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 12, 31, 23, 59, 59, 999)
                },
                new Datumsereignis
                {
                    ID = 2,
                    Ueberschrift = "Silvester",
                    Nachricht = "In diesem Jahr lasst Ihr es am letzten Tag des Jahres ordentlich krachen und bestellt in der örtlichen Schenke eine Runde nach der anderen für Euch und das halbe Dorf. Nach einem ausgewachsenen Kater am nächsten Tag stellt Ihr fest, dass Euch die Sause {0} gekostet hat.",
                    NurGueltigReligion = EnumGueltigkeitReligion.ReligionIstEgal,
                    TalerMultiplikator = -2,
                    AnsehenMultiplikator = 2,
                    GesundheitMultiplikator = -1,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 12, 31),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 12, 31, 23, 59, 59, 999)
                },
                new Datumsereignis
                {
                    ID = 3,
                    Ueberschrift = "Ostern",
                    Nachricht = "Bei den diesjährigen Oster- und Passionsspielen in Eurer Heimatstadt trefft Ihr zufällig einen alten Freund wieder und erfahrt, dass mittlerweile einflussreiche Würdenträger zu seinen Bekannten zählen. Nach angeregten Gesprächen und schwelgen in Erinnerungen sichert er Euch zu, bei Ihnen ein gutes Wort für Eure Geschäfte einzulegen.",
                    NurGueltigReligion = EnumGueltigkeitReligion.NurGueltigMitReligion,
                    TalerMultiplikator = 0,
                    AnsehenMultiplikator = 1,
                    GesundheitMultiplikator = 0,
                    NurAnOsternGueltig = true,
                    GueltigVonDatum = DateTime.MinValue,
                    GueltigBisDatum = DateTime.MinValue
                },
                new Datumsereignis
                {
                    ID = 3,
                    Ueberschrift = "Ostern",
                    Nachricht = "Dieses Jahr habt Ihr Euch der Fastenzeit in besonderem Maße entzogen, indem Ihr bei Tisch nach Herzenslust zugelangt habt. Als krönenden Abschluss veranstaltet Ihr an Ostern einen kleinen Festschmaus mit Wein und Bier im Überfluss. Am nächsten Tag stellt Ihr entsetzt fest, dass Euer Bauch deutlich rundlicher geworden ist.",
                    NurGueltigReligion = EnumGueltigkeitReligion.NurGueltigOhneReligion,
                    TalerMultiplikator = 0,
                    AnsehenMultiplikator = 0,
                    GesundheitMultiplikator = -1,
                    NurAnOsternGueltig = true,
                    GueltigVonDatum = DateTime.MinValue,
                    GueltigBisDatum = DateTime.MinValue
                },
                new Datumsereignis
                {
                    ID = 4,
                    Ueberschrift = "Neujahr",
                    Nachricht = "Gerade als Ihr Euch im tiefem Schnee aufmachen wollt, um Eure Neujahrsgedichte als Neujahrsgrüße an Eure lieben Freunde und Bekannten zu verteilen, trefft Ihr einen hohen Würdenträger, der sich von Eurer Geste sehr angetan zeigt. Als Ihr ihm ein Gedicht zusteckt, sichert er Euch zu, beim nächsten Ratstreffen ein gutes Wort für Euch einzulegen.",
                    NurGueltigReligion = EnumGueltigkeitReligion.ReligionIstEgal,
                    TalerMultiplikator = 0,
                    AnsehenMultiplikator = 1,
                    GesundheitMultiplikator = 0,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 1, 1),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 1, 1, 23, 59, 59, 999)
                },
                new Datumsereignis
                {
                    ID = 4,
                    Ueberschrift = "Neujahr",
                    Nachricht = "Bei der Neujahrspredigt fällt Euch in diesem Jahr ein edel gekleideter Fremder in einer der hinteren Reihe der Kirche auf. Am Ende der Messe verlässt er hastig das Gotteshaus und Ihr folgt ihm neugierig. Plötzlich fällt ihm ein schwerer Münzbeutel aus der Tasche, ohne dass er es bemerkt. Ihr eilt ihm nach doch er ist bereits im Nebel verschwunden. Ohne Scham steckt Ihr den Beutel ein und freut euch über die {0}. Welch' ein Jahresanfang!",
                    NurGueltigReligion = EnumGueltigkeitReligion.NurGueltigMitReligion,
                    TalerMultiplikator = 2,
                    AnsehenMultiplikator = 0,
                    GesundheitMultiplikator = 0,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 1, 1),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 1, 1, 23, 59, 59, 999)
                },
                new Datumsereignis
                {
                    ID = 4,
                    Ueberschrift = "Neujahr",
                    Nachricht = "Bei Eurem Stammtisch der Intellektuellen des Ortes in der hiesigen Schänke, an welchem Ihr statt der Neujahrspredigt teilnehmt, lasst Ihr Euch dieses Mal zu einigen unbedachten Äußerungen hinreißen. Kopfschüttelnd werdet Ihr schließlich mit der Zeche allein gelassen und Ihr habt das Gefühl, dass Euer Ansehen gelitten hat. Der Wirt verlangt {0}.",
                    NurGueltigReligion = EnumGueltigkeitReligion.NurGueltigOhneReligion,
                    TalerMultiplikator = -1,
                    AnsehenMultiplikator = -1,
                    GesundheitMultiplikator = 0,
                    NurAnOsternGueltig = false,
                    GueltigVonDatum = new DateTime(AktuellesDatum.Year, 1, 1),
                    GueltigBisDatum = new DateTime(AktuellesDatum.Year, 1, 1, 23, 59, 59, 999)
                }
            };

            #endregion
        }
        #endregion

        #region Einfache Getter
        public double GetKupplerProzente() { return KupplerProzente; }
        public int GetKreditZinsMin() { return KreditZinsMin; }
        public int GetKreditZinsMax() { return KreditZinsMax; }
        public int GetKITeilnehmerProWahl() { return KITeilnehmerProWahl; }
        public int GetDeliktpunktPreis() { return DeliktpunktPreis; }
        public double GetKirchenzehnt() { return Kirchenzehnt; }
        public int GetKonvertierkosten() { return Konvertierkosten; }
        public int GetAustrittskosten() { return Austrittskosten; }
        public int GetAnsehenProTaler() { return AnsehenProTaler; }
        public int GetMaxAnzahlSkills() { return maxAnzahlSkills; }
        public int GetKartenSpielenMinTaler() { return KartenSpielenMinTaler; }
        public double GetKartenSpielenProzentsatz() { return kartenSpielenProzentsatz; }
        public double GetErmordungProzentsatz() { return ErmordungProzentsatz; }
        public int GetErmordungsChance() { return ErmordungsChance; }
        public int GetVergifteterWeinChance() { return VergifteterWeinChance; }
        public int GetLagerraumBasisPreis() { return LagerraumBasisPreis; }
        public int GetMaxReichtum() { return MaxReichtum; }
        public int GetMaxCrime() { return MaxCrime; }
        public int GetMaxAnzahlWahlen() { return maxAnzahlWahlen; }
        public int GetMaxAnzahlAmtsenthebungen() { return maxAnzahlAmtsenthebungen; }
        public int GetMaxRohID() { return maxRohID; }
        public int GetMaxRohStufe1ID() { return maxRohStufe1ID; }
        public int GetMaxRohStufe2ID() { return maxRohStufe2ID; }
        public int GetEinkaufspreisZuschlag() { return EinkaufspreisZuschlag; }
        public int GetMaxKinderAnzahl() { return maxKinderAnzahl; }
        public int GetMinKindSlotNr() { return minKindSlotNr; }
        public int GetMaxPriv() { return maxPrivilegien; }
        public int GetMaxKredite() { return maxKrediteAnzahl; }
        public Adelstitel GetTitelX(int x) { return Tit[x]; }
        public IPrivileg GetPrivX(int x) { return Privilegien[x]; }
        public Karawane GetKarawane(int x) { return Kara[x]; }
        public int GetDefaultKarawane() { return DefaultKarawane; }
        public int GetMaxKarawane() { return maxKarawane; }
        public int GetMinKarawane() { return minKarawane; }
        public Haus GetHaus(int x) { return Haeuser[x]; }
        public int GetMaxHausID() { return MaxHausID; }
        public int GetStartHausID() { return StartHausID; }
        public string GetWerbereaktion(int i) { return Werbereaktionen[i]; }
        public string GetTexteTodesursachenX(int X) { return TexteTodesursachen[X]; }
        public int GetMaxTexteTodesursachen() { return maxTexteTodesursachen; }
        public Werbegeschenk GetWerbegeschenk(int i) { return WG[i]; }
        public int GetMaxWerbegeschenke() { return maxWerbegeschenke; }
        public int GetWerbegeschenkGrenzeBillig() { return WerbegeschenkGrenzeBillig; }
        public int GetWerbegeschenkGrenzeMittelteuer() { return WerbegeschenkGrenzeMittelteuer; }
        public int GetMaxProdSlots() { return maxProdSlots; }
        public int GetMaxAnzahlVonEinemRohstoff() { return maxAnzahlVonEinemRohstoff; }
        public int GetMaxWerkstaettenProStadt() { return maxWerkstaettenProStadt; }
        public int GetMaxArbeiterAnzahl() { return maxArbeiterAnzahl; }
        public string GetKINameX(int x) { return KINames[x]; }
        public int GetMaxKIID() { return maxKIID; }
        public int GetMinKIID() { return minKIID; }
        public int GetMaennerFrauenGrenze() { return MaennerFrauenGrenze; }
        public int GetStartLagerraum() { return Startlagerraum; }
        public int GetStartgold() { return StartGold; }
        public int GetChanceFuerKind() { return ChanceFuerKind; }
        public int GetChanceFuerKindStirbt() { return ChanceFuerKindStirbt; }
        public int GetMaxGesundheit() { return maxGesundheit; }
        public int GetMaxSchulden() { return maxSchulden; }
        public int GetKerkerGesundheit() { return KerkerGesundheit; }
        public int GetKerkerAnsehen() { return KerkerAnsehen; }
        public int GetMinSterbeAlter() { return minSterbeAlter; }
        public int GetMinGesundheit() { return MinGesundheit; }
        public Amt GetAmtwithID(int id) { return Aemter[id]; }
        public int GetMinTitelStadtEbene() { return MinTitelStadtEbene; }
        public int GetMaxAmtID() { return MaxAmtID; }
        public int GetMinTitelLandEbene() { return MinTitelLandEbene; }
        public int GetMinTitelReichsEbene() { return MinTitelReichsEbene; }
        public int GetMaxAmtStadtID() { return MaxAmtStadtID; }
        public int GetMaxAmtLandID() { return MaxAmtLandID; }
        public int GetMaxTitelID() { return MaxTitelID; }
        public int GetMaxWahlKandidaten() { return MaxWahlKandidaten; }
        public int GetMaxAbsetzSympathie() { return MaxAbsetzSympathie; }
        public int GetGerichtsKlagepunkte() { return GerichtsKlagepunkte; }
        public int GetMaxWahlWaehler() { return MaxWahlWaehler; }
        public string GetReligionsNamenX(int X) { return ReligionsNamen[X]; }
        public int GetmaxAnzahlGerichtsverhandlungen() { return maxAnzahlGerichtsverhandlungen; }
        public int GetRelKathID() { return RelKathID; }
        public int GetRelEvanID() { return RelEvanID; }
        public int GetRelMinID() { return RelMinID; }
        public int GetRelFreiID() { return RelFreiID; }
        public int GetRelMaxID() { return RelMaxID; }
        public double GetMaxUmsatzsteuer() { return maxUmsatzsteuer; }
        public double GetMinUmsatzsteuer() { return minUmsatzsteuer; }
        public int GetMaxStufenID() { return maxStufeID; }
        public string GetStufenNameX(int X) { return StufenNamen[X]; }
        /// <summary>
        /// Die Kosten für den Spieler beim Hinzufügen eines Spielers zu einem Spiel, wenn er die Stadt selbst auswählt und nicht zufällig durch das Spiel entscheiden lässt.
        /// </summary>
        /// <returns></returns>
        public int GetNSPStadtwahlKosten() { return NeuerSpielerStadtwahlkosten; }
        /// <summary>
        /// Die Kosten für den Spieler beim Hinzufügen eines Spielers zu einem Spiel, wenn er den Rohstoff selbst auswählt und nicht zufällig durch das Spiel entscheiden lässt.
        /// </summary>
        /// <returns></returns>
        public int GetNSPRohwahlKosten() { return NeuerSpielerRohwahlkosten; }
        public int GetMaxNameLength() { return maxNameLength; }
        public int GetGesetzgrenzeFinanz() { return GesetzgrenzeFinanz; }
        public int GetGesetzgrenzeKirche() { return GesetzgrenzeKirche; }
        public int GetGesetzgrenzeStraf() { return GesetzgrenzeStraf; }
        public int GetGesetzAnzahlStraf() { return GesetzAnzahlStraf; }
        public int GetGesetzAnzahlKirch() { return GesetzAnzahlKirch; }
        public int GetGesetzAnzahlFinanz() { return GesetzAnzahlFinanz; }
        public int GetMinNameLength() { return minNameLength; }
        public int GetMaxKatastrohpen() { return MaxKatastrophen; }
        public double GetWerbegeschenkFaktor() { return Werbegeschenkfaktor; }
        public string[] GetGerichtsGesetzesvorwurf() { return GerichtsGesetzesvorwurf; }
        public int GetMinStadtID() { return minStadtID; }
        public int GetMaxReichID() { return maxReichID; }
        public int GetMaxStadtID() { return maxStadtID; }
        public int GetMaxLandID() { return maxLandID; }
        public int GetMinReichID() { return minReichID; }
        public int GetMinLandID() { return minLandID; }
        public double GetStandardUmsatzSteuer() { return StandardUmsatzSteuer; }
        public int GetStartAlter() { return StartAlter; }
        public int GetMaxAlter() { return maxAlter; }
        public int GetHumminVerblJahre() { return HumMinVerblJahre; }
        public int GetHummaxVerblJahre() { return HumMaxVerblJahre; }
        public int GetKIminVerblJahre() { return KIminVerblJahre; }
        public int GetKImaxVerblJahre() { return KImaxVerblJahre; }
        public int GetmaxKorruptionsGelder() { return maxKorruptionsGelder; }
        public int GetMinRohGrundPreis() { return MinRohGrundPreis; }
        public int GetMaxRohGrundPreis() { return MaxRohGrundPreis; }
        public int GetMaxAnzahlStrafen() { return maxAnzahlStrafen; }
        public IStrafe GetStrafartX(int x) { return Strafarten[x]; }
        public int GetMaxGesetze() { return maxGesetze; }
        public int GetGesetzXDefUntergrenze(int X) { return GesetzDefUntergrenze[X]; }
        public int GetGesetzXDefObergrenze(int X) { return GesetzDefObergrenze[X]; }
        public int GetMaxStaedteProLand() { return maxStaedteProLand; }
        public int GetStadtRechteck(int stadt, int x) { return StadtRechtecke[stadt, x]; }
        public int GetStuetzpunktRechteck(int stuetzpunktID, int x) { return StuetzpunktRechtecke[stuetzpunktID - 1, x]; }
        public int GetTippsMaxIndex() { return TippsMaxIndex; }
        public Random Rnd { get; private set; } = new Random();
        public double GetMaxZollsatz() { return maxZollsatz; }
        public double GetMinZollsatz() { return minZollsatz; }
        #endregion
    }
}
