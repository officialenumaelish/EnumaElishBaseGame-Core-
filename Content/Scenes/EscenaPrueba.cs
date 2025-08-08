using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Entities;
using MonoGameLibrary.Events;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace EnumaElishMonoGameMultiplayer.Core.Content.Scenes
{
    public class EscenaPrueba : Scene
    {

        private Player jugador;

        private Sprite _mainSprite;
        private Vector2 _mainSpritePosition;

        private AnimatedSprite _mainAnimatedSprite;
        private Vector2 _mainAnimatedSpritePosition;

        private const float MOVEMENT_SPEED = 5.0f;

        private Tilemap _tilemap;

     

        private SoundEffect _bounceSoundEffect;
        private SoundEffect _collectSoundEffect;

        public EscenaPrueba()
        {
          SpriteManager = new SpriteManager();
        }


        private SoundEffect _uiSoundEffect;


     
        private void InitializeUI()
        {
         
        }

        SpriteFont font;
        Button button;
        public override void Initialize()
        {
            TextureAtlas textureAtlas = TextureAtlas.FromFile(Content, "Sprites/referencia.xml");
            TextureAtlas ta = TextureAtlas.FromFile(Content, "Utilities/hoja_de_utilidades.xml");

            button = new Button();
                
            button.AddSprite(MonoGameLibrary.Entities.ButtonState.normal, ta.CreateSprite("standar"));
            button.AddSprite(MonoGameLibrary.Entities.ButtonState.hover, ta.CreateSprite("standar_hover"));
            button.Position(new Vector2(500));
            button.Scale(new Vector2(0.5f));
            button.Clicked += () => { Console.WriteLine("Presionado"); Debug.WriteLine("Pesionado"); };
            _tilemap = Tilemap.FromFile(Content, "Sprites/tilemap_definitions.xml");
            _tilemap.Scale = new Vector2(0.3f);

            font = Content.Load<SpriteFont>("Fonts/mainfont");
            // Crear sprites
            _mainSprite = textureAtlas.CreateSprite("botella_dorada");
            _mainAnimatedSprite = textureAtlas.CreateAnimatedSprite("main");

            // Configurar posiciones iniciales
            _mainSpritePosition = new Vector2(50f, 50f);
            _mainAnimatedSpritePosition = new Vector2(200f, 50f);

            jugador = new Player(textureAtlas);
            jugador.Position(new Vector2(200f));

            jugador.AddAnimacion(PlayerState.CorrerDerecha, "main");
            jugador.AddAnimacion(PlayerState.CorrerIzquierda, "main2");
            jugador.ActivateAnimation(PlayerState.CorrerIzquierda);

            // NO configures Origin aquí si tu SpriteManager lo va a manejar
            // _mainSprite.Origin = _mainSpritePosition;  ← REMOVER
            // _mainAnimatedSprite.Origin = _mainAnimatedSpritePosition;  ← REMOVER

            Audio = new AudioController();

            // Load the bounce sound effect.s
            _bounceSoundEffect = Content.Load<SoundEffect>("Audio/bounce");

            // Load the collect sound effect.
            _collectSoundEffect = Content.Load<SoundEffect>("Audio/collect");

            camera = new Camera2D()
            {
                Scale = new Vector2(1f, 1f)
            };


            //gum

            InitializeUI();
            base.Initialize();
        }

        public override void LoadContent()
        {
            SpriteManager.AddPlayer(jugador);
            SpriteManager.colisionesJugador.Add(_mainSprite);
            SpriteManager.colisionesJugador.Add(_mainAnimatedSprite);
            SpriteManager.Add(_mainSprite, new Vector2());
            SpriteManager.Add(_mainAnimatedSprite, new Vector2());
            base.LoadContent();
        }

        HashSet<int> sinColisionarSprite = new HashSet<int>();
        HashSet<int> sinColisionarSpriteAnimated = new HashSet<int>();
        public override void Update(GameTime gameTime)
        {
            button.Update(gameTime);
            SpriteManager.UpdateAnimated(gameTime);
            sinColisionarSprite.Add(_mainAnimatedSprite.SpriteId);
            sinColisionarSpriteAnimated.Add(_mainSprite.SpriteId);


            SpriteManager.SetPosition(_mainAnimatedSprite, _mainAnimatedSpritePosition, sinColisionarSpriteAnimated);
            interceptarTeclado();


            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        { 
            EnumaElishMonoGameMultiplayerGame.GraphicsDevice.Clear(Color.CornflowerBlue);
            var brocha = EnumaElishMonoGameMultiplayerGame.SpriteBatch;
            brocha.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());

            _tilemap.Draw(brocha);
            SpriteManager.Draw(brocha);
            EnumaElishMonoGameMultiplayerGame.SpriteBatch.End();

            //InitializeUI();
             
            MonoGameLibrary.Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            button.Draw(brocha);
            //if (_titleScreenButtonsPanel.IsVisible)
            //{
            //    // Begin the sprite batch to prepare for rendering.

            //    // The color to use for the drop shadow text.
            //    Color dropShadowColor = Color.Black * 0.5f;

            //    // Draw the Dungeon text slightly offset from it is original position and
            //    // with a transparent color to give it a drop shadow
            //    MonoGameLibrary.Core.SpriteBatch.DrawString(font, "Hols", _mainSpritePosition + new Vector2(10, 10), dropShadowColor, 0.0f, _mainSpritePosition, 1.0f, SpriteEffects.None, 1.0f);

            //    // Draw the Dungeon text on top of that at its original position
            //    MonoGameLibrary.Core.SpriteBatch.DrawString(font, "DUNGEON_TEXT", _mainSpritePosition, Color.White, 0.0f, _mainSpritePosition, 1.0f, SpriteEffects.None, 1.0f);

            //    // Draw the Slime text slightly offset from it is original position and
            //    // with a transparent color to give it a drop shadow
            //    MonoGameLibrary.Core.SpriteBatch.DrawString(font, "SLIME_TEXT", _mainSpritePosition + new Vector2(10, 10), dropShadowColor, 0.0f, _mainSpritePosition, 1.0f, SpriteEffects.None, 1.0f);

            //    // Draw the Slime text on top of that at its original position
            //    MonoGameLibrary.Core.SpriteBatch.DrawString(font, "SLIME_TEXT", _mainSpritePosition, Color.White, 0.0f, _mainSpritePosition, 1.0f, SpriteEffects.None, 1.0f);

            //}

            EnumaElishMonoGameMultiplayerGame.SpriteBatch.End();
            base.Draw(gameTime);
        }



        private void interceptarTeclado()
        {
            KeyboardInfo keyboard = MonoGameLibrary.Core.Input.Keyboard;

            float estadoAnterior_Y = jugador.Y();
            float estadoAnterior_X = jugador.X();

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

            int velocidad = (int)MOVEMENT_SPEED;
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Space))
            {
                velocidad += 3;
            }
            else
            {
            }

            if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A))
            {
                jugador.MoveLeft(velocidad);
                jugador.ActivateAnimation(PlayerState.CorrerIzquierda);

            }

            if (state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D))
            {
                jugador.MoveRigth(velocidad);
                jugador.ActivateAnimation(PlayerState.CorrerDerecha);

            }

            if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W))
            {
                jugador.MoveUp(velocidad);
            }

            if (state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S))
            {
                jugador.MoveDown(velocidad);
            }
            if (keyboard.IsKeyDown(Keys.Enter))
            {
               

            }


            //_mainCharacterPosition.X = MathHelper.Clamp(
            //        _mainCharacterPosition.X,
            //        0,
            //        _worldWidth - _mainCharacter.Width);

            //_mainCharacterPosition.Y = MathHelper.Clamp(
            //    _mainCharacterPosition.Y,
            //    0,
            //    _worldHeight - _mainCharacter.Height);
           bool colision = SpriteManager.SetPlayerPosition(jugador.Position());

            if (colision)
            {
                _bounceSoundEffect.Play();
                jugador.Y(estadoAnterior_Y);
                jugador.X(estadoAnterior_X);
            }
        }
    }
}

