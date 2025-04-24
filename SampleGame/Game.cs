using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Content;
using SampleGame.Engine.Graphics;


namespace SampleGame
{
    public class Game
    {
        private readonly Engine.Core.Window _window;

        public Game(Engine.Core.Window window)
        {
            _window = window;
        }

        // Game loading logic goes here
        public void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            ResourceLoader.Instance.LoadAsset(@"C:\Users\sande\source\repos\SampleGame\SampleGame\Assets\erato.obj");

            Mesh model = new Mesh("erato.obj");


        }

        // Game unloading logic goes here
        public void OnUnload()
        {

        }

        // Render logic goes here
        public void OnRenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        // Update logic goes here
        public void OnUpdateFrame()
        {
            var input = _window.KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                _window.Close();
            }
        }
    }
}
