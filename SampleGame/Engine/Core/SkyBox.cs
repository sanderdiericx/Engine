using OpenTK.Graphics.OpenGL4;
using SampleGame.Engine.Content;
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

        public Skybox(string skyBoxName)
        {
            vertices = GraphicsUtilities.GetCubeVertices();

            ImageResult[] images = ResourceLoader.Instance.GetSkybox(skyBoxName);

            // Generate texture
            cubeMapTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMapTexture);

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
