using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MonoGameWindowsStarter
{
    class Player : iUpdateable, iCollidable
    {
        public enum MovingState
        {
            South = 0,
            North = 1,
            West = 2,
            East = 3,
            Idle = 4,
        }

        public enum SpeedState
        {
            Normal = 0,
            Boost = 1,
            Penalty = 2,
        }

        public enum FightingState
        {
            Idle = 0,
            Block = 1,
            Attack = 2,
            Invincible = 3,
        }

        /// <summary>
        /// How quickly the animation should advance frames (1/8 second as milliseconds)
        /// </summary>
        const int ANIMATION_FRAME_RATE = 124;

        /// <summary>
        /// How quickly the player should move
        /// </summary>
        const float PLAYER_SPEED = 100;

        /// <summary>
        /// The width of the animation frames
        /// </summary>
        const int FRAME_WIDTH = 75;

        /// <summary>
        /// The height of the animation frames
        /// </summary>
        const int FRAME_HEIGHT = 100;

        const int FRAME_HEIGHT_GAP = 25;

        const float SPEED_DEFAULT = 1;
        const float SPEED_BOOST = 3;
        const float SPEED_PENALTY = .3f;
        const float MAX_STAMINA = 3000;

        // Other variables
        Game game;
        Texture2D texture;
        Texture2D powerUpBar;
        Texture2D powerUpBarBack;
        int powerUpBarSize;
        TimeSpan powerUpTimer;
        public MovingState moving_state;
        MovingState prev_moving_state;
        public FightingState fighting_state;
        TimeSpan timer;
        int frame;
        public BoundingRectangle Bounds;
        public Vector2 curPosition;
        public Bomb bomb;
        public SoundEffect ouchSFX;
        public SoundEffect ouchBombSFX;
        KeyboardState old_keyboard;
        KeyboardState keyboard;
        SpriteFont font;
        public float stamina;
        Texture2D staminaBar;
        Texture2D staminaBarBack;
        double staminaDelay = 500;
        TimeSpan staminaDelayTimer;
        int staminaBarSize;
        bool drainedStaminaPenalty = false;
        float speedVar = SPEED_DEFAULT;
        public SpeedState speed_state;
        

        /// <summary>
        /// Creates a new player object
        /// </summary>
        /// <param name="game"></param>
        public Player(Game game)
        {
            this.game = game;
            timer = new TimeSpan(0);
            curPosition = new Vector2(2, 352);
            Bounds = new BoundingRectangle(curPosition.X, curPosition.Y, 32, 45);
            moving_state = MovingState.Idle;
            prev_moving_state = MovingState.Idle;
            fighting_state = FightingState.Idle;
            speed_state = SpeedState.Normal;
            powerUpTimer = new TimeSpan(0);
            bomb = new Bomb(game);
            keyboard = Keyboard.GetState();
            stamina = MAX_STAMINA;
        }

        /// <summary>
        /// Loads the sprite's content
        /// </summary>
        public void LoadContent()
        {
            texture = game.Content.Load<Texture2D>("spritesheet");
            ouchSFX = game.Content.Load<SoundEffect>("ouchSFX");
            ouchBombSFX = game.Content.Load<SoundEffect>("ouchBombSFX");
            font = game.Content.Load<SpriteFont>("defaultFont");
            powerUpBar = game.Content.Load<Texture2D>("powerUpBarProgress");
            powerUpBarBack = game.Content.Load<Texture2D>("powerUpBarBack");
            staminaBar = game.Content.Load<Texture2D>("powerUpBarProgress");
            staminaBarBack = game.Content.Load<Texture2D>("powerUpBarBack");
            bomb.LoadContent();
        }

        /// <summary>
        /// Update the sprite, moving and animating it
        /// </summary>
        /// <param name="gameTime">The GameTime object</param>
        public void Update(GameTime gameTime)
        {
            old_keyboard = keyboard;
            keyboard = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Bomb PowerUp
            powerUpTimer += gameTime.ElapsedGameTime;
            if (keyboard.IsKeyDown(Keys.Space) 
                && !old_keyboard.IsKeyDown(Keys.Space) 
                && (int)powerUpTimer.TotalMilliseconds > 10000)
            {
                powerUpTimer = new TimeSpan(0);
                prev_moving_state = moving_state;
                moving_state = MovingState.Idle;

                // Place Bomb in front or below character
                switch(prev_moving_state)
                {
                    case MovingState.East:
                        bomb.Place(new Vector2(Bounds.X + Bounds.Width / 4, Bounds.Y + Bounds.Height / 2));
                        break;
                    case MovingState.North:
                        bomb.Place(new Vector2(Bounds.X + Bounds.Width / 4, Bounds.Y + Bounds.Height / 2));
                        break;
                    case MovingState.West:
                        bomb.Place(new Vector2(Bounds.X + Bounds.Width / 4, Bounds.Y + Bounds.Height / 2));
                        break;
                    case MovingState.South:
                        bomb.Place(new Vector2(Bounds.X + Bounds.Width / 4, Bounds.Y + Bounds.Height / 2));
                        break;
                    default:
                        bomb.Place(new Vector2(Bounds.X + Bounds.Width / 4, Bounds.Y + Bounds.Height / 2));
                        break;
                }
            }
            if (powerUpTimer.TotalMilliseconds > 10000) powerUpBarSize = 100;
            else powerUpBarSize = (int)(10 * powerUpTimer.TotalSeconds);

            bomb.Update(gameTime);

            // Stamina
            if(stamina < 0)
            {
                drainedStaminaPenalty = true;
                speedVar = SPEED_PENALTY;
                staminaDelayTimer = new TimeSpan(0);
                stamina = 0;
                speed_state = SpeedState.Penalty;
            }
            if((keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt)) && !drainedStaminaPenalty)
            {
                speedVar = SPEED_BOOST;
                stamina-=(float)gameTime.ElapsedGameTime.TotalMilliseconds;
                speed_state = SpeedState.Boost;
            }
            if (!(keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt)) && !drainedStaminaPenalty)
            {
                speedVar = SPEED_DEFAULT;
                stamina += (float)gameTime.ElapsedGameTime.TotalMilliseconds * .9f;
                speed_state = SpeedState.Normal;
            }
            if (drainedStaminaPenalty && !(staminaDelayTimer.TotalMilliseconds < staminaDelay))
            {
                stamina += (float)gameTime.ElapsedGameTime.TotalMilliseconds * .4f;
            }
            if(stamina >= MAX_STAMINA)
            {
                drainedStaminaPenalty = false;
                stamina = MAX_STAMINA;
            }
            staminaDelayTimer += gameTime.ElapsedGameTime;
            if (stamina == MAX_STAMINA) staminaBarSize = 100;
            else staminaBarSize = (int)(100 * (stamina / MAX_STAMINA));

            // Update the player state based on input
            if (keyboard.IsKeyDown(Keys.Up))
            {
                prev_moving_state = moving_state;
                moving_state = MovingState.North;
                curPosition.Y -= delta * PLAYER_SPEED * speedVar;
            }
            else if (keyboard.IsKeyDown(Keys.Left))
            {
                prev_moving_state = moving_state;
                moving_state = MovingState.West;
                curPosition.X -= delta * PLAYER_SPEED * speedVar;
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                prev_moving_state = moving_state;
                moving_state = MovingState.East;
                curPosition.X += delta * PLAYER_SPEED * speedVar;
            }
            else if (keyboard.IsKeyDown(Keys.Down))
            {
                prev_moving_state = moving_state;
                moving_state = MovingState.South;
                curPosition.Y += delta * PLAYER_SPEED * speedVar;
            }
            else
            {
                moving_state = MovingState.Idle;
            }

            if(curPosition.X < 0)
            {
                curPosition.X = 0;
            }

            // Update the player animation timer when the player is moving
            if (moving_state != MovingState.Idle) timer += gameTime.ElapsedGameTime;

            // Determine the frame should increase.  Using a while 
            // loop will accomodate the possiblity the animation should 
            // advance more than one frame.
            while (timer.TotalMilliseconds > ANIMATION_FRAME_RATE)
            {
                // increase by one frame
                frame++;
                // reduce the timer by one frame duration
                timer -= new TimeSpan(0, 0, 0, 0, ANIMATION_FRAME_RATE);
            }

            // Keep the frame within Bounds (there are four frames)
            frame %= 4;

            Bounds.X = curPosition.X;
            Bounds.Y = curPosition.Y;
        }

        /// <summary>
        /// Renders the sprite on-screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // determine the source rectagle of the sprite's current frame
            var source = new Rectangle(
                frame * (FRAME_WIDTH), // X value 
                (int)moving_state % 4 * (FRAME_HEIGHT + FRAME_HEIGHT_GAP), // Y value
                FRAME_WIDTH, // Width 
                FRAME_HEIGHT // Height
                );
            int x_value;
            if(moving_state == MovingState.Idle)
            {
                switch(prev_moving_state)
                {
                    case MovingState.East:
                        x_value = 1;
                        break;
                    default:
                        x_value = 0;
                        break;
                }
                source = new Rectangle(
                x_value * (FRAME_WIDTH), // X value 
                (int)prev_moving_state % 4 * (FRAME_HEIGHT + FRAME_HEIGHT_GAP), // Y value
                FRAME_WIDTH, // Width 
                FRAME_HEIGHT // Height
                );
            }
            bomb.Draw(spriteBatch);

            spriteBatch.DrawString(font, "Bomb Power: ", new Vector2(200, 0), Color.DarkRed);
            spriteBatch.Draw(powerUpBarBack, new Rectangle(300, 0, 100, 20), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 1);
            spriteBatch.Draw(powerUpBar, new Rectangle(300, 5, powerUpBarSize, 10), null, Color.Red, 0, Vector2.Zero, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, "Stamina: ", new Vector2(500, 0), Color.DarkBlue);
            spriteBatch.Draw(staminaBarBack, new Rectangle(600, 0, 100, 20), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 1);
            spriteBatch.Draw(staminaBar, new Rectangle(600, 5, staminaBarSize, 10), null, Color.LightBlue, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, $"{stamina}", new Vector2(625, 0), Color.White);
            spriteBatch.DrawString(font, $"{staminaDelayTimer.TotalMilliseconds}", new Vector2(700, 0), Color.White);
            spriteBatch.DrawString(font, $"{drainedStaminaPenalty && !(staminaDelayTimer.TotalMilliseconds < staminaDelay)}", new Vector2(700, 20), Color.White);
            //           spriteBatch.DrawString(font, $"{powerUpBarSize}", new Vector2(500, 10), Color.Black);

            //spriteBatch.DrawString(
            //    font,
            //    $"powerUpTimer.TotalMilliseconds:{powerUpTimer.TotalMilliseconds}",
            //    new Vector2(200, 0),
            //    Color.White
            //    );

            // render the sprite
            spriteBatch.Draw(texture, Bounds, source, Color.White);

        }

        public Vector2 Position()
        {
            return new Vector2(Bounds.X, Bounds.Y);
        }
    }
}
