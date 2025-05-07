using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SampleGame.Engine.Graphics
{
    internal class Shader : IDisposable
    {
        private bool disposedValue = false;
        public int Handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            // Define 2 handles to the shaders
            int VertexShader, FragmentShader;

            // Read the shader texts
            string VertexShaderSource = File.ReadAllText(vertexPath);
            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            // Bind the source code to the shaders
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compile and check for errors
            int success;

            GL.CompileShader(VertexShader);
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {

                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine(infoLog);
            }

            GL.CompileShader(FragmentShader);
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine(FragmentShader);
            }

            // Link the shaders together into the handle
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            // Check for errors
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine(infoLog);
            }

            // Cleanup
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        // Assign a integer to a uniform
        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        // Assign a matrix to a uniform
        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref matrix);
        }

        // Assign a vector 3 to a uniform
        public void SetVector3(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, vector);
        }

        // Assign a boolean to a uniform
        public void SetBool(string name, bool boolean)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, boolean ? 1 : 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Stop the gc from cleaning up resources, as we already cleaned it up
        }

        // Runs when shader is collected buy garbage collection
        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! did you forget to call Dispose()?");
            }
        }
    }
}