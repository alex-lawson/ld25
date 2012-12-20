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
    public class Label
    {
        int dur;
        int frame = 0;
        public bool killMe = false;

        String text;

        Vector2 pos;

        String font;

        public Label(Vector2 p, String s, int d = -1, String f = "label")
        {
            text = s;
            pos = p;

            dur = d;

            font = f;

            pos.X -= Game.font[font].MeasureString(text).X / 2;
            pos.Y -= Game.font[font].MeasureString(text).Y / 2;
        }

        public void SetText(String newtext)
        {
            pos.X += Game.font[font].MeasureString(text).X / 2;
            pos.Y += Game.font[font].MeasureString(text).Y / 2;

            text = newtext;

            pos.X -= Game.font[font].MeasureString(text).X / 2;
            pos.Y -= Game.font[font].MeasureString(text).Y / 2;
        }

        public void Update(GameTime gameTime)
        {
            if (dur != -1)
            {
                frame++;
                if (frame >= dur) killMe = true;
            }
        }

        public void Draw()
        {
            Draw(Color.White);
        }

        public void Draw(Color c)
        {
            Game.sb.DrawString(Game.font[font], text, pos, c);
        }
    }
}
