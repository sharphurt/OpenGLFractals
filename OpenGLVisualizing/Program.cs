using System;
using Fractal3D;

namespace OpenGLVisualizing
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //new Triangle2DWindow().Run();
            new Triangle3DWindow().Run();
        }
    }
}