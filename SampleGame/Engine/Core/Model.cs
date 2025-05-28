using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SampleGame.Engine.Content;
using SampleGame.Engine.Graphics;
using SampleGame.Engine.Utilities;

namespace SampleGame.Engine.Core
{
    public class Model
    {
        internal Vector3[] vertices;
        internal Vector2[] texCoords;
        internal Vector3[] normals;

        internal List<Material> materials;
        internal Dictionary<Material, Mesh> meshes;
        internal bool isInitialized;

        public Matrix4 rotation;
        public Matrix4 transform;
        public Matrix4 scale;

        public Model(string nameOBJ, string nameMTL)
        {
            isInitialized = false;

            rotation = Matrix4.Identity;
            transform = Matrix4.Identity;
            scale = Matrix4.Identity;

            string[] dataObj = ResourceLoader.Instance.GetAsset(nameOBJ);
            string[] dataMtl = ResourceLoader.Instance.GetAsset(nameMTL);

            var output = ModelUtilities.ParseOBJ(dataObj);

            vertices = output.Item1;
            texCoords = output.Item2;
            normals = output.Item3;

            materials = ModelUtilities.ParseMTL(dataMtl);

            meshes = ModelUtilities.GetMeshes(dataObj, vertices, normals, texCoords, materials);
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                foreach (var (material, mesh) in meshes)
                {
                    (int, int, int) output = Engine.InitializeMesh(mesh.UniqueVertexBuffer, mesh.Indices);

                    mesh.VertexArrayObject = output.Item1;
                    mesh.VertexBufferObject = output.Item2;
                    mesh.ElementBufferObject = output.Item3;

                    // Clear unused buffer
                    mesh.UniqueVertexBuffer = null;
                }

                isInitialized = true;
            }
            else
            {
                Console.WriteLine("InitializeModel: Model is already initialized!");
            }
        }

        public void RotateX(float degrees)
        {
            rotation *= Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(degrees));
        }

        public void RotateY(float degrees)
        {
            rotation *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(degrees));
        }

        public void RotateZ(float degrees)
        {
            rotation *= Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(degrees));
        }

        public void Rotate(float xDegrees, float yDegrees, float zDegrees)
        {
            rotation *= Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(zDegrees)) * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(yDegrees)) * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(xDegrees));
        }

        public void ResetRotation()
        {
            rotation = Matrix4.Identity;
        }

        public void TransformX(float x)
        {
            transform *= Matrix4.CreateTranslation(new Vector3(x, 0, 0));
        }

        public void TransformY(float y)
        {
            transform *= Matrix4.CreateTranslation(new Vector3(0, y, 0));
        }

        public void TransformZ(float z)
        {
            transform *= Matrix4.CreateTranslation(new Vector3(0, 0, z));
        }

        public void Transform(float x, float y, float z)
        {
            transform *= Matrix4.CreateTranslation(new Vector3(x, y, z));
        }

        public void SetPosition(float x, float y, float z)
        {
            transform = Matrix4.Identity * Matrix4.CreateTranslation(new Vector3(x, y, z));
        }

        public void ScaleX(float x)
        {
            scale *= Matrix4.CreateScale(new Vector3(x, 1, 1));
        }

        public void ScaleY(float y)
        {
            scale *= Matrix4.CreateScale(new Vector3(1, y, 1));
        }
        public void ScaleZ(float z)
        {
            scale *= Matrix4.CreateScale(new Vector3(1, 1, z));
        }

        public void Scale(float scaleFactor)
        {
            scale *= Matrix4.CreateScale(scaleFactor);
        }

        public void SetScale(float x, float y, float z)
        {
            scale = Matrix4.Identity * Matrix4.CreateScale(new Vector3(x, y, z));
        }
    }
}
