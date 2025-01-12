using System.IO;

using Conspiratio.Lib.Gameplay.Spielwelt;

using JetBrains.Annotations;

namespace Conspiratio.Lib.Allgemein
{
    public class NewGameManager
    {
        private readonly string _savegamePath;

        public NewGameManager(string savegamePath)
        {
            _savegamePath = savegamePath;
        }
        
        [PublicAPI]
        public int MaxLengthOfGameName
        {
            get
            {
                string savegamePathWithFilename = Path.Combine(_savegamePath, "_1600.dat");
                int savegamePathLength = savegamePathWithFilename.Length;
                int maxlength = 256 - savegamePathLength;

                if (maxlength < 0) // fallback from settings (standard: 12), if savegame path is longer then 256 chars
                    maxlength = SW.Statisch.GetMaxNameLength();

                return maxlength;
            }
        }
        
        public bool CreateNewGame(string name, int playerCount, bool cheating, bool showDeaths, bool testmode, out string error)
        {
            if (!ValidateName(name, out error))
                return false;
            
            SW.Dynamisch.SpielName = SanitizeName(name);
            SW.Dynamisch.SetAktivSpielerAnzahl(playerCount);
            SW.Dynamisch.Cheatmodus = cheating;
            SW.Dynamisch.TodesfaelleAnzeigen = showDeaths;
            SW.Dynamisch.Testmodus = testmode;
            
            return true;
        }

        [PublicAPI]
        public bool ValidateName(string name, out string error)
        {
            error = Resources.The_game_name_must_consist_of_at_least_three_characters;
            
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Length < 3) 
                return false;

            error = "";
            return true;
        }
        
        [PublicAPI]
        public string SanitizeName(string name)
        {
            name = RemoveInvalidChars(name);
            
            if (name.Length > MaxLengthOfGameName)
                return name.Substring(0, MaxLengthOfGameName);

            return name;
        }
        
        private static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
