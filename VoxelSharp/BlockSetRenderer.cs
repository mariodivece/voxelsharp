namespace VoxelSharp
{
    using Common;
    using Engine;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Mathematics;
    using System.IO;
    using System.Text;

    public class BlockSetRenderer : ShaderProgram
    {
        public const int MaxSpotLights = 4;
        public const int MaxPointLights = 4;

        private readonly string VertexSource = Utils.ShaderPath("default.vert");
        private readonly string FragmentSource = Utils.ShaderPath("default.frag");

        private readonly BlockScene Scene;

        public BlockSetRenderer(BlockScene scene)
            : base()
        {
            Scene = scene;
            if (Scene.IsInstanceRendered)
            {
                VertexSource = Utils.ShaderPath("instanced.vert");
                FragmentSource = Utils.ShaderPath("instanced.frag");
            }
            
            Add(new Shader(Path.GetFileName(VertexSource), File.ReadAllText(VertexSource, Encoding.UTF8), ShaderType.VertexShader));

            var fragmentSource = File.ReadAllText(FragmentSource, Encoding.UTF8);
            fragmentSource = fragmentSource.Replace("#define NR_POINT_LIGHTS 4", $"#define NR_POINT_LIGHTS {MaxPointLights}");
            fragmentSource = fragmentSource.Replace("#define NR_SPOT_LIGHTS 4", $"#define NR_SPOT_LIGHTS {MaxSpotLights}");

            Add(new Shader(Path.GetFileName(FragmentSource), fragmentSource, ShaderType.FragmentShader));
            Compile();
        }

        public void ApplyCamera()
        {
            try
            {
                CheckBound();
                IsCheckBoundDisabled = true;

                Set("view", Scene.Camera.GetViewMatrix());
                Set("projection", Scene.Camera.GetProjectionMatrix());
                Set("viewPos", Scene.Camera.Position);
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

                var material = Scene.BlockSet.Material;

                Set("material.diffuse", material.Diffuse.HasValue ? 1 : 0);
                if (material.Diffuse.HasValue)
                    Set("material.diffuse", material.Diffuse.Value);

                Set("material.specular", material.Specular.HasValue ? 1 : 0);
                if (material.Specular.HasValue)
                    Set("material.specular", material.Specular.Value);

                Set("material.shininess", material.Shininess);
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

                var dirLight = Scene.DirectionalLight;
                Set("dirLight.direction", dirLight.Direction);
                Set("dirLight.ambient", dirLight.Ambient);
                Set("dirLight.diffuse", dirLight.Diffuse);
                Set("dirLight.specular", dirLight.Specular);

                var i = 0;
                foreach (var light in Scene.PointLights)
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
                foreach (var light in Scene.SpotLights)
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
