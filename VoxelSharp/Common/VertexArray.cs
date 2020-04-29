namespace VoxelSharp.Common
{
    using OpenToolkit.Graphics.OpenGL4;
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
    }
}
