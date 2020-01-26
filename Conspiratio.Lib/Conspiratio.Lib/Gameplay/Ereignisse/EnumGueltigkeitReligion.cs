namespace Conspiratio.Lib.Gameplay.Ereignisse
{
    public enum EnumGueltigkeitReligion
    {
        /// <summary>
        /// Gibt an, dass die Religions für das Ereignis keine Rolle spielt.
        /// </summary>
        ReligionIstEgal,

        /// <summary>
        ///  Gibt an, dass ein Ereignis nur für Spieler gültig ist, die einer Relegion angehören und nicht für konfessionslose Spieler.
        /// </summary>
        NurGueltigMitReligion,

        /// <summary>
        ///  Gibt an, dass ein Ereignis nur für Spieler gültig ist, die keiner Relegion angehören und somit konfessionslos sind.
        /// </summary>
        NurGueltigOhneReligion
    }
}
