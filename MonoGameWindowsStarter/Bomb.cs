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
    class Bomb : iCollidable
    {
        Game game;
        TimeSpan activated_timer;
        TimeSpan detonated_timer;
        TimeSpan timer;
        Texture2D bomb;
        Texture2D explosion;
        public BoundingCircle Bounds;
        public BoundingCircle Explosion;
        public SoundEffect fuseSFX;
        public SoundEffect explosionSFX;
        public bool placed;
        public bool active;
        public bool toDetonate;
        public bool detonated;
        bool explosionP;
        ParticleSystem explosionParticleSystem;
        Texture2D explosionParticle;
        Random random = new Random();

        public Bomb(Game game)
        {
            this.game = game;
        }

        public void Initialize()
        {
            placed = false;
            active = false;
            toDetonate = false;
            detonated = false;
            explosionP = false;
        }

        public void LoadContent()
        {
            bomb = game.Content.Load<Texture2D>("bomb");
            explosion = game.Content.Load<Texture2D>("explosion");
            fuseSFX = game.Content.Load<SoundEffect>("fuseSFX");
            explosionSFX = game.Content.Load<SoundEffect>("explosionSFX");

            explosionParticle = game.Content.Load<Texture2D>("particle");
            explosionParticleSystem = newExplosionParticleSystem(game.GraphicsDevice, 5000, explosionParticle);
        }

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime;
            if(placed)
            {
                fuseSFX.Play();
                toDetonate = true;
                placed = false;
            }
            if(timer.TotalMilliseconds - activated_timer.TotalMilliseconds > fuseSFX.Duration.TotalMilliseconds && toDetonate)
            {
                explosionSFX.Play();
                active = false;
                detonated = true;
                toDetonate = false;
                detonated_timer = new TimeSpan(0);
                explosionP = true;
            }
            if (timer.TotalMilliseconds - detonated_timer.TotalMilliseconds > explosionSFX.Duration.TotalMilliseconds * 10 && detonated)
            {
                detonated = false;
            }
            if(explosionP)
            {
                explosionParticleSystem.Update(gameTime);
            }
            if (timer.TotalMilliseconds - detonated_timer.TotalMilliseconds > explosionSFX.Duration.TotalMilliseconds * 11 && explosionP)
            {
                explosionP = false;
                explosionParticleSystem = newExplosionParticleSystem(game.GraphicsDevice, 5000, explosionParticle);
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(active)
            {
                spriteBatch.Draw(bomb, Bounds, Color.White);
            }
            if(detonated)
            {
                //spriteBatch.Draw(explosion, Explosion, Color.White);
            }
            if(explosionP)
            {
                explosionParticleSystem.Draw();
            }
            
        }

        public Vector2 Position()
        {
            return new Vector2(Bounds.X, Bounds.Y);
        }

        public void Place(Vector2 position)
        {
            Bounds.Radius = 12;
            Bounds.X = position.X + Bounds.Radius;
            Bounds.Y = position.Y + Bounds.Radius;
            Explosion.Radius = 30;
            Explosion.X = Bounds.X;
            Explosion.Y = Bounds.Y;
            activated_timer = new TimeSpan(0);
            timer = new TimeSpan(0);
            placed = true;
            active = true;
            toDetonate = false;
            detonated = false;
        }

        public ParticleSystem newExplosionParticleSystem(GraphicsDevice graphicsDevice, int size, Texture2D explosionParticle)
        {
            explosionParticleSystem = new ParticleSystem(graphicsDevice, size, explosionParticle);
            explosionParticleSystem.Emitter = new Vector2(100, 100);
            explosionParticleSystem.SpawnPerFrame = 10;
            // Set the SpawnParticle method
            explosionParticleSystem.SpawnParticle = (ref Particle particle) =>
            {
                Vector2 particlePosition = new Vector2(
                    MathHelper.Lerp(Position().X - 20, Position().X + 20, (float)random.NextDouble()), // X between this X +-5
                    MathHelper.Lerp(Position().Y - 20, Position().Y + 20, (float)random.NextDouble()) // Y between this Y +-5
                    );
                particle.Position = particlePosition - new Vector2(Bounds.Radius, Bounds.Radius);
                Vector2 particleVelocity = (particlePosition - Position()) * 3;
                particle.Velocity = particleVelocity;
                Vector2 particleAcceleration = 0.1f * new Vector2((float)-random.NextDouble(), (float)-random.NextDouble());
                particle.Acceleration = particleAcceleration;
                particle.Color = Color.Red;
                if (particleVelocity.Length() > 30f)
                {
                    particle.Color = Color.Yellow;
                }
                particle.Scale = 1f;
                particle.Life = 1.0f;
            };

            // Set the UpdateParticle method
            explosionParticleSystem.UpdateParticle = (float deltaT, ref Particle particle) =>
            {
                particle.Velocity += deltaT * particle.Acceleration;
                particle.Position += deltaT * particle.Velocity;
                particle.Scale -= deltaT;
                particle.Life -= 2*deltaT;
            };

            return explosionParticleSystem;
        }
    }
}
