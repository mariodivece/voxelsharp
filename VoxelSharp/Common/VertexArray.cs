namespace VoxelSharp.Common
{
    using OpenTK.Graphics.OpenGL;
    using System;

    public class VertexArray : GLBindableObject
    {
        private static readonly Action<GLBindableObject> GenericBindCallback = (obj) => GL.BindVertexArray(obj.Handle);

        private static readonly Action<GLBindableObject> GenericUnbindCallback = (_) => GL.BindVertexArray(0);


        public VertexArray()
        {
            Handle = GL.GenVertexArray();
        }

        protected override Action<GLBindableObject> BindCallback { get; } = GenericBindCallback;

        protected override Action<GLBindableObject> UnbindCallback { get; } = GenericUnbindCallback;

        protected override void Delete()
        {
            GL.DeleteVertexArray(Handle);
        }

        public void AddPointer(int shaderArgIndex, int itemCount, VertexAttribPointerType itemType, int stride, int offset, int divisor)
        {
            if (!IsBound)
                throw new InvalidOperationException("Bind this object before calling this method");

            GL.EnableVertexAttribArray(shaderArgIndex);
            GL.VertexAttribPointer(shaderArgIndex, itemCount, itemType, false, stride, offset);
            if (divisor > 0)
                GL.VertexAttribDivisor(shaderArgIndex, divisor);
        }

        public void AddPointer(int shaderArgIndex, int itemCount, VertexAttribPointerType itemType, int stride, int offset) =>
            AddPointer(shaderArgIndex, itemCount, itemType, stride, offset, 0);

        public void AddPointer(ShaderProgram shader, string shaderArgName, int itemCount, VertexAttribPointerType itemType, int stride, int offset, int divisor) =>
            AddPointer(shader.GetAttribLocation(shaderArgName), itemCount, itemType, stride, offset, divisor);

        public void AddPointer(ShaderProgram shader, string shaderArgName, int itemCount, VertexAttribPointerType itemType, int stride, int offset) =>
            AddPointer(shader.GetAttribLocation(shaderArgName), itemCount, itemType, stride, offset, 0);
    }
}
