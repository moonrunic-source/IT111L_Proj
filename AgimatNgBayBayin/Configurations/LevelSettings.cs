namespace AgimatNgBayBayin.Configurations
{
    /// <summary>
    /// The word-length rules of a level. WordBank uses these to pick which words fit the level.
    /// </summary>
    public class LevelSettings
    {
        public int MinSyllablesPerWord { get; private set; }
        public int MaxSyllablesPerWord { get; private set; }

        public LevelSettings(int minSyllablesPerWord, int maxSyllablesPerWord)
        {
            MinSyllablesPerWord = minSyllablesPerWord;
            MaxSyllablesPerWord = maxSyllablesPerWord;
        }
    }
}
