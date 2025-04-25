using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SampleGame.Engine.Core;

namespace SampleGame.Engine.Graphics
{
    internal class Mesh
    {
        public int VertexArrayObject;
        public int VertexBufferObject;
        public int ElementBufferObject;

        public Mesh(Window window, List<Vector3> vertices, List<Vector2> textureCoordinates, List<Vector3> normals)
        {
            float[] vertexData = GetInterleavedVertexData(vertices, textureCoordinates, normals);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

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
        }

        private static float[] GetInterleavedVertexData(List<Vector3> vertices, List<Vector2> textureCoordinates, List<Vector3> normals)
        {
            bool HasTexCoords = textureCoordinates.Count > 0 && textureCoordinates != null;
            bool hasNormals = normals.Count > 0 && normals != null;

            // 3 position, 3 normals, 2 texture coordinates
            const int stride = 8;
            float[] vertexData = new float[vertices.Count * stride];

            try
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    int dataIndex = i * stride;

                    // Position
                    vertexData[dataIndex] = vertices[i].X;
                    vertexData[dataIndex + 1] = vertices[i].Y;
                    vertexData[dataIndex + 2] = vertices[i].Z;

                    // Normals
                    if (hasNormals && i < normals.Count)
                    {
                        // Use zero normal if not enough normals provided
                        Vector3 normal = i < normals.Count ? normals[i] : Vector3.Zero;
                        vertexData[dataIndex + 3] = normal.X;
                        vertexData[dataIndex + 4] = normal.Y;
                        vertexData[dataIndex + 5] = normal.Z;
                    }

                    // Texture coordinates
                    if (HasTexCoords && i < textureCoordinates.Count)
                    {
                        // Use zero UVs if not enough texture coordinates provided
                        Vector2 texCoord = i < textureCoordinates.Count ? textureCoordinates[i] : Vector2.Zero;
                        vertexData[dataIndex + 6] = texCoord.X;
                        vertexData[dataIndex + 7] = texCoord.Y;
                    }
                }
            }
            catch
            {
                Console.WriteLine("GetInterleavedVertexData: an error has occured while trying to build vertex buffer.");
            }

            return vertexData;
        }
    }
}
