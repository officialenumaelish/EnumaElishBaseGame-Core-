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

        protected override void Initialize()
        {
            base.Initialize();

            // Start playing the background music.
            Audio.PlaySong(_themeSong);

            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Start the game with the title scene.
            ChangeScene(/*new ArenaScene(viewportWidth, viewportHeight)*/ new GameScene(viewportWidth, viewportHeight));
        }

        protected override void LoadContent() { 
        
            // Load the background theme music.
            _themeSong = Content.Load<Song>("Audio/theme");
        }




    }

}