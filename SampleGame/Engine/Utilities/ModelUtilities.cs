using OpenTK.Mathematics;
using SampleGame.Engine.Graphics;

namespace SampleGame.Engine.Utilities
{
    internal class ModelUtilities
    {
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
                string line = data[i];

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
                    if (!float.TryParse(line.Substring("Ns ".Length), out ns))
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
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
                    if (!float.TryParse(line.Substring("d ".Length), out opacity))
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                        opacity = 0;
                    }
                }
                else if (line.StartsWith("Tr"))
                {
                    if (!float.TryParse(line.Substring("Tr ".Length), out opacity))
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
                    if (!int.TryParse(line.Substring("illum ".Length), out illum))
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
            string textureName = line.Substring(name.Length).Trim();

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

            if (!float.TryParse(data[0], out f1) || !float.TryParse(data[1], out f2) || !float.TryParse(data[2], out f3))
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

            if (!float.TryParse(data[0], out f1) || !float.TryParse(data[1], out f2))
            {
                Console.WriteLine($"ParseVector2: parsing error. Line: {line}");

                return Vector2.Zero;
            }

            return new Vector2(f1, f2);
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
