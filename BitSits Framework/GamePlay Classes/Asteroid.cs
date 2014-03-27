using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Asteroid
    {
        Texture2D tex;

        const int blastNumber = 5;
        Texture2D[] blastTex = new Texture2D[blastNumber];
        Vector2[] blastPos = new Vector2[blastNumber];
        double[] blastAngle = new double[blastNumber];
        float blastDisp = 1.52f, time = 0, scale = 1, blastDirection = 0;

        float prevDisp, disp;

        public int Score { get; private set; }

        const float maxDisp = 55;
        const float DestroyVelocity = 120, ComboDestroyVelocity = 40;
        const float StarDestroyVelocity = 175, StarComboVelocity = 60;

        public readonly Body body;
        bool isBodyDestroyed = false, velocityExceeded = false, isNotVisible = false;

        public bool IsNotVisible { get { return (velocityExceeded && scale <= 0) || isNotVisible; } }

        public bool VelocityExceeded { get { return velocityExceeded; } }

        public bool IsBodyDestroyed { get { return isBodyDestroyed; } }

        Bonus bonus = Bonus.None;

        Vector2 originalPosition;

        GameContent gameContent;
        SoundEffect blastSound;
        SoundEffectInstance blastSoundInstance;

        public Asteroid(GameContent gameContent, int index, Body body)
        {
            this.gameContent = gameContent;
            tex = gameContent.asteroid[index];
            this.body = body;
            body.SetUserData(this);
            originalPosition = body.Position;

            for (int i = 0; i < 5; i++)
                blastTex[i] = gameContent.asteroid[(index + i) % 16];

            blastSound = gameContent.boom[index % 3];
            blastSoundInstance = blastSound.CreateInstance();
            blastSoundInstance.Volume = 0.8f;
        }

        public void Destroy(string hitBy)
        {
            float velocity = Math.Abs(body.GetLinearVelocity().Length());
            float factor = 1;

            if (!velocityExceeded)
            {
                if ((hitBy == "Ship" || hitBy == "Ship2") && velocity > DestroyVelocity)
                {
                    velocityExceeded = true;
                    bonus = hitBy == "Ship2" ? Bonus.Combo : Bonus.Hit;

                    if (velocity >= StarDestroyVelocity) bonus = Bonus.StarSpeed;
                }

                else if (hitBy == "Other" && velocity > ComboDestroyVelocity)
                {
                    velocityExceeded = true; bonus = Bonus.Combo;
                    if (velocity >= StarComboVelocity) bonus = Bonus.MegaCombo;

                    factor = 2;
                }

                blastDirection = body.Position.Y < originalPosition.Y ? -1 : 1;
                Score = (int)Math.Ceiling(velocity * factor);
            }
        }

        public Bonus GetBonus()
        {
            if (isBodyDestroyed) return Bonus.None;

            prevDisp = disp;
            disp = Math.Abs(Vector2.Distance(originalPosition, body.Position));

            if ((prevDisp > disp || disp > maxDisp) && velocityExceeded && !isBodyDestroyed)
            {
                isBodyDestroyed = true;
                body.GetWorld().DestroyBody(body);

                blastAngle[2] = Math.Atan(
                    (body.Position.Y - originalPosition.Y) / (body.Position.X - originalPosition.X));
                if (blastAngle[2] < 0) blastAngle[2] += Math.PI;

                blastAngle[1] = blastAngle[2] + (float)Math.PI / 3;
                blastAngle[3] = blastAngle[2] - (float)Math.PI / 3;

                blastAngle[0] = blastAngle[2] + (float)Math.PI / 3 * 2;
                blastAngle[4] = blastAngle[2] - (float)Math.PI / 3 * 2;

                for (int i = 0; i < blastNumber; i++) blastPos[i] = body.Position;

                blastSoundInstance.Play();

                return bonus;
            }

            return Bonus.None;
        }

        public void NotVisible()
        {
            if (isNotVisible || velocityExceeded) return;

            isNotVisible = true;
            body.GetWorld().DestroyBody(body);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!isBodyDestroyed)
                spriteBatch.Draw(tex, body.Position, null, Color.White, body.Rotation,
                    new Vector2(tex.Width / 2, tex.Height / 2), 1, SpriteEffects.None, 1);

            else if (scale > 0)
            {
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (time < 5f)
                {
                    Vector2 position = body.Position;
                    int b = (int)bonus - 2;
                    float width = gameContent.combo[0].Width / 2;
                    position.X = MathHelper.Clamp(position.X, width, 375 - width);

                    if (bonus != Bonus.Hit)
                        spriteBatch.Draw(gameContent.combo[b], position, null, Color.White, 0,
                            new Vector2(width, width * 2), 1, SpriteEffects.None, 1);

                    spriteBatch.DrawString(gameContent.font, "+" + Score, position - new Vector2(width / 2, 0),
                        Color.Turquoise, 0, Vector2.Zero, new Vector2(.5f), SpriteEffects.None, 1);
                }

                scale = .5f - time * .05f;

                for (int i = 0; i < blastNumber; i++)
                {
                    blastPos[i] = blastPos[i] +
                        new Vector2((float)Math.Cos(blastAngle[i]), (float)Math.Sin(blastAngle[i]))
                        * blastDisp * blastDirection;

                    spriteBatch.Draw(blastTex[i], blastPos[i], null, Color.White, time * 2,
                        new Vector2(blastTex[i].Width / 2), scale, SpriteEffects.None, 1);
                }
            }
        }
    }
}
