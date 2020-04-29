namespace VoxelSharp.Common
{
    using OpenToolkit.Graphics.OpenGL4;
    using System;

    public class VertexBuffer : GLBindableObject
    {
        private static readonly Action<GLBindableObject> GenericBindCallback = (obj) => GL.BindBuffer(BufferTarget.ArrayBuffer, obj.Handle);

        private static readonly Action<GLBindableObject> GenericUnbindCallback = (_) => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public VertexBuffer()
        {
            Handle = GL.GenBuffer();
        }

        public VertexBuffer(float[] data)
            : this()
        {
            Data = data;
        }

        public BufferUsageHint UsageHint { get; set; } = BufferUsageHint.StaticDraw;

        public float[] Data { get; set; }

        protected override Action<GLBindableObject> BindCallback { get; } = GenericBindCallback;

        protected override Action<GLBindableObject> UnbindCallback { get; } = GenericUnbindCallback;

        public void Commit()
        {
            if (Data == null) return;

            using (Bind())
                GL.BufferData(BufferTarget.ArrayBuffer, Data.Length * sizeof(float), Data, UsageHint);
        }

        protected override void Delete() => GL.DeleteBuffer(Handle);
    }
}
