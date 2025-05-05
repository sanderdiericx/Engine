using OpenTK.Mathematics;
using SampleGame.Engine.Graphics;
using System.Globalization;

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

        public static Dictionary<Material, Mesh> GetMeshes(string[] data, List<Vector3> vertices, List<Vector3> normals, List<Vector2> textureCoordinates, List<Material> materials)
        {
            Dictionary<Material, MeshData> meshDataPerMaterial = new Dictionary<Material, MeshData>();
            Dictionary<Material, Mesh> meshes = new Dictionary<Material, Mesh>();

            List<Vector3> currentVertices = new List<Vector3>();
            List<Vector3> currentNormals = new List<Vector3>();
            List<Vector2> currentTextureCoordinates = new List<Vector2>();

            Material currentMaterial = materials[0];
            bool isFirstMaterial = true;

            for (int i = 0; i < data.Length; i++)
            {
                string line = data[i];

                if (line.StartsWith("usemtl "))
                {
                    // Find the material name
                    string materialName = line.Substring("usemtl ".Length);

                    if (!isFirstMaterial)
                    {
                        if (!meshDataPerMaterial.ContainsKey(currentMaterial))
                        {
                            meshDataPerMaterial.Add(currentMaterial, new MeshData());
                        }

                        meshDataPerMaterial[currentMaterial].Vertices.AddRange(currentVertices);
                        meshDataPerMaterial[currentMaterial].Normals.AddRange(currentNormals);
                        meshDataPerMaterial[currentMaterial].TextureCoordinates.AddRange(currentTextureCoordinates);

                        currentVertices = new List<Vector3>();
                        currentNormals = new List<Vector3>();
                        currentTextureCoordinates = new List<Vector2>();
                    }
                    else
                    {
                        isFirstMaterial = false;
                    }

                    // Find the material and set it as current material
                    bool materialFound = false;
                    foreach (Material material in materials)
                    {
                        if (!materialFound)
                        {
                            if (material.MaterialName == materialName)
                            {
                                currentMaterial = material;
                                materialFound = true;
                            }
                        }
                    }
                }
                else if (line.StartsWith("f "))
                {
                    string[] lineData = line.Substring("f ".Length).Trim().Split(' ');
                    List<Corner> foundCorners = new List<Corner>();

                    foreach (var setData in lineData)
                    {
                        Corner currentCorner = new Corner();

                        string[] stringData = setData.Split('/');

                        if (stringData.Length == 1)
                        {
                            ParseInt(line, stringData[0], ref currentCorner.Vertex);
                        }
                        else if (stringData.Length == 2)
                        {
                            ParseInt(line, stringData[0], ref currentCorner.Vertex);
                            ParseInt(line, stringData[1], ref currentCorner.Texture);
                        }
                        else if (stringData.Length == 3)
                        {
                            ParseInt(line, stringData[0], ref currentCorner.Vertex);
                            ParseInt(line, stringData[1], ref currentCorner.Texture);
                            ParseInt(line, stringData[2], ref currentCorner.Normal);
                        }

                        // Convert negative indices into positive ones
                        currentCorner.Vertex = (currentCorner.Vertex > 0) ? currentCorner.Vertex - 1 : vertices.Count + currentCorner.Vertex - 1;
                        currentCorner.Normal = (currentCorner.Normal > 0) ? currentCorner.Normal - 1 : normals.Count + currentCorner.Normal - 1;
                        currentCorner.Texture = (currentCorner.Texture > 0) ? currentCorner.Texture - 1 : textureCoordinates.Count + currentCorner.Texture - 1;

                        foundCorners.Add(currentCorner);
                    }

                    // Handles triangles, quads and polygons
                    for (int j = 1; j < foundCorners.Count - 1; j++)
                    {
                        currentVertices.Add(vertices[foundCorners[0].Vertex]);
                        currentNormals.Add(normals[foundCorners[0].Normal]);
                        currentTextureCoordinates.Add(textureCoordinates[foundCorners[0].Texture]);

                        currentVertices.Add(vertices[foundCorners[j].Vertex]);
                        currentNormals.Add(normals[foundCorners[j].Normal]);
                        currentTextureCoordinates.Add(textureCoordinates[foundCorners[j].Texture]);

                        currentVertices.Add(vertices[foundCorners[j + 1].Vertex]);
                        currentNormals.Add(normals[foundCorners[j + 1].Normal]);
                        currentTextureCoordinates.Add(textureCoordinates[foundCorners[j + 1].Texture]);
                    }
                }
            }

            // Add the final mesh manually
            if (!meshDataPerMaterial.ContainsKey(currentMaterial))
            {
                meshDataPerMaterial.Add(currentMaterial, new MeshData());
            }

            meshDataPerMaterial[currentMaterial].Vertices.AddRange(currentVertices);
            meshDataPerMaterial[currentMaterial].Normals.AddRange(currentNormals);
            meshDataPerMaterial[currentMaterial].TextureCoordinates.AddRange(currentTextureCoordinates);

            // Convert meshdata into meshes
            foreach (var (material, meshData) in meshDataPerMaterial)
            {
                meshes.Add(material, new Mesh(meshData.Vertices, meshData.TextureCoordinates, meshData.Normals));
            }

            return meshes;
        }

        public struct Corner()
        {
            public int Vertex = -1;
            public int Texture = -1;
            public int Normal = - 1;
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

                        ResetMaterialParamaters(ref materialName, ref ka, ref kd, ref ks, ref ns, ref opacity, ref illum, ref diffuseMap, ref specularMap, ref normalMap);
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

        private static void ResetMaterialParamaters(ref string materialName, ref Vector3 ka, ref Vector3 kd, ref Vector3 ks, ref float ns, ref float opacity, ref int illum, ref Texture diffuseMap, ref Texture specularMap, ref Texture normalMap)
        {
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
    }
}