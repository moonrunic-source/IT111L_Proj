using System;
using System.Collections.Generic;

// exception for a level that doesn't exist
public class InvalidLevelException : Exception
{
    public InvalidLevelException(string message) : base(message)
    {
    }
}

// what happened when the player picked a letter
public enum LetterResult
{
    Ignored,          // no word right now
    Correct,          // right letter but the word isn't done
    Wrong,            // wrong letter, the form takes a life
    WordCompleted,    // the word is done
    LevelCompleted    // the level is done
}

// one level
public class Level
{
    // properties, can be read anywhere but only changed in this class
    public int Number { get; private set; }
    public string[] Words { get; private set; }
    public int WordsToWin { get; private set; }     // words needed to finish the level
    public int MissingCount { get; private set; }   // how many blanks per word
    public float TimeLimit { get; private set; }    // seconds per word, 0 means no timer
    public float Speed { get; private set; }        // how fast the tiles go
    public bool ShowHint { get; private set; }      // show the baybayin hint

    // constructor
    public Level(int number, string[] words, int wordsToWin, int missingCount,
                 float timeLimit, float speed, bool showHint)
    {
        Number = number;
        Words = words;
        WordsToWin = wordsToWin;
        MissingCount = missingCount;
        TimeLimit = timeLimit;
        Speed = speed;
        ShowHint = showHint;
    }

    // how a letter looks in the word, normal levels just show the letter
    public virtual string DisplayLetter(char letter)
    {
        return letter.ToString();
    }
}

// inheritance: Level 4 is a Level but it shows baybayin instead of letters
public class BaybayinLevel : Level
{
    public BaybayinLevel(int number, string[] words, int wordsToWin, int missingCount,
                         float timeLimit, float speed, bool showHint)
        : base(number, words, wordsToWin, missingCount, timeLimit, speed, showHint)
    {
    }

    public override string DisplayLetter(char letter)
    {
        return BaybayinCodex.GetGlyph(letter);
    }
}

// handles the words and the levels
public class LevelManager
{
    // private fields
    private Level[] levels;
    private Level current;
    private CodexProgress progress;
    private Random rand = new Random();

    private string word;                              // the word now, null if none
    private char[] shown;                             // letters on screen, _ is a blank
    private List<int> blanks = new List<int>();       // where the blanks are
    private int filled;                               // blanks filled so far
    private int done;                                 // words finished in this level
    private bool tutorial;                            // true while in the tutorial
    private int tutorialLeft;                         // tutorial words left
    private float time;                               // time left for the word

    // properties
    public string Word { get { return word; } }
    public CodexProgress Progress { get { return progress; } }
    public string SaveError { get; private set; }
    public bool InTutorial { get { return tutorial; } }
    public float TimeLeft { get { return time; } }

    public int LevelNumber
    {
        get { return current == null ? 0 : current.Number; }
    }

    public float ScrollSpeed
    {
        get { return current == null ? 1f : current.Speed; }
    }

    public bool ShowHint
    {
        get { return tutorial || (current != null && current.ShowHint); }
    }

    public bool IsLastLevel
    {
        get { return current != null && current.Number == levels.Length; }
    }

    // the tutorial uses the Level 1 settings
    private Level Active
    {
        get { return tutorial ? levels[0] : current; }
    }

    // constructor, makes the 4 levels and loads the saved progress
    public LevelManager()
    {
        SaveError = "";

        levels = new Level[]
        {
            new Level(1, new string[] { "ARAW", "BATA", "MATA", "BAHAY", "PUSA", "LUPA", "ISDA" },
                      5, 1, 0f, 1.0f, true),
            new Level(2, new string[] { "TUBIG", "KAMAY", "SULAT", "BAYAN", "GABI", "HALAMAN", "PAMILYA" },
                      6, 2, 15f, 1.5f, false),
            new Level(3, new string[] { "KAIBIGAN", "KALAYAAN", "KASAYSAYAN", "MAHARLIKA", "PILIPINAS", "KAPAYAPAAN", "TAGUMPAY" },
                      6, 3, 12f, 2.0f, false),
            new BaybayinLevel(4, new string[] { "BAYBAYIN", "KULTURA", "BAYANIHAN", "AGIMAT", "ALAMAT", "DAMBANA", "SAMBAYANAN", "LAKANDULA" },
                      8, 3, 20f, 1.5f, false)
        };

        progress = CodexProgress.Load();
    }

    // ----- levels -----

    // levelNumber is 1 to 4, anything else throws InvalidLevelException
    public void StartLevel(int levelNumber, bool skipTutorial)
    {
        if (levelNumber < 1 || levelNumber > levels.Length)
        {
            throw new InvalidLevelException("Level " + levelNumber + " does not exist.");
        }

        current = levels[levelNumber - 1];
        done = 0;
        tutorial = !skipTutorial;
        tutorialLeft = 3;
        NewWord();
    }

    // after game over, starts the same level again without the tutorial
    public void RestartLevel()
    {
        StartLevel(current.Number, true);
    }

    // picks a random word and takes out some letters
    private void NewWord()
    {
        string[] wordList = Active.Words;
        string lastWord = word;

        do
        {
            word = wordList[rand.Next(wordList.Length)];
        } while (word == lastWord);

        shown = word.ToCharArray();
        blanks = new List<int>();

        while (blanks.Count < Active.MissingCount)
        {
            int pick = rand.Next(word.Length);
            if (!blanks.Contains(pick))
            {
                blanks.Add(pick);
            }
        }
        blanks.Sort();

        foreach (int i in blanks)
        {
            shown[i] = '_';
        }

        filled = 0;
        time = tutorial ? 0f : current.TimeLimit;
    }

    // ----- words -----

    // call this when the player picks a letter tile
    public LetterResult CollectLetter(char letter)
    {
        if (word == null)
        {
            return LetterResult.Ignored;
        }

        char c = char.ToUpper(letter);
        int spot = blanks[filled];            // the next blank

        if (c != word[spot])
        {
            return LetterResult.Wrong;
        }

        shown[spot] = c;
        filled++;
        progress.LearnLetter(c);

        if (filled < blanks.Count)
        {
            return LetterResult.Correct;
        }
        return FinishWord();
    }

    private LetterResult FinishWord()
    {
        progress.CompleteWord(word);
        SaveProgress();

        if (tutorial)
        {
            tutorialLeft--;
            if (tutorialLeft <= 0)
            {
                tutorial = false;
            }
        }
        else
        {
            done++;
            if (done >= current.WordsToWin)
            {
                word = null;

                // unlock the next level, or the whole codex after the last level
                if (current.Number < levels.Length && progress.HighestLevel <= current.Number)
                {
                    progress.HighestLevel = current.Number + 1;
                }
                if (current.Number == levels.Length)
                {
                    progress.FullyUnlocked = true;
                }
                SaveProgress();
                return LetterResult.LevelCompleted;
            }
        }

        NewWord();
        return LetterResult.WordCompleted;
    }

    // saving can fail, the game keeps going if it does
    private void SaveProgress()
    {
        try
        {
            progress.Save();
            SaveError = "";
        }
        catch (CodexSaveException e)
        {
            SaveError = e.Message;
        }
    }

    // ----- timer -----

    // call this from a Timer, returns true if time ran out (new word is already picked)
    public bool Tick(float secondsPassed)
    {
        if (word == null || time <= 0f)
        {
            return false;
        }

        time -= secondsPassed;
        if (time <= 0f)
        {
            NewWord();
            return true;
        }
        return false;
    }

    // ----- helpers for the form -----

    // one string for each slot, "_" is a blank (baybayin symbols on Level 4)
    public string[] GetSlots()
    {
        if (word == null)
        {
            return new string[0];
        }

        string[] slots = new string[shown.Length];
        for (int i = 0; i < shown.Length; i++)
        {
            slots[i] = shown[i] == '_' ? "_" : Active.DisplayLetter(shown[i]);
        }
        return slots;
    }

    // symbols of the letters that are still missing (the hint)
    public string GetHint()
    {
        if (word == null)
        {
            return "";
        }

        string hint = "";
        for (int i = filled; i < blanks.Count; i++)
        {
            hint += BaybayinCodex.GetGlyph(word[blanks[i]]) + " ";
        }
        return hint.Trim();
    }

    // letters for the tiles, the needed letter is always in there
    public List<char> GetSpawnLetters(int count)
    {
        List<char> result = new List<char>();
        if (word == null)
        {
            return result;
        }

        result.Add(word[blanks[filled]]);
        while (result.Count < count)
        {
            char c = BaybayinCodex.TileLetters[rand.Next(BaybayinCodex.TileLetters.Length)];
            if (!result.Contains(c))
            {
                result.Add(c);
            }
        }

        // shuffle so the needed letter isn't always first
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            char temp = result[i];
            result[i] = result[j];
            result[j] = temp;
        }
        return result;
    }
}
