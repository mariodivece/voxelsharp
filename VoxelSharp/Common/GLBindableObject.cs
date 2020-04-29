namespace VoxelSharp.Common
{
    using System;
    using System.Collections.Concurrent;

    public abstract class GLBindableObject : GLObject
    {
        private static readonly ConcurrentDictionary<int, BindReleaser> Slots = new ConcurrentDictionary<int, BindReleaser>();
        private readonly int m_DefaultBindSlot;

        protected GLBindableObject()
        {
            m_DefaultBindSlot = (GetType().Name).GetHashCode(StringComparison.Ordinal);
        }

        public bool IsBound => Slots.TryGetValue(GetBindSlot(), out var bindSlot) &&
            bindSlot != null &&
            HasValidHandle &&
            bindSlot.BoundObject.Handle == Handle;

        public bool CanBind => !Slots.ContainsKey(GetBindSlot());

        protected virtual int GetBindSlot() => m_DefaultBindSlot;

        protected abstract Action<GLBindableObject> BindCallback { get; }
        
        protected abstract Action<GLBindableObject> UnbindCallback { get; }

        public virtual IDisposable Bind()
        {
            var bindSlot = GetBindSlot();
            if (Handle == 0)
                throw new InvalidOperationException($"Object handle cannot be zero.");

            if (!CanBind)
                throw new InvalidOperationException($"Bind slot '{bindSlot}' already bound. Please dispose the existing bind lock.");

            var releaser = new BindReleaser(bindSlot, this);

            try
            {
                BindCallback?.Invoke(this);
            }
            catch
            {
                throw;
            }

            Slots[bindSlot] = releaser;
            return releaser;
        }

        private sealed class BindReleaser : IDisposable
        {
            public BindReleaser(int bindSlot, GLBindableObject boundObject)
            {
                BindSlot = bindSlot;
                BoundObject = boundObject;
            }

            public GLBindableObject BoundObject { get; }

            public int BindSlot { get; }

            public void Dispose()
            {
                Slots.TryRemove(BindSlot, out _);
                BoundObject?.UnbindCallback?.Invoke(BoundObject);
            }
        }
    }
}
