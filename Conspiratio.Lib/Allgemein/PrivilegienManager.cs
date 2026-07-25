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
        /// Aktualisiert die Privilegien des Spielers (abhängig von Amt, Titel und Familienstand).
        /// </summary>
        [PublicAPI]
        public void AktualisierePrivilegien()
        {
            SW.Dynamisch.PrivilegienAktualisieren();
        }

        /// <summary>
        /// Liefert die Privilegien, die der aktive Spieler derzeit besitzt.
        /// </summary>
        [PublicAPI]
        public List<PrivilegInfo> GetPrivilegien()
        {
            var privilegien = new List<PrivilegInfo>();
            var spieler = SW.Dynamisch.GetAktHum();

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
