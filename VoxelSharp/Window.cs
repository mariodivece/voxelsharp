namespace VoxelSharp
{
    using OpenToolkit.Graphics.OpenGL4;
    using OpenToolkit.Mathematics;
    using OpenToolkit.Windowing.Common;
    using OpenToolkit.Windowing.Common.Input;
    using OpenToolkit.Windowing.Desktop;
    using System.Collections.Generic;
    using System.Linq;

    public class Window : GameWindow
    {
        private static readonly Color4 ClearColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f);
        private readonly Queue<double> FpsMeasures = new Queue<double>(2048);

        private BlockScene Scene;

        public Window()
            : base(new GameWindowSettings(), new NativeWindowSettings { Size = new Vector2i(1920, 1080), Title = "Sample 01" })
        {
            VSync = VSyncMode.Off;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(ClearColor);
            GL.Enable(EnableCap.DepthTest);
            Scene = new BlockScene();
            Scene.Camera.AspectRatio = Size.X / (float)Size.Y;

            base.OnLoad();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var spotLight = Scene.SpotLights[0];
            spotLight.Position = Scene.Camera.Position;
            spotLight.Direction = Scene.Camera.Front;
            Scene.Render();
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

            var camera = Scene.Camera;

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
            Scene.Camera.Fov -= e.OffsetY;
            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            Scene.Camera.AspectRatio = Size.X / (float)Size.Y;
            base.OnResize(e);
        }
    }
}
