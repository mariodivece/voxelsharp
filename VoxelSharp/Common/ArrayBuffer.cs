namespace VoxelSharp.Common
{
    using OpenToolkit.Graphics.OpenGL4;
    using System;
    using System.Runtime.InteropServices;

    public class ArrayBuffer<T> : GLBindableObject
        where T : unmanaged
    {
        private static readonly Action<GLBindableObject> GenericBindCallback = (obj) => GL.BindBuffer(BufferTarget.ArrayBuffer, obj.Handle);

        private static readonly Action<GLBindableObject> GenericUnbindCallback = (_) => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public ArrayBuffer()
        {
            Handle = GL.GenBuffer();
        }

        public ArrayBuffer(T[] data)
            : this()
        {
            Data = data;
        }

        public BufferUsageHint UsageHint { get; set; } = BufferUsageHint.StaticDraw;

        public T[] Data { get; set; }

        protected override Action<GLBindableObject> BindCallback { get; } = GenericBindCallback;

        protected override Action<GLBindableObject> UnbindCallback { get; } = GenericUnbindCallback;

        public void Commit()
        {
            if (Data == null) return;

            using (Bind())
                GL.BufferData(BufferTarget.ArrayBuffer, Data.Length * Marshal.SizeOf<T>(), Data, UsageHint);
        }

        protected override void Delete() => GL.DeleteBuffer(Handle);
    }
}
