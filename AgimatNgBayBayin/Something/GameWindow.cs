using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace AgimatNgBayBayin.Something
{
    public class GameWindow : Form
    {
        private readonly System.Windows.Forms.Timer _gameTime = new System.Windows.Forms.Timer();
        private readonly Stopwatch _clock = new Stopwatch();

        public GameWindow()
        {

        }

        public void CloseWindow()
        {
            Close();
        }

    }
}
