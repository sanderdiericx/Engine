using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using SampleGame.Engine.Content;
using SampleGame.Engine.Core;

namespace SampleGame
{
    class Game : IGame
    {
        void IGame.OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            ResourceLoader.Instance.LoadFolder("Assets");

            Model model = new Model("erato.obj", "erato.mtl");
        }

        void IGame.OnUnload()
        {

        }

        void IGame.OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        void IGame.OnUpdateFrame(FrameEventArgs args)
        {
            
        }

        void IGame.OnResize(ResizeEventArgs e)
        {
            
        }
    }
}
