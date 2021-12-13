using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fractal3D;
using Fractals;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenGLVisualizing
{
    public class Triangle2DWindow : GameWindow
    {
        private readonly SerpinskyTriangle _triangle = new SerpinskyTriangle(Vector3.Zero, Vector3.Zero, 10);

        private readonly Camera _camera = new Camera();
        private Vector2 _lastMousePos;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private Shader _shader;

        private float[] _vertices = new float[0];

        private int[] _firsts, _counts;

        private bool _isUpdated;
        
        private int _counter;

        private string _title
        {
            get
            {
                return _counter != 0 
                   ? $"2D Теругольник Серпинского | Просчитано: {(_counter / (_vertices.Length / 3f)) * 100f}%"
                   : $"2D Теругольник Серпинского | {_vertices.Length / 3} вершин";
            }
        }

        private Task IncreaseFractal => Task.Factory.StartNew(() =>
        {
            lock (_triangle)
            {
                _triangle.SplitUp();
                RenderTriangleVertices();
            }
        });

        public Triangle2DWindow() : base(1280, 720, OpenTK.Graphics.GraphicsMode.Default, "3D Теругольник Серпинского")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _shader = new Shader("Shaders/TwoD/vertexShader.glsl", "Shaders/TwoD/fragmentShader.glsl");

            RenderTriangleVertices();

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
                BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader.Use();

            SetupView();

            MouseDown += ProcessMouse;

            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            CursorVisible = false;
        }

        private void ProcessMouse(object sender, MouseButtonEventArgs args)
        {
            if (args.Button != MouseButton.Left) return;
            Task.WhenAll(IncreaseFractal).ContinueWith((task => _isUpdated = true));
        }

        private (int[] firsts, int[] counts) CalculateBufferLayout()
        {
            var firsts = new int[_vertices.Length / 3];
            for (var i = 0; i < firsts.Length; i++) firsts[i] = i * 3;

            var counts = Enumerable.Repeat(3, _vertices.Length / 3).ToArray();
            return (firsts, counts);
        }

        private void RenderTriangleVertices()
        {
            var triangles = new List<List<Vector3>>();
            _counter = 0;

            _triangle.Render(ref triangles, ref _counter);
            lock (_vertices)
            {
                _vertices = triangles
                    .SelectMany(list => list)
                    .Select(v => new[] {v.X, v.Y, v.Z})
                    .SelectMany(v => v)
                    .ToArray();
            }

            _counter = 0;
            (_firsts, _counts) = CalculateBufferLayout();
        }

        private void SetupView()
        {
            float aspectRatio = Width / (float) Height;
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            Matrix4 perspective =
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.00001f, 10000);

            GL.MultMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetupView();
        }

        private void UpdateVertices()
        {
            GL.BufferData(BufferTarget.ArrayBuffer,
                _vertices.Length * sizeof(float), _vertices,
                BufferUsageHint.StaticDraw);
            _isUpdated = false;
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            ProcessKeyboard();

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            var viewMatrix = _camera.GetViewMatrix();
            GL.LoadMatrix(ref viewMatrix);

            _shader.Use();

            var perspective =
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float) Width / Height, 0.1f, 100);
            var model = Matrix4.Identity;
            var view = _camera.GetViewMatrix();

            _shader.SetMat4("model", model);
            _shader.SetMat4("view", view);
            _shader.SetMat4("projection", perspective);

            _shader.SetVec3("objectColor", new Vector3(0.5f, 0.1f, 1));

            if (_isUpdated)
                UpdateVertices();

            GL.BindVertexArray(_vertexArrayObject);
            GL.MultiDrawArrays(PrimitiveType.Triangles, _firsts, _counts, _vertices.Length / 9);

            SwapBuffers();

            Title = _title;
        }

        private void ProcessKeyboard()
        {
            if (Keyboard.GetState().IsKeyDown(Key.W)) _camera.Move(0f, 1, 0f);

            if (Keyboard.GetState().IsKeyDown(Key.S)) _camera.Move(0f, -1, 0f);

            if (Keyboard.GetState().IsKeyDown(Key.A)) _camera.Move(-1, 0f, 0f);

            if (Keyboard.GetState().IsKeyDown(Key.D)) _camera.Move(1, 0f, 0f);

            if (Keyboard.GetState().IsKeyDown(Key.Q)) _camera.Move(0f, 0f, 1);

            if (Keyboard.GetState().IsKeyDown(Key.E)) _camera.Move(0f, 0f, -1);

            if (Keyboard.GetState().IsKeyDown(Key.Escape)) Exit();

            _camera.MoveSpeed = Keyboard.GetState().IsKeyDown(Key.ShiftLeft) ? 0.05f : 0.005f;

            if (!Focused)
                return;

            Vector2 delta = _lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _lastMousePos += delta;

            _camera.AddRotation(delta.X, delta.Y);
            if (Focused)
                Mouse.SetPosition(Width / 2f, Height / 2f);
            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            _shader.Dispose();
            base.OnUnload(e);
        }
    }
}