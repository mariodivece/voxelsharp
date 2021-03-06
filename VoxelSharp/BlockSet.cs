﻿namespace VoxelSharp
{
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Mathematics;
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

        private readonly List<Block> m_Blocks = new(BlockCount);

        public BlockSet(BlockScene scene)
        {
            // associate the scene
            Scene = scene;

            // Load Textures
            DiffuseMap = new Texture(Utils.TexturePath("container2.png"));
            SpecularMap = new Texture(Utils.TexturePath("container2_specular.png"));

            var blockCount = PyramidBlocks(true, 30); // ExplodeBlocks();
            Console.WriteLine($"Block Count: {blockCount}");

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
                        // we can't just pass the mat4x4 because attrib size cannot be biggler than 4 floats.
                        // so we need to add 4 different pointers, each 4 floats to add all 4 matrix pointers
                        // the divisor argument specifies every nth instance -- we want 1 as the divisor.
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
                // data[i].Transpose();
            }

            InstanceVBO.Data = data;
            InstanceVBO.Commit();
        }

        private int ExplodeBlocks()
        {
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

                var randomRotation = new Vector3(
                    random.Next(0, 90),
                    random.Next(0, 90),
                    random.Next(0, 90));

                m_Blocks.Add(new Block { Position = randomPosition, Scale = randomScale, Rotation = randomRotation });
            }

            return BlockCount;
        }

        private int PyramidBlocks(bool simplify, int width)
        {
            var genBlocks = new List<Block>(width * width * width);

            for (var y = 0; y < width / 2; y++)
            {
                for (var x = 0 + y; x < width - y; x++)
                {
                    for (var z = 0 + y; z < width - y; z++)
                    {
                        genBlocks.Add(new Block
                        {
                            Position = new Vector3(x - (width / 2), y, z - width),
                            Rotation = new Vector3(0f, 0f, 0f)
                        });
                    }
                }
            }

            // Only keep blocks on the outside
            if (simplify)
            {
                foreach (var block in genBlocks)
                {
                    var p = block.Position;

                    var neighborCount =
                        genBlocks.Count(b => b.Position.X == p.X - 1 && b.Position.Y == p.Y && b.Position.Z == p.Z) +
                        genBlocks.Count(b => b.Position.X == p.X + 1 && b.Position.Y == p.Y && b.Position.Z == p.Z) +
                        genBlocks.Count(b => b.Position.X == p.X && b.Position.Y == p.Y - 1 && b.Position.Z == p.Z) +
                        genBlocks.Count(b => b.Position.X == p.X && b.Position.Y == p.Y + 1 && b.Position.Z == p.Z) +
                        genBlocks.Count(b => b.Position.X == p.X && b.Position.Y == p.Y && b.Position.Z == p.Z - 1) +
                        genBlocks.Count(b => b.Position.X == p.X && b.Position.Y == p.Y && b.Position.Z == p.Z + 1);

                    if (neighborCount >= 6)
                        continue;

                    m_Blocks.Add(block);
                }
            }
            else
            {
                m_Blocks.AddRange(genBlocks);
            }

            return m_Blocks.Count;
        }
    }
}
