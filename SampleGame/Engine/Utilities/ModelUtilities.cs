using OpenTK.Mathematics;
using SampleGame.Engine.Graphics;
using System.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SampleGame.Engine.Utilities
{
    internal class ModelUtilities
    {
        class MeshData
        {
            public List<Vector3> Vertices = new List<Vector3>();
            public List<Vector3> Normals = new List<Vector3>();
            public List<Vector2> TextureCoordinates = new List<Vector2>();
        }

        public struct Corner()
        {
            public int Vertex = -1;
            public int Texture = -1;
            public int Normal = -1;
        }

        // Parses the face data of a .obj file into meshes with their respective materials, expects text, vertices, normals, texture coordinates and a list of all materials
        public static Dictionary<Material, Mesh> GetMeshes(string[] data, List<Vector3> vertices, List<Vector3> normals, List<Vector2> textureCoordinates, List<Material> materials)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Sort face lines by what material they use
            Dictionary<Material, List<string>> linesPerMaterial = new Dictionary<Material, List<string>>();

            Material currentMaterial = null;
            List<string> currentLines = new List<string>();
            bool firstMaterial = true;

            // Convert materials to a dictionary for a faster lookup
            var materialMap = materials.ToDictionary(m => m.MaterialName);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("usemtl "))
                {
                    if (!firstMaterial)
                    {
                        if (!linesPerMaterial.TryAdd(currentMaterial, currentLines))
                        {
                            linesPerMaterial[currentMaterial].AddRange(currentLines);
                        }

                        currentMaterial = null;
                        currentLines = new List<string>();
                    }
                    else
                    {
                        firstMaterial = false;
                    }

                    // Split the line into 2 to find the material name
                    ReadOnlySpan<char> span = data[i].AsSpan();
                    int spaceIndex = span.IndexOf(' ');
                    var materialNameSpan = span.Slice(spaceIndex + 1);

                    materialMap.TryGetValue(materialNameSpan.ToString(), out currentMaterial);

                }
                else if (data[i].StartsWith("f "))
                {
                    currentLines.Add(data[i]);
                }
            }

            // Add the final set
            if (currentMaterial != null && currentLines.Count > 0)
            {
                if (!linesPerMaterial.TryAdd(currentMaterial, currentLines))
                {
                    linesPerMaterial[currentMaterial].AddRange(currentLines);
                }
            }

            // Parse face lines on multiple threads and add them to a mesh data dictionary
            ConcurrentDictionary<Material, Mesh> meshes = new ConcurrentDictionary<Material, Mesh>();

            Parallel.ForEach(linesPerMaterial, kvp =>
            {
                List<Vector3> currentVertices = new List<Vector3>();
                List<Vector2> currentTextureCoordinates = new List<Vector2>();
                List<Vector3> currentNormals = new List<Vector3>();

                foreach (var line in kvp.Value)
                {
                    List<Corner> foundCorners = new List<Corner>();
                    Corner currentCorner = new Corner();

                    ReadOnlySpan<char> span = line.AsSpan().Slice(2); // Slice 2 because we need to skip "f "

                    int firstSlash = span.IndexOf('/');
                    int secondSlash = span.Slice(firstSlash + 1).IndexOf('/') + firstSlash + 1;
                    int firstSpace = span.IndexOf(' ');

                    int thirdSlash = span.Slice(firstSpace + 1).IndexOf('/') + firstSpace + 1;
                    int fourthSlash = span.Slice(thirdSlash + 1).IndexOf('/') + thirdSlash + 1;
                    int secondSpace = span.Slice(firstSpace + 1).IndexOf(' ') + firstSpace + 1;

                    int fifthSlash = span.Slice(secondSpace + 1).IndexOf('/') + secondSpace + 1;
                    int sixthSlash = span.Slice(fifthSlash + 1).IndexOf('/') + fifthSlash + 1;

                    // Vertex 1
                    var vertexIndexSpan1 = span.Slice(0, firstSlash);
                    var textureIndexSpan1 = span.Slice(firstSlash + 1, secondSlash - firstSlash - 1);
                    var normalIndexSpan1 = span.Slice(secondSlash + 1, firstSpace - secondSlash - 1);

                    // Vertex 2
                    var vertexIndexSpan2 = span.Slice(firstSpace + 1, thirdSlash - firstSpace - 1);
                    var textureIndexSpan2 = span.Slice(thirdSlash + 1, fourthSlash - thirdSlash - 1);
                    var normalIndexSpan2 = span.Slice(fourthSlash + 1, secondSpace - fourthSlash - 1);

                    // Vertex 3
                    var vertexIndexSpan3 = span.Slice(secondSpace + 1, fifthSlash - secondSpace - 1);
                    var textureIndexSpan3 = span.Slice(fifthSlash + 1, sixthSlash - fifthSlash - 1);
                    var normalIndexSpan3 = span.Slice(sixthSlash + 1);

                    foundCorners.Add(ConvertToCorner(vertexIndexSpan1, textureIndexSpan1, normalIndexSpan1, vertices, normals, textureCoordinates));
                    foundCorners.Add(ConvertToCorner(vertexIndexSpan2, textureIndexSpan2, normalIndexSpan2, vertices, normals, textureCoordinates));
                    foundCorners.Add(ConvertToCorner(vertexIndexSpan3, textureIndexSpan3, normalIndexSpan3, vertices, normals, textureCoordinates));

                    // Handles triangles, quads and polygons
                    for (int i = 1; i < foundCorners.Count - 1; i++)
                    {
                        currentVertices.Add(vertices[foundCorners[0].Vertex]);
                        currentNormals.Add(normals[foundCorners[0].Normal]);
                        currentTextureCoordinates.Add(textureCoordinates[foundCorners[0].Texture]);

                        currentVertices.Add(vertices[foundCorners[i].Vertex]);
                        currentNormals.Add(normals[foundCorners[i].Normal]);
                        currentTextureCoordinates.Add(textureCoordinates[foundCorners[i].Texture]);

                        currentVertices.Add(vertices[foundCorners[i + 1].Vertex]);
                        currentNormals.Add(normals[foundCorners[i + 1].Normal]);
                        currentTextureCoordinates.Add(textureCoordinates[foundCorners[i + 1].Texture]);
                    }

                }

                meshes.TryAdd(kvp.Key, new Mesh(currentVertices, currentTextureCoordinates, currentNormals));
            });


            stopwatch.Stop();
            Console.WriteLine($"Parsed meshes in: {stopwatch.ElapsedMilliseconds}ms");

            return meshes.ToDictionary();
        }

        private static Corner ConvertToCorner(ReadOnlySpan<char> vertexSpan, ReadOnlySpan<char> textureSpan, ReadOnlySpan<char> normalSpan, List<Vector3> vertices, List<Vector3> normals, List<Vector2> textureCoordinates)
        {
            Corner corner = new Corner();

            int.TryParse(vertexSpan, out corner.Vertex);
            int.TryParse(textureSpan, out corner.Texture);
            int.TryParse(normalSpan, out corner.Normal);

            // Convert negative indices into positive ones
            corner.Vertex = (corner.Vertex > 0) ? corner.Vertex - 1 : vertices.Count + corner.Vertex - 1;
            corner.Normal = (corner.Normal > 0) ? corner.Normal - 1 : normals.Count + corner.Normal - 1;
            corner.Texture = (corner.Texture > 0) ? corner.Texture - 1 : textureCoordinates.Count + corner.Texture - 1;

            return corner;
        }

        public static List<Material> ParseMTL(string[] data)
        {
            List<Material> materials = new List<Material>();

            bool firstMaterial = true;

            // Start with empty material paramaters
            string materialName = string.Empty;
            Vector3 ka = Vector3.Zero;
            Vector3 kd = Vector3.Zero;
            Vector3 ks = Vector3.Zero;
            float ns = 0f;
            float opacity = 0f;
            int illum = 0;

            Texture diffuseMap = null;
            Texture specularMap = null;
            Texture normalMap = null;

            for (int i = 0; i < data.Length; i++)
            {
                string line = data[i].TrimStart();

                if (line.StartsWith("newmtl ")) // Create new material
                {
                    if (!firstMaterial) // Make sure we don't create an empty material on the first line
                    {
                        Material material = new Material(materialName, ka, kd, ks, ns, opacity, illum, diffuseMap, specularMap, normalMap);

                        materials.Add(material);

                        // Reset material variables
                        materialName = string.Empty;
                        ka = Vector3.Zero;
                        kd = Vector3.Zero;
                        ks = Vector3.Zero;
                        ns = 0f;
                        opacity = 0f;
                        illum = 0;

                        diffuseMap = null;
                        specularMap = null;
                        normalMap = null;
                    }
                    else
                    {
                        firstMaterial = false;
                    }

                    materialName = line.Substring(7).Trim();
                }
                else if (line.StartsWith("Ns "))
                {
                    if (!float.TryParse(line.Substring("Ns ".Length), NumberStyles.Float, CultureInfo.InvariantCulture, out ns))
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete. Line: ({line})");
                        ns = 0;
                    }
                }
                else if (line.StartsWith("Ka "))
                {
                    ka = ParseVector3(line, "Ka ");
                }
                else if (line.StartsWith("Kd "))
                {
                    kd = ParseVector3(line, "Kd ");
                }
                else if (line.StartsWith("Ks "))
                {
                    ks = ParseVector3(line, "Ks ");
                }
                else if (line.StartsWith("d "))
                {
                    if (!float.TryParse(line.Substring("d ".Length), NumberStyles.Float, CultureInfo.InvariantCulture, out opacity))
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                        opacity = 0;
                    }
                }
                else if (line.StartsWith("Tr"))
                {
                    if (!float.TryParse(line.Substring("Tr ".Length), NumberStyles.Float, CultureInfo.InvariantCulture, out opacity))
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                        opacity = 0;
                    }
                    else
                    {
                        // Transparency is the opposite of dissolve
                        opacity = 1.0f - opacity;
                    }
                }
                else if (line.StartsWith("illum "))
                {
                    if (!int.TryParse(line.Substring("illum ".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out illum))
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                        illum = 0;
                    }
                }
                else if (line.StartsWith("map_Kd "))
                {
                    diffuseMap = ParseTexture(line, "map_Kd");
                }
                else if (line.StartsWith("map_Ks "))
                {
                    specularMap = ParseTexture(line, "map_Ks");
                }
                else if (line.StartsWith("map_Kn "))
                {
                    normalMap = ParseTexture(line, "map_Kn");
                }
            }

            // Add the final material manually
            Material lastMaterial = new Material(materialName, ka, kd, ks, ns, opacity, illum, diffuseMap, specularMap, normalMap);
            materials.Add(lastMaterial);

            return materials;
        }

        public static (List<Vector3>, List<Vector2>, List<Vector3>) ParseOBJ(string[] data)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            for (int i = 0; i < data.Length; i++)
            {
                string line = data[i];

                if (line.StartsWith("v "))
                {
                    vertices.Add(ParseVector3(line, "v "));
                }
                else if (line.StartsWith("vt "))
                {
                    textureCoordinates.Add(ParseVector2(line, "vt "));
                }
                else if (line.StartsWith("vn "))
                {
                    normals.Add(ParseVector3(line, "vn "));
                }
            }

            return (vertices, textureCoordinates, normals);
        }

        private static Texture ParseTexture(string line, string name)
        {
            string textureName = Path.GetFileName(line.Substring(name.Length).Trim());

            string texturePath = Texture.FindTextureFilePath(textureName);

            if (texturePath != "")
            {
                return Texture.LoadFromFile(texturePath);
            }
            else
            {
                Console.WriteLine($"ParseTexture: the texture {textureName} was not found in the Assets folder.");
                return null;
            }
        }

        // Parses a line into a vector 3, expects 3 float values
        private static Vector3 ParseVector3(string line, string name)
        {
            string[] data = line.Substring(name.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            float f1, f2, f3;

            if (!float.TryParse(data[0], NumberStyles.Float, CultureInfo.InvariantCulture, out f1) || !float.TryParse(data[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f2) || !float.TryParse(data[2], NumberStyles.Float, CultureInfo.InvariantCulture, out f3))
            {
                Console.WriteLine($"ParseVector3: parsing error. Line: {line}");

                return Vector3.Zero;
            }

            return new Vector3(f1, f2, f3);
        }

        // Parses a line into a vector 2, expects 2 float values
        private static Vector2 ParseVector2(string line, string name)
        {
            string[] data = line.Substring(name.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            float f1, f2;

            if (!float.TryParse(data[0], NumberStyles.Float, CultureInfo.InvariantCulture, out f1) || !float.TryParse(data[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f2))
            {
                Console.WriteLine($"ParseVector2: parsing error. Line: {line}");

                return Vector2.Zero;
            }

            return new Vector2(f1, f2);
        }

        // Parses a string into a float. expects the line, the string and a reference to what it parses to
        private static void ParseFloat(string line, string data, ref float output)
        {
            if (!float.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out output))
            {
                Console.WriteLine($"ParseFloat: parsing error. Line: ({line})");
                output = 0;
            }
        }

        // Parses a string into a int. expects the line, the string and a reference to what it parses to
        private static void ParseInt(string line, string data, ref int output)
        {
            if (!int.TryParse(data, NumberStyles.Integer, CultureInfo.InvariantCulture, out output))
            {
                Console.WriteLine($"ParseInt: parsing error. Line: ({line})");
                output = 0;
            }
        }
    }
}