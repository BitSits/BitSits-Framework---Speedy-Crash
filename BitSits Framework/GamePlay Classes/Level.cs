using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Box2D.XNA;

namespace BitSits_Framework
{
    public enum Bonus
    {
        None = 0,

        /// <summary>
        /// Hit with ship
        /// </summary> 
        Hit = 1,

        /// <summary>
        /// One asteroid hits another
        /// </summary>
        Combo = 2,

        /// <summary>
        /// Ship hits the asteroid with high velocity
        /// </summary>
        StarSpeed = 3,

        /// <summary>
        /// One asteroid hits another with high velocity
        /// </summary>
        MegaCombo = 4,
    }

    class Level : IDisposable
    {
        #region Fields

        public const int TileWidth = 75;
        public const int TileHeight = 75;
        const  int Width = 5, Height = 100;

        const int FrontClearance = 450, BackClearance = 300;

        public int Score { get; private set; }
        public int LevelIndex { get; private set; }

        public bool IsLevelUp { get; private set; }

        public GameContent GameContent { get; private set; }

        World world = new World(Vector2.Zero, true);

        List<Asteroid> asteroids = new List<Asteroid>();
        Body ship, ground;
        Vector2 originalShipPos;

        List<Bonus> bonuses = new List<Bonus>();
        List<float> showTime = new List<float>();

        float cameraPosY;
        float[] layerPos = new float[2];

        bool leftFlame, rightFlame;
        SoundEffectInstance engineInstance;

        Random random = new Random();

        const float MaxFuel = 10;
        float fuel = MaxFuel;

        float asteroidsCount = 40;

        #endregion

        #region Initialization


        public Level(GameContent gameContent, int levelIndex, int score)
        {
            GameContent = gameContent;

            LevelIndex = levelIndex;
            Score = score;

            LoadGround();

            LoadShip();

            LoadAsteroidField((int)asteroidsCount);

            engineInstance = gameContent.engine.CreateInstance();
            engineInstance.IsLooped = true;
            engineInstance.Volume = 0.65f;
        }

        private void LoadGround()
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Static;
            bd.position = Vector2.Zero;
            ground = world.CreateBody(bd);

            Vector2[] vertices = new Vector2[4];
            vertices[0] = new Vector2(0, 0);
            vertices[1] = new Vector2(Width * TileWidth, 0);
            vertices[2] = new Vector2(Width * TileWidth, Height * TileHeight);
            vertices[3] = new Vector2(0, Height * TileHeight);

            PolygonShape shape = new PolygonShape();
            shape.SetAsEdge(vertices[0], vertices[1]); ground.CreateFixture(shape, 1);
            shape.SetAsEdge(vertices[1], vertices[2]); ground.CreateFixture(shape, 1);
            shape.SetAsEdge(vertices[2], vertices[3]); ground.CreateFixture(shape, 1);
            shape.SetAsEdge(vertices[3], vertices[0]); ground.CreateFixture(shape, 1);
        }

        private void LoadAsteroidField(int numberOFAsteroid)
        {
            foreach (Asteroid a in asteroids) a.NotVisible();
            asteroids = new List<Asteroid>();

            int aHeight = Height * TileHeight - FrontClearance - BackClearance - 600;
            aHeight /= TileHeight;

            numberOFAsteroid = Math.Min(Width * aHeight, numberOFAsteroid);

            List<int> prevIndex = new List<int>();
            int number = 0;

            while (number < numberOFAsteroid)
            {
                int index = random.Next(Width * aHeight);

                if (prevIndex.Contains(index)) continue;
                prevIndex.Add(index);

                Vector2 position = new Vector2((index % Width) * TileWidth + TileWidth / 2,
                    (index / Width) * TileHeight + TileHeight / 2 + FrontClearance + BackClearance);

                LoadAsteroid(position);
                number++;
            }
        }

        private void LoadShip()
        {
            Vector2 position = new Vector2(Width * TileWidth / 2, Height * TileHeight - 600 + FrontClearance);

            Texture2D texture = GameContent.ship;
            BodyDef bd = new BodyDef();
            bd.fixedRotation = true;
            bd.type = BodyType.Dynamic;
            bd.linearDamping = 0.5f;
            bd.angularDamping = 0.1f;
            bd.position = position;

            ship = world.CreateBody(bd);
            ship.SetUserData(texture);

            PolygonShape pShape = new PolygonShape();

            Vector2[] vertices = new Vector2[3];
            vertices[0] = new Vector2(0, -texture.Height / 2);
            vertices[1] = new Vector2(texture.Width / 2, texture.Height / 2);
            vertices[2] = new Vector2(-texture.Width / 2, texture.Height / 2);

            pShape.Set(vertices, 3);

            FixtureDef fd = new FixtureDef();
            fd.density = 0.1f;
            fd.shape = pShape;
            fd.friction = 5;
            fd.restitution = .5f;

            ship.CreateFixture(fd);
            originalShipPos = ship.Position;
        }

        private void LoadAsteroid(Vector2 positon)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            //bd.angularVelocity = -2.0f;
            bd.position = positon;

            Body asteroid = world.CreateBody(bd);

            CircleShape pShape = new CircleShape();
            pShape._radius = GameContent.asteroid[0].Width / 2;

            FixtureDef fd = new FixtureDef();
            fd.shape = pShape;
            fd.density = 0.05f;
            fd.friction = 0.3f;

            asteroid.CreateFixture(fd);

            DistanceJointDef jd = new DistanceJointDef();
            jd.bodyA = ground;
            jd.bodyB = asteroid;
            jd.localAnchorA = asteroid.Position;
            jd.localAnchorB = Vector2.Zero;
            jd.frequencyHz = 0.18f;
            jd.dampingRatio = 0.95f;
            jd.length = 0;
            jd.collideConnected = false;

            world.CreateJoint(jd);

            asteroids.Add(new Asteroid(GameContent, random.Next(16), asteroid));
        }

        public void Dispose() { }


        #endregion

        #region Update and HandleInput

        public void Update(GameTime gameTime)
        {
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 10, 10);

            if (world.ContactCount > 0)
            {
                for (ContactEdge ce = ship.GetContactList(); ce != null; ce = ce.Next)
                {
                    bool otherHasExceeded = false;
                    foreach (Asteroid a in asteroids)
                        if (a.VelocityExceeded && !a.IsBodyDestroyed) otherHasExceeded = true;

                    if (ce.Other.GetUserData() is Asteroid)
                        ((Asteroid)ce.Other.GetUserData()).Destroy("Ship" + (otherHasExceeded ? "2" : ""));
                }

                for (Contact c = world.GetContactList(); c != null; c = c.GetNext())
                {
                    Body a = c.GetFixtureA().GetBody(), b = c.GetFixtureB().GetBody();
                    if (a.GetUserData() is Asteroid && b.GetUserData() is Asteroid)
                    {
                        if (((Asteroid)a.GetUserData()).VelocityExceeded)
                            ((Asteroid)b.GetUserData()).Destroy("Other");

                        else if (((Asteroid)b.GetUserData()).VelocityExceeded)
                            ((Asteroid)a.GetUserData()).Destroy("Other");
                    }
                }
            }

            List<Asteroid> toRemove = new List<Asteroid>();
            foreach (Asteroid a in asteroids)
            {
                Bonus bonus = a.GetBonus();
                if (bonus != Bonus.None)
                {
                    bonuses.Add(bonus);
                    Score += a.Score * (int)bonus;

                    fuel = Math.Min(fuel + a.Score * (int)bonus * 0.0015f, MaxFuel);
                }

                if (a.body.Position.Y - ship.Position.Y > BackClearance) a.NotVisible();

                if (a.IsNotVisible) toRemove.Add(a);
            }

            foreach (Asteroid a in toRemove) asteroids.Remove(a);

            if (fuel <= 0 && ship.GetLinearVelocity().Length() < 1f) IsLevelUp = true;

            float movement = ship.Position.Y - (cameraPosY + FrontClearance);
            cameraPosY = cameraPosY + movement;
            cameraPosY = MathHelper.Clamp
                (cameraPosY, 0, TileHeight * Height - 600);

            if (cameraPosY == 0)
            {
                ship.Position = new Vector2(ship.Position.X, originalShipPos.Y);
                asteroidsCount += 2;
                LoadAsteroidField((int)Math.Floor(asteroidsCount));
            }
        }


        public void HandleInput(InputState input, PlayerIndex? controllingPlayer)
        {
            leftFlame = rightFlame = false;
            KeyboardState keyboardState = input.CurrentKeyboardStates[(int)controllingPlayer];

            if (fuel > 0.0f)
            {
                float f = 500, sideDampning = 0.5f;
                if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
                {
                    leftFlame = rightFlame = true;
                    ship.ApplyLinearImpulse(new Vector2(0, -f), ship.Position);
                }
                else
                    ship.ApplyLinearImpulse(new Vector2(0, .4f * -f), ship.Position);

                //if ((keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                //    )//&& ship.GetLinearVelocity().Y < 0)
                //      ship.ApplyLinearImpulse(new Vector2(0, f), ship.Position);

                if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
                {
                    rightFlame = true;
                    ship.ApplyLinearImpulse(new Vector2(-f * sideDampning, 0), ship.Position);
                }
                else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
                {
                    leftFlame = true;
                    ship.ApplyLinearImpulse(new Vector2(f * sideDampning, 0), ship.Position);
                }
            }

            if (leftFlame || rightFlame)
            {
                fuel -= 0.0003f * MaxFuel * ((leftFlame ? 1 : 0) + (rightFlame ? 1 : 0)) / 2; 
                engineInstance.Resume();
            }
            else
            {
                fuel -= 0.0001f * MaxFuel; engineInstance.Pause();
            }
        }



        #endregion

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawLayers(spriteBatch);

            spriteBatch.End();

            Matrix transform = Matrix.CreateTranslation(0, -cameraPosY, 0);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, transform);

            //spriteBatch.Draw(GameContent.bar, new Vector2(0, FrontClearance), Color.White);

            foreach (Asteroid a in asteroids)
                a.Draw(spriteBatch, gameTime);

            //DrawShip(spriteBatch, null);

            spriteBatch.End();

            spriteBatch.Begin();

            DrawShip(spriteBatch, "ghost");

            float velocity = ship.GetLinearVelocity().Length();
            if (velocity < 1) velocity = 0;

            spriteBatch.DrawString(GameContent.font, "velocity = " + velocity.ToString("000"),
                new Vector2(10, 500), Color.White);

            DrawFuelLevel(spriteBatch);
        }

        void DrawShip(SpriteBatch spriteBatch, string type)
        {
            Vector2 position = ship.Position;

            if (type == "ghost") position = new Vector2(position.X, FrontClearance);

            spriteBatch.Draw((Texture2D)ship.GetUserData(), position, null, Color.White, ship.Rotation,
                new Vector2(((Texture2D)ship.GetUserData()).Width / 2, ((Texture2D)ship.GetUserData()).Height / 2),
                1, SpriteEffects.None, 1);

            if (leftFlame)
                spriteBatch.Draw(GameContent.flame, position + new Vector2(-13, 27), null, Color.White, 0,
                    new Vector2(GameContent.flame.Width / 2, 0), 1, SpriteEffects.None, 1);

            if (rightFlame)
                spriteBatch.Draw(GameContent.flame, position + new Vector2(+13, 27), null, Color.White, 0,
                    new Vector2(GameContent.flame.Width / 2, 0), 1, SpriteEffects.FlipHorizontally, 1);
        }

        void DrawLayers(SpriteBatch spriteBatch)
        {
            float velocityY = ship.GetLinearVelocity().Y;

            for (int i = 0; i < 2; i++)
            {
                layerPos[i] += velocityY * (0.00001f + 0.02f * i); 
                float y = layerPos[i];
                int top = (int)Math.Floor(y / GameContent.background[0].Height);
                int bottom = top + 1;
                y = (y / GameContent.background[i].Height - top) * -GameContent.background[0].Height;

                spriteBatch.Draw(GameContent.background[i], new Vector2(0, y), Color.White);
                spriteBatch.Draw(GameContent.background[i], new Vector2(0, y + GameContent.background[i].Height),
                  Color.White);
            }
        }

        void DrawFuelLevel(SpriteBatch spriteBatch)
        {
            int maxHeight = 500;
            int height = (int)(fuel / MaxFuel * maxHeight), width = 5;
            Point position = new Point(350, 50);

            spriteBatch.Draw(GameContent.blank,
                new Rectangle(position.X, position.Y + (maxHeight - height), width, height),
                null, new Color(Color.White, 0.5f));
        }

        #endregion
    }
}
