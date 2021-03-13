namespace VoxelSharp.Common
{
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ShaderProgram : GLBindableObject
    {
        private readonly List<Shader> m_Shaders = new(16);

        private readonly Dictionary<string, int> m_UniformLocations = new(256);

        public ShaderProgram()
        {
            Handle = GL.CreateProgram();
        }

        public bool IsCompiled { get; protected set; }

        public IReadOnlyList<Shader> Shaders => m_Shaders;

        public IReadOnlyDictionary<string, int> UniformLocations => m_UniformLocations;

        protected override Action<GLBindableObject> BindCallback { get; } = (obj) =>
        {
            if (!(obj as ShaderProgram).IsCompiled)
                throw new InvalidOperationException("Compile before using this shader pipeline program.");

            GL.UseProgram(obj.Handle);
        };

        protected override Action<GLBindableObject> UnbindCallback { get; } = (_) => GL.UseProgram(0);

        public void Add(Shader shader)
        {
            if (IsCompiled)
                throw new InvalidOperationException("Shader pipeline already compiled");

            m_Shaders.Add(shader);
        }

        public void Compile()
        {
            if (IsCompiled) return;

            // Compile shaders individually
            foreach (var shader in m_Shaders)
                shader.Compile();

            // Attach them to this pipeline
            foreach (var shader in m_Shaders)
                GL.AttachShader(Handle, shader.Handle);

            // We link the pipeline together with individual shaders
            GL.LinkProgram(Handle);

            // Check for linking errors
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({Handle})");
            }

            // We no longer need them once these are linked
            foreach (var shader in m_Shaders)
                GL.DetachShader(Handle, shader.Handle);

            // delete the internal shader
            foreach (var shader in m_Shaders)
                shader.Dispose();

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                m_UniformLocations.Add(key, location);
            }

            IsCompiled = true;
        }

        protected override void Delete()
        {
            GL.DeleteProgram(Handle);
        }

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string name, int data)
        {
            CheckBound();
            GL.Uniform1(m_UniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string name, float data)
        {
            CheckBound();
            GL.Uniform1(m_UniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string name, Matrix4 data)
        {
            CheckBound();
            GL.UniformMatrix4(m_UniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string name, Vector3 data)
        {
            CheckBound();
            GL.Uniform3(m_UniformLocations[name], data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAttribLocation(string name) => GL.GetAttribLocation(Handle, name);

        protected bool IsCheckBoundDisabled { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckBound()
        {
            if (!!IsCheckBoundDisabled) return;
            if (!IsCompiled) throw new InvalidOperationException("Compile before setting uniforms");
            if (!IsBound) throw new InvalidOperationException("Bind before calling this method");
        }
    }
}
