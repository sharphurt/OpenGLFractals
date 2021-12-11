using System;
using OpenTK;

namespace Fractal3D
{
    public class Camera
    {
        public Vector3 Position = new Vector3(1, 1, 1);
        public Vector3 Orientation = new Vector3((float) Math.PI, 0f, 0f); 
        public float MoveSpeed = 0.01f;
        public float MouseSensitivity = 0.0025f;

        public Vector3 LookAt => new Vector3
        {
            X = (float) (Math.Sin(Orientation.X) * Math.Cos(Orientation.Y)),
            Y = (float) Math.Sin(Orientation.Y),
            Z = (float) (Math.Cos(Orientation.X) * Math.Cos(Orientation.Y))
        };

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + LookAt, Vector3.UnitY);
        }

        public void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float) Math.Sin(Orientation.X), 0, (float) Math.Cos(Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void MoveForward(float step)
        {
            var m = LookAt * step;
            Move(m.X, m.Y, m.Z);
        }

        public void AddRotation(float x, float y)
        {
            x *= MouseSensitivity;
            y *= MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float) Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float) Math.PI / 2.0f - 0.1f),
                (float) -Math.PI / 2.0f + 0.1f);
        }
    }
}