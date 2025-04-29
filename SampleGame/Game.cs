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
            ResourceLoader.Instance.LoadFolder("Assets");

            model = new Model("erato.obj", "erato.mtl");
            model.InitializeModel();

            camera = new Camera(new Vector3(0, 0, -3), RenderEngine.Aspect);

            model.RotateModel(0, 10, 10);
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

        }

        void IGame.OnResize(ResizeEventArgs e)
        {

        }
    }
}
