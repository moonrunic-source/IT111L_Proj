using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using AgimatNgBayBayin;
using AgimatNgBayBayin.Assets;
using AgimatNgBayBayin.Configurations;
using AgimatNgBayBayin.HelperSystems;

namespace AgimatNgBayBayin.Something
{
    public class GameWindow : Form
    {
        private readonly System.Windows.Forms.Timer _gameTime = new System.Windows.Forms.Timer();
        private readonly Stopwatch _clock = new Stopwatch();

        private readonly LevelManager _levels = new LevelManager();
        private readonly List<BayBayinCharacters> _tiles = new List<BayBayinCharacters>();
        private readonly Random _rng = new Random();
        private Agimat? _agimat;

        public GameWindow()
        {
            Text = "Agimat ng Baybayin";
            ClientSize = new Size(960, 540);     // tiles use lanes down to y = 450, so keep it at least this tall
            DoubleBuffered = true;

            // INITIALIZE: start level 1 with the tutorial (pass true to skip it)
            _levels.StartLevel(1, false);
            SpawnTiles();

            _gameTime.Interval = 16;               // about 60 updates per second
            _gameTime.Tick += OnGameTick;
            _clock.Start();
            _gameTime.Start();
        }

        // UPDATE: runs every timer tick
        private void OnGameTick(object? sender, EventArgs e)
        {
            float seconds = (float)_clock.Elapsed.TotalSeconds;
            _clock.Restart();

            // the word timer, true means time ran out and a life was lost
            if (_levels.Tick(seconds))
            {
                if (_levels.IsGameOver)
                {
                    OnGameOver();
                    return;
                }
                SpawnTiles();
            }

            // move the tiles and the Agimat (Container.Update scrolls them to the left)
            foreach (BayBayinCharacters tile in _tiles)
            {
                tile.Update(seconds);
            }
            _agimat?.Update(seconds);

            // Container.Update sets isAlive to false once something scrolled off the left side
            _tiles.RemoveAll(t => !t.isAlive);
            if (_agimat != null && !_agimat.isAlive)
            {
                _agimat = null;
            }

            // all tiles missed, send a fresh set
            if (_tiles.Count == 0 && _levels.CurrentWord != null)
            {
                SpawnTiles();
            }

            Invalidate();
        }

        // new tiles for the next blank, the needed syllable is always one of them
        // they start just outside the right edge, one lane each
        private void SpawnTiles()
        {
            _tiles.Clear();
            List<Syllable> syllables = _levels.GetSpawnSyllables(4);
            for (int i = 0; i < syllables.Count; i++)
            {
                float x = ClientSize.Width + i * 60;
                float y = 150 + i * 100;
                _tiles.Add(_levels.CreateTile(syllables[i], x, y));
            }
        }

        // TODO: replace the mouse click with your player's hitbox if you have one,
        // then call HandleTileHit / HandleAgimatHit when it touches a tile
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_agimat != null &&
                new RectangleF(_agimat.X, _agimat.Y, _agimat.Width, _agimat.Height).Contains(e.Location))
            {
                HandleAgimatHit();
                return;
            }

            foreach (BayBayinCharacters tile in _tiles)
            {
                if (new RectangleF(tile.X, tile.Y, tile.Width, tile.Height).Contains(e.Location))
                {
                    HandleTileHit(tile);
                    return;
                }
            }
        }

        private void HandleTileHit(BayBayinCharacters tile)
        {
            switch (_levels.CollectTile(tile))
            {
                case SyllableResult.Correct:
                    SpawnTiles();                       // tiles for the next blank
                    break;

                case SyllableResult.WordCompleted:
                    SpawnTiles();
                    if (_agimat == null)
                    {
                        _agimat = _levels.TrySpawnAgimat(ClientSize.Width, _rng.Next(150, ClientSize.Height - 150));
                    }
                    break;

                case SyllableResult.Wrong:
                    _tiles.Remove(tile);                // the wrong tile disappears
                    break;

                case SyllableResult.LevelCompleted:
                    _agimat = null;
                    if (_levels.IsLastLevel)
                    {
                        _tiles.Clear();
                        // show the full codex
                    }
                    else
                    {
                        _levels.StartLevel(_levels.LevelNumber + 1, true);
                        SpawnTiles();
                    }
                    break;

                case SyllableResult.GameOver:
                    OnGameOver();
                    break;
            }
        }

        private void HandleAgimatHit()
        {
            if (_agimat == null) return;

            bool healed = _levels.CollectAgimat(_agimat);   // +1 life (if not full) and bonus points
            _agimat = null;
            // use "healed" to play a sound or show a +1 life effect
        }

        private void OnGameOver()
        {
            _tiles.Clear();
            _agimat = null;
            // show a game over screen, then:
            _levels.RestartLevel();
            SpawnTiles();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            foreach (BayBayinCharacters tile in _tiles)
            {
                tile.DrawPicture(g);
            }
            _agimat?.DrawPicture(g);

            // the word, like "BA  _" (Baybayin characters on Level 4)
            string word = string.Join("  ", _levels.GetSlots());
            string font = _levels.WordIsBaybayin ? FontItems.GlyphFont : FontItems.TextFont;
            using (var brush = new SolidBrush(FontItems.BaybayinFontColor))
            {
                FontWriter.DrawCentered(g, word, FontWriter.Get(font, 32f), brush,
                    new RectangleF(0, 20, ClientSize.Width, 60));

                // lives, score, and the hint when the level has one
                string hud = "Lives: " + _levels.Lives + "   Score: " + _levels.Score;
                if (_levels.ShowHint)
                {
                    hud += "   Hint: " + _levels.GetHint();
                }
                FontWriter.DrawCentered(g, hud, FontWriter.Get(FontItems.TextFont, 14f), brush,
                    new RectangleF(0, 90, ClientSize.Width, 30));
            }
        }

        public void CloseWindow()
        {
            Close();
        }
    }
}
