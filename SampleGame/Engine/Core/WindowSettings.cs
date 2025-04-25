namespace SampleGame.Engine.Core
{
    public class WindowSettings
    {
        public int Width;
        public int Height;
        public string Title;
        public bool Vsync;
        public bool Fullscreen;
       
        public WindowSettings(int width, int height, string title, bool vsync, bool fullscreen)
        {
            Width = width;
            Height = height;
            Title = title;
            Vsync = vsync;
            Fullscreen = fullscreen;
        }
    }
}
