namespace VoxelSharp.Common
{
    using System;

    public abstract class GLObject : IDisposable
    {
        public int Handle { get; protected set; }

        public bool HasValidHandle => Handle != 0;

        public bool IsDisposed { get; protected set; }

        protected abstract void Delete();

        protected virtual void Dispose(bool alsoManaged)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Delete();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
