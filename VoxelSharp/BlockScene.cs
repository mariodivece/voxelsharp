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
            SetupLights();
            Renderer = new BlockSetRenderer(this);
            BlockSet = new BlockSet(this);
        }

        public Camera Camera { get; } = new Camera();

        public DirectionalLight DirectionalLight { get; } = new DirectionalLight
        {
            Direction = new Vector3(-0.2f, -1.0f, -0.3f),
            Ambient = new Vector3(0.05f, 0.05f, 0.05f),
            Diffuse = new Vector3(0.4f, 0.4f, 0.4f),
            Specular = new Vector3(0.5f, 0.5f, 0.5f)
        };

        public bool IsInstanceRendered { get; } = true;

        public IReadOnlyList<PointLight> PointLights => m_PointLights;

        public IReadOnlyList<SpotLight> SpotLights => m_SpotLights;

        public BlockSet BlockSet { get; }

        public ShaderProgram Renderer { get; }

        public void Render()
        {
            using (BlockSet.MeshVAO.Bind())
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

                            if (IsInstanceRendered)
                            {
                                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 36, BlockSet.Blocks.Count);
                            }
                            else
                            {
                                foreach (var block in BlockSet.Blocks)
                                {
                                    blockRenderer.ApplyModel(block.ComputeMatrix());
                                    GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetupLights()
        {
            for (var i = 0; i < BlockSetRenderer.MaxSpotLights; i++)
                m_SpotLights.Add(new SpotLight());

            for (var i = 0; i < BlockSetRenderer.MaxPointLights; i++)
                m_PointLights.Add(new PointLight());

            var pointLightPositions = new Vector3[]
            {
                new Vector3(-5, 2, -10),
                new Vector3(-5, 2, -20),
                new Vector3(5, 2, -10),
                new Vector3(5, 2, -20)
                /*
                new Vector3(-5, 2, 5),
                new Vector3(5, 2, -5),
                new Vector3(5, 2, 5)
                */
            };

            for (var i = 0; i < pointLightPositions.Length && i < m_PointLights.Count; i++)
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

            // camera
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

            // top-down spotlight
            spotLight = SpotLights[1];
            spotLight.Position = new Vector3(0, 6, -15);
            spotLight.Direction = new Vector3(0, -1, 0);
            spotLight.IsEnabled = true;
            spotLight.Ambient = new Vector3(0, 1, 1);
            spotLight.Diffuse = Vector3.One;
            spotLight.Specular = Vector3.One;
            spotLight.Constant = 1f;
            spotLight.Linear = 0.09f;
            spotLight.Quadratic = 0.032f;
            spotLight.Cutoff = (float)Math.Cos(MathHelper.DegreesToRadians(12.5f));
            spotLight.OuterCutoff = spotLight.Cutoff; // (float)Math.Cos(MathHelper.DegreesToRadians(30.5f));
        }
    }
}
