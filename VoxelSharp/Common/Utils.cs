namespace VoxelSharp.Common
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class Utils
    {
        static Utils()
        {
            var codeBase = Assembly.GetEntryAssembly().CodeBase;
            BaseDirectory = string.IsNullOrWhiteSpace(codeBase)
                ? Path.GetFullPath(".")
                : Path.GetDirectoryName(new Uri(codeBase).LocalPath);

            ResourcesDirectory = Path.Combine(BaseDirectory, "Resources");
            ShadersDirectory = Path.Combine(ResourcesDirectory, "Shaders");
            TexturesDirectory = Path.Combine(ResourcesDirectory, "Textures");
        }

        public static string BaseDirectory { get; }

        public static string ResourcesDirectory { get; }

        public static string ShadersDirectory { get; }

        public static string TexturesDirectory { get; }
    }
}
