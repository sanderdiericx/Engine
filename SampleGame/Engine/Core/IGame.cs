using OpenTK.Windowing.Common;

namespace SampleGame.Engine.Core
{
    public interface IGame
    {
        void OnLoad();
        void OnUnload();
        void OnRenderFrame(FrameEventArgs args);
        void OnUpdateFrame(FrameEventArgs args);
        void OnResize(ResizeEventArgs e);
    }
}
