using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameWindowsStarter
{
    /// <summary>
    /// A class representing a particle system.
    /// </summary>
    public class ParticleSystem
    {
        Particle[] particles;
        Texture2D texture;
        SpriteBatch spriteBatch;
        Random random = new Random();
        int nextIndex = 0;

        public Vector2 Emitter { get; set; }
        public int SpawnPerFrame { get; set; }


        /// <summary>
        /// Constructs a new particle engine 
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="size">The maximum number of particles in the system</param>
        /// <param name="texture">The texture of the particles</param> 
        public ParticleSystem(GraphicsDevice graphicsDevice, int size, Texture2D texture)
        {
            this.particles = new Particle[size];
            this.spriteBatch = new SpriteBatch(graphicsDevice);
            this.texture = texture;
        }


        /// <summary> 
        /// Updates the particle system, spawining new particles and 
        /// moving all live particles around the screen 
        /// </summary>
        /// <param name="gameTime">A structure representing time in the game</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Part 1: Spawn new particles 
            for (int i = 0; i < SpawnPerFrame; i++)
            {
                // Create & Spawn the particle at nextIndex
                particles[nextIndex].Position = Emitter;
                particles[nextIndex].Velocity = 100 * new Vector2((float)random.NextDouble(), (float)random.NextDouble());
                particles[nextIndex].Acceleration = 0.1f * new Vector2((float)random.NextDouble(), (float)random.NextDouble());
                particles[nextIndex].Color = Color.White;
                particles[nextIndex].Scale = 1f;
                particles[nextIndex].Life = 3.0f;

                // Advance the index 
                nextIndex++;
                if (nextIndex > particles.Length - 1) nextIndex = 0;
            }

            // Part 2: Update Particles
            for (int i = 0; i < particles.Length; i++)
            {
                // Skip any "dead" particles
                if (particles[i].Life <= 0) continue;

                // TODO: Update the individual particles
                particles[i].Velocity += deltaTime * particles[i].Acceleration;
                particles[i].Position += deltaTime * particles[i].Velocity;
                particles[i].Life -= deltaTime;
            }
        }


        /// <summary>
        /// Draw the active particles in the particle system
        /// </summary>
        public void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            // Iterate through the particles and draw them
            for (int i = 0; i < particles.Length; i++)
            {
                // Skip any "dead" particles
                if (particles[i].Life <= 0) continue;

                // Draw the individual particles
                spriteBatch.Draw(texture, particles[i].Position, null, particles[i].Color, 0f, Vector2.Zero, particles[i].Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

    }
}
