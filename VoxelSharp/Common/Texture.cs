namespace VoxelSharp.Common
{
    using OpenToolkit.Graphics.OpenGL4;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    public class Texture : GLBindableObject
    {
        private TextureTarget BindTarget;
        private TextureUnit BindUnit;

        public Texture(string filePath)
        {
            Handle = GL.GenTexture();

            // Bind the handle
            using (Bind(TextureTarget.Texture2D, TextureUnit.Texture0))
            {
                // Load the image
                using (var image = new Bitmap(filePath))
                {
                    var data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        image.Width,
                        image.Height,
                        0,
                        OpenToolkit.Graphics.OpenGL4.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);
                }

                // Now that our texture is loaded, we can set a few settings to affect how the image appears on rendering.

                // First, we set the min and mag filter. These are used for when the texture is scaled down and up, respectively.
                // Here, we use Linear for both. This means that OpenGL will try to blend pixels, meaning that textures scaled too far will look blurred.
                // You could also use (amongst other options) Nearest, which just grabs the nearest pixel, which makes the texture look pixelated if scaled too far.
                // NOTE: The default settings for both of these are LinearMipmap. If you leave these as default but don't generate mipmaps,
                // your image will fail to render at all (usually resulting in pure black instead).
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                // Now, set the wrapping mode. S is for the X axis, and T is for the Y axis.
                // We set this to Repeat so that textures will repeat when wrapped. Not demonstrated here since the texture coordinates exactly match
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }

        protected override Action<GLBindableObject> BindCallback { get; } = (obj) =>
        {
            var texture = obj as Texture;
            GL.ActiveTexture(texture.BindUnit);
            GL.BindTexture(texture.BindTarget, texture.Handle);
        };

        protected override Action<GLBindableObject> UnbindCallback { get; } = null;

        public IDisposable Bind(TextureTarget target, TextureUnit unit)
        {
            // Activate texture
            // Multiple textures can be bound, if your shader needs more than just one.
            // If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
            // The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.

            BindTarget = target;
            BindUnit = unit;
            return base.Bind();
        }

        public override IDisposable Bind() => throw new NotSupportedException("Please use the alternate bind method.");

        protected override int GetBindSlot() => $"{BindTarget}.{BindUnit}".GetHashCode();

        protected override void Delete() => GL.DeleteTexture(Handle);
    }
}
