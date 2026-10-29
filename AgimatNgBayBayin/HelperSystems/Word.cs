using System.Collections.Generic;
using System.Linq;

namespace AgimatNgBayBayin.HelperSystems
{
    /// <summary>A Tagalog word split into syllables, with a meaning used as a clue.</summary>
    public class Word
    {
        private readonly List<Syllable> _syllables = new List<Syllable>();

        public IReadOnlyList<Syllable> Syllables { get { return _syllables; } }
        public string Meaning { get; private set; }

        /// <summary>The whole word in Latin letters, e.g. "KABAYO".</summary>
        public string Latin
        {
            get { return string.Concat(_syllables.Select(s => s.Latin)); }
        }

        /// <param name="hyphenatedSyllables">Example: "KA-BA-YO"</param>
        /// <param name="meaning">Example: "horse"</param>
        public Word(string hyphenatedSyllables, string meaning)
        {
            foreach (string part in hyphenatedSyllables.Split('-'))
                _syllables.Add(new Syllable(part));

            Meaning = meaning;
        }
    }
}
