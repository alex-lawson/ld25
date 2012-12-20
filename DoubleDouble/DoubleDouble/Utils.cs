using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DoubleDouble
{
    public static class Utils
    {
        static Utils()
        {

        }

        public static Point toPoint(this Vector2 v)
        {
            return new Point((int)v.X, (int)v.Y);
        }

        public static Vector2 toVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}
