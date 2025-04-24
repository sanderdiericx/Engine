using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Content;

namespace SampleGame.Engine.Graphics
{
    class Mesh
    {
        private List<Vector3> _vertices;
        private List<Vector3> _normals;
        private List<Vector2> _textureCoordinates;

        public Mesh(string assetName)
        {
            string[] data = ResourceLoader.Instance.GetAsset(assetName);

            _vertices = ParseVertices(data);
            _normals = ParseNormals(data);
        }

        private static List<Vector3> ParseTextureCoordinates(string[] data)
        {
                        
        }

        private static List<Vector3> ParseNormals(string[] data)
        {
            List<Vector3> normals = new List<Vector3>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("vn "))
                {
                    string[] line = data[i].Substring(3).Split(' ');

                    try
                    {
                        normals.Add(new Vector3(float.Parse(line[0]), float.Parse(line[1]), float.Parse(line[2])));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseNormals: parsing error at line {i + 1}. Mesh may be incomplete.");
                    }
                }
            }

            return normals;
        }

        private static List<Vector3> ParseVertices(string[] data)
        {
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("v "))
                {
                    string[] line = data[i].Substring(2).Split(' ');

                    try
                    {
                        vertices.Add(new Vector3(float.Parse(line[0]), float.Parse(line[1]), float.Parse(line[2])));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseVertices: parsing error at line {i + 1}. Mesh may be incomplete.");
                    }
                }
            }

            return vertices;
        }
    }
}
