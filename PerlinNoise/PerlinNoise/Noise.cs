using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;


namespace PerlinNoise
{   
    public static class Noise2d
    {
    
        public static double Noise(double x, double y, int[] permutationTable, Vector2[] gradients)
        {
            Vector2 inputVector = new Vector2(Math.Floor(x),Math.Floor(y));
            
            Vector2[] corners = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
            double total = 0;
            foreach(var currentCorner in corners)
            {
                Vector2 ij = inputVector + currentCorner;
                Vector2 uv = new Vector2(x - ij.x, y - ij.y);
                int index = permutationTable[(int)ij.x % permutationTable.Length];
                index = permutationTable[(index + (int)ij.y) % permutationTable.Length];
                Vector2 gradient = gradients[index];
                total += Q(uv.x, uv.y) * Vector2.Dot(gradient, uv);
            }
            return total;
        }

        private static double Q(double x, double y)
        {
            return Fade(x) * Fade(y);
        }

        private static double Fade(double x)
        {
            x = Math.Abs(x);
            return (double)1 - (x * x * x * (x * (x * 6 - 15) + 10));
        }

    }
}
