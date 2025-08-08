using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using EnumaElishMonoGameMultiplayer.Content.Scenes;
using EnumaElishMonoGameMultiplayer.Core.Content.Scenes;
using EnumaElishMonoGameMultiplayer.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace EnumaElishMonoGameMultiplayer.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    public class EnumaElishMonoGameMultiplayerGame : MonoGameLibrary.Core
    {
        private Song _themeSong;

        public EnumaElishMonoGameMultiplayerGame() : base("Dungeon Slime", 1920, 1080, false)
        {
            //this.IsMouseVisible = false;
            //this.Window.AllowUserResizing = true;
            Graphics.IsFullScreen = true;
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Tap;

            Graphics.ApplyChanges();
        }

        private void InitializeGum()
        {
            // Initialize the Gum service
            GumService.Default.Initialize(this);

            // Tell the Gum service which content manager to use.  We will tell it to
            // use the global content manager from our Core.
            GumService.Default.ContentLoader.XnaContentManager = MonoGameLibrary.Core.Content;

            // Register keyboard input for UI control.
            FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

            // Register gamepad input for Ui control.
            FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);

            // Customize the tab reverse UI navigation to also trigger when the keyboard
            // Up arrow key is pushed.
            FrameworkElement.TabReverseKeyCombos.Add(
               new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

            // Customize the tab UI navigation to also trigger when the keyboard
            // Down arrow key is pushed.
            FrameworkElement.TabKeyCombos.Add(
               new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });

            // The assets created for the UI were done so at 1/4th the size to keep the size of the
            // texture atlas small.  So we will set the default canvas size to be 1/4th the size of
            // the game's resolution then tell gum to zoom in by a factor of 4.
            GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth / 4.0f;
            GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight / 4.0f;
            GumService.Default.Renderer.Camera.Zoom = 4.0f;
        }

        protected override void Initialize()
        {
            base.Initialize();
            InitializeGum();
            // Start playing the background music.
            //Audio.PlaySong(_themeSong);

            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Start the game with the title scene.
            ChangeScene(/*new ArenaScene(viewportWidth, viewportHeight)*/ new EscenaPrueba(/*viewportWidth, viewportHeight*/));
        }

        protected override void LoadContent() { 
        
            // Load the background theme music.
            _themeSong = Content.Load<Song>("Audio/theme");
        }




    }

}