namespace SampleGame.Engine.Content
{
    public class ResourceLoader
    {
        private Dictionary<string, string[]> LoadedAssets;

        private static string[] _supportedExtensions = { ".obj", ".mtl" };

        // Turn into singleton instance so it can be accesssed anywhere
        private static readonly ResourceLoader _instance = new ResourceLoader();
        public static ResourceLoader Instance => _instance;

        private ResourceLoader()
        {
            LoadedAssets = new Dictionary<string, string[]>();
        }

        public void LoadAsset(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);

            if (File.Exists(filePath) && !LoadedAssets.ContainsKey(fileName) && _supportedExtensions.Contains(extension))
            {
                string[] data = File.ReadAllLines(filePath);

                LoadedAssets.Add(fileName, data);
            }
            else if (LoadedAssets.ContainsKey(fileName)) // Duplicate filename
            {
                Console.WriteLine($"LoadAsset: {fileName} is already used. Please use another name.");
            }
            else if (_supportedExtensions.Contains(extension)) // File was not found
            {
                Console.WriteLine($"LoadAsset: {filePath} was not found and thus could not be loaded.");
            }
        }

        public void LoadFolder(string folderPath)
        {
            string[] filePaths = Directory.GetFiles(folderPath);

            foreach (var path in filePaths)
            {
                LoadAsset(path);
            }
        }

        public string[] GetAsset(string fileName)
        {
            if (LoadedAssets.TryGetValue(fileName, out var data))
            {
                return data;
            }

            Console.WriteLine($"GetAsset: requested asset could not be found. ({fileName})");
            return []; // Returns an empty string array if unsuccessfull
        }
    }
}
