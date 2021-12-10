using System.Collections.Generic;
using System.Linq;
using Fractals.Entities;
using Fractals.Entities.ThreeD;
using OpenTK;

namespace Fractals
{
    public class SerpinskyTriangle3D
    {
        private readonly Vector3 Offset;

        public float Scale { get; private set; }

        private static readonly Vector3[] _offsets = {
            new Vector3(-1, -1, -1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1),
            new Vector3(0, 1, 0)
        };

        private Vector3 Position => ParentPosition + Offset * Scale;

        private Vector3 ParentPosition { get; set; }

        private List<SerpinskyTriangle3D> _parts = new List<SerpinskyTriangle3D>();

        public SerpinskyTriangle3D(Vector3 pos, Vector3 offset, float scale)
        {
            Offset = offset;
            Scale = scale;
            ParentPosition = pos;
        }

        public void SplitUp()
        {
            if (!_parts.Any())
            {
                Split();
                return;
            }

            foreach (var part in _parts)
            {
                part.SplitUp();
            }
        }

        private void Split()
        {
            _parts = new List<SerpinskyTriangle3D>();

            foreach (var t in _offsets)
            {
                var triangle = new SerpinskyTriangle3D(Position, t, Scale / 2);
                _parts.Add(triangle);
            }
        }

        public void Render(ref List<List<Vector3>> faces)
        {
            if (!_parts.Any())
            {
                var t = new Pyramid {X = Position.X, Y = Position.Y, Z = Position.Z, Scale = Scale};
                faces.Add(t.Vertices);
                return;
            }

            for (var i = 0; i < _offsets.Length; i++)
            {
                var triangle = _parts[i];
                triangle.Render(ref faces);
            }
        }

        public void SetScale(float scale)
        {
            Scale = scale;
            foreach (var part in _parts)
            {
                part.ParentPosition = Position;
                part.SetScale(scale / 2);
            }
        }
    }
}