namespace VoxelSharp.Engine
{
    using OpenToolkit.Mathematics;

    public class DirectionalLight : LightBase
    {
        public Vector3 Direction { get; set; } = new Vector3(-0.2f, -1.0f, -0.3f);
    }
}
