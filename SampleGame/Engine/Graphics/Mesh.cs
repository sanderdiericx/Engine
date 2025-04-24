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
        public List<Vector3> _vertices;
        public List<Vector2> _textureCoordinates;
        public List<Vector3> _normals;

        public Mesh(string assetName)
        {
            string[] data = ResourceLoader.Instance.GetAsset(assetName);

            var output = ParseData(data);

            _vertices = output.Item1;
            _textureCoordinates = output.Item2;
            _normals = output.Item3;

        }

        private static (List<Vector3>, List<Vector2>, List<Vector3>) ParseData(string[] data)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            

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
                else if (data[i].StartsWith("vt "))
                {
                    string[] line = data[i].Substring(3).Split(' ');

                    try
                    {
                        textureCoordinates.Add(new Vector2(float.Parse(line[0]), float.Parse(line[1])));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseTextureCoordinates: parsing error at line {i + 1}. Mesh may be incomplete.");
                    }
                }
                else if (data[i].StartsWith("vn "))
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

            return (vertices, textureCoordinates, normals);
        }
    }
}
