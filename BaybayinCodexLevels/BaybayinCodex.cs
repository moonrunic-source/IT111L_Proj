using System;
using System.Collections.Generic;
using System.IO;

// exception if saving fails
public class CodexSaveException : Exception
{
    public CodexSaveException(string message) : base(message)
    {
    }
}

// the baybayin codex
// the members are static so we don't need to make an object of this class
public class BaybayinCodex
{
    // letters that show up on the tiles
    public static readonly string TileLetters = "ABDGHIKLMNPRSTUWY";

    // each letter and baybayin letter (unicode U+1700 to U+171F) dont forget noto sans tagalog font
    private static Dictionary<char, string> glyphs = new Dictionary<char, string>
    {
        { 'A', "\u1700" }, { 'I', "\u1701" }, { 'U', "\u1702" },
        { 'K', "\u1703" }, { 'G', "\u1704" }, { 'T', "\u1706" },
        { 'D', "\u1707" }, { 'N', "\u1708" }, { 'P', "\u1709" },
        { 'B', "\u170A" }, { 'M', "\u170B" }, { 'Y', "\u170C" },
        { 'R', "\u170D" }, { 'L', "\u170E" }, { 'W', "\u170F" },
        { 'S', "\u1710" }, { 'H', "\u1711" }
    };

    // gives the symbol of a letter, or the letter itself if it has none
    public static string GetGlyph(char letter)
    {
        string glyph;
        if (glyphs.TryGetValue(char.ToUpper(letter), out glyph))
        {
            return glyph;
        }
        return letter.ToString();
    }
}

// remembers what the player learned
// FileStream and StreamWriter save it, StreamReader loads it
public class CodexProgress
{
    // private fields
    private HashSet<char> learned = new HashSet<char>();
    private HashSet<string> doneWords = new HashSet<string>();
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

    public int LettersLearned
    {
        get { return learned.Count; }
    }

    public int WordsCompleted
    {
        get { return doneWords.Count; }
    }

    // methods
    public void LearnLetter(char letter)
    {
        learned.Add(char.ToUpper(letter));
    }

    public void CompleteWord(string word)
    {
        doneWords.Add(word.ToUpper());
    }

    public bool IsLetterUnlocked(char letter)
    {
        return unlocked || learned.Contains(char.ToUpper(letter));
    }

    // saves everything to the file, throws CodexSaveException if it fails
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.WriteLine("letters=" + new string(new List<char>(learned).ToArray()));
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
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    string key = parts[0];
                    string value = parts[1];

                    if (key == "letters")
                    {
                        foreach (char c in value)
                        {
                            p.learned.Add(c);
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
