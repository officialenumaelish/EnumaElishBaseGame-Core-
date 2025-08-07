using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnumaElishMonoGameMultiplayer.Content.Scenes;
using EnumaElishMonoGameMultiplayer.Core;
using EnumaElishMonoGameMultiplayer.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace EnumaElishMonoGameMultiplayer.Content.Scenes
{
    public class GameScene : Scene
    {

        // Defines the slime animated sprite.
        private AnimatedSprite _slime;

        // Defines the bat animated sprite.
        private AnimatedSprite _bat;

        // Tracks the position of the slime.
        private Vector2 _slimePosition;

        // Speed multiplier when moving.
        private const float MOVEMENT_SPEED = 5.0f;

        // Tracks the position of the bat.
        private Vector2 _batPosition;

        // Tracks the velocity of the bat.
        private Vector2 _batVelocity;

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        // Defines the bounds of the room that the slime and bat are contained within.
        private Rectangle _roomBounds;

        // The sound effect to play when the bat bounces off the edge of the screen.
        private SoundEffect _bounceSoundEffect;

        // The sound effect to play when the slime eats a bat.
        private SoundEffect _collectSoundEffect;

        // The SpriteFont Description used to draw text
        private SpriteFont _font;

        // Tracks the players score.
        private int _score;

        // Defines the position to draw the score text at.
        private Vector2 _scoreTextPosition;

        // Defines the origin used when drawing the score text.
        private Vector2 _scoreTextOrigin;


        private Sprite _botellaDorada;
        private Vector2 _botellaPosition;
        private Rectangle _colisionBotellaDorada;
        private Sprite _primerArbol;
        private Vector2 posicionPrimerArbol;
        private Rectangle _colisionPrimerArbol;

        private Sprite _segundoAbol;
        private Sprite _tercerArbol;
        private Sprite _cuartoArbol;
        private Sprite _quintoArbol;
        public Sprite _sextoArbol;
        private Sprite customCursor;
        private Vector2 cursorPosition;
        private int velocidadBotella = 3;

        private Vector2 texto;
        private Camera2D camera;

        private SaveAndDrawAll saveAndDrawAll;

        public static AudioController Audio { get; private set; }

        int _width;
        int _height;
        public GameScene(int width, int heigth)
        {
            _width = width;
            _height = heigth;
            saveAndDrawAll = new SaveAndDrawAll();

        }

        // The sound effect to play when the bat bounces off the edge of the screen.

        public override void Initialize()
        {
            // LoadContent is called during base.Initialize().
            base.Initialize();

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);


            // Set the position of the score text to align to the left edge of the
            // room bounds, and to vertically be at the center of the first tile.
            _scoreTextPosition = new Vector2(300, _tilemap.TileHeight * 0.5f);

            // Set the origin of the text so it is left-centered.
            float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
            _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);

            texto = new Vector2(0);

        }

        public override void LoadContent()
        {
            TextureAtlas textureAtlas = TextureAtlas.FromFile(Content, "Sprites/referencia.xml");
            _tilemap = Tilemap.FromFile(Content, "Sprites/tilemap_definitions.xml");
            _tilemap.Scale = new Vector2(0.28f, 0.28f);

            _primerArbol = textureAtlas.CreateSprite("arbol_rosado_1");
            _primerArbol.Scale = new Vector2(2f, 2f);

            _botellaDorada = textureAtlas.CreateSprite("botella_dorada");
            _botellaDorada.Scale = new Vector2(4f, 4f);

            _segundoAbol = textureAtlas.CreateSprite("arbol_rosado_1");
            _segundoAbol.Scale = new Vector2(2f, 2f);

            _tercerArbol = textureAtlas.CreateSprite("arbol_rosado_1");
            _tercerArbol.Scale = new Vector2(2f, 2f);

            customCursor = textureAtlas.CreateSprite("botella_dorada"); // Asegúrate de tenerlo en el Content Pipeline

            Audio = new AudioController();


            // Load the bounce sound effect.
            _bounceSoundEffect = Content.Load<SoundEffect>("Audio/bounce");

            // Load the collect sound effect.
            _collectSoundEffect = Content.Load<SoundEffect>("Audio/collect");


            _font = Content.Load<SpriteFont>("Fonts/mainfont");

            camera = new Camera2D();
            _botellaPosition.X = 1;
            _botellaPosition.Y = 1;

            _botellaPosition = new();
            _slimePosition = new();
            saveAndDrawAll.Add(_tercerArbol, _botellaPosition);
            saveAndDrawAll.Add(_primerArbol, _slimePosition);

            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {

            var worldWidth = _tilemap.TileWidth * _tilemap.TileWidth;
            var worldHeight = _tilemap.TileHeight * _tilemap.TileHeight;


            int viewportWidth =_width;
            int viewportHeight = _height;

            camera.Position = _botellaPosition - new Vector2(viewportWidth / 2, viewportHeight / 2);

            camera.Position = new Vector2(
                MathHelper.Clamp(camera.Position.X, 0, worldWidth - viewportWidth),
                MathHelper.Clamp(camera.Position.Y, 0, worldHeight - viewportHeight)
            );

            _colisionBotellaDorada = new Rectangle(
               (int)_botellaPosition.X,
               (int)_botellaPosition.Y,
               (int)_botellaDorada.Width,
               (int)_botellaDorada.Height
           );

          

            interceptarTeclado();
            CheckGamePadInput();
            // TODO: Add your update logic here
            MouseState mouseState = Mouse.GetState();
            cursorPosition = new Microsoft.Xna.Framework.Vector2(mouseState.X, mouseState.Y);

            saveAndDrawAll.SetPosition(_tercerArbol, _botellaPosition);
            saveAndDrawAll.SetPosition(_primerArbol, _slimePosition);
            // Update the audio controller.
            Audio.Update();

            base.Update(gameTime);
        }

        private void AssignRandomBatVelocity()
        {
            // Generate a random angle.
            float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

            // Convert angle to a direction vector.
            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);
            Vector2 direction = new Vector2(x, y);

            // Multiply the direction vector by the movement speed
            _batVelocity = direction * MOVEMENT_SPEED;
        }

        private void CheckKeyboardInput()
        {
            // Get a reference to the keyboard inof
            KeyboardInfo keyboard = MonoGameLibrary.Core.Input.Keyboard;

            // If the escape key is pressed, return to the title screen.
            if (MonoGameLibrary.Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            {
                MonoGameLibrary.Core.ChangeScene(new TitleScene(_width, _height));
            }

            // If the space key is held down, the movement speed increases by 1.5
            float speed = MOVEMENT_SPEED;
            if (keyboard.IsKeyDown(Keys.Space))
            {
                speed *= 1.5f;
            }

            // If the W or Up keys are down, move the slime up on the screen.
            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            {
                _slimePosition.Y -= speed;
            }

            // if the S or Down keys are down, move the slime down on the screen.
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            {
                _slimePosition.Y += speed;
            }

            // If the A or Left keys are down, move the slime left on the screen.
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            {
                _slimePosition.X -= speed;
            }

            // If the D or Right keys are down, move the slime right on the screen.
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            {
                _slimePosition.X += speed;
            }

            // If the M key is pressed, toggle mute state for audio.
            if (keyboard.WasKeyJustPressed(Keys.M))
            {
                MonoGameLibrary.Core.Audio.ToggleMute();
            }

            // If the + button is pressed, increase the volume.
            if (keyboard.WasKeyJustPressed(Keys.OemPlus))
            {
                MonoGameLibrary.Core.Audio.SongVolume += 0.1f;
                MonoGameLibrary.Core.Audio.SoundEffectVolume += 0.1f;
            }

            // If the - button was pressed, decrease the volume.
            if (keyboard.WasKeyJustPressed(Keys.OemMinus))
            {
                MonoGameLibrary.Core.Audio.SongVolume -= 0.1f;
                MonoGameLibrary.Core.Audio.SoundEffectVolume -= 0.1f;
            }
        }

        private void CheckGamePadInput()
        {
            // Get the gamepad info for gamepad one.
            GamePadInfo gamePadOne = MonoGameLibrary.Core.Input.GamePads[(int)PlayerIndex.One];

            // If the A button is held down, the movement speed increases by 1.5
            // and the gamepad vibrates as feedback to the player.
            float speed = MOVEMENT_SPEED;
            if (gamePadOne.IsButtonDown(Buttons.A))
            {
                speed *= 1.5f;
                GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
            }
            else
            {
                GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
            }

            // Check thumbstick first since it has priority over which gamepad input
            // is movement.  It has priority since the thumbstick values provide a
            // more granular analog value that can be used for movement.
            if (gamePadOne.LeftThumbStick != Vector2.Zero)
            {
                _slimePosition.X += gamePadOne.LeftThumbStick.X * speed;
                _slimePosition.Y -= gamePadOne.LeftThumbStick.Y * speed;
            }
            else
            {
                // If DPadUp is down, move the slime up on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadUp))
                {
                    _slimePosition.Y -= speed;
                }

                // If DPadDown is down, move the slime down on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadDown))
                {
                    _slimePosition.Y += speed;
                }

                // If DPapLeft is down, move the slime left on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
                {
                    _slimePosition.X -= speed;
                }

                // If DPadRight is down, move the slime right on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadRight))
                {
                    _slimePosition.X += speed;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var cubeta = EnumaElishMonoGameMultiplayerGame.SpriteBatch;
            // Clear the back buffer.
            // Clears the screen with the MonoGame orange color before drawing.
            EnumaElishMonoGameMultiplayerGame.GraphicsDevice.Clear(Color.CornflowerBlue);
            cubeta.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
            HashSet<Sprite> SpritesNoDibujar = new HashSet<Sprite>();

            SpritesNoDibujar.Add(_primerArbol);

            saveAndDrawAll.DrawAllExcept(cubeta, SpritesNoDibujar);
            //saveAndDrawAll.DrawAll(cubeta);
            //_tilemap.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch);
            //_botellaDorada.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch,
            //    _slimePosition
            // );

            //posicionPrimerArbol = new Vector2(200, 400);
            //_primerArbol.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, posicionPrimerArbol);

            //_segundoAbol.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, new Vector2(300, 50));

            //_tercerArbol.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, _botellaPosition);

            //// Draw the score
            //EnumaElishMonoGameMultiplayerGame.SpriteBatch.DrawString(
            //    _font,              // spriteFontsddds
            //    $"Score: {_score}", // text
            //    _scoreTextPosition, // position
            //    Color.LightPink,        // color
            //    0.0f,               // rotation
            //    _scoreTextOrigin,   // origin
            //    1.0f,               // scale
            //    SpriteEffects.None, // effects
            //    0.0f                // layerDepth
            //);

            //EnumaElishMonoGameMultiplayerGame.SpriteBatch.DrawString(_font, "Hola Mundo", new Vector2(), Color.Black);

            //// Define los offsets y las nuevas dimensiones para el rectángulo de colisión
            //float collisionWidth = _primerArbol.Width * 0.2f;  // 20% del ancho original
            //float collisionHeight = _primerArbol.Height * 0.3f; // 30% del alto original

            //// Calcula la posición X e Y del rectángulo para centrarlo en la base
            //// Para centrar horizontalmente: (ancho_total / 2) - (ancho_colision / 2)
            //float offsetX = (_primerArbol.Width - collisionWidth) / 2;
            //// Para colocarlo en la parte inferior: alto_total - alto_colision
            //float offsetY = _primerArbol.Height - collisionHeight;


            //_colisionPrimerArbol = new Rectangle(
            //    (int)(posicionPrimerArbol.X + offsetX) + 20,
            //    (int)(posicionPrimerArbol.Y + offsetY),
            //    (int)collisionWidth,
            //    (int)collisionHeight
            //);

            //customCursor.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, cursorPosition);

            EnumaElishMonoGameMultiplayerGame.SpriteBatch.End();

            ////_slime.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, _slimePosition);


            base.Draw(gameTime);
        }


        bool SPACE_WAS_PRESED = false;
        private void interceptarTeclado()
        {
            KeyboardInfo keyboard = MonoGameLibrary.Core.Input.Keyboard;


            // If the M key is pressed, toggle mute state for audio.
            if (keyboard.WasKeyJustPressed(Keys.M))
            {
                Audio.ToggleMute();
            }

            // If the + button is pressed, increase the volume.
            if (keyboard.WasKeyJustPressed(Keys.OemPlus))
            {
                Audio.SongVolume += 0.1f;
                Audio.SoundEffectVolume += 0.1f;
            }

            // If the - button was pressed, decrease the volume.
            if (keyboard.WasKeyJustPressed(Keys.OemMinus))
            {
                Audio.SongVolume -= 0.1f;
                Audio.SoundEffectVolume -= 0.1f;
            }

            int velocidad = velocidadBotella;
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Space))
            {
                velocidad += 3;
                if (!SPACE_WAS_PRESED)
                {
                    Audio.PlaySoundEffect(_collectSoundEffect);
                    SPACE_WAS_PRESED = true;
                    _score += 100;

                }

            }
            else
            {
                SPACE_WAS_PRESED = false;
            }

            // Simula y aplica cada dirección por separado
            Vector2 nuevaPosicion = _botellaPosition;

            if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A))
            {
                var rect = new Rectangle((int)(nuevaPosicion.X - velocidad), (int)nuevaPosicion.Y,
                    (int)_botellaDorada.Width, (int)_botellaDorada.Height);
                if (!rect.Intersects(_colisionPrimerArbol))
                {
                    _botellaPosition.X -= velocidad;
                }
                else
                {
                    // Play the bounce sound effect.
                    Audio.PlaySoundEffect(_bounceSoundEffect);
                }
            }

            if (state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D))
            {
                var rect = new Rectangle((int)(nuevaPosicion.X + velocidad), (int)nuevaPosicion.Y,
                    (int)_botellaDorada.Width, (int)_botellaDorada.Height);
                if (!rect.Intersects(_colisionPrimerArbol))
                {
                    _botellaPosition.X += velocidad;
                }
                else
                {
                    // Play the bounce sound effect.
                    Audio.PlaySoundEffect(_bounceSoundEffect);
                }
            }

            if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W))
            {
                var rect = new Rectangle((int)nuevaPosicion.X, (int)(nuevaPosicion.Y - velocidad),
                    (int)_botellaDorada.Width, (int)_botellaDorada.Height);
                if (!rect.Intersects(_colisionPrimerArbol))
                {
                    _botellaPosition.Y -= velocidad;
                }
                else
                {
                    // Play the bounce sound effect.
                    Audio.PlaySoundEffect(_bounceSoundEffect);
                }
            }

            if (state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S))
            {
                var rect = new Rectangle((int)nuevaPosicion.X, (int)(nuevaPosicion.Y + velocidad),
                    (int)_botellaDorada.Width, (int)_botellaDorada.Height);
                if (!rect.Intersects(_colisionPrimerArbol))
                {
                    _botellaPosition.Y += velocidad;
                }
                else
                {
                    // Play the bounce sound effect.
                    Audio.PlaySoundEffect(_bounceSoundEffect);
                }
            }
        }

    }
}
