namespace VoxelSharp.Engine
{
    using OpenTK.Mathematics;

    public abstract class LightBase
    {
        public Vector3 Ambient { get; set; } = new Vector3(0.05f, 0.05f, 0.05f);

        public Vector3 Diffuse { get; set; } = new Vector3(0.4f, 0.4f, 0.4f);

        public Vector3 Specular { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);
    }
}
