namespace AgimatNgBayBayin.HelperSystems
{
    /// <summary>One Baybayin character: its Latin sound and its glyph.</summary>
    public class Syllable
    {
        public string Latin { get; private set; }
        public string Glyph { get; private set; }

        public Syllable(string latin)
        {
            Latin = latin.ToUpperInvariant();
            Glyph = BaybayinConverter.ToGlyph(Latin);
        }

        /// <summary>
        /// Two syllables match when they draw the SAME glyph
        /// (Baybayin writes E/I and O/U with one character, so "LE" equals "LI").
        /// </summary>
        public bool Matches(Syllable other)
        {
            return other != null && Glyph == other.Glyph;
        }
    }
}
