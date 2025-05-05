using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SampleGame.Engine.Graphics
{
    internal class Mesh
    {
        public int VertexArrayObject;
        public int VertexBufferObject;
        public int ElementBufferObject;

        public float[] UniqueVertexBuffer;
        public uint[] Indices;

        public Mesh(List<Vector3> vertices, List<Vector2> textureCoordinates, List<Vector3> normals)
        {
            float[] vertexBuffer = GetInterleavedVertexData(vertices, textureCoordinates, normals);

            (float[], uint[]) uniqueData = GetIndices(vertexBuffer);

            UniqueVertexBuffer = uniqueData.Item1;
            Indices = uniqueData.Item2;
        }

        public struct ComboKey
        {
            public float x;
            public float y;
            public float z;
            public float nx;
            public float ny;
            public float nz;
            public float u;
            public float v;
        }

        // Returns a list of indices and a unique vertex buffer to draw by eliminating duplicates, expects a float array with vertex positions, normals and texture coordinates
        private static (float[], uint[]) GetIndices(float[] vertexData)
        {

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();

            List<uint> indices = new List<uint>();
            Dictionary<ComboKey, uint> foundCombos = new Dictionary<ComboKey, uint>();

            uint indexCount = 0;

            for (int i = 0; i < vertexData.Length; i += 8) // Stride of 8
            {
                ComboKey comboKey = new ComboKey();
                comboKey.x = vertexData[i];
                comboKey.y = vertexData[i + 1];
                comboKey.z = vertexData[i + 2];
                comboKey.nx = vertexData[i + 3];
                comboKey.ny = vertexData[i + 4];
                comboKey.nz = vertexData[i + 5];
                comboKey.u = vertexData[i + 6];
                comboKey.v = vertexData[i + 7];

                // Check if the vertex already exists in the dictionary
                if (!foundCombos.TryGetValue(comboKey, out uint existingIndex))
                {
                    foundCombos.Add(comboKey, indexCount);

                    indices.Add(indexCount);
                    indexCount++;

                    vertices.Add(new Vector3(comboKey.x, comboKey.y, comboKey.z));
                    normals.Add(new Vector3(comboKey.nx, comboKey.ny, comboKey.nz));
                    textureCoordinates.Add(new Vector2(comboKey.u, comboKey.v));
                }
                else
                {
                    indices.Add(existingIndex);
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
