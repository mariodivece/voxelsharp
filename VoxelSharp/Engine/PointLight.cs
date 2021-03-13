namespace VoxelSharp.Engine
{
    using OpenTK.Mathematics;

    public class PointLight : LightBase
    {
        public PointLight()
        {
            Ambient = new Vector3(0.05f, 0.05f, 0.05f);
            Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
            Specular = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public bool IsEnabled { get; set; }

        public Vector3 Position { get; set; }

        public float Constant { get; set; } = 1f;

        public float Linear { get; set; } = 0.09f;

        public float Quadratic { get; set; } = 0.032f;
    }
}
