using OpenTK.Graphics.OpenGL4;
using SampleGame.Engine.Utilities;
using StbImageSharp;

namespace SampleGame.Engine.Core
{
    public class Skybox
    {
        internal float[] vertices;
        internal int cubeMapTexture;
        internal int vertexArrayObject;
        internal int vertexBufferObject;

        public Skybox()
        {
            vertices = GraphicsUtilities.GetCubeVertices();

            // Define face textures
            string[] faces = {
                "Assets/SkyBox/px.png", "Assets/SkyBox/nx.png",
                "Assets/SkyBox/py.png", "Assets/SkyBox/ny.png",
                "Assets/SkyBox/pz.png", "Assets/SkyBox/nz.png"
            };

           
            // Generate texture
            cubeMapTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapTexture);

            ImageResult[] images = new ImageResult[6];

            // Read image files
            for (int i = 0; i < faces.Length; i++)
            {
                if (File.Exists(faces[i]))
                {
                    using (Stream stream = File.OpenRead(faces[i]))
                    {
                        images[i] = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    }
                }
                else
                {
                    Console.WriteLine($"Skybox: Face texture file could not be located. Skybox may be incomplete. ({faces[i]})");
                }
            }

            // Pass the imagedata to opengl
            for (int i = 0; i < images.Length; i++)
            {
                TextureTarget faceTarget = TextureTarget.TextureCubeMapPositiveX + i;

                GL.TexImage2D(faceTarget, 0, PixelInternalFormat.Rgba, images[i].Width, images[i].Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, images[i].Data);
            }


            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

            // Assign VAO and VBO
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int stride = 3 * sizeof(float);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        }
    }
}
