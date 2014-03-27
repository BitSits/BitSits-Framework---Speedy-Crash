using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    /// <summary>
    /// All the Contents of the Game is loaded and stored here
    /// so that all other screen can copy from here
    /// </summary>
    public class GameContent
    {
        public readonly ContentManager content;

        // Textures
        public readonly Texture2D blank;
        public readonly Texture2D gradient;
        public readonly Texture2D menuBackground;

        public readonly Texture2D ship, flame;
        public readonly Texture2D[] asteroid = new Texture2D[16];

        public readonly Texture2D[] background = new Texture2D[2];
        public readonly Texture2D info, intro;

        public readonly Texture2D[] combo = new Texture2D[3];
        public readonly Texture2D levelUp, gameOver;

        public readonly Texture2D bar;

        // Fonts
        public readonly SpriteFont font;

        // Songs
        public Song song;

        // Sound Effects
        public readonly SoundEffect engine;
        public readonly SoundEffect explosion;
        public readonly SoundEffect[] boom = new SoundEffect[3];


        /// <summary>
        /// Load GameContents
        /// </summary>
        public GameContent(GameComponent screenManager)
        {
            if (content == null)
                content = new ContentManager(screenManager.Game.Services, "Content");

            blank = content.Load<Texture2D>("Graphics/blank");
            gradient = content.Load<Texture2D>("Graphics/gradient");
            menuBackground = content.Load<Texture2D>("Graphics/menuBackground");

            ship = content.Load<Texture2D>("Graphics/ship");
            background[0] = content.Load<Texture2D>("Graphics/background0");
            background[1] = content.Load<Texture2D>("Graphics/background1");
            flame = content.Load<Texture2D>("Graphics/flame");
            intro = content.Load<Texture2D>("Graphics/intro");
            info = content.Load<Texture2D>("Graphics/info");

            bar = content.Load<Texture2D>("Graphics/bar");

            for (int i = 0; i < 16; i++)
                asteroid[i] = content.Load<Texture2D>("Graphics/asteroid" + i.ToString("00"));

            for (int i = 0; i < 3; i++)
                combo[i] = content.Load<Texture2D>("Graphics/combo" + i);

            levelUp = content.Load<Texture2D>("Graphics/levelUp");
            gameOver = content.Load<Texture2D>("Graphics/gameOver");

            font = content.Load<SpriteFont>("Fonts/lunaFont");

            for (int i = 0; i < 3; i++)
                boom[i] = content.Load<SoundEffect>("Audio/boom" + i);

            explosion = content.Load<SoundEffect>("Audio/explosion");
            engine = content.Load<SoundEffect>("Audio/engine");

            song = content.Load<Song>("Audio/PILL - MU.S.GA Music for Strategy Games 03 - Fearless");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1.0f;

            SoundEffect.MasterVolume = 1.0f;

            //Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            screenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload GameContents
        /// </summary>
        public void UnloadContent() { content.Unload(); }
    }
}
