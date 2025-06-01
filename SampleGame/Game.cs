using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SampleGame.Engine.Content;
using SampleGame.Engine.Core;
using RenderEngine = SampleGame.Engine.Core.Engine;

namespace SampleGame
{
    class Game : IGame
    {
        Model model;
        Camera camera;
        Skybox skyBox;

        void IGame.OnLoad()
        {
            ResourceLoader.Instance.LoadWavefrontFolder(@"Assets\san_miguel");

            model = new Model("san-miguel.obj", "san-miguel.mtl");
            model.Initialize();

            camera = new Camera(new Vector3(0, 0, -3));
            camera.Fov = 90;

            skyBox = new Skybox();

            model.SetPosition(-10, 0, 0);
            model.Scale(0.5f);

            ResourceLoader.Instance.UnloadWavefrontFolder(@"Assets\san_miguel");
        }

        void IGame.OnUnload()
        {

        }

        void IGame.OnRenderFrame(FrameEventArgs args)
        {
            RenderEngine.RenderSkybox(skyBox, camera);

            RenderEngine.RenderModel(model, camera);
        }

        void IGame.OnUpdateFrame(FrameEventArgs args)
        {
            camera.HandleMovement(args, 3f);
            camera.HandleCamera(0.3f);
        }

        void IGame.OnResize(ResizeEventArgs e)
        {

        }
    }
}
