using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace SampleGame.Engine.Graphics
{
    internal class Texture
    {
        public readonly int Handle;

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        // Load a texture from a file and generate a handle for it
        public static Texture LoadFromFile(string path)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(1); // Flip because OpenGL loads opposite direction as StbImage

            // Read image file and pass to OpenGL
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Check for OpenGL errors
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception($"OpenGL error occurred: {error}");
            }

            return new Texture(handle);
        }

        public static string FindTextureFilePath(string textureName)
        {
            string texturePath = "";

            // Search through the Assets directory to find the path to the texture name
            string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
            foreach (var file in Directory.EnumerateFiles(assetsPath, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file) == textureName)
                {
                    texturePath = file;
                }
            }

            return texturePath;
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}