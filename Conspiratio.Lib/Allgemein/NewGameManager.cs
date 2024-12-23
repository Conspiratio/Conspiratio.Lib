using System.IO;

using Conspiratio.Lib.Gameplay.Spielwelt;

namespace Conspiratio.Lib.Allgemein
{
    public class NewGameManager
    {
        private string _savegamePath;

        public NewGameManager(string savegamePath)
        {
            _savegamePath = savegamePath;
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

        public bool ValidateName(string name, out string error)
        {
            error = "Der Spielname muss aus mindestens drei Zeichen bestehen";
            
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Length < 3) 
                return false;

            error = "";
            return true;
        }

        public string SanitizeName(string name)
        {
            string savegamePathWithFilename = Path.Combine(_savegamePath, "_1600.dat");
            int savegamePathLength = savegamePathWithFilename.Length;
            int maxlength = 256 - savegamePathLength;

            if (maxlength < 0)  // fallback from settings (standard: 12), if savegame path is longer then 256 chars
                maxlength = SW.Statisch.GetMaxNameLength();
            
            name = RemoveInvalidChars(name);
            
            if (name.Length > maxlength)
                return name.Substring(0, maxlength);

            return name;
        }
        
        private string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
