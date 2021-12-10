﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
 using Fractals;
 using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace PureGLFractal
{
    class Game : GameWindow
    {
        private SerpinskyTriangle _triangle = new SerpinskyTriangle(Vector3.One, Vector3.One, 1);
        public Game() : base(1280, 720, OpenTK.Graphics.GraphicsMode.Default, "Хороший вопрос")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0, 0, 0, 1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);
            
            Mouse.ButtonDown += (sender, args) =>
            {
                if (args.Button == MouseButton.Left)
                    _triangle.SplitUp();
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            
            var projection =
                Matrix4.CreatePerspectiveFieldOfView((float) Math.PI / 4, Width / (float) Height, 1.0f, 64.0f);
        
            GL.LoadMatrix(ref projection);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (Keyboard[Key.Escape])
                Exit();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var viewModel = Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref viewModel);
            

            _triangle.Render();
            
            GL.PushMatrix();
            GL.Rotate(1f, new Vector3(0, 1, 0));
            GL.PopMatrix();
            
            SwapBuffers();
        }
    }
}