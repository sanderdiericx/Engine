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

        void IGame.OnLoad()
        {
            ResourceLoader.Instance.LoadFolder(@"Assets\plane");

            model = new Model("piper_pa18.obj", "piper_pa18.mtl");
            model.InitializeModel();

            camera = new Camera(new Vector3(0, 0, -3), RenderEngine.WindowVariables.Aspect);

            model.SetModelPosition(-10, 0, 0);
            model.ScaleModel(0.5f);
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
            camera.HandleMovement(args, 3f);
            camera.HandleCamera(0.8f);
        }

        void IGame.OnResize(ResizeEventArgs e)
        {

        }
    }
}
