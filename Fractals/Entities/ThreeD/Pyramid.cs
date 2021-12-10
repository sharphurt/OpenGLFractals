using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

namespace Fractals.Entities.ThreeD
{
    public class Pyramid : Entity
    {
        public override List<Vector3> Vertices
        {
            get
            {
                var result = new List<Vector3>();
                foreach (var face in _faces)
                {
                    var v1 = _vertices[face[0]];
                    var v2 = _vertices[face[1]];
                    var v3 = _vertices[face[2]];

                    var normal = Vector3.Cross(v2 - v1, v2 - v3);
                    
                    foreach (var index in face)
                    {
                        var v = _vertices[index];
                        result.Add(new Vector3(X + v.X * Scale, Y + v.Y * Scale, Z + v.Z * Scale));  
                        result.Add(normal);
                    }
                }

                return result;
            }
        }

        private int[][] _faces =
        {
            new[] {3, 2, 0},
            new[] {2, 1, 0},
            new[] {1, 4, 0},
            new[] {4, 3, 0},
            new[] {1, 2, 3},
            new[] {3, 4, 1}
        };

        private List<Vector3> _vertices = new List<Vector3>
        {
            /*0*/ new Vector3(0, 1, 0),
            /*1*/ new Vector3(-1, -1, 1),
            /*2*/ new Vector3(1, -1, 1),
            /*3*/ new Vector3(1, -1, -1),
            /*4*/ new Vector3(-1, -1, -1)
        };
    }
}