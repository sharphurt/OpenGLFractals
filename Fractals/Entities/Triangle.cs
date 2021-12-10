using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Fractals.Entities
{
    public class Triangle : Entity
    {
        public override List<Vector3> Vertices
        {
            get
            {
                return new List<Vector3>
                {
                    new Vector3(X - 1 * Scale, Y - 1 * Scale, Z),
                    new Vector3(X + 1 * Scale, Y - 1 * Scale, Z),
                    new Vector3(X, Y + 1 * Scale, Z)
                };
            }
        }
    }
}