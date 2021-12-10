using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace Fractals.Entities
{
    public abstract class Entity
    {
        public virtual List<Vector3> Vertices => new List<Vector3>();
        
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Scale { get; set; } = 1;

        public float Rotation { get; set; }
    }
}