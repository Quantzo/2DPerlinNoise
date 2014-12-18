using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PerlinNoise
{
    public class Vector2
    {
        public double x {get; private set; }
        public double y {get; private set; }

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(vec1.x + vec2.x, vec1.y + vec2.y);
        }

        public static double Dot(Vector2 vec1, Vector2 vec2)
        {
            return ((vec1.x * vec2.x) + (vec1.y * vec2.y));
        }

        public static Vector2 operator -(Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(vec1.x - vec2.x, vec1.y - vec2.y);
        }

        public double LengthSquared()
        {
            return (double)Math.Sqrt((double)Math.Sqrt((this.x * this.x) + (this.y * this.y)));
        }

        public void Normalize()
        {
            double len = (double)Math.Sqrt((this.x * this.x) + (this.y * this.y));
            this.x = this.x / len;
            this.y = this.y / len;
        }
    }
}
