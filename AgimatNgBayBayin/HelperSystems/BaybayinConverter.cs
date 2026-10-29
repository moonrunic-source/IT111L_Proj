using System;
using System.Collections.Generic;

namespace AgimatNgBayBayin.HelperSystems
{
    /// <summary>
    /// Turns a Latin syllable (KA, BI, NGO, A ...) into its Baybayin character
    /// using the Unicode "Tagalog" block (U+1700 - U+171F).
    /// Baybayin is an abugida: a consonant sign means "consonant + A",
    /// and a kudlit (mark) changes the vowel to E/I or O/U.
    /// </summary>
    public static class BaybayinConverter
    {
        private const string KudlitForEorI = "\u1712";
        private const string KudlitForOorU = "\u1713";

        private static readonly Dictionary<string, char> BaseConsonants = new Dictionary<string, char>
        {
            { "K", '\u1703' }, { "G", '\u1704' }, { "NG", '\u1705' }, { "T", '\u1706' },
            { "D", '\u1707' }, { "R", '\u1707' },  // D and R share one character
            { "N", '\u1708' }, { "P", '\u1709' }, { "B", '\u170A' }, { "M", '\u170B' },
            { "Y", '\u170C' }, { "L", '\u170E' }, { "W", '\u170F' }, { "S", '\u1710' },
            { "H", '\u1711' }
        };

        private static readonly Dictionary<char, string> StandaloneVowels = new Dictionary<char, string>
        {
            { 'A', "\u1700" },
            { 'E', "\u1701" }, { 'I', "\u1701" },  // E and I share one character
            { 'O', "\u1702" }, { 'U', "\u1702" }   // O and U share one character
        };

        public static string ToGlyph(string latinSyllable)
        {
            string syllable = latinSyllable.ToUpperInvariant();
            char vowel = syllable[syllable.Length - 1];

            if (!StandaloneVowels.ContainsKey(vowel))
                throw new ArgumentException("Syllable '" + latinSyllable + "' must end with A, E, I, O or U.");

            // Single vowel syllable, e.g. "A", "I", "O"
            if (syllable.Length == 1)
                return StandaloneVowels[vowel];

            // Consonant + vowel syllable, e.g. "KA", "BI", "NGO"
            string consonant = syllable.Substring(0, syllable.Length - 1);
            char baseCharacter;
            if (!BaseConsonants.TryGetValue(consonant, out baseCharacter))
                throw new ArgumentException("Unknown consonant '" + consonant + "' in syllable '" + latinSyllable + "'.");

            return baseCharacter + GetKudlit(vowel);
        }

        private static string GetKudlit(char vowel)
        {
            switch (vowel)
            {
                case 'E':
                case 'I': return KudlitForEorI;
                case 'O':
                case 'U': return KudlitForOorU;
                default: return string.Empty;   // 'A' needs no mark
            }
        }
    }
}
