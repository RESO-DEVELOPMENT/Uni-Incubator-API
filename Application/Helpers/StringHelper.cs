using System.Text;

namespace Application.Helpers
{
    public static class StringHelper
    {
        public static string SubstringWithDots(this string str, int startIndex, int length)
        {
            if (str.Length > length)
                return str.Substring(startIndex, length) + "...";
            else
                return str;
        }
        private static Random _random = new Random();

        public static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.

            // char is a single Unicode character
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
        public static string Generate(int length)
        {
            // Create a string of all possible characters.
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // Generate a random string of the specified length.
            string randomString = new string(Enumerable.Repeat(characters, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());

            return randomString;
        }
    }
}