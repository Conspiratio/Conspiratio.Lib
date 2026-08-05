using System.Collections.Generic;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    /// <summary>
    /// Kapselt die Privilegien des aktiven Spielers: das Auflisten der aktuell gültigen Privilegien
    /// (abhängig von Amt, Titel und Familienstand) und das Ausführen eines Privilegs über dessen
    /// PrivExecute. Passive Privilegien zeigen dabei nur eine Information, aktionierbare öffnen den
    /// zugehörigen Dialog über die SW.UI-Schnittstellen.
    /// </summary>
    public class PrivilegienManager
    {
        /// <summary>
        /// Kennung des immer verfügbaren „Ahnentafel"-Eintrags. Kein echtes Lib-Privileg, sondern ein
        /// synthetischer Listeneintrag – der Client fängt diese ID ab und öffnet die Ahnentafel-Anzeige.
        /// Liegt bewusst weit über den echten Privileg-IDs (< <see cref="Spielwelt.StatischeSpieldaten.GetMaxPriv"/>).
        /// </summary>
        public const int AhnentafelPrivilegId = 10000;

        /// <summary>
        /// Kennung des Eintrags „Mätresse nehmen" (Issue #8). Kein echtes Lib-Privileg, sondern ein
        /// synthetischer Listeneintrag – der Client fängt die ID ab und wickelt die Aktion über den
        /// <see cref="MaetresseManager"/> ab. Nur sichtbar, solange der Spieler noch keine Mätresse hat.
        /// </summary>
        public const int MaetressePrivilegId = 10001;

        /// <summary>
        /// Kennung des Eintrags „Fechtunterricht nehmen" (Issue #17). Synthetischer, immer verfügbarer
        /// Listeneintrag – der Client fängt die ID ab und öffnet den Fechtunterricht-Dialog.
        /// </summary>
        public const int FechtunterrichtPrivilegId = 10002;

        /// <summary>
        /// Kennung des Eintrags „Zum Duell fordern" (Issue #17). Synthetischer Listeneintrag – der Client
        /// öffnet die Personen-Karte (Modus 14) zur Auswahl eines Amtsträgers. Nur sichtbar, solange der
        /// Spieler in diesem Jahr noch kein Duell geführt hat.
        /// </summary>
        public const int DuellPrivilegId = 10003;

        /// <summary>
        /// Aktualisiert die Privilegien des Spielers (abhängig von Amt, Titel und Familienstand).
        /// </summary>
        [PublicAPI]
        public void AktualisierePrivilegien()
        {
            SW.Dynamisch.PrivilegienAktualisieren();
        }

        /// <summary>
        /// Liefert die Privilegien, die der aktive Spieler derzeit besitzt. Die Ahnentafel steht als
        /// immer verfügbarer Eintrag (ab Spielstart) stets an erster Stelle.
        /// </summary>
        [PublicAPI]
        public List<PrivilegInfo> GetPrivilegien()
        {
            var privilegien = new List<PrivilegInfo>
            {
                new PrivilegInfo(AhnentafelPrivilegId, "Ahnentafel einsehen")
            };

            var spieler = SW.Dynamisch.GetAktHum();

            // „Mätresse nehmen" (Issue #8): immer wählbar, solange der Spieler noch keine unterhält.
            if (!spieler.HatMaetresse())
                privilegien.Add(new PrivilegInfo(MaetressePrivilegId, "Mätresse nehmen"));

            // Fechtunterricht & Duell (Issue #17): immer verfügbar; ein Duell nur einmal pro Jahr.
            privilegien.Add(new PrivilegInfo(FechtunterrichtPrivilegId, "Fechtunterricht nehmen"));
            if (!spieler.DuellGefuehrtDiesesJahr)
                privilegien.Add(new PrivilegInfo(DuellPrivilegId, "Amtsträger beleidigen"));

            for (int i = 1; i < SW.Statisch.GetMaxPriv(); i++)
            {
                if (spieler.CheckPrivilegX(i))
                    privilegien.Add(new PrivilegInfo(i, SW.Statisch.GetPrivX(i).Name));
            }

            return privilegien;
        }

        /// <summary>
        /// Führt das Privileg mit der angegebenen ID aus (zeigt Infos oder öffnet den zugehörigen Dialog).
        /// </summary>
        [PublicAPI]
        public void FuehreAus(int privilegId)
        {
            SW.Statisch.GetPrivX(privilegId).PrivExecute();
        }
    }

    /// <summary>Ein Privileg des Spielers (ID und Name).</summary>
    public class PrivilegInfo
    {
        public PrivilegInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}
