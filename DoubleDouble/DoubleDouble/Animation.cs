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
    public class Animation
    {
        String texname;
        Vector2 pos;

        int szSrc = 10;
        int szDisp = 50;

        int frame = 0;
        int frames = 5;
        TimeSpan fTime = TimeSpan.FromSeconds(0.06);
        TimeSpan cfTime = new TimeSpan();

        public bool killMe = false;
        bool repeat = false;

        public Animation(Vector2 v, String tname)
        {
            pos = v;
            texname = tname;
        }

        public void Update(GameTime gameTime)
        {
            cfTime += gameTime.ElapsedGameTime;
            if (cfTime > fTime)
            {
                cfTime -= fTime;

                frame++;
                if (frame >= frames)
                {
                    if (repeat) frame = 0;
                    else killMe = true;
                }
            }
        }

        public void Draw(Point offs = new Point())
        {
            if (!killMe)
            {
                Rectangle cutter = new Rectangle(szSrc * frame, 0, szSrc, szSrc);
                Rectangle drect = new Rectangle((int)pos.X + offs.X, (int)pos.Y + offs.Y, szDisp, szDisp);
                Game.sb.Draw(Game.tex[texname], drect, cutter, new Color(1, 1, 1, 0.2f));
            }
        }
    }
}
