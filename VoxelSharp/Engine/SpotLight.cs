namespace VoxelSharp.Engine
{
    using OpenToolkit.Mathematics;

    public class SpotLight : PointLight
    {
        public SpotLight()
        {
            Ambient = new Vector3(0.0f, 0.0f, 0.0f);
            Diffuse = new Vector3(1.0f, 1.0f, 1.0f);
            Specular = new Vector3(1.0f, 1.0f, 1.0f);
            Constant = 1f;
            Linear = 0.09f;
            Quadratic = 0.032f;
        }

        public Vector3 Direction { get; set; }

        public float Cutoff { get; set; }

        public float OuterCutoff { get; set; }
    }
}
