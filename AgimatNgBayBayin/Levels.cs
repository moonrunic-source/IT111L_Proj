using System;
using System.Collections.Generic;
using System.Linq;
using AgimatNgBayBayin.Assets;
using AgimatNgBayBayin.Configurations;
using AgimatNgBayBayin.HelperSystems;

namespace AgimatNgBayBayin
{
    // exception for a level that doesn't exist
    public class InvalidLevelException : Exception
    {
        public InvalidLevelException(string message) : base(message)
        {
        }
    }

    // what happened when the player picked a syllable tile
    public enum SyllableResult
    {
        Ignored,          // no word right now
        Correct,          // right syllable but the word isn't done
        Wrong,            // wrong syllable, the player lost a life
        WordCompleted,    // the word is done
        LevelCompleted,   // the level is done
        GameOver          // no lives left
    }

    // one level
    public class Level
    {
        // properties, can be read anywhere but only changed in this class
        public int Number { get; private set; }
        public List<Word> Words { get; private set; }               // words from WordBank for this level
        public List<Syllable> TileSyllables { get; private set; }   // syllables the tiles can show
        public int WordsToWin { get; private set; }                 // words needed to finish the level
        public int MissingCount { get; private set; }               // how many blanks per word
        public float TimeLimit { get; private set; }                // seconds per word, 0 means no timer
        public float Speed { get; private set; }                    // how fast the tiles go
        public bool ShowHint { get; private set; }                  // show the Baybayin hint of the missing syllables
        public bool ShowLatinOnTiles { get; private set; }          // tiles also show the Latin syllable

        // constructor, the words come straight from WordBank (by the syllable rules in settings)
        public Level(int number, LevelSettings settings, int wordsToWin, int missingCount,
                     float timeLimit, float speed, bool showHint, bool showLatinOnTiles)
        {
            Number = number;
            WordsToWin = wordsToWin;
            MissingCount = missingCount;
            TimeLimit = timeLimit;
            Speed = speed;
            ShowHint = showHint;
            ShowLatinOnTiles = showLatinOnTiles;

            Words = WordBank.GetWordsForLevel(settings);
            TileSyllables = WordBank.GetUniqueSyllablesForLevel(settings);
        }

        // how a syllable looks in the word, normal levels just show the Latin syllable
        public virtual string DisplaySyllable(Syllable syllable)
        {
            return syllable.Latin;
        }
    }

    // inheritance: Level 4 is a Level but it shows Baybayin instead of Latin
    public class BaybayinLevel : Level
    {
        public BaybayinLevel(int number, LevelSettings settings, int wordsToWin, int missingCount,
                             float timeLimit, float speed, bool showHint, bool showLatinOnTiles)
            : base(number, settings, wordsToWin, missingCount, timeLimit, speed, showHint, showLatinOnTiles)
        {
        }

        public override string DisplaySyllable(Syllable syllable)
        {
            return BaybayinCodex.GetGlyph(syllable.Latin);   // goes through BaybayinConverter
        }
    }

    // handles the words, the lives, the score and the levels
    public class LevelManager
    {
        public const int MaxLives = 3;
        public const int PointsPerWord = 100;
        public const int AgimatBonusPoints = 250;
        private const int WordsPerAgimat = 3;     // an Agimat shows up after every 3 words
        private const int TutorialWords = 3;
        public const float PixelsPerSecond = 120f;   // Level.Speed 1.0 = 120 pixels per second (Container.Update uses pixels)

        // private fields
        private Level[] levels;
        private Level? current;
        private CodexProgress progress;
        private Random rand = new Random();

        private Word? word;                                // the word now, null if none
        private bool[] isBlank = Array.Empty<bool>();      // true for the syllables that are still blank
        private List<int> blanks = new List<int>();        // where the blanks are
        private int filled;                                // blanks filled so far
        private int done;                                  // words finished in this level
        private bool tutorial;                             // true while in the tutorial
        private int tutorialLeft;                          // tutorial words left
        private float time;                                // time left for the word
        private int lives;
        private int score;
        private int wordsSinceAgimat;
        private bool agimatReady;

        // properties
        public Word? CurrentWord { get { return word; } }
        public CodexProgress Progress { get { return progress; } }
        public string SaveError { get; private set; }
        public bool InTutorial { get { return tutorial; } }
        public float TimeLeft { get { return time; } }
        public int Lives { get { return lives; } }
        public int Score { get { return score; } }
        public bool IsGameOver { get { return current != null && word == null && lives <= 0; } }
        public bool AgimatReady { get { return agimatReady; } }

        // the word as Latin text, like "BATA"
        public string WordText
        {
            get { return word == null ? "" : word.Latin; }
        }

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

        public bool ShowLatinOnTiles
        {
            get { return tutorial || (current != null && current.ShowLatinOnTiles); }
        }

        // true when the word on screen is written in Baybayin (Level 4)
        public bool WordIsBaybayin
        {
            get { return Active is BaybayinLevel; }
        }

        public bool IsLastLevel
        {
            get { return current != null && current.Number == levels.Length; }
        }

        // the tutorial uses the Level 1 settings
        private Level Active
        {
            get { return tutorial ? levels[0] : current!; }
        }

        // constructor, makes the 4 levels and loads the saved progress
        public LevelManager()
        {
            SaveError = "";

            //                      level, syllables (min, max), words to win, blanks, time, speed, hint, latin on tiles
            levels = new Level[]
            {
                new Level(1,         new LevelSettings(2, 2),  5, 1,  0f, 1.0f, true,  true),
                new Level(2,         new LevelSettings(2, 3),  6, 2, 15f, 1.5f, false, true),
                new Level(3,         new LevelSettings(3, 4),  6, 2, 12f, 2.0f, false, false),
                new BaybayinLevel(4, new LevelSettings(2, 4),  8, 3, 20f, 1.5f, false, true)
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
            lives = MaxLives;
            score = 0;
            wordsSinceAgimat = 0;
            agimatReady = false;
            tutorial = !skipTutorial;
            tutorialLeft = TutorialWords;
            NewWord();
        }

        // after game over, starts the same level again without the tutorial
        public void RestartLevel()
        {
            if (current == null)
            {
                return;
            }
            StartLevel(current.Number, true);
        }

        // picks a random word from the level (from WordBank) and blanks some syllables
        private void NewWord()
        {
            List<Word> wordList = Active.Words;
            Word? lastWord = word;

            do
            {
                word = wordList[rand.Next(wordList.Count)];
            } while (wordList.Count > 1 && word == lastWord);

            int count = word.Syllables.Count;
            int missing = Math.Max(1, Math.Min(Active.MissingCount, count));

            isBlank = new bool[count];
            blanks = new List<int>();

            while (blanks.Count < missing)
            {
                int pick = rand.Next(count);
                if (!blanks.Contains(pick))
                {
                    blanks.Add(pick);
                    isBlank[pick] = true;
                }
            }
            blanks.Sort();

            filled = 0;
            time = Active.TimeLimit;     // the tutorial uses Level 1 which has no timer
        }

        // ----- words -----

        // call this when the player hits a tile (the tile is a BayBayinCharacters)
        public SyllableResult CollectTile(BayBayinCharacters tile)
        {
            return CollectSyllable(tile._syllable);
        }

        // call this when the player picks a syllable
        public SyllableResult CollectSyllable(Syllable picked)
        {
            if (word == null)
            {
                return SyllableResult.Ignored;
            }

            int spot = blanks[filled];                    // the next blank
            Syllable needed = word.Syllables[spot];

            // Syllable.Matches compares the Baybayin character, so syllables that
            // share one (like E and I, or D and R) both count
            if (!picked.Matches(needed))
            {
                return LoseLife();
            }

            isBlank[spot] = false;
            filled++;
            progress.LearnSyllable(needed.Latin);

            if (filled < blanks.Count)
            {
                return SyllableResult.Correct;
            }
            return FinishWord();
        }

        private SyllableResult FinishWord()
        {
            progress.CompleteWord(WordText);
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
                score += PointsPerWord;

                wordsSinceAgimat++;
                if (wordsSinceAgimat >= WordsPerAgimat)
                {
                    wordsSinceAgimat = 0;
                    agimatReady = true;
                }

                if (done >= current!.WordsToWin)
                {
                    word = null;
                    agimatReady = false;

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
                    return SyllableResult.LevelCompleted;
                }
            }

            NewWord();
            return SyllableResult.WordCompleted;
        }

        // the player loses a life (wrong tile or time ran out), no life is lost in the tutorial
        private SyllableResult LoseLife()
        {
            if (tutorial)
            {
                return SyllableResult.Wrong;
            }

            lives--;
            if (lives <= 0)
            {
                lives = 0;
                word = null;
                return SyllableResult.GameOver;
            }
            return SyllableResult.Wrong;
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

        // ----- Agimat (health recovery + bonus points) -----

        // makes an Agimat when one is ready (after every few words), null if not
        // the Agimat scrolls at the same speed as the tiles
        public Agimat? TrySpawnAgimat(float x, float y)
        {
            if (!agimatReady || word == null || tutorial)
            {
                return null;
            }

            agimatReady = false;
            return new Agimat(x, y, ScrollSpeed * PixelsPerSecond);
        }

        // call this when the player catches an Agimat
        // gives back one life (up to MaxLives) and bonus points, returns true if a life was restored
        public bool CollectAgimat(Agimat agimat)
        {
            if (agimat == null || word == null)
            {
                return false;
            }

            score += AgimatBonusPoints;

            if (lives < MaxLives)
            {
                lives++;
                return true;
            }
            return false;
        }

        // ----- timer -----

        // call this from a Timer, returns true if time ran out
        // (the player lost a life and a new word is picked, check IsGameOver after)
        public bool Tick(float secondsPassed)
        {
            if (word == null || tutorial || time <= 0f)
            {
                return false;
            }

            time -= secondsPassed;
            if (time > 0f)
            {
                return false;
            }

            if (LoseLife() != SyllableResult.GameOver)
            {
                NewWord();
            }
            return true;
        }

        // ----- helpers for the form -----

        // one string for each syllable of the word, "_" is a blank
        // (Baybayin characters on Level 4, Latin syllables on the others)
        public string[] GetSlots()
        {
            if (word == null)
            {
                return new string[0];
            }

            string[] slots = new string[word.Syllables.Count];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = isBlank[i] ? "_" : Active.DisplaySyllable(word.Syllables[i]);
            }
            return slots;
        }

        // Baybayin characters of the syllables that are still missing (the hint)
        public string GetHint()
        {
            if (word == null)
            {
                return "";
            }

            string hint = "";
            for (int i = filled; i < blanks.Count; i++)
            {
                hint += BaybayinCodex.GetGlyph(word.Syllables[blanks[i]].Latin) + " ";
            }
            return hint.Trim();
        }

        // syllables for the tiles, the needed syllable is always in there
        public List<Syllable> GetSpawnSyllables(int count)
        {
            List<Syllable> result = new List<Syllable>();
            if (word == null)
            {
                return result;
            }

            result.Add(word.Syllables[blanks[filled]]);

            // fill the rest with random syllables from the level, no two with the same character
            foreach (Syllable s in Active.TileSyllables.OrderBy(x => rand.Next()))
            {
                if (result.Count >= count)
                {
                    break;
                }
                if (!result.Any(r => r.Matches(s)))
                {
                    result.Add(s);
                }
            }

            // shuffle so the needed syllable isn't always first
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                Syllable temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }
            return result;
        }

        // makes a tile for the form, it scrolls at the level speed
        // and shows the Latin syllable only if the level says so
        public BayBayinCharacters CreateTile(Syllable syllable, float x, float y)
        {
            return new BayBayinCharacters(x, y, ScrollSpeed * PixelsPerSecond, syllable, ShowLatinOnTiles);
        }
    }
}
