using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Content;
using SampleGame.Engine.Graphics;
using System.Globalization;

namespace SampleGame.Engine.Utilities
{
    public class ModelUtilities
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
                    try
                    {
                        ns = float.Parse(line.Substring(3));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    }
                }
                else if (line.StartsWith("Ka "))
                {
                    string[] lineData = line.Substring(3).Split(' ');

                    try
                    {
                        ka = new Vector3(float.Parse(lineData[0]), float.Parse(lineData[1]), float.Parse(lineData[2]));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    }
                }
                else if (line.StartsWith("Kd "))
                {
                    string[] lineData = line.Substring(3).Split(' ');

                    try
                    {
                        kd = new Vector3(float.Parse(lineData[0]), float.Parse(lineData[1]), float.Parse(lineData[2]));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    }
                }
                else if (line.StartsWith("Ks "))
                {
                    string[] lineData = line.Substring(3).Split(' ');

                    try
                    {
                        ks = new Vector3(float.Parse(lineData[0]), float.Parse(lineData[1]), float.Parse(lineData[2]));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    } 
                }
                else if (line.StartsWith("d "))
                {
                    try
                    {
                        opacity = float.Parse(line.Substring(2));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    }
                }
                else if (line.StartsWith("Tr"))
                {
                    try
                    {
                        // Transparency is the opposite of dissolve
                        opacity = 1.0f - float.Parse(line.Substring(3));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    }
                }
                else if (line.StartsWith("illum "))
                {
                    try
                    {
                        illum = int.Parse(line.Substring(6));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseMTL: parsing error at line {i + 1}. Material may be incomplete.");
                    }
                }
                else if (line.StartsWith("map_Kd "))
                {
                    string textureName = line.Substring(7);

                    string texturePath = Texture.FindTextureFilePath(textureName);

                    if (texturePath != "")
                    {
                        diffuseMap = Texture.LoadFromFile(texturePath);
                    }
                    else
                    {
                        Console.WriteLine($"CreateTexture: the texture {textureName} was not found in the Assets folder.");
                    }
                }
                else if (line.StartsWith("map_Ks "))
                {
                    string textureName = line.Substring(7);

                    string texturePath = Texture.FindTextureFilePath(textureName);

                    if (texturePath != "")
                    {
                        specularMap = Texture.LoadFromFile(texturePath);
                    }
                    else
                    {
                        Console.WriteLine($"CreateTexture: the texture {textureName} was not found in the Assets folder.");
                    }
                }
                else if (line.StartsWith("map_Kn "))
                {
                    string textureName = line.Substring(7);

                    string texturePath = Texture.FindTextureFilePath(textureName);

                    if (texturePath != "")
                    {
                        normalMap = Texture.LoadFromFile(texturePath);
                    }
                    else
                    {
                        Console.WriteLine($"CreateTexture: the texture {textureName} was not found in the Assets folder.");
                    }
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
                    string[] lineData = line.Substring(2).Split(' ');

                    try
                    {
                        vertices.Add(new Vector3(float.Parse(lineData[0]), float.Parse(lineData[1]), float.Parse(lineData[2])));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseOBJ: parsing error at line {i + 1}. Mesh may be incomplete.");
                    }
                }
                else if (line.StartsWith("vt "))
                {
                    string[] lineData = line.Substring(3).Split(' ');

                    try
                    {
                        textureCoordinates.Add(new Vector2(float.Parse(lineData[0]), float.Parse(lineData[1])));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseOBJ: parsing error at line {i + 1}. Mesh may be incomplete.");
                    }
                }
                else if (line.StartsWith("vn "))
                {
                    string[] lineData = line.Substring(3).Split(' ');

                    try
                    {
                        normals.Add(new Vector3(float.Parse(lineData[0]), float.Parse(lineData[1]), float.Parse(lineData[2])));
                    }
                    catch
                    {
                        Console.WriteLine($"ParseOBJ: parsing error at line {i + 1}. Mesh may be incomplete.");
                    }
                }
            }

            return (vertices, textureCoordinates, normals);
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
