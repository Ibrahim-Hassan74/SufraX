using System.Security.Cryptography;

namespace EStoreX.Core.Helper
{
    public static class PasswordGenerator
    {
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*()_-+=<>?";

        private static readonly string All = Upper + Lower + Digits + Special;

        public static string Generate(int length = 16)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8");

            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];

            chars[0] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
            chars[1] = Lower[RandomNumberGenerator.GetInt32(Lower.Length)];
            chars[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
            chars[3] = Special[RandomNumberGenerator.GetInt32(Special.Length)];

            for (int i = 4; i < length; i++)
            {
                chars[i] = All[bytes[i] % All.Length];
            }

            return new string(chars.OrderBy(x => RandomNumberGenerator.GetInt32(1000)).ToArray());
        }
    }
}
