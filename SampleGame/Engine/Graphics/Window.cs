using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Core;
using static SampleGame.Engine.Core.Engine;

namespace SampleGame.Engine.Graphics
{
    internal class Window : GameWindow
    {
        public Shader ModelShader;
        public Shader SkyboxShader;
        private IGame _game;

        public int SizeX;
        public int SizeY;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, IGame game)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _game = game;
            
            ModelShader = new Shader("Engine/Graphics/Shaders/shader.vert", "Engine/Graphics/Shaders/shader.frag");
            SkyboxShader = new Shader("Engine/Graphics/Shaders/skybox.vert", "Engine/Graphics/Shaders/skybox.frag");

            SizeX = Size.X;
            SizeY = Size.Y;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _game.OnLoad();

            GL.Enable(EnableCap.Multisample);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

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

            WindowVariables.Keyboard = KeyboardState;
            WindowVariables.Mouse = MouseState;

            KeyboardState keyboard = WindowVariables.Keyboard;

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

            // Update size and aspect
            WindowVariables.SizeX = SizeX;
            WindowVariables.SizeY = SizeY;
            WindowVariables.Aspect = WindowVariables.SizeX / (float)WindowVariables.SizeY;

            GL.Viewport(0, 0, Size.X, Size.Y); // Make sure the viewport gets properly changed
        }
    }
}
