using System.IO;
using System.Threading.Tasks;

using Conspiratio.Lib.Allgemein;
using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Tests
{
    /// <summary>Beantwortet jede Ja/Nein-Frage mit einer vorab festgelegten Antwort.</summary>
    public sealed class JaNeinStub : IYesNoQuestion
    {
        private readonly DialogResultGame _antwort;

        public JaNeinStub(DialogResultGame antwort = DialogResultGame.Yes) => _antwort = antwort;

        public Task<DialogResultGame> ShowDialogText(string frage, string ja = "Ja", string nein = "Nein")
            => Task.FromResult(_antwort);
    }

    /// <summary>Verschluckt Textmeldungen und merkt sich die letzte – praktisch für Zusicherungen.</summary>
    public sealed class TextStub : IShowText
    {
        public string LetzterText { get; private set; }

        public Task ShowDialog(string text)
        {
            LetzterText = text;
            return Task.CompletedTask;
        }
    }

    /// <summary>Entscheidet für ein menschliches Erpressungsopfer stets gleich.</summary>
    public sealed class ErpressungStub : IErpressungDialog
    {
        private readonly bool _beugtSich;

        public ErpressungStub(bool beugtSich) => _beugtSich = beugtSich;

        public Task<bool> FrageOpfer(string frage) => Task.FromResult(_beugtSich);
    }

    /// <summary>
    /// Baut für einen Test eine frische, vollständige Spielwelt auf.
    ///
    /// Der Spielzustand liegt in den statischen <see cref="SW"/>-Fassaden, ist also global. Deshalb
    /// laufen die Tests bewusst nacheinander (siehe <c>AssemblyInfo.cs</c>) und jeder Test beginnt mit
    /// einem eigenen <see cref="Starte"/>, statt sich auf den Zustand eines vorherigen zu verlassen.
    /// </summary>
    public static class TestSpielwelt
    {
        /// <summary>
        /// Legt ein neues Spiel mit <paramref name="menschen"/> menschlichen Spielern an und macht den
        /// ersten zum aktiven Spieler. Mit <paramref name="seed"/> läuft der Aufbau reproduzierbar ab –
        /// das betrifft alles, was beim Spielstart ausgewürfelt wird (KI-Bosheit, verbleibende Jahre).
        /// </summary>
        public static void Starte(int menschen = 1, IErpressungDialog erpressungDialog = null, int? seed = null)
        {
            SW.Statisch.Initialisieren();

            // Nach Initialisieren, denn dieses legt den Zufallsgenerator neu an.
            if (seed.HasValue)
                SW.Statisch.SetRnd(seed.Value);

            string pfad = Path.Combine(Path.GetTempPath(), "conspiratio-tests");
            Directory.CreateDirectory(pfad);

            var neuesSpiel = new NewGameManager(pfad);
            neuesSpiel.CreateNewGame("Test", menschen, false, true, false, out _);

            SW.UI.Initialisieren(new JaNeinStub(), new TextStub(), null, null, null, null, null, null, null,
                                 null, erpressungDialog);

            var setup = new PlayerSetupManager();
            setup.Starte();

            for (int i = 0; i < menschen; i++)
                setup.ErstelleSpieler("Spieler" + (i + 1), true, 3, SW.Statisch.GetRelKathID(), 5, true, 1);

            setup.Beende();
            SW.Dynamisch.SetAktiverSpieler(1);
        }

        /// <summary>
        /// Setzt einen KI-Gegner auf ein bestimmtes Amt und eine feste Bosheit.
        ///
        /// Die Bosheit wird je Spiel ausgewürfelt und bestimmt unter anderem die Gegnerstärke. Ohne
        /// Festlegung schwanken abhängige Werte von Lauf zu Lauf so stark, dass sie wie echte
        /// Regressionen aussehen – deshalb pinnen die Tests sie.
        /// </summary>
        public static int SetzeKiGegner(int nummer, int amtId, int bosheit = 50, int gebietId = 1)
        {
            int kiId = SW.Statisch.GetMinKIID() + nummer;
            var ki = SW.Dynamisch.GetKIwithID(kiId);

            ki.SetAmt(amtId, gebietId);
            ki.SetBosheit(bosheit);

            return kiId;
        }

        /// <summary>Stattet den aktiven Spieler mit Spionage und Beweispunkten gegen ein Ziel aus.</summary>
        public static void GibBeweise(int zielId, int beweispunkte)
        {
            var spionage = SW.Dynamisch.GetAktHum().GetAktiveSpionage(zielId);
            spionage.SetKosten(100);
            spionage.SetDelikte(beweispunkte);
        }
    }
}
