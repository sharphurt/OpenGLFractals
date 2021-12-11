using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fractals;
using Fractals.Entities.ThreeD;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Fractal3D
{
    public class Form1 : GameWindow
    {
        private SerpinskyTriangle3D _triangle = new SerpinskyTriangle3D(Vector3.Zero, Vector3.Zero, 1);

        //     private SerpinskyTriangle _triangle = new SerpinskyTriangle(Vector3.Zero, Vector3.Zero, 1);

        private bool _isPyramidEnabled, _isTriangleEnabled;

        private Camera _camera = new Camera();
        private Vector2 _lastMousePos;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private Shader _shader;

        private Vector3 _lightPosition = new Vector3(1.2f, 10.0f, 2.0f);

        private float[] _vertices;

        private int[] _firsts, _counts;

        private bool isUpdated;

        private Task IncreaseFractal => Task.Factory.StartNew(() =>
        {
            _triangle.SplitUp();
            RenderTriangle();
        });

        public Form1() : base(1280, 720, OpenTK.Graphics.GraphicsMode.Default, "Хороший вопрос")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _shader = new Shader("Shaders/vertShader.glsl", "Shaders/fragShader.glsl");

            RenderTriangle();

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
                BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);


            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shader.Use();

            SetupPerspective();
            //SetupIsometric();
            MouseDown += ProcessMouse;

            KeyDown += (sender, args) =>
            {
                switch (args.Key)
                {
                    case Key.F1:
                        _isPyramidEnabled = !_isPyramidEnabled;
                        break;
                    case Key.F2:
                        _isTriangleEnabled = !_isTriangleEnabled;
                        break;
                }
            };

            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            CursorVisible = false;
        }

        private (int[] firsts, int[] counts) CalculateFirstsCounts()
        {
            var firsts = new int[_vertices.Length / 18];
            for (var i = 0; i < firsts.Length; i++) firsts[i] = i * 9;

            var counts = Enumerable.Repeat(18, _vertices.Length / 18).ToArray();
            return (firsts, counts);
        }

        private void RenderTriangle()
        {
            var triangles = new List<List<Vector3>>();

            _triangle.Render(ref triangles);

            _vertices = triangles
                .SelectMany(list => list)
                .Select(v => new[] {v.X, v.Y, v.Z})
                .SelectMany(v => v)
                .ToArray();

            (_firsts, _counts) = CalculateFirstsCounts();
        }

        private void SetupIsometric()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();

            GL.LoadIdentity();

            const int _scale = -20;
            GL.Ortho(_scale, -_scale, _scale * 0.7, -_scale * 0.7, _scale, 100);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();

            GL.LoadIdentity();
        }

        private void SetupPerspective()
        {
            float aspectRatio = Width / (float) Height;
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            Matrix4 perspective =
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, 100);

            GL.MultMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetupPerspective();
        }

        private void ProcessInput()
        {
            const float Sensivity = 0.0001f;

            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                _camera.Move(0f, 1 * Sensivity, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                _camera.Move(0f, -1 * Sensivity, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                _camera.Move(-1 * Sensivity, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                _camera.Move(1 * Sensivity, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                _camera.Move(0f, 0f, 1 * Sensivity);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                _camera.Move(0f, 0f, -1 * Sensivity);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }

            _camera.MoveSpeed = Keyboard.GetState().IsKeyDown(Key.ShiftLeft) ? 0.05f : 0.1f;

            if (Focused)
            {
                Vector2 delta = _lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                _lastMousePos += delta;

                _camera.AddRotation(delta.X, delta.Y);
                if (Focused)
                    Mouse.SetPosition(Width / 2f, Height / 2f);
                _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
        }

        private void ProcessMouse(object sender, MouseButtonEventArgs args)
        {
            if (args.Button != MouseButton.Left) return;

            if (Keyboard.GetState()[Key.LControl])
            {
                Task.WhenAll(IncreaseFractal).ContinueWith((task => isUpdated = true));
            }

            /*else if (_isPyramidEnabled)
                _pyramid.SplitUp();*/
        }

        private void UpdateVertices()
        {
            GL.BufferData(BufferTarget.ArrayBuffer,
                _vertices.Length * sizeof(float), _vertices,
                BufferUsageHint.StaticDraw);
            isUpdated = false;
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            ProcessInput();

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

            _shader.SetVec3("lightPos", _lightPosition);
            _shader.SetVec3("lightColor", new Vector3(1, 1, 1));
            _shader.SetVec3("objectColor", new Vector3(1, 1, 0.31f));

            if (isUpdated)
                UpdateVertices();
            GL.BindVertexArray(_vertexArrayObject);
            GL.MultiDrawArrays(PrimitiveType.Triangles, _firsts, _counts, _vertices.Length / 18);
            
            // _triangle.Render();

            SwapBuffers();
        }

        private void DrawAxes()
        {
            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(-300.0f, 0.0f, 0.0f);
            GL.Vertex3(300.0f, 0.0f, 0.0f);
            GL.End();

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0.0f, -300.0f, 0.0f);
            GL.Vertex3(0.0f, 300.0f, 0.0f);
            GL.End();

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.DodgerBlue);
            GL.Vertex3(0.0f, 0.0f, -300f);
            GL.Vertex3(0.0f, 0.0f, 300.0f);
            GL.End();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            base.OnUnload(e);
        }
    }
}

/* ПРОЧЕЕ
 //примеры прочих видов
 Matrix4 Vid(String pos)
        {
            switch (pos)
            {
                case "1": //X (вид сбоку сверху)": 
                  return Matrix4.LookAt(new Vector3(300, 230, 0), new Vector3(0, 50, 0),new Vector3(0, 1, 0));
                case "2": //Y (вид сверху)": 
                  return Matrix4.LookAt(new Vector3(0, 300, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0));
                case "3": //Z (вид сбоку прямо)": 
                  return Matrix4.LookAt(new Vector3(0, 150, 400), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                case "4": //-X (вид сбоку прямо)": 
                  return Matrix4.LookAt(new Vector3(-300, 300, 200), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                case "5": //-Y (вид снизу)": 
                  return Matrix4.LookAt(new Vector3(0, -100, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0));
                case "6": //-Z (вид снизу сбоку)": 
                  return Matrix4.LookAt(new Vector3(0, 100, -400), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            default: 
                return Matrix4.LookAt(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            }
        }

    //Выбор вида через comboBox
    Matrix4 modelview = Vid(comboBox2.SelectedIndex.ToString());
*/