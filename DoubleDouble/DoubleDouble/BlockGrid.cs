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
    public class BlockGrid
    {
        public static Block[,] bAry;
        public static List<Block> bList;

        public static Point dims, ul;
        public static Rectangle bounds;
        public static int bSize = 50;

        public static Point cLoc = new Point(3, 6);

        static Random rand;

        public BlockGrid(Point d, Point o)
        {
            rand = new Random();

            ul = o;
            dims = d;
            bounds = new Rectangle(0, 0, dims.X, dims.Y);

            bList = new List<Block>();

            for (int x = 0; x < dims.X; x++ )
            {
                for (int y = 0; y < 4; y++)
                {
                    bList.Add(new Block((BlockType)rand.Next(4), x, y));
                }
            }

            MakeArray();
        }

        public void MakeArray()
        {
            bAry = new Block[dims.X, dims.Y];

            foreach (Block b in bList)
            {
                bAry[b.gX, b.gY] = b;
            }
        }

        public void Update(GameTime gameTime)
        {
            //update blocks
            foreach (Block b in bList)
            {
                b.Update(gameTime);
            }
        }

        public void MoveCursor(int ox, int oy)
        {
            int mx = cLoc.X + ox;
            int my = cLoc.Y + oy;

            if (mx >= 0 && mx <= dims.X - 2 && my >= 1 && my <= dims.Y - 1)
            {
                cLoc = new Point(mx, my);

                if (Game.SOUNDFX) Game.sfx["bump"].Play(Game.sfxVolume, 0, 0);
            }


        }

        public bool AddRow()
        {
            bool success = true;
            //check to make sure top row is clear
            for (int x = 0; x < dims.X; x++)
            {
                if (bAry[x, dims.Y - 1] == null)
                {
                    bList.Add(new Block((BlockType)rand.Next(4), x, dims.Y - 1));
                }
                else
                {
                    success = false;
                }
            }

            MakeArray();

            DropBlocks();

            return success;
        }

        public void StirBlocks(bool cw = true)
        {
            List<Block> stirlist = new List<Block>();

            for (int x = 0; x <= 1; x++) for (int y = 0; y >= -1; y--) if (bAry[cLoc.X + x, cLoc.Y + y] != null) stirlist.Add(bAry[cLoc.X + x, cLoc.Y + y]);
            //wait... what was i going to do with this? nice long line, at least...

            if (stirlist.Count > 0)
            {

                if (cw)
                {
                    MoveBlock(cLoc.X, cLoc.Y, 1, 0);
                    MoveBlock(cLoc.X, cLoc.Y - 1, 0, 1);
                    MoveBlock(cLoc.X + 1, cLoc.Y - 1, -1, 0);
                    MoveBlock(cLoc.X + 1, cLoc.Y, 0, -1);
                }
                else
                {
                    MoveBlock(cLoc.X, cLoc.Y, 0, -1);
                    MoveBlock(cLoc.X, cLoc.Y - 1, 1, 0);
                    MoveBlock(cLoc.X + 1, cLoc.Y - 1, 0, 1);
                    MoveBlock(cLoc.X + 1, cLoc.Y, -1, 0);
                }

                MakeArray();

                if (stirlist.Count < 4)
                {
                    DropBlocks();
                }

                if (Game.SOUNDFX) Game.sfx["stir"].Play(Game.sfxVolume, 0, 0);
            }
        }

        public void MoveBlock(int tx, int ty, int ox, int oy)
        {
            if (bAry[tx, ty] != null)
            {
                bAry[tx, ty].Shift(ox, oy);
            }
        }

        public void ClearBlock(int tx, int ty)
        {
            if (bAry[tx, ty] != null)
            {
                Game.anims.Add(new Animation(Pos2Vector(tx, ty + 1), "pop"));
                bList.Remove(bAry[tx, ty]);
                bAry[tx, ty] = null;
            }
        }

        public int ClearType(BlockType ct)
        {
            //build all candidate rectangles
            List<List<Rectangle>> cands = new List<List<Rectangle>>();
            cands.Add(GetSingles(ct));

            string debug = "";
            for (int i = 1; i < 6; i++)
            {
                cands.Add(GetDoubles(cands[i - 1]));
                debug += " "+cands[i].Count.ToString();
            }

            //Console.WriteLine("Color: {0}, Matches: {1}", ct, debug);

            //cull overlapping
            List<Rectangle> toclear = new List<Rectangle>();
            for (int i = 5; i > 1; i--)
            {
                foreach (Rectangle r in cands[i])
                {
                    bool overlap = false;
                    foreach (Rectangle cr in toclear)
                    {
                        if (cr.Intersects(r))
                        {
                            overlap = true;
                        }
                    }

                    if (!overlap) toclear.Add(r);
                }
            }

            //Console.WriteLine("Clearing {0} areas...", toclear.Count);

            int score = 0;
            foreach (Rectangle r in toclear)
            {
                List<Block> rcont = bList.Where(b => r.Contains(b.PointLoc())).ToList();

                int points = rcont.Count * rcont.Count * 10;
                score += points;
                String ctext = points.ToString();
                Game.labels.Add(new Label(Pos2Vector(r.X + (r.Width / 2f), r.Y + (r.Height / 2f)), ctext, 30, "number"));

                foreach (Block b in rcont) ClearBlock(b.gX, b.gY);
            }

            MakeArray();

            DropBlocks();

            if (score > 0)
            {
                if (Game.SOUNDFX) Game.sfx["clear"].Play(Game.sfxVolume, 0, 0);
            }

            //TODO: return points cleared
            return score;
        }

        public List<Rectangle> GetSingles(BlockType ct)
        {
            List<Block> matched = bList.Where(b => b.type == ct).ToList();
            List<Rectangle> rlist = new List<Rectangle>();

            foreach (Block b in matched)
            {
                rlist.Add(new Rectangle(b.gX, b.gY, 1, 1));
            }

            return rlist;
        }

        public List<Rectangle> GetDoubles(List<Rectangle> cands)
        {
            List<Rectangle> rlist = new List<Rectangle>();

            foreach (Rectangle r in cands)
            {
                //check above
                Rectangle tr = new Rectangle(r.X, r.Y - r.Height, r.Width, r.Height);
                if (cands.Contains(tr))
                {
                    //Console.WriteLine("Found one above!");
                    Rectangle r4c = new Rectangle(r.X, r.Y - r.Height, r.Width, r.Height * 2);
                    if (!rlist.Contains(r4c)) rlist.Add(r4c);
                    //else Console.WriteLine("Didn't add duplicate r4");
                }

                //check to the right
                tr = new Rectangle(r.X + r.Width, r.Y, r.Width, r.Height);
                if (cands.Contains(tr))
                {
                    //Console.WriteLine("Found one to the right!");
                    Rectangle r4c = new Rectangle(r.X, r.Y, r.Width * 2, r.Height);
                    if (!rlist.Contains(r4c)) rlist.Add(r4c);
                    //else Console.WriteLine("Didn't add duplicate r4");
                }
            }

            return rlist;
        }

        public void DropBlocks()
        {
            for (int y = 0; y < dims.Y; y++) 
            {
                for (int x = 0; x < dims.X; x++)
                {
                    if (bAry[x, y] == null)
                    {
                        //check up the column
                        int dropht = 0;
                        for (int ty = y + 1; ty < dims.Y; ty++)
                        {
                            if (bAry[x, ty] != null)
                            {
                                dropht = ty - y;
                                break;
                            }
                        }

                        //drop em like it's hot
                        if (dropht > 0)
                        {
                            List<Block> dlist = bList.Where(b => b.gX == x && b.gY > y).ToList();

                            foreach (Block b in dlist)
                            {
                                MoveBlock(b.gX, b.gY, 0, -dropht);
                            }

                            MakeArray();
                        }
                    }
                }
            }
        }

        public void DrawBackground()
        {
            Game.sb.Draw(Game.tex["blank"], new Rectangle(ul.X - 10, ul.Y - 10, bSize * dims.X + 20, bSize * dims.Y + 20), Color.Black * 0.7f);
        }

        public void DrawBlocks()
        {
            foreach (Block b in bList)
            {
                Point op = b.GetOffset().toPoint();

                Rectangle drect = new Rectangle(ul.X + op.X + (bSize * b.gX), ul.Y + op.Y + (bSize * (dims.Y - 1 - b.gY)), bSize, bSize);

                //Game.sb.Draw(Game.tex["block"], drect, b.GetColor());
                Game.sb.Draw(Game.tex[Game.blockTex[(int)b.type]], drect, Color.White);
            }
        }

        public void DrawCursor()
        {
            Rectangle drect = new Rectangle(ul.X + (bSize * cLoc.X), ul.Y + (bSize * (dims.Y - 1 - cLoc.Y)), bSize * 2, bSize * 2);

            Game.sb.Draw(Game.tex["cursor"], drect, Color.White);
        }

        public static Vector2 Pos2Vector(float px, float py)
        {
            return new Vector2(bSize * px + ul.X, bSize * (dims.Y - py) + ul.Y);
        }
    }
}
