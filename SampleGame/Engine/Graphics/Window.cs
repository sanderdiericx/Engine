using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Core;
using RenderEngine = SampleGame.Engine.Core.Engine;

namespace SampleGame.Engine.Graphics
{
    internal class Window : GameWindow
    {
        public Shader Shader;
        private IGame _game;

        public int SizeX;
        public int SizeY;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, IGame game)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _game = game;
            
            Shader = new Shader("Engine/Graphics/Shaders/shader.vert", "Engine/Graphics/Shaders/shader.frag");
            Shader.Use();
            Shader.SetInt("texture0", 0);

            SizeX = Size.X;
            SizeY = Size.Y;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _game.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _game.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _game.OnRenderFrame(args);

            // Swap between buffers
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            RenderEngine.WindowVariables.Keyboard = KeyboardState;
            RenderEngine.WindowVariables.Mouse = MouseState;

            KeyboardState keyboard = RenderEngine.WindowVariables.Keyboard;

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            _game.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            _game.OnResize(e);

            SizeX = Size.X;
            SizeY = Size.Y;

            GL.Viewport(0, 0, Size.X, Size.Y); // Make sure the viewport gets properly changed
        }
    }
}
