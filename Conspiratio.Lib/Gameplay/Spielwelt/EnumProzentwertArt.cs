using System;

namespace Conspiratio.Lib.Gameplay.Spielwelt
{
    /// <summary>
    /// Stellt alle vorhandenen Arten eines Prozentwerts dar, der im Fenster 'ProzentwertFestlegenForm' eingestellt werden kann.
    /// </summary>
    [Serializable]
    public enum ProzentwertArt
    {
        UmsatzsteuerStadt,
        ZollsatzZollburg,
        SicherheitTarnungStuetzpunkt,
        ZustandStuetzpunkt,
        KapazitaetStuetzpunkt
    }
}
