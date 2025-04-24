using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Graphics;

namespace SampleGame.Engine.Core
{
    public class Window : GameWindow
    {
        private readonly Game _game;
        public Shader Shader;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _game = new Game(this);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _game.OnLoad();

            Shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _game.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            _game.OnRenderFrame();

            // swap between buffers
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            _game.OnUpdateFrame();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y); // Make sure the viewport gets properly changed
        }
    }
}
