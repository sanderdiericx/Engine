using OpenTK.Mathematics;
using SampleGame.Engine.Graphics;
using System.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System;
using System.Buffers;
using OpenTK.Graphics.ES20;

namespace SampleGame.Engine.Utilities
{
    internal class ModelUtilities
    {
        public struct Corner()
        {
            public int Vertex = -1;
            public int Texture = -1;
            public int Normal = -1;
        }

        // Parses the face data of a .obj file into meshes with their respective materials, expects text, vertices, normals, texture coordinates and a list of all materials
        public static Dictionary<Material, Mesh> GetMeshes(string[] data, Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, List<Material> materials)
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

                    if (!materialMap.TryGetValue(materialNameSpan.ToString(), out currentMaterial))
                    {
                        Console.WriteLine($"GetMeshes: Requested material could not be found. Material: {materialNameSpan}");
                    }
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
                int triangleSum = 0;

                // Prefetch array size
                foreach(var line in kvp.Value)
                {
                    if (line.StartsWith("f "))
                    {
                        // Count how many corners are on this line
                        ReadOnlySpan<char> span = line.AsSpan().Slice(2);

                        int currentCorners = 0;

                        while (true)
                        {
                            int spaceIndex = span.IndexOf(' ');
                            currentCorners++;

                            if (spaceIndex == -1)
                            {
                                break;
                            }

                            span = span.Slice(spaceIndex + 1);
                        }

                        triangleSum += 3 * (currentCorners - 2);
                    }
                }

                Vector3[] currentVertices = new Vector3[triangleSum + 1];
                Vector2[] currentTexCoords = new Vector2[triangleSum + 1];
                Vector3[] currentNormals = new Vector3[triangleSum + 1];
                
                int vertexCount = 0;
                int textureCount = 0;
                int normalCount = 0;

                Span<Corner> foundCorners = stackalloc Corner[3];
                int lastSize = 3;

                foreach (var line in kvp.Value)
                {
                    int slashCount = CountSpanChar(line, '/');
                    int cornerCount = slashCount / 2;

                    // Make sure we're only allocating a different size when needed
                    if (cornerCount > lastSize)
                    {
                        foundCorners = stackalloc Corner[cornerCount];
                        lastSize = cornerCount;
                    }

                    // Clear foundCorners buffer
                    for (int i = 0; i < cornerCount; i++)
                    {
                        foundCorners[i] = new Corner { Vertex = -1, Texture = -1, Normal = -1 };
                    }

                    ReadOnlySpan<char> span = line.AsSpan().Slice(2); // Slice 2 because we need to skip "f "

                    for (int i = 0; i < cornerCount; i++)
                    {
                        int firstSpace = span.IndexOf(' ');

                        int firstSlash = span.IndexOf('/');
                        int secondSlash = span.Slice(firstSlash + 1).IndexOf('/') + firstSlash + 1;

                        var vertexIndexSpan = span.Slice(0, firstSlash);
                        var textureIndexSpan = span.Slice(firstSlash + 1, secondSlash - firstSlash - 1);
                        var normalIndexSpan = span.Slice(secondSlash + 1);

                        span = span.Slice(firstSpace + 1);

                        foundCorners[i] = ConvertToCorner(vertexIndexSpan, textureIndexSpan, normalIndexSpan, vertices, normals, texCoords);
                    }

                    // Handles triangles, quads and polygons
                    for (int i = 1; i < cornerCount - 1; i++)
                    {
                        currentVertices[vertexCount] = (vertices[foundCorners[0].Vertex]);
                        currentNormals[normalCount] = (normals[foundCorners[0].Normal]);
                        currentTexCoords[textureCount] = (texCoords[foundCorners[0].Texture]);

                        vertexCount++;
                        normalCount++;
                        textureCount++;

                        currentVertices[vertexCount] = (vertices[foundCorners[i].Vertex]);
                        currentNormals[normalCount] = (normals[foundCorners[i].Normal]);
                        currentTexCoords[textureCount] = (texCoords[foundCorners[i].Texture]);

                        vertexCount++;
                        normalCount++;
                        textureCount++;

                        currentVertices[vertexCount] = (vertices[foundCorners[i + 1].Vertex]);
                        currentNormals[normalCount] = (normals[foundCorners[i + 1].Normal]);
                        currentTexCoords[textureCount] = (texCoords[foundCorners[i + 1].Texture]);

                        vertexCount++;
                        normalCount++;
                        textureCount++;
                    }
                }

                meshes.TryAdd(kvp.Key, new Mesh(currentVertices, currentTexCoords, currentNormals));
            });

            stopwatch.Stop();
            Console.WriteLine($"Parsed {meshes.Count} meshes in: {stopwatch.ElapsedMilliseconds}ms");

            return meshes.ToDictionary();
        }

        private static int CountSpanChar(ReadOnlySpan<char> span, char search)
        {
            int count = 0;
            foreach (char c in span)
            {
                if (c == search)
                {
                    count++;
                }
            }

            return count;
        }

        private static Corner ConvertToCorner(ReadOnlySpan<char> vertexSpan, ReadOnlySpan<char> textureSpan, ReadOnlySpan<char> normalSpan, Vector3[] vertices, Vector3[] normals, Vector2[] texCoords)
        {
            Corner corner = new Corner();

            int.TryParse(vertexSpan, out corner.Vertex);
            int.TryParse(textureSpan, out corner.Texture);
            int.TryParse(normalSpan, out corner.Normal);

            // Convert negative indices into positive ones
            corner.Vertex = (corner.Vertex > 0) ? corner.Vertex - 1 : vertices.Length + corner.Vertex - 1;
            corner.Normal = (corner.Normal > 0) ? corner.Normal - 1 : normals.Length + corner.Normal - 1;
            corner.Texture = (corner.Texture > 0) ? corner.Texture - 1 : texCoords.Length + corner.Texture - 1;

            return corner;
        }

        public static List<Material> ParseMTL(string[] data)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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
                    ka = SpanToVector3(line.AsSpan().Slice("Ka ".Length));
                }
                else if (line.StartsWith("Kd "))
                {
                    kd = SpanToVector3(line.AsSpan().Slice("Kd ".Length));
                }
                else if (line.StartsWith("Ks "))
                {
                    ks = SpanToVector3(line.AsSpan().Slice("Ks ".Length));
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

            stopwatch.Stop();
            Console.WriteLine($"Parsed {materials.Count} materials in: {stopwatch.ElapsedMilliseconds}ms");

            return materials;
        }

        public static (Vector3[], Vector2[], Vector3[]) ParseOBJ(string[] data)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int vertexCount = 0;
            int textureCount = 0;
            int normalCount = 0;

            // Prefetch array sizes
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("v "))
                {
                    vertexCount++;
                }
                else if (data[i].StartsWith("vt "))
                {
                    textureCount++;
                }
                else if (data[i].StartsWith("vn "))
                {
                    normalCount++;
                }
            }

            Vector3[] vertices = new Vector3[vertexCount + 1];
            Vector2[] texCoords = new Vector2[textureCount + 1];
            Vector3[] normals = new Vector3[normalCount + 1];

            // Reset counters for reuse
            vertexCount = 0;
            textureCount = 0;
            normalCount = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("v "))
                {
                    ReadOnlySpan<char> span = data[i].AsSpan().Slice(2);

                    vertices[vertexCount] = SpanToVector3(span);

                    vertexCount++;
                }
                else if (data[i].StartsWith("vt "))
                {
                    ReadOnlySpan<char> span = data[i].AsSpan().Slice(3);

                    texCoords[textureCount] = SpanToVector2(span);

                    textureCount++;
                }
                else if (data[i].StartsWith("vn "))
                {
                    ReadOnlySpan<char> span = data[i].AsSpan().Slice(3);

                    normals[normalCount] = SpanToVector3(span);

                    normalCount++;
                }
            }


            stopwatch.Stop();
            Console.WriteLine($"Parsed vertex data in {stopwatch.ElapsedMilliseconds}ms");

            return (vertices, texCoords, normals);
        }

        private static Vector2 SpanToVector2(ReadOnlySpan<char> span)
        {
            span = span.Trim();

            ReadOnlySpan<char> floatSpan1;
            ReadOnlySpan<char> floatSpan2;

            int firstSpace = span.IndexOf(' ');

            if (CountSpanChar(span, ' ') == 1)
            {
                floatSpan1 = span.Slice(0, firstSpace);
                floatSpan2 = span.Slice(firstSpace);
            }
            else
            {
                int secondSpace = span.Slice(firstSpace + 1).IndexOf(' ') + firstSpace + 1;

                floatSpan1 = span.Slice(0, firstSpace);
                floatSpan2 = span.Slice(firstSpace + 1, secondSpace - firstSpace - 1);
            }
            
            return new Vector2(
                float.Parse(floatSpan1, CultureInfo.InvariantCulture),
                float.Parse(floatSpan2, CultureInfo.InvariantCulture));
        }


        private static Vector3 SpanToVector3(ReadOnlySpan<char> span)
        {
            span = span.Trim();

            ReadOnlySpan<char> floatSpan1;
            ReadOnlySpan<char> floatSpan2;
            ReadOnlySpan<char> floatSpan3;

            int firstSpace = span.IndexOf(' ');
            int secondSpace = span.Slice(firstSpace + 1).IndexOf(' ') + firstSpace + 1;

            if (CountSpanChar(span, ' ') == 2)
            {
                floatSpan1 = span.Slice(0, firstSpace);
                floatSpan2 = span.Slice(firstSpace + 1, secondSpace - firstSpace - 1);
                floatSpan3 = span.Slice(secondSpace);
            }
            else
            {
                int thirdSpace = span.Slice(secondSpace + 1).IndexOf(' ') + secondSpace + 1;

                floatSpan1 = span.Slice(0, firstSpace);
                floatSpan2 = span.Slice(firstSpace + 1, secondSpace - firstSpace - 1);
                floatSpan3 = span.Slice(secondSpace + 1, thirdSpace - secondSpace - 1);
            }

            return new Vector3(
                float.Parse(floatSpan1, CultureInfo.InvariantCulture),
                float.Parse(floatSpan2, CultureInfo.InvariantCulture),
                float.Parse(floatSpan3, CultureInfo.InvariantCulture));
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
    }
}