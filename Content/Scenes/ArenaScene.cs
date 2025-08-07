using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using EnumaElishMonoGameMultiplayer.Content.Scenes;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.ServerSideRendering;

namespace EnumaElishMonoGameMultiplayer.Core.Content.Scenes
{
    public class ArenaScene : Scene
    {

        #region Server
        private HubConnection _hubConnection;
        private string _playerId = Guid.NewGuid().ToString();


        #endregion
        #region Graphics
        private Tilemap _tilemap;
        private Sprite _mainCharacter;
        private Camera2D camera;
        private Sprite _player2;
        // En la parte superior de la clase ArenaScene
        private float _worldWidth;
        private float _worldHeight;

        private int _width;
        private int _height;

        #endregion

        #region Position
        private Vector2 _mainCharacterPosition;
        private Vector2 _player2Position;
        #endregion

        #region Colitions
        private Rectangle _mainCharacterCollisionBox;
        private Rectangle _player2CollisionBox;
        #endregion

        #region Input
        private const float MOVEMENT_SPEED = 5.0f;
        public static AudioController Audio { get; private set; }

        private SoundEffect _bounceSoundEffect;
        private SoundEffect _collectSoundEffect;

        #endregion

        public ArenaScene(int width, int height)
        {
            _width = width;
            _height = height;
            TouchPanel.EnableMouseGestures = true;  // para probar en PC con mouse  

        }
        // Cambia esto:
        // public override void Initialize()

        // Por esto:

        private async Task ConnectToHub()
        {

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://mysoftmy-001-site15.anytempurl.com/arenahub", options => {
                    //smarterasp: http://mysoftmy-001-site15.anytempurl.com/arenahub
                    //azure: https://backendapppastoral-app-202508011.purpleflower-fdb48b7c.eastus2.azurecontainerapps.io //esta pausada la api en prod
                    // Fallback a LongPolling si WebSockets da problemas
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect()
                .Build();

            // Suscripción al evento de recibir posiciones
            _hubConnection.On<string, float, float>("ReceivePosition", (playerId, x, y) =>
            {
                _player2Position.X = x;
                _player2Position.Y = y;
                Console.WriteLine($"Jugador {playerId} está en X: {x}, Y: {y}");
            });


            // Manejo de reconexión / cierre
            _hubConnection.Reconnecting += ex => {
                Console.WriteLine("Reconnecting...");
                return Task.CompletedTask;
            };
            _hubConnection.Reconnected += id => {
                Console.WriteLine("Reconnected, connectionId=" + id);
                return Task.CompletedTask;
            };
            _hubConnection.Closed += ex => {
                Console.WriteLine("Connection closed, ex=" + ex?.Message);
                return Task.CompletedTask;
            };

            try
            {
                try
                {
                    await _hubConnection.StartAsync();
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine("Error de conexión:");
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.InnerException?.Message);
                }
                Debug.WriteLine("✔️ SignalR conectado! ConnectionId: " + _hubConnection.ConnectionId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al conectar SignalR: " + ex);
            }
        }
        public override void Initialize()
        {

            base.Initialize();
        }

        public override void LoadContent()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Tap;


            Task.Run(ConnectToHub().GetAwaiter);

            TextureAtlas textureAtlas = TextureAtlas.FromFile(Content, "Sprites/referencia.xml");
            _tilemap = Tilemap.FromFile(Content, "Sprites/tilemap_definitions.xml");
            _tilemap.Scale = new Vector2(0.3f);
            _mainCharacter = textureAtlas.CreateSprite("botella_dorada");
            _mainCharacter.Scale = new Vector2(1.5f, 1.5f);

            _player2 = textureAtlas.CreateSprite("botella_dorada");
            _player2.Scale = new Vector2(1.5f, 1.5f);

            Audio = new AudioController();
                
            // Load the bounce sound effect.s
            _bounceSoundEffect = Content.Load<SoundEffect>("Audio/bounce");

            // Load the collect sound effect.
            _collectSoundEffect = Content.Load<SoundEffect>("Audio/collect");

            camera = new Camera2D()
            {
                Scale = new Vector2(1f, 1f)
            };

            _mainCharacterPosition = new Vector2(100, 100);
            //

          

            base.LoadContent();
        }   

        public override void Update(GameTime gameTime)
        {

            //if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            //{

            //}
            // ————— 1) PRIMERO procesa input y colisiones —————
            ProcessTouch(gameTime);
            interceptarTeclado();



          


            // ————— 2) Calcula worldWidth/worldHeight (usar lógicas SIN escalar) —————
            float logicalWorldWidth = _tilemap.Columns * _tilemap.TileWidth;
            float logicalWorldHeight = _tilemap.Rows * _tilemap.TileHeight;

            // ————— AQUÍ AÑADIMOS EL CLAMPING —————
            // Restringe la posición del personaje a los límites del mapa
            _mainCharacterPosition.X = MathHelper.Clamp(
                _mainCharacterPosition.X,
                0,
                logicalWorldWidth - _mainCharacter.Width);

            _mainCharacterPosition.Y = MathHelper.Clamp(
                _mainCharacterPosition.Y,
                0,
                logicalWorldHeight - _mainCharacter.Height);
            // ————— 3) Centrar y clampear la cámara —————
            // Tamaño de la vista en unidades de mundo según el scale de cámara
            float viewWorldWidth = _width / camera.Scale.X;
            float viewWorldHeight = _height / camera.Scale.Y;

            // Posición deseada centrada en personaje
            Vector2 desiredPos = _mainCharacterPosition
                - new Vector2(viewWorldWidth / 2, viewWorldHeight / 2);

            // Clamp
            camera.Position = new Vector2(
                MathHelper.Clamp(desiredPos.X, 0, logicalWorldWidth - viewWorldWidth),
                MathHelper.Clamp(desiredPos.Y, 0, logicalWorldHeight - viewWorldHeight)
            );

            // ————— 4) Finalmente actualiza colisiones, audio, base.Update —————
            _mainCharacterCollisionBox = new Rectangle(
                (int)_mainCharacterPosition.X,
                (int)_mainCharacterPosition.Y,
                (int)_mainCharacter.Width,
                (int)_mainCharacter.Height);

            _player2CollisionBox = new Rectangle(
                (int)_player2Position.X,
                (int)_player2Position.Y,
                (int)_player2.Width,
                (int)_player2.Height);

            Audio.Update();
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            EnumaElishMonoGameMultiplayerGame.GraphicsDevice.Clear(Color.CornflowerBlue);
            EnumaElishMonoGameMultiplayerGame.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());

            _tilemap.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch);
            _mainCharacter.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, _mainCharacterPosition);

            _player2.Draw(EnumaElishMonoGameMultiplayerGame.SpriteBatch, _player2Position);

            EnumaElishMonoGameMultiplayerGame.SpriteBatch.End();
            base.Draw(gameTime);
        }




        #region InputKeyboard

        bool SPACE_WAS_PRESED;
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

            int velocidad = (int)MOVEMENT_SPEED;
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Space))
            {
                velocidad += 3;
                if (!SPACE_WAS_PRESED)
                {
                    Audio.PlaySoundEffect(_collectSoundEffect);
                    SPACE_WAS_PRESED = true;
                }

            }
            else
            {
                SPACE_WAS_PRESED = false;
            }

            // Simula y aplica cada dirección por separado
            Vector2 nuevaPosicion = _mainCharacterPosition;

            if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A))
            {
                var rect = new Rectangle((int)(nuevaPosicion.X - velocidad), (int)nuevaPosicion.Y,
                    (int)_mainCharacter.Width, (int)_mainCharacter.Height);
                if (!rect.Intersects(_player2CollisionBox))
                {
                    _mainCharacterPosition.X -= velocidad;
                    // Después de mover a _mainCharacterPosition…
                    if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                    {
                        try
                        {
                            // Trazamos el envío
                            Debug.WriteLine(
                                $"▶️ Enviando posición: {_mainCharacterPosition.X:F1}, {_mainCharacterPosition.Y:F1}"
                            );


                            _hubConnection.SendAsync(
                               "UpdatePosition",
                               _playerId,
                               _mainCharacterPosition.X,
                               _mainCharacterPosition.Y
                           );
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("❌ Error SendAsync: " + ex.Message);
                        }
                    }
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
                    (int)_mainCharacter.Width, (int)_mainCharacter.Height);
                if (!rect.Intersects(_player2CollisionBox))
                {
                    _mainCharacterPosition.X += velocidad;
                    // Después de mover a _mainCharacterPosition…
                    if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                    {
                        try
                        {
                            // Trazamos el envío
                            Debug.WriteLine(
                                $"▶️ Enviando posición: {_mainCharacterPosition.X:F1}, {_mainCharacterPosition.Y:F1}"
                            );


                            _hubConnection.SendAsync(
                               "UpdatePosition",
                               _playerId,
                               _mainCharacterPosition.X,
                               _mainCharacterPosition.Y
                           );
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("❌ Error SendAsync: " + ex.Message);
                        }
                    }
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
                    (int)_mainCharacter.Width, (int)_mainCharacter.Height);
                if (!rect.Intersects(_player2CollisionBox))
                {
                    _mainCharacterPosition.Y -= velocidad;
                    // Después de mover a _mainCharacterPosition…
                    if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                    {
                        try
                        {
                            // Trazamos el envío
                            Debug.WriteLine(
                                $"▶️ Enviando posición: {_mainCharacterPosition.X:F1}, {_mainCharacterPosition.Y:F1}"
                            );


                            _hubConnection.SendAsync(
                               "UpdatePosition",
                               _playerId,
                               _mainCharacterPosition.X,
                               _mainCharacterPosition.Y
                           );
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("❌ Error SendAsync: " + ex.Message);
                        }
                    }
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
                    (int)_mainCharacter.Width, (int)_mainCharacter.Height);
                if (!rect.Intersects(_player2CollisionBox))
                {
                    _mainCharacterPosition.Y += velocidad;
                    // Después de mover a _mainCharacterPosition…
                    if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                    {
                        try
                        {
                            // Trazamos el envío
                            Debug.WriteLine(
                                $"▶️ Enviando posición: {_mainCharacterPosition.X:F1}, {_mainCharacterPosition.Y:F1}"
                            );


                            _hubConnection.SendAsync(
                               "UpdatePosition",
                               _playerId,
                               _mainCharacterPosition.X,
                               _mainCharacterPosition.Y
                           );
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("❌ Error SendAsync: " + ex.Message);
                        }
                    }
                }
                else
                {
                    // Play the bounce sound effect.
                    Audio.PlaySoundEffect(_bounceSoundEffect);
                }


               
            }
            if (keyboard.IsKeyDown(Keys.Enter))
            {
                if (!_player2CollisionBox.Intersects(_mainCharacterCollisionBox))
                {
                    _player2Position.X += 2;
                    _player2Position.Y += 2;
                }

            }


            //_mainCharacterPosition.X = MathHelper.Clamp(
            //        _mainCharacterPosition.X,
            //        0,
            //        _worldWidth - _mainCharacter.Width);

            //_mainCharacterPosition.Y = MathHelper.Clamp(
            //    _mainCharacterPosition.Y,
            //    0,
            //    _worldHeight - _mainCharacter.Height);

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
                _mainCharacterPosition.Y -= speed;
            }

            // if the S or Down keys are down, move the slime down on the screen.
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            {
                _mainCharacterPosition.Y += speed;
            }

            // If the A or Left keys are down, move the slime left on the screen.
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            {
                _mainCharacterPosition.X -= speed;
            }

            // If the D or Right keys are down, move the slime right on the screen.
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            {
                _mainCharacterPosition.X += speed;
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
                _mainCharacterPosition.X += gamePadOne.LeftThumbStick.X * speed;
                _mainCharacterPosition.Y -= gamePadOne.LeftThumbStick.Y * speed;
            }
            else
            {
                // If DPadUp is down, move the slime up on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadUp))
                {
                    _mainCharacterPosition.Y -= speed;
                }

                // If DPadDown is down, move the slime down on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadDown))
                {
                    _mainCharacterPosition.Y += speed;
                }

                // If DPapLeft is down, move the slime left on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
                {
                    _mainCharacterPosition.X -= speed;
                }

                // If DPadRight is down, move the slime right on the screen.
                if (gamePadOne.IsButtonDown(Buttons.DPadRight))
                {
                    _mainCharacterPosition.X += speed;
                }
            }
        }

        #endregion

        private void ProcessTouch(GameTime gameTime)
        {
            // Obtén el tiempo delta para movimiento consistente
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Mientras haya gestos disponibles
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                if (gesture.GestureType == GestureType.FreeDrag)
                {
                    // gesture.Delta es un Vector2 con el desplazamiento del dedo en pixeles
                    Vector2 drag = gesture.Delta;

                    // Ajusta la velocidad: tal vez quieras escalar drag 
                    // por MOVEMENT_SPEED y por delta de tiempo
                    Vector2 movimiento = drag * MOVEMENT_SPEED * delta;

                    // Aplica colisión igual que con teclado:
                    var futuraCaja = new Rectangle(
                        (int)(_mainCharacterPosition.X + movimiento.X),
                        (int)(_mainCharacterPosition.Y + movimiento.Y),
                        (int)_mainCharacter.Width,
                        (int)_mainCharacter.Height
                    );

                    if (!futuraCaja.Intersects(_player2CollisionBox))
                    {
                        _mainCharacterPosition += movimiento;

                        // Envía posición por SignalR
                        if (_hubConnection?.State == HubConnectionState.Connected)
                        {
                            _hubConnection.SendAsync(
                                "UpdatePosition",
                                _playerId,
                                _mainCharacterPosition.X,
                                _mainCharacterPosition.Y
                            );
                        }
                    }
                    else
                    {
                        Audio.PlaySoundEffect(_bounceSoundEffect);
                    }
                }
                else if (gesture.GestureType == GestureType.Tap)
                {
                    // Ejemplo: en cada tap centras la cámara ahí, o disparas algo…
                    // Vector2 tapPos = gesture.Position;
                }
            }
        }
    }
}
