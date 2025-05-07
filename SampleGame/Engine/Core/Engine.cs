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

        public static WindowVariable WindowVariables;

        private Engine()
        {
            Instance = this;
        }

        public struct WindowVariable
        {
            public KeyboardState Keyboard;
            public MouseState Mouse;
            public float Aspect;
            public int SizeX;
            public int SizeY;
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
                WindowVariables.SizeX = Window.SizeX;
                WindowVariables.SizeY = Window.SizeY;
                WindowVariables.Aspect = WindowVariables.SizeX / (float)WindowVariables.SizeY;

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
                    Window.Shader.Use();

                    Window.Shader.SetInt("texture0", 0);

                    if (material.DiffuseMap != null)
                    {
                        Window.Shader.SetBool("useTexture", true);

                        material.DiffuseMap.Use(TextureUnit.Texture0);
                    }
                    else 
                    {
                        Window.Shader.SetBool("useTexture", false);

                        Window.Shader.SetVector3("materialColor", material.Kd);
                    }

                    // Set uniforms
                    var world = model.transform * model.rotation * model.scale;
                    Window.Shader.SetMatrix4("model", world);
                    Window.Shader.SetMatrix4("view", camera.GetViewMatrix());
                    Window.Shader.SetMatrix4("projection", camera.GetProjectionMatrix());

                    GL.BindVertexArray(mesh.VertexArrayObject);
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
