namespace VoxelSharp
{
    using OpenToolkit.Graphics.OpenGL4;
    using OpenToolkit.Mathematics;
    using System;
    using System.Collections.Generic;
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

            // Load mesh
            BlockVertexBuffer = Block.VertexBuffer;
            BlockVertexAtrribs = new VertexArray();
            

            // Bind mesh parameters to shader arguments
            using (BlockVertexBuffer.Bind())
            {
                using (BlockVertexAtrribs.Bind())
                {
                    var dataType = VertexAttribPointerType.Float;
                    var floatSize = sizeof(float);
                    var stride = 8 * floatSize;

                    BlockVertexAtrribs.AddPointer(Scene.Renderer, "aPos", 3, dataType, stride, 0);
                    BlockVertexAtrribs.AddPointer(Scene.Renderer, "aNormal", 3, dataType, stride, 3 * floatSize);
                    BlockVertexAtrribs.AddPointer(Scene.Renderer, "aTexCoords", 2, dataType, stride, 6 * floatSize);
                }
            }

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

            // The model matrices buffer contains model matrices for all cubes
            UpdateMatricesBuffer();
        }

        public BlockScene Scene { get; }

        public IReadOnlyList<Block> Blocks => m_Blocks;

        public ArrayBuffer<float> BlockVertexBuffer { get; }

        public VertexArray BlockVertexAtrribs { get; }

        public ArrayBuffer<Matrix4> ModelMatricesBuffer { get; private set; }

        public Material Material { get; } = new Material { Specular = new Vector3(0.5f, 0.5f, 0.5f), Shininess = 32f };

        public Texture DiffuseMap { get; }

        public Texture SpecularMap { get; }

        public void UpdateMatricesBuffer()
        {
            if (ModelMatricesBuffer == null)
                ModelMatricesBuffer = new ArrayBuffer<Matrix4>();

            var data = new Matrix4[Blocks.Count];
            for(var i = 0; i < data.Length; i++)
                data[i] = Blocks[i].ComputeMatrix();

            using (ModelMatricesBuffer.Bind())
            {
                ModelMatricesBuffer.Data = data;
                ModelMatricesBuffer.Commit();
            }
        }
    }
}
