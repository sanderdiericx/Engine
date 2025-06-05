using StbImageSharp;

namespace SampleGame.Engine.Content
{
    public class ResourceLoader
    {
        private Dictionary<string, string[]> LoadedWavefronts;
        private Dictionary<string, ImageResult[]> LoadedSkyboxes;

        private static string[] _supportedExtensions = { ".obj", ".mtl" };

        // Turn into singleton instance so it can be accesssed anywhere
        private static readonly ResourceLoader _instance = new ResourceLoader();
        public static ResourceLoader Instance => _instance;

        private ResourceLoader()
        {
            LoadedWavefronts = new Dictionary<string, string[]>();
            LoadedSkyboxes = new Dictionary<string, ImageResult[]>();
        }


        public void LoadSkybox(string skyBoxName, string folderPath, string px, string nx, string py, string ny, string pz, string nz)
        {
            string[] faces = {
                px, nx,
                py, ny,
                pz, nz
            };

            ImageResult[] images = new ImageResult[6];

            StbImage.stbi_set_flip_vertically_on_load(0);

            // Read image files
            for (int i = 0; i < faces.Length; i++)
            {
                if (File.Exists(Path.Combine(folderPath, faces[i])))
                {
                    using (Stream stream = File.OpenRead(Path.Combine(folderPath, faces[i])))
                    {
                        images[i] = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    }
                }
                else
                {
                    Console.WriteLine($"LoadSkybox: Face texture file could not be located. Skybox may be incomplete. ({faces[i]})");
                }
            }
            
            if (!LoadedSkyboxes.ContainsKey(skyBoxName))
            {
                LoadedSkyboxes.Add(skyBoxName, images);
            }
            else
            {
                Console.WriteLine($"LoadSkybox: {skyBoxName} is already used. Please use another name.");
            }
        }

        public void LoadWavefrontAsset(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);

            if (File.Exists(filePath) && !LoadedWavefronts.ContainsKey(fileName) && _supportedExtensions.Contains(extension))
            {
                string[] data = File.ReadAllLines(filePath);

                LoadedWavefronts.Add(fileName, data);
            }
            else if (LoadedWavefronts.ContainsKey(fileName)) // Duplicate filename
            {
                Console.WriteLine($"LoadAsset: {fileName} is already used. Please use another name.");
            }
            else if (_supportedExtensions.Contains(extension)) // File was not found
            {
                Console.WriteLine($"LoadAsset: {filePath} was not found and thus could not be loaded.");
            }
        }

        public void LoadWavefrontFolder(string folderPath)
        {
            string[] filePaths = Directory.GetFiles(folderPath);

            foreach (var path in filePaths)
            {
                LoadWavefrontAsset(path);
            }
        }

        public void UnloadWavefrontAsset(string fileName)
        {
            if (!LoadedWavefronts.Remove(fileName))
            {
                Console.Write($"UnloadWavefrontAsset: requested asset could not be unloaded as it was not found. ({fileName})");
            }
        }

        public void UnloadWavefrontFolder(string folderPath)
        {
            string[] filePaths = Directory.GetFiles(folderPath);

            foreach (var path in filePaths)
            {
                string fileName = Path.GetFileName(path);

                // Remove found files from memory
                LoadedWavefronts.Remove(fileName);
            }
        }

        internal string[] GetWavefrontAsset(string fileName)
        {
            if (LoadedWavefronts.TryGetValue(fileName, out var data))
            {
                return data;
            }

            Console.WriteLine($"GetWavefrontAsset: requested asset could not be found. ({fileName})");
            return []; // Returns an empty string array if unsuccessfull
        }

        internal ImageResult[] GetSkybox(string skyBoxName)
        {
            if (LoadedSkyboxes.TryGetValue(skyBoxName, out var data))
            {
                return data;
            }

            Console.WriteLine($"GetSkybox: requested asset could not be found. ({skyBoxName})");
            return []; // Returns an empty imageresult array if unsuccessfull
        }
    }
}
