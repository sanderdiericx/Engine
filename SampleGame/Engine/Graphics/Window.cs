using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SampleGame.Engine.Core;

namespace SampleGame.Engine.Graphics
{
    internal class Window : GameWindow
    {
        public Shader Shader;
        private IGame _game;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, IGame game)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _game = game;
            Shader = new Shader("Engine/Graphics/Shaders/shader.vert", "Engine/Graphics/Shaders/shader.frag");
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _game.OnLoad();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _game.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            _game.OnRenderFrame(args);

            // Swap between buffers
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            _game.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            _game.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y); // Make sure the viewport gets properly changed
        }
    }
}
