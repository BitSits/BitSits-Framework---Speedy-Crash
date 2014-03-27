using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BitSits_Framework
{
    class IntroScreen : GameScreen
    {
        string type;
        float time = 0;

        public IntroScreen(string type)
        {
            this.type = type;

            if (type == "gameOver")
                IsPopup = true;
        }

        public override void LoadContent() { base.LoadContent(); }

        public override void HandleInput(InputState input)
        {
            MouseState mouseState = input.CurrentMouseState[0];
            Point pos = new Point(mouseState.X, mouseState.Y);

            if (mouseState.LeftButton == ButtonState.Pressed
                && input.LastMouseState[0].LeftButton == ButtonState.Released
                && ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.Contains(pos)
                && time > 0.5f)
            {
                if (type == "gameOver")
                    LoadingScreen.Load(ScreenManager, false, null, new IntroScreen("intro"));

                else if (type == "intro")
                    ScreenManager.AddScreen(new IntroScreen("info"), PlayerIndex.One);                    
                else
                    ScreenManager.AddScreen(new GameplayScreen(), PlayerIndex.One);

                ScreenManager.RemoveScreen(this);
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 1 / 6);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            if(type == "gameOver")
                spriteBatch.Draw(ScreenManager.GameContent.gameOver, new Vector2(130, 250), Color.White);

            else if (type == "info")
            {
                spriteBatch.Draw(ScreenManager.GameContent.background[0], Vector2.Zero, Color.White);
                spriteBatch.Draw(ScreenManager.GameContent.info, Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Draw(ScreenManager.GameContent.background[0], Vector2.Zero, Color.White);
                spriteBatch.Draw(ScreenManager.GameContent.intro, Vector2.Zero, Color.White);
            }

            spriteBatch.End();
        }
    }
}
