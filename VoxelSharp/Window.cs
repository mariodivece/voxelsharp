namespace VoxelSharp
{
    using OpenToolkit.Graphics.OpenGL4;
    using OpenToolkit.Mathematics;
    using OpenToolkit.Windowing.Common;
    using OpenToolkit.Windowing.Common.Input;
    using OpenToolkit.Windowing.Desktop;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Common;
    using Engine;
    using System.Linq;

    public class Window : GameWindow
    {
        private const int BlockCount = 4096;
        private const int BlockDistanceRange = 250;

        private static readonly Color4 ClearColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f);

        private DefaultShaderProgram RenderShader;
        private Texture DiffuseMap;
        private Texture SpecularMap;

        private ArrayBuffer<float> BlockVertexBuffer;
        private VertexArray BlockVertexAtrribs;

        private SpotLight SpotLight;

        private readonly List<Block> Blocks = new List<Block>(BlockCount);
        private readonly Queue<double> FpsMeasures = new Queue<double>(2048);

        public Window()
            : base(new GameWindowSettings(), new NativeWindowSettings { Size = new Vector2i(1920, 1080), Title = "Sample 01" })
        {
            VSync = VSyncMode.Off;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(ClearColor);
            GL.Enable(EnableCap.DepthTest);

            RenderShader = new DefaultShaderProgram();

            DiffuseMap = new Texture(Path.Combine(Utils.TexturesDirectory, "container2.png"));
            SpecularMap = new Texture(Path.Combine(Utils.TexturesDirectory, "container2_specular.png"));

            BlockVertexBuffer = Block.VertexBuffer;
            BlockVertexAtrribs = new VertexArray();

            using (BlockVertexBuffer.Bind())
            {
                using (BlockVertexAtrribs.Bind())
                {
                    var dataType = VertexAttribPointerType.Float;
                    var floatSize = sizeof(float);
                    var stride = 8 * floatSize;

                    BlockVertexAtrribs.AddPointer(RenderShader, "aPos", 3, dataType, stride, 0);
                    BlockVertexAtrribs.AddPointer(RenderShader, "aNormal", 3, dataType, stride, 3 * floatSize);
                    BlockVertexAtrribs.AddPointer(RenderShader, "aTexCoords", 2, dataType, stride, 6 * floatSize);
                }
            }

            RenderShader.Camera.AspectRatio = Size.X / (float)Size.Y;

            RenderShader.Material.Diffuse = null;
            RenderShader.Material.Specular = new Vector3(0.5f, 0.5f, 0.5f);
            RenderShader.Material.Shininess = 32f;

            RenderShader.DirectionalLight.Direction = new Vector3(-0.2f, -1.0f, -0.3f);
            RenderShader.DirectionalLight.Ambient = new Vector3(0.05f, 0.05f, 0.05f);
            RenderShader.DirectionalLight.Diffuse = new Vector3(0.4f, 0.4f, 0.4f);
            RenderShader.DirectionalLight.Specular = new Vector3(0.5f, 0.5f, 0.5f);

            var pointLightPositions = new Vector3[]
            {
                new Vector3(0.7f, 0.2f, 2.0f),
                new Vector3(2.3f, -3.3f, -4.0f),
                new Vector3(-4.0f, 2.0f, -12.0f),
                new Vector3(0.0f, 0.0f, -3.0f)
            };

            for (var i = 0; i < pointLightPositions.Length; i++)
            {
                var light = RenderShader.PointLights[i];
                light.IsEnabled = true;
                light.Position = pointLightPositions[i];
                light.Ambient = Vector3.One * 0.05f;
                light.Diffuse = Vector3.One * 0.8f;
                light.Specular = Vector3.One;
                light.Constant = 1f;
                light.Linear = 0.09f;
                light.Quadratic = 0.032f;
            }

            SpotLight = RenderShader.SpotLights[0];
            SpotLight.IsEnabled = true;
            SpotLight.Ambient = Vector3.Zero;
            SpotLight.Diffuse = Vector3.One;
            SpotLight.Specular = Vector3.One;
            SpotLight.Constant = 1f;
            SpotLight.Linear = 0.09f;
            SpotLight.Quadratic = 0.032f;
            SpotLight.Cutoff = (float)Math.Cos(MathHelper.DegreesToRadians(12.5f));
            SpotLight.OuterCutoff = (float)Math.Cos(MathHelper.DegreesToRadians(30.5f));

            var random = new Random();

            var scaleRangeMin = 80;
            var scaleRangeMax = 150;

            for (var i = 0; i < BlockCount; i++)
            {
                var randomPosition = new Vector3(
                    random.Next(-BlockDistanceRange, BlockDistanceRange) / 10f,
                    random.Next(-BlockDistanceRange, BlockDistanceRange) / 10f,
                    random.Next(-BlockDistanceRange, BlockDistanceRange) / 10f);

                // var randomScale = Vector3.One;
                var randomScale = new Vector3(
                    random.Next(scaleRangeMin, scaleRangeMax) / 100f,
                    random.Next(scaleRangeMin, scaleRangeMax) / 100f,
                    random.Next(scaleRangeMin, scaleRangeMax) / 100f);

                Blocks.Add(new Block { Position = randomPosition, Scale = randomScale });
            }

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            SpotLight.Position = RenderShader.Camera.Position;
            SpotLight.Direction = RenderShader.Camera.Front;

            using (BlockVertexAtrribs.Bind())
            {
                using (DiffuseMap.Bind(TextureTarget.Texture2D, TextureUnit.Texture0))
                {
                    using (SpecularMap.Bind(TextureTarget.Texture2D, TextureUnit.Texture1))
                    {
                        using (RenderShader.Bind())
                        {
                            RenderShader.ApplyCamera();
                            RenderShader.ApplyMaterial();
                            RenderShader.ApplyLights();

                            foreach (var block in Blocks)
                            {
                                //var model = Matrix4.Identity;
                                //model *= Matrix4.CreateTranslation(CubePositions[i]);
                                //var angle = 20.0f * i;
                                // model *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);
                                //RenderShader.Set("model", model);
                                RenderShader.ApplyModel(block.ComputeMatrix());
                                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
                            }
                        }
                    }
                }
            }


            SwapBuffers();

            base.OnRenderFrame(args);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            /*
            if (!IsFocused)
            {
                return;
            }
            */

            var fpsMeasure = 1d / e.Time;
            FpsMeasures.Enqueue(fpsMeasure);
            Title = $"FPS: {fpsMeasure:0.000} | AVG: {FpsMeasures.Average():0.000}";
            if (FpsMeasures.Count > 1000)
                FpsMeasures.Dequeue();

            if (IsKeyDown(Key.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            var camera = RenderShader.Camera;

            if (IsKeyDown(Key.W))
                camera.Position += camera.Front * cameraSpeed * (float)e.Time; // Forward

            if (IsKeyDown(Key.S))
                camera.Position -= camera.Front * cameraSpeed * (float)e.Time; // Backwards

            if (IsKeyDown(Key.A))
                camera.Position -= camera.Right * cameraSpeed * (float)e.Time; // Left

            if (IsKeyDown(Key.D))
                camera.Position += camera.Right * cameraSpeed * (float)e.Time; // Right

            if (IsKeyDown(Key.Space))
                camera.Position += camera.Up * cameraSpeed * (float)e.Time; // Up

            if (IsKeyDown(Key.LShift))
                camera.Position -= camera.Up * cameraSpeed * (float)e.Time; // Down

            camera.Yaw += MouseDelta.X * sensitivity;
            camera.Pitch -= MouseDelta.Y * sensitivity;

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            RenderShader.Camera.Fov -= e.OffsetY;
            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            RenderShader.Camera.AspectRatio = Size.X / (float)Size.Y;
            base.OnResize(e);
        }
    }
}
