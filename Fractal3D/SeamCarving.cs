using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Fractal3D
{
    public class SeamCarving : GameWindow
    {
        private Camera _camera = new Camera();
        private Vector2 _lastMousePos;

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        private Shader _shader;
        private Texture _texture;

        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
            0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };


        public SeamCarving() : base(1280, 720, OpenTK.Graphics.GraphicsMode.Default, "Хороший вопрос")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            
            _shader = new Shader("Shaders/seamCarvingVert.glsl", "Shaders/seamCarvingFrag.glsl");
            _texture = Texture.LoadFromFile("Assets/img.jpg");
            
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shader.Use();
            _texture.Use(TextureUnit.Texture0);

            SetupPerspective();

            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            CursorVisible = false;
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
            
            var perspective =
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float) Width / Height, 0.1f, 100);
            var model = Matrix4.Identity;
            var view = _camera.GetViewMatrix();

            _shader.Use();
            _texture.Use(TextureUnit.Texture0);
            
            _shader.SetMat4("model", model);
            _shader.SetMat4("view", view);
            _shader.SetMat4("projection", perspective);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

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