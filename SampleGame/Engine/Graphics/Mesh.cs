using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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

            (float[], uint[]) uniqueData = GetIndices(vertexData);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, uniqueData.Item1.Length * sizeof(float), uniqueData.Item1, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, uniqueData.Item2.Length * sizeof(uint), uniqueData.Item2, BufferUsageHint.StaticDraw);

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

        // TO DO: Optimize dictionary lookup by switching key and val order
        // Returns a list of indices and a unique vertex buffer to draw by eliminating duplicates, expects a float array with vertex positions, normals and texture coordinates
        private static (float[], uint[]) GetIndices(float[] vertexData)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();

            List<uint> indices = new List<uint>();
            Dictionary<uint, (Vector3, Vector3, Vector2)> foundCombos = new Dictionary<uint, (Vector3, Vector3, Vector2)>();

            uint indexCount = 0;

            for (int i = 0; i < vertexData.Length; i += 8) // Stride of 8
            {
                Vector3 vertex = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                Vector3 normal = new Vector3(vertexData[i + 3], vertexData[i + 4], vertexData[i + 5]);
                Vector2 textureCoordinate = new Vector2(vertexData[i + 6], vertexData[i + 7]);

                (Vector3, Vector3, Vector2) combo = (vertex, normal, textureCoordinate);

                if (!foundCombos.ContainsValue(combo))
                {
                    foundCombos.Add(indexCount, combo);

                    indices.Add(indexCount);
                    indexCount++;

                    vertices.Add(vertex);
                    normals.Add(normal);
                    textureCoordinates.Add(textureCoordinate);
                }
                else
                {
                    // Find the indexCount associated with this combo
                    uint index = 0;
                    foreach (var (key, value) in foundCombos)
                    {
                        if (value == combo)
                        {
                            index = key;
                        }
                    }

                    indices.Add(index);
                }
            }

            return (GetInterleavedVertexData(vertices, textureCoordinates, normals), indices.ToArray());
        }

        // Converts 3 lists of vertex data into a float array
        private static float[] GetInterleavedVertexData(List<Vector3> vertices, List<Vector2> textureCoordinates, List<Vector3> normals)
        {
            bool HasTexCoords = textureCoordinates.Count > 0;
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
