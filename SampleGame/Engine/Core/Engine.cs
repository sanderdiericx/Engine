using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Utilities;
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
                    Window.ModelShader.Use();

                    Window.ModelShader.SetInt("texture0", 0);

                    if (material.DiffuseMap != null)
                    {
                        Window.ModelShader.SetBool("useTexture", true);

                        material.DiffuseMap.Use(TextureUnit.Texture0);
                    }
                    else
                    {
                        Window.ModelShader.SetBool("useTexture", false);

                        Window.ModelShader.SetVector3("materialColor", material.Kd);
                    }

                    // Set uniforms
                    var world = model.transform * model.rotation * model.scale;
                    Window.ModelShader.SetMatrix4("model", world);
                    Window.ModelShader.SetMatrix4("view", camera.GetViewMatrix());
                    Window.ModelShader.SetMatrix4("projection", camera.GetProjectionMatrix());

                    GL.BindVertexArray(mesh.VertexArrayObject);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
            else
            {
                Console.WriteLine("RenderModel: Model is not initialized.");
            }
        }

        public static void RenderSkybox(Skybox skybox, Camera camera)
        {
            Matrix4 viewMatrix = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            // Remove translation
            viewMatrix.Row3 = new Vector4(0, 0, 0, viewMatrix.Row3.W);

            GL.DepthFunc(DepthFunction.Lequal);

            // Disable depth writing
            GL.DepthMask(false);

            Window.SkyboxShader.Use();
            Window.SkyboxShader.SetMatrix4("view", viewMatrix);
            Window.SkyboxShader.SetMatrix4("projection", projection);
            Window.SkyboxShader.SetInt("skybox", 1);

            // Draw skybox
            GL.BindVertexArray(skybox.vertexArrayObject);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, skybox.cubeMapTexture);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // Re enable depth writing
            GL.DepthMask(true);

            GL.DepthFunc(DepthFunction.Less);
        }
    }
}
