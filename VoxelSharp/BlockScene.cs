namespace VoxelSharp
{
    using OpenToolkit.Mathematics;
    using System;
    using System.Collections.Generic;
    using VoxelSharp.Common;
    using VoxelSharp.Engine;
    using OpenToolkit.Graphics.OpenGL4;

    public class BlockScene
    {
        private readonly List<PointLight> m_PointLights = new List<PointLight>(BlockSetRenderer.MaxPointLights);
        private readonly List<SpotLight> m_SpotLights = new List<SpotLight>(BlockSetRenderer.MaxSpotLights);

        public BlockScene()
        {
            for (var i = 0; i < BlockSetRenderer.MaxSpotLights; i++)
                m_SpotLights.Add(new SpotLight());

            for (var i = 0; i < BlockSetRenderer.MaxPointLights; i++)
                m_PointLights.Add(new PointLight());

            Renderer = new BlockSetRenderer(this);
            BlockSet = new BlockSet(this);
            SetupLights();
        }

        public Camera Camera { get; } = new Camera();

        public DirectionalLight DirectionalLight { get; } = new DirectionalLight
        {
            Direction = new Vector3(-0.2f, -1.0f, -0.3f),
            Ambient = new Vector3(0.05f, 0.05f, 0.05f),
            Diffuse = new Vector3(0.4f, 0.4f, 0.4f),
            Specular = new Vector3(0.5f, 0.5f, 0.5f)
        };

        public IReadOnlyList<PointLight> PointLights => m_PointLights;

        public IReadOnlyList<SpotLight> SpotLights => m_SpotLights;

        public BlockSet BlockSet { get; }

        public ShaderProgram Renderer { get; }

        public void Render()
        {
            using (BlockSet.BlockVertexAtrribs.Bind())
            {
                using (BlockSet.DiffuseMap.Bind(TextureTarget.Texture2D, TextureUnit.Texture0))
                {
                    using (BlockSet.SpecularMap.Bind(TextureTarget.Texture2D, TextureUnit.Texture1))
                    {
                        using (Renderer.Bind())
                        {
                            var blockRenderer = Renderer as BlockSetRenderer;
                            blockRenderer.ApplyCamera();
                            blockRenderer.ApplyMaterial();
                            blockRenderer.ApplyLights();

                            foreach (var block in BlockSet.Blocks)
                            {
                                //var model = Matrix4.Identity;
                                //model *= Matrix4.CreateTranslation(CubePositions[i]);
                                //var angle = 20.0f * i;
                                // model *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);
                                //RenderShader.Set("model", model);
                                blockRenderer.ApplyModel(block.ComputeMatrix());
                                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                            }
                        }
                    }
                }
            }
        }

        private void SetupLights()
        {
            var pointLightPositions = new Vector3[]
            {
                new Vector3(0.7f, 0.2f, 2.0f),
                new Vector3(2.3f, -3.3f, -4.0f),
                new Vector3(-4.0f, 2.0f, -12.0f),
                new Vector3(0.0f, 0.0f, -3.0f)
            };

            for (var i = 0; i < pointLightPositions.Length; i++)
            {
                var light = PointLights[i];
                light.IsEnabled = true;
                light.Position = pointLightPositions[i];
                light.Ambient = Vector3.One * 0.05f;
                light.Diffuse = Vector3.One * 0.8f;
                light.Specular = Vector3.One;
                light.Constant = 1f;
                light.Linear = 0.09f;
                light.Quadratic = 0.032f;
            }

            var spotLight = SpotLights[0];
            spotLight.IsEnabled = true;
            spotLight.Ambient = Vector3.Zero;
            spotLight.Diffuse = Vector3.One;
            spotLight.Specular = Vector3.One;
            spotLight.Constant = 1f;
            spotLight.Linear = 0.09f;
            spotLight.Quadratic = 0.032f;
            spotLight.Cutoff = (float)Math.Cos(MathHelper.DegreesToRadians(12.5f));
            spotLight.OuterCutoff = (float)Math.Cos(MathHelper.DegreesToRadians(30.5f));
        }
    }
}
