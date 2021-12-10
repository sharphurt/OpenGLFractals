using System;
using System.Drawing;

namespace Fractals
{
    public static class ColorUtils
    {
        private static Random random = new Random();
        
        public static Color GenerateRandomColor()
        {
            return Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        }
    }
}