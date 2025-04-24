using OpenTK.Mathematics;
using SampleGame.Engine.Content;
using SampleGame.Engine.Graphics;
using SampleGame.Engine.Utilities;

namespace SampleGame.Engine.Core
{
    public class Model
    {
        private List<Vector3> _vertices;
        private List<Vector2> _textureCoordinates;
        private List<Vector3> _normals;
        private List<Material> _materials;

        public Model(string nameOBJ, string nameMTL)
        {
            string[] dataObj = ResourceLoader.Instance.GetAsset(nameOBJ);
            string[] dataMtl = ResourceLoader.Instance.GetAsset(nameMTL);

            var output = ModelUtilities.ParseOBJ(dataObj);

            _vertices = output.Item1;
            _textureCoordinates = output.Item2;
            _normals = output.Item3;

            _materials = ModelUtilities.ParseMTL(dataMtl);
        }
    }
}
