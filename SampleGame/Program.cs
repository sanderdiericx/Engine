using SampleGame.Engine.Core;

namespace SampleGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Engine.Core.Engine.Run<Game>(new WindowSettings(800, 600, "test", false, false));
        }
    }
}
