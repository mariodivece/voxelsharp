namespace VoxelSharp
{
    using OpenToolkit.Graphics.OpenGL4;
    using OpenToolkit.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VoxelSharp.Common;
    using VoxelSharp.Engine;

    public class BlockSet
    {
        private const int BlockCount = 4096;

        private const int BlockDistanceRange = 250;
        private const int ScaleRangeMin = 80;
        private const int ScaleRangeMax = 150;

        private readonly List<Block> m_Blocks = new List<Block>(BlockCount);

        public BlockSet(BlockScene scene)
        {
            // associate the scene
            Scene = scene;

            // Load Textures
            DiffuseMap = new Texture(Utils.TexturePath("container2.png"));
            SpecularMap = new Texture(Utils.TexturePath("container2_specular.png"));

            // Setup the individual blocks
            var random = new Random();
            for (var i = 0; i < BlockCount; i++)
            {
                var randomPosition = new Vector3(
                    random.Next(-BlockDistanceRange, BlockDistanceRange) / 10f,
                    random.Next(-BlockDistanceRange, BlockDistanceRange) / 10f,
                    random.Next(-BlockDistanceRange, BlockDistanceRange) / 10f);

                // var randomScale = Vector3.One;
                var randomScale = new Vector3(
                    random.Next(ScaleRangeMin, ScaleRangeMax) / 100f,
                    random.Next(ScaleRangeMin, ScaleRangeMax) / 100f,
                    random.Next(ScaleRangeMin, ScaleRangeMax) / 100f);

                m_Blocks.Add(new Block { Position = randomPosition, Scale = randomScale });
            }

            // Load mesh
            VertexVBO = Block.VertexBuffer;
            MeshVAO = new VertexArray();

            // The model matrices buffer contains model matrices for all cubes
            UpdateMatricesBuffer();

            // Bind mesh parameters to shader arguments
            using (MeshVAO.Bind())
            {
                var unitType = VertexAttribPointerType.Float;
                var unitSize = sizeof(float);
                var stride = 8 * unitSize;

                // Bind geometry to the attribute buffer
                using (VertexVBO.Bind())
                {
                    MeshVAO.AddPointer(Scene.Renderer, "aPos", 3, unitType, stride, 0);
                    MeshVAO.AddPointer(Scene.Renderer, "aNormal", 3, unitType, stride, 3 * unitSize);
                    MeshVAO.AddPointer(Scene.Renderer, "aTexCoords", 2, unitType, stride, 6 * unitSize);
                }

                if (Scene.IsInstanceRendered)
                {
                    var mat4Size = Marshal.SizeOf<Matrix4>();
                    var vec4Size = Marshal.SizeOf<Vector4>();

                    // Bind model matrices (transformations) to the attribute buffer
                    using (InstanceVBO.Bind())
                    {
                        var modelLoc = Scene.Renderer.GetAttribLocation("aInstanceMatrix");
                        for (var i = 0; i < 4; i++)
                            MeshVAO.AddPointer(modelLoc + i, 4, unitType, mat4Size, i * vec4Size, 1);
                    }
                }
            }
        }

        public BlockScene Scene { get; }

        public IReadOnlyList<Block> Blocks => m_Blocks;

        public ArrayBuffer<float> VertexVBO { get; }

        public ArrayBuffer<Matrix4> InstanceVBO { get; private set; }

        public VertexArray MeshVAO { get; }

        public Material Material { get; } = new Material { Specular = new Vector3(0.5f, 0.5f, 0.5f), Shininess = 32f };

        public Texture DiffuseMap { get; }

        public Texture SpecularMap { get; }

        public void UpdateMatricesBuffer()
        {
            if (InstanceVBO == null)
                InstanceVBO = new ArrayBuffer<Matrix4>();

            var data = new Matrix4[Blocks.Count];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = Blocks[i].ComputeMatrix();
                data[i].Transpose();
            }

            InstanceVBO.Data = data;
            InstanceVBO.Commit();
        }
    }
}
