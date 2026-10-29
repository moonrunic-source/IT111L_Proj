namespace AgimatNgBayBayin.Assets
{
    public abstract class BaseEntity
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public bool isAlive { get; set; }
        /* <summary> 
         * Loads an image to the screen
         * </summary>*/
        public static Image LoadImage(string file)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "ExistingAssets", file);
            return File.Exists(path) ? Image.FromFile(path) : null;
        }
        protected BaseEntity()
        {
            isAlive = true;
        }

        public abstract void Update(float time);
        public abstract void DrawPicture(Graphics g);
    }
}
