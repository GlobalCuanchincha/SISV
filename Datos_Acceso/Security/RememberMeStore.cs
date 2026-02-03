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

        public static void Save(string username)
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(FilePath, (username ?? "").Trim(), Encoding.UTF8);
        }

        public static bool TryLoad(out string username)
        {
            username = "";

            if (!File.Exists(FilePath))
                return false;

            var text = (File.ReadAllText(FilePath, Encoding.UTF8) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var parts = text.Split(new[] { '|' }, 2);
            username = (parts[0] ?? "").Trim();

            if (parts.Length == 2)
            {
                try { Save(username); } catch { }
            }

            return !string.IsNullOrWhiteSpace(username);
        }

        public static void Clear()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }


        public static void Save(string username, string passwordProtectedBase64)
            => Save(username);

        public static bool TryLoad(out string username, out string passwordProtectedBase64)
        {
            passwordProtectedBase64 = "";
            return TryLoad(out username);
        }
    }
}
