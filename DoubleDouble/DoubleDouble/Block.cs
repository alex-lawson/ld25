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
    public class Block
    {
        public BlockType type;

        List<Point> tq;
        public Vector2 offset;
        float tFrames = 0;
        float tFrame = 0;
        int tDur = 5;

        public int gX, gY;

        public Block(BlockType bt, int gridx, int gridy)
        {
            type = bt;

            gX = gridx;
            gY = gridy;

            offset = new Vector2();

            tq = new List<Point>();
        }

        public void Update(GameTime gameTime)
        {
            //update transition(s)
            if (tq.Count > 0)
            {
                tFrame++;

                //finish current transition
                if (tFrame >= tFrames)
                {
                    tFrame = 0;
                    tq.RemoveAt(0);

                    //start next transition
                    if (tq.Count > 0)
                    {
                        tFrames = tDur * (Math.Abs(tq[0].X) + Math.Abs(tq[0].Y));
                    }
                }
            }
        }

        public Point PointLoc()
        {
            return new Point(gX, gY);
        }

        public Color GetColor()
        {
            return Game.blockColor[(int)(type)];
        }

        public void Trans(int ox, int oy)
        {
            if (tq.Count == 0)
            {
                tq.Add(new Point(-ox, oy));
                tFrames = tDur * (Math.Abs(ox) + Math.Abs(oy));
            }
            else
            {
                if ((ox != 0 && tq[0].X != 0) || (oy != 0 && tq[0].Y != 0))
                {
                    //combine
                    tq[0] = new Point(tq[0].X + ox, tq[0].Y + oy);

                    //check for cancellation
                    if (Math.Abs(tq[0].X) + Math.Abs(tq[0].Y) == 0)
                    {
                        tq.RemoveAt(0);
                    }
                    else
                    {
                        //calculate new frame limit
                        tFrames += tDur * (Math.Abs(ox) + Math.Abs(oy));
                    }
                }
                else
                {
                    tq.Add(new Point(-ox, oy));
                }
            }
        }

        public void Shift(int ox, int oy)
        {
            if (BlockGrid.bounds.Contains(new Point(gX + ox, gY + oy)))
            {
                Trans(ox, oy);
                gX += ox;
                gY += oy;
            }
        }

        public Vector2 GetOffset()
        {
            if (tq.Count > 0)
            {
                float prog = tFrame / (2 * tFrames);
                Vector2 baseoff = new Vector2(tq[0].X * BlockGrid.bSize, tq[0].Y * BlockGrid.bSize);

                Vector2 basepos = new Vector2(0, 0);
                //calculate cumulative offsets
                if (tq.Count > 1)
                {
                    for (int i = 1; i < tq.Count; i++)
                    {
                        basepos.X += tq[i].X * BlockGrid.bSize;
                        basepos.Y += tq[i].Y * BlockGrid.bSize;
                    }
                }
                return Vector2.SmoothStep(basepos + baseoff, basepos - baseoff, prog);
            }
            else return new Vector2();
        }
    }
}
