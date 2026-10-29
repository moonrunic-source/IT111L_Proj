using System;
using System.Collections.Generic;
using System.Linq;
using AgimatNgBayBayin.Configurations;

namespace AgimatNgBayBayin.HelperSystems
{
    /// <summary>
    /// The dictionary of the game.
    /// To add words, add a line to AllWords: new Word("SYL-LA-BLES", "meaning").
    /// Traditional Baybayin has no final consonants, so every syllable ends in a vowel.
    /// </summary>
    public static class WordBank
    {
        private static readonly List<Word> AllWords = new List<Word>
        {
            // 2 syllables
            new Word("BA-TA", "child"),     new Word("MA-TA", "eye"),
            new Word("KA-MA", "bed"),       new Word("PU-SA", "cat"),
            new Word("LI-MA", "five"),      new Word("A-SO", "dog"),
            new Word("DA-GA", "rat"),       new Word("BA-KA", "cow"),
            new Word("GA-TA", "coconut milk"), new Word("TA-LA", "star"),
            new Word("LU-PA", "soil"),      new Word("TA-MA", "correct"),
            new Word("BA-BA", "chin"),      new Word("I-SA", "one"),
            new Word("PA-LA", "shovel"),

            // 3 syllables
            new Word("KA-BA-YO", "horse"),  new Word("PA-LA-KA", "frog"),
            new Word("LA-ME-SA", "table"),  new Word("BA-GA-NI", "hero"),
            new Word("MA-HA-BA", "long"),   new Word("PA-GA-SA", "hope"),
            new Word("MA-SA-YA", "happy"),  new Word("MA-LA-YA", "free"),

            // 4 syllables
            new Word("KA-LA-BA-SA", "squash"), new Word("PA-LA-MU-TI", "decoration")
        };

        public static List<Word> GetAllWords()
        {
            return new List<Word>(AllWords);
        }

        /// <summary>Returns the words that fit the length rules of a level.</summary>
        public static List<Word> GetWordsForLevel(LevelSettings settings)
        {
            return AllWords.Where(w => w.Syllables.Count >= settings.MinSyllablesPerWord &&
                                       w.Syllables.Count <= settings.MaxSyllablesPerWord).ToList();
        }

        /// <summary>Picks a random word for the level, avoiding an immediate repeat.</summary>
        public static Word PickRandom(LevelSettings settings, Random rng, Word previousWord)
        {
            List<Word> candidates = GetWordsForLevel(settings);
            if (candidates.Count > 1 && previousWord != null)
                candidates.Remove(previousWord);

            return candidates[rng.Next(candidates.Count)];
        }

        public static List<Syllable> GetUniqueSyllablesForLevel(LevelSettings settings)
        {
            return GetUniqueSyllables(GetWordsForLevel(settings));
        }

        /// <summary>All different characters used by the given words (no duplicates, sorted A-Z).</summary>
        public static List<Syllable> GetUniqueSyllables(IEnumerable<Word> words)
        {
            var byGlyph = new Dictionary<string, Syllable>();
            foreach (Word word in words)
                foreach (Syllable syllable in word.Syllables)
                    if (!byGlyph.ContainsKey(syllable.Glyph))
                        byGlyph.Add(syllable.Glyph, syllable);

            return byGlyph.Values.OrderBy(s => s.Latin).ToList();
        }
    }
}
