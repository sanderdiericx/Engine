using OpenTK.Mathematics;

namespace SampleGame.Engine.Graphics
{
    internal class Material
    {
        public string MaterialName;
        public Vector3 Ka;
        public Vector3 Kd;
        public Vector3 Ks;
        public float Ns;
        public float Opacity;
        public int Illum;

        public Texture DiffuseMap;
        public Texture SpecularMap;
        public Texture NormalMap;

        public Material(string materialName, Vector3 ka, Vector3 kd, Vector3 ks, float ns, float opacity, int illum, Texture diffuseMap, Texture specularMap, Texture normalMap)
        {
            MaterialName = materialName;
            Ka = ka;
            Kd = kd;
            Ks = ks;
            Ns = ns;
            Opacity = opacity;
            Illum = illum;
            DiffuseMap = diffuseMap;
            SpecularMap = specularMap;
            NormalMap = normalMap;
        }
    }
}
