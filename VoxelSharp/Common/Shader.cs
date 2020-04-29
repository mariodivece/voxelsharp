namespace VoxelSharp.Common
{
    using OpenToolkit.Graphics.OpenGL4;
    using System;

    public class Shader : GLObject
    {
        public Shader(string name, string source, ShaderType shaderType)
        {
            Name = name;
            ShaderType = shaderType;
            Handle = GL.CreateShader(ShaderType);
            GL.ShaderSource(Handle, source);
        }

        public ShaderType ShaderType { get; }

        public string Name { get; }

        public bool IsCompiled { get; private set; }

        public void Compile()
        {
            if (IsCompiled)
                return;

            // Try to compile the shader
            GL.CompileShader(Handle);

            // Check for compilation errors
            GL.GetShader(Handle, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(Handle);
                throw new Exception($"Error occurred whilst compiling Shader({Handle}).\n\n{infoLog}");
            }

            IsCompiled = true;
        }

        protected override void Delete() => GL.DeleteShader(Handle);
    }
}
