using OpenTK.Graphics.OpenGL4;
using SampleGame.Engine.Core;
using Window = SampleGame.Engine.Graphics.Window;
namespace SampleGame.Engine.Utilities
{
    internal class GraphicsUtilities
    {
        // Uploads mesh data to the GPU, expects a float array of unique vertices and a uint array of indices to draw
        public static (int, int, int) UploadMesh(Window window, float[] verticesData, uint[] indices)
        {
            int vertexArrayObject, vertexBufferObject, elementBufferObject;

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, verticesData.Length * sizeof(float), verticesData, BufferUsageHint.StaticDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            int stride = 8 * sizeof(float);

            var vertexLocation = window.Shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, stride, 0);

            var normalLocation = window.Shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            var texCoordLocation = window.Shader.GetAttribLocation("aTexcoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            GL.BindVertexArray(0);

            return (vertexArrayObject, vertexBufferObject, elementBufferObject);
        }
    }
}
