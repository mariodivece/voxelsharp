namespace VoxelSharp
{
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Mathematics;
    using OpenTK.Windowing.Common;
    using OpenTK.Windowing.Desktop;
    using OpenTK.Windowing.GraphicsLibraryFramework;
    using System.Collections.Generic;
    using System.Linq;

    public class Window : GameWindow
    {
        private static readonly Color4 ClearColor = new(0.2f, 0.3f, 0.3f, 1.0f);
        private readonly Queue<double> FpsMeasures = new(2048);

        private BlockScene Scene;

        public Window()
            : base(new GameWindowSettings { IsMultiThreaded = false }, new NativeWindowSettings { Size = new Vector2i(1920, 1080), Title = "Sample 01" })
        {
            VSync = VSyncMode.Off; // VSyncMode.Adaptive;
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

            if (IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            var camera = Scene.Camera;
            
            if (IsKeyDown(Keys.W))
                camera.Position += camera.Front * cameraSpeed * (float)e.Time; // Forward

            if (IsKeyDown(Keys.S))
                camera.Position -= camera.Front * cameraSpeed * (float)e.Time; // Backwards

            if (IsKeyDown(Keys.A))
                camera.Position -= camera.Right * cameraSpeed * (float)e.Time; // Left

            if (IsKeyDown(Keys.D))
                camera.Position += camera.Right * cameraSpeed * (float)e.Time; // Right

            if (IsKeyDown(Keys.Space))
                camera.Position += camera.Up * cameraSpeed * (float)e.Time; // Up

            if (IsKeyDown(Keys.LeftShift))
                camera.Position -= camera.Up * cameraSpeed * (float)e.Time; // Down

            if (IsAnyMouseButtonDown)
            {
                camera.Yaw += MouseState.Delta.X * sensitivity;
                camera.Pitch -= MouseState.Delta.Y * sensitivity;
            }

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
