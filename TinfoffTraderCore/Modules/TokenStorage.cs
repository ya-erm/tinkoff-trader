using System.IO;
using System.Linq;
using TinkoffTraderCore.Utils;

namespace TinkoffTraderCore.Modules
{
    public class TokenStorage
    {
        public static string TokenFileName { get; } = "token.txt";

        public static bool HasTokenFile => File.Exists(TokenFileName);

        public static bool IsTokenFileEncrypted
        {
            get
            {
                if (!HasTokenFile) return false;

                return File.ReadLines(TokenFileName).FirstOrDefault() == "encrypted";
            }
        }

        public static void SaveToken(string token)
        {
            File.WriteAllText(TokenFileName, token);
        }

        public static void SaveTokenEncrypted(string token, string password)
        {
            var encryptedText = Encryptor.Encrypt(token, password);

            File.WriteAllText(TokenFileName, "encrypted\n" + encryptedText);
        }

        public static string LoadToken()
        {
            return File.Exists(TokenFileName) ? File.ReadAllText(TokenFileName) : null;
        }

        public static string LoadTokenEncrypted(string password)
        {
            if (!File.Exists(TokenFileName))
            {
                return null;
            }

            var encryptedText = File.ReadAllLines(TokenFileName)[1];

            var decryptedText = Encryptor.Decrypt(encryptedText, password);

            return decryptedText;
        }
    }
}
