using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SampleGame.Engine.Utilities;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Graphics;
using System.Security.Cryptography.X509Certificates;
using Window = SampleGame.Engine.Graphics.Window;

namespace SampleGame.Engine.Core
{
    public class Engine
    {
        private static Window Window;
        public static Engine Instance { get; private set; }

        public static int SizeX; 
        public static int SizeY;
        public static float Aspect;

        private Engine()
        {
            Instance = this;
        }

        // Runs the engine and creates an interface to interact with
        public static void Run<T>(WindowSettings windowSettings) where T : IGame, new()
        {
            IGame game = new T();

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(windowSettings.Width, windowSettings.Height),
                Title = windowSettings.Title,
                WindowState = windowSettings.Fullscreen ? WindowState.Fullscreen : WindowState.Normal,
                Vsync = windowSettings.Vsync ? VSyncMode.On : VSyncMode.Off
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings, game))
            {
                Window = window;

                // Update size and aspect
                SizeX = Window.SizeX;
                SizeY = Window.SizeY;
                Aspect = SizeX / (float)SizeY;

                if (window == null)
                {
                    Console.WriteLine("Engine window is not initialized yet.");
                }
                else
                {
                    Window.Run();
                }
            }
        }

        internal static (int, int, int) InitializeMesh(float[] verticesData, uint[] indices)
        {
            var output = GraphicsUtilities.UploadMesh(Window, verticesData, indices);

            return (output.Item1, output.Item2, output.Item3);
        }

        // Renders the model to the screen
        public static void RenderModel(Model model, Camera camera)
        {
            if (model.isInitialized)
            {
                foreach (var (material, mesh) in model.meshes)
                {
                    GL.BindVertexArray(mesh.VertexArrayObject);

                    material.DiffuseMap.Use(TextureUnit.Texture0);

                    Window.Shader.Use();

                    // Set uniforms
                    var world = model.transform * model.rotation * model.scale;

                    Window.Shader.SetMatrix4("model", world);
                    Window.Shader.SetMatrix4("view", camera.GetViewMatrix());
                    Window.Shader.SetMatrix4("projection", camera.GetProjectionMatrix());

                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
            else
            {
                Console.WriteLine("RenderModel: Model is not initialized.");
            }
        }
    }
}
