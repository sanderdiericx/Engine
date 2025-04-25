using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SampleGame.Engine.Graphics;

namespace SampleGame.Engine.Core
{
    public class Engine
    {
        public Engine()
        {
            
        }

        public static void Run<T>(WindowSettings windowSettings) where T : IGame, new()
        {
            IGame game = new T();

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(windowSettings.Width, windowSettings.Height),
                Title = windowSettings.Title,
                WindowState = windowSettings.Fullscreen ? WindowState.Fullscreen : WindowState.Normal,
                Vsync = windowSettings.Vsync ? VSyncMode.On : VSyncMode.Off
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings, game))
            {
                window.Run();
            }
        }
    }
}
