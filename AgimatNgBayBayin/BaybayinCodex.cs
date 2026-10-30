using System;
using System.Collections.Generic;
using System.IO;
using AgimatNgBayBayin.HelperSystems;

namespace AgimatNgBayBayin
{
    // exception if saving fails
    public class CodexSaveException : Exception
    {
        public CodexSaveException(string message) : base(message)
        {
        }
    }

    // the baybayin codex
    // the members are static so we don't need to make an object of this class
    public static class BaybayinCodex
    {
        // gives the Baybayin character of a Latin syllable (KA, BI, NGO, A ...)
        // the real conversion is done by BaybayinConverter, if it can't convert
        // we give back the syllable itself so the game doesn't crash
        public static string GetGlyph(string syllable)
        {
            if (string.IsNullOrEmpty(syllable))
            {
                return "";
            }

            try
            {
                return BaybayinConverter.ToGlyph(syllable);
            }
            catch (ArgumentException)
            {
                return syllable;
            }
        }
    }

    // remembers what the player learned
    // FileStream and StreamWriter save it, StreamReader loads it
    public class CodexProgress
    {
        // private fields
        private HashSet<string> learned = new HashSet<string>();     // syllables, like "BA"
        private HashSet<string> doneWords = new HashSet<string>();   // words, like "BATA"
        private int highest = 1;
        private bool unlocked = false;

        private static string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgimatNgBaybayin", "codex.txt");

        // properties so other classes can use the fields
        public int HighestLevel
        {
            get { return highest; }
            set { highest = value; }
        }

        public bool FullyUnlocked
        {
            get { return unlocked; }
            set { unlocked = value; }
        }

        public int SyllablesLearned
        {
            get { return learned.Count; }
        }

        public int WordsCompleted
        {
            get { return doneWords.Count; }
        }

        // methods
        public void LearnSyllable(string syllable)
        {
            learned.Add(syllable.ToUpperInvariant());
        }

        public void CompleteWord(string word)
        {
            doneWords.Add(word.ToUpperInvariant());
        }

        public bool IsSyllableUnlocked(string syllable)
        {
            return unlocked || learned.Contains(syllable.ToUpperInvariant());
        }

        // saves everything to the file, throws CodexSaveException if it fails
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.WriteLine("syllables=" + string.Join(",", learned));
                    writer.WriteLine("words=" + string.Join(",", doneWords));
                    writer.WriteLine("highest=" + highest);
                    writer.WriteLine("unlocked=" + unlocked);
                }
            }
            catch (IOException e)
            {
                throw new CodexSaveException("Could not save the codex: " + e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new CodexSaveException("No permission to save the codex: " + e.Message);
            }
        }

        // loads from the file, if there is no file or it is broken we start fresh
        public static CodexProgress Load()
        {
            CodexProgress p = new CodexProgress();

            if (!File.Exists(filePath))
            {
                return p;
            }

            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(file))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length != 2)
                        {
                            continue;
                        }

                        string key = parts[0];
                        string value = parts[1];

                        if (key == "syllables")
                        {
                            foreach (string s in value.Split(','))
                            {
                                if (s != "")
                                {
                                    p.learned.Add(s);
                                }
                            }
                        }
                        else if (key == "words")
                        {
                            foreach (string w in value.Split(','))
                            {
                                if (w != "")
                                {
                                    p.doneWords.Add(w);
                                }
                            }
                        }
                        else if (key == "highest")
                        {
                            p.highest = int.Parse(value);
                        }
                        else if (key == "unlocked")
                        {
                            p.unlocked = bool.Parse(value);
                        }
                    }
                }
            }
            catch (IOException)
            {
                // can't read the file, keep what we got so far
            }
            catch (FormatException)
            {
                // bad number or true/false in the file, keep what we got so far
            }

            return p;
        }
    }
}
