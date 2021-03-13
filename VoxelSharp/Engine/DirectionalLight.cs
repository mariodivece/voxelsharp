namespace VoxelSharp.Engine
{
    using OpenTK.Mathematics;

    public class DirectionalLight : LightBase
    {
        public Vector3 Direction { get; set; } = new Vector3(-0.2f, -1.0f, -0.3f);
    }
}
