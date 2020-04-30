namespace VoxelSharp.Engine
{
    using Common;
    using OpenToolkit.Graphics.OpenGL4;
    using OpenToolkit.Mathematics;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class DefaultShaderProgram : ShaderProgram
    {
        private const int MaxSpotLights = 8;
        private const int MaxPointLights = 8;

        private static readonly string VertexSource = Path.Combine(Utils.ShadersDirectory, "default.vert");
        private static readonly string FragmentSource = Path.Combine(Utils.ShadersDirectory, "default.frag");
        private readonly List<PointLight> m_PointLights = new List<PointLight>(MaxPointLights);
        private readonly List<SpotLight> m_SpotLights = new List<SpotLight>(MaxSpotLights);
        
        public DefaultShaderProgram()
            : base()
        {
            for (var i = 0; i < MaxSpotLights; i++)
                m_SpotLights.Add(new SpotLight());

            for (var i = 0; i < MaxPointLights; i++)
                m_PointLights.Add(new PointLight());

            Add(new Shader(Path.GetFileName(VertexSource), File.ReadAllText(VertexSource, Encoding.UTF8), ShaderType.VertexShader));

            var fragmentSource = File.ReadAllText(FragmentSource, Encoding.UTF8);
            fragmentSource = fragmentSource.Replace("#define NR_POINT_LIGHTS 4", $"#define NR_POINT_LIGHTS {MaxPointLights}");
            fragmentSource = fragmentSource.Replace("#define NR_SPOT_LIGHTS 4", $"#define NR_SPOT_LIGHTS {MaxSpotLights}");

            Add(new Shader(Path.GetFileName(FragmentSource), fragmentSource, ShaderType.FragmentShader));
            Compile();
        }

        public Camera Camera { get; } = new Camera();

        public Material Material { get; } = new Material { Specular = new Vector3(0.5f, 0.5f, 0.5f), Shininess = 32f };

        public DirectionalLight DirectionalLight { get; } = new DirectionalLight(); 

        public IReadOnlyList<PointLight> PointLights => m_PointLights;

        public IReadOnlyList<SpotLight> SpotLights => m_SpotLights;

        public void ApplyCamera()
        {
            try
            {
                CheckBound();
                IsCheckBoundDisabled = true;

                Set("view", Camera.GetViewMatrix());
                Set("projection", Camera.GetProjectionMatrix());
                Set("viewPos", Camera.Position);
            }
            finally
            {
                IsCheckBoundDisabled = false;
            }
        }

        public void ApplyMaterial()
        {
            try
            {
                CheckBound();
                IsCheckBoundDisabled = true;

                Set("material.diffuse", Material.Diffuse.HasValue ? 1 : 0);
                if (Material.Diffuse.HasValue)
                    Set("material.diffuse", Material.Diffuse.Value);

                Set("material.specular", Material.Specular.HasValue ? 1 : 0);
                if (Material.Specular.HasValue)
                    Set("material.specular", Material.Specular.Value);

                Set("material.shininess", Material.Shininess);
            }
            finally
            {
                IsCheckBoundDisabled = false;
            }
        }

        public void ApplyLights()
        {
            try
            {
                CheckBound();
                IsCheckBoundDisabled = true;

                Set("dirLight.direction", DirectionalLight.Direction);
                Set("dirLight.ambient", DirectionalLight.Ambient);
                Set("dirLight.diffuse", DirectionalLight.Diffuse);
                Set("dirLight.specular", DirectionalLight.Specular);

                var i = 0;
                foreach (var light in PointLights)
                {
                    Set($"pointLights[{i}].enabled", light.IsEnabled ? 1 : 0);

                    if (!light.IsEnabled)
                        continue;

                    Set($"pointLights[{i}].position", light.Position);
                    Set($"pointLights[{i}].ambient", light.Ambient);
                    Set($"pointLights[{i}].diffuse", light.Diffuse);
                    Set($"pointLights[{i}].specular", light.Specular);
                    Set($"pointLights[{i}].constant", light.Constant);
                    Set($"pointLights[{i}].linear", light.Linear);
                    Set($"pointLights[{i}].quadratic", light.Quadratic);
                    
                    i++;
                }

                i = 0;
                foreach (var light in SpotLights)
                {
                    Set($"spotLights[{i}].enabled", light.IsEnabled ? 1 : 0);

                    if (!light.IsEnabled)
                        continue;

                    Set($"spotLights[{i}].position", light.Position);
                    Set($"spotLights[{i}].direction", light.Direction);
                    Set($"spotLights[{i}].ambient", light.Ambient);
                    Set($"spotLights[{i}].diffuse", light.Diffuse);
                    Set($"spotLights[{i}].specular", light.Specular);
                    Set($"spotLights[{i}].constant", light.Constant);
                    Set($"spotLights[{i}].linear", light.Linear);
                    Set($"spotLights[{i}].quadratic", light.Quadratic);
                    Set($"spotLights[{i}].cutOff", light.Cutoff);
                    Set($"spotLights[{i}].outerCutOff", light.OuterCutoff);

                    i++;
                }

            }
            finally
            {
                IsCheckBoundDisabled = false;
            }
        }

        public void ApplyModel(Matrix4 model)
        {
            Set("model", model);
        }
    }
}
