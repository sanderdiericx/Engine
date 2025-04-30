using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Content;
using SampleGame.Engine.Core;
using RenderEngine = SampleGame.Engine.Core.Engine;

namespace SampleGame
{
    class Game : IGame
    {
        Model model;
        Camera camera;

        bool firstMove = true;
        Vector2 lastPos;

        void IGame.OnLoad()
        {
            ResourceLoader.Instance.LoadFolder("Assets");

            model = new Model("erato.obj", "erato.mtl");
            model.InitializeModel();

            camera = new Camera(new Vector3(0, 0, -3), RenderEngine.Aspect);

            model.SetModelPosition(0, 0, 0);
        }

        void IGame.OnUnload()
        {

        }

        void IGame.OnRenderFrame(FrameEventArgs args)
        {
            RenderEngine.RenderModel(model, camera);
        }

        void IGame.OnUpdateFrame(FrameEventArgs args)
        {
            var input = RenderEngine.Keyboard;

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                camera.Position += camera.Front * cameraSpeed * (float)args.Time;
            }

            if (input.IsKeyDown(Keys.S))
            {
                camera.Position -= camera.Front * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.A))
            {
                camera.Position -= camera.Right * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.D))
            {
                camera.Position += camera.Right * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.Space))
            {
                camera.Position += camera.Up * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                camera.Position -= camera.Up * cameraSpeed * (float)args.Time;
            }

            var mouse = RenderEngine.Mouse;

            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }


        }

        void IGame.OnResize(ResizeEventArgs e)
        {

        }
    }
}
