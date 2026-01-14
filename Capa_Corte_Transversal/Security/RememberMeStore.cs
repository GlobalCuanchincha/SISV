using System;
using System.IO;
using System.Text;

namespace Capa_Corte_Transversal.Security
{
    public static class RememberMeStore
    {
        private static readonly string Folder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SISV");

        private static readonly string FilePath = Path.Combine(Folder, "remember.dat");

        public static void Save(string username, string passwordProtectedBase64)
        {
            Directory.CreateDirectory(Folder);

            File.WriteAllText(FilePath, username + "|" + passwordProtectedBase64, Encoding.UTF8);
        }

        public static bool TryLoad(out string username, out string passwordProtectedBase64)
        {
            username = "";
            passwordProtectedBase64 = "";

            if (!File.Exists(FilePath))
                return false;

            var text = File.ReadAllText(FilePath, Encoding.UTF8);
            var parts = text.Split(new[] { '|' }, 2);

            if (parts.Length != 2)
                return false;

            username = parts[0];
            passwordProtectedBase64 = parts[1];
            return true;
        }

        public static void Clear()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
