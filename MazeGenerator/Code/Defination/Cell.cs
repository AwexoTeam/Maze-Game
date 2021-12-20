using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Cell
{
    public static float wallSize = 10;
    public static float baseOffset = 100;
    public Room room;
    public CellSide[] sides;
    public Point indexCoords;
    public string roomID;
    public RoomType type;
    public RoomType originalType;
    public Direction dir;
    public bool isNeighbour;

    public Cell(Point indexCoords)
    {
        this.indexCoords = indexCoords;

        sides = new CellSide[4];
        sides[0] = new CellSide(this, Direction.North);
        sides[1] = new CellSide(this, Direction.East);
        sides[2] = new CellSide(this, Direction.South);
        sides[3] = new CellSide(this, Direction.West);
    }

    public void Draw(Graphics g)
    {
        GameState state = GameMaster.maker.state;

        Array.ForEach(sides, s => s.CalculateSides(this));
        
        SolidBrush b = new SolidBrush(Color.Black);
        
        Color c = Color.Black;
        bool isColorFloorOn = MazeMakerDebugger.HasFeature(DebugFeatures.ColoredFlooring);
        if (indexCoords != Player.position && isColorFloorOn)
        {
            switch (type)
            {
                case RoomType.Corner:
                    b.Color = Color.Aqua;
                    break;
                case RoomType.Corridor:
                    b.Color = Color.Green;
                    break;
                case RoomType.Nexus:
                    b.Color = Color.Magenta;
                    break;
                case RoomType.Dead_End:
                    b.Color = Color.Yellow;
                    break;
                case RoomType.Room:
                    b.Color = Color.Brown;
                    break;
                case RoomType.Debug:
                    b.Color = Color.Crimson;
                    break;

                default:
                    b.Color = Color.Black;
                    break;
            }
        }

        bool isDisableFOWOn = MazeMakerDebugger.HasFeature(DebugFeatures.DisableFOW);
        if (!isColorFloorOn && isDisableFOWOn)
            b.Color = Color.White;

        if (isColorFloorOn || isDisableFOWOn)
            b.Color = Color.FromArgb(150, b.Color);

        PointF a = sides[0].a;
        float x = a.X;
        float y = a.Y - Cell.wallSize;


        isNeighbour = UpdateIfPlayerNeighbour(g, ref b, ref c);

        RectangleF floorRect = new RectangleF(x, y, Cell.wallSize, Cell.wallSize);

        bool isAnimationOn = MazeMakerDebugger.HasFeature(DebugFeatures.Animation);
        if (this == MazeMaker.lastCell && isAnimationOn && state == GameState.Generating)
            b.Color = Color.Red;
        if (MazeMaker.drawnN.Count > 0 && MazeMaker.drawnN.Contains(this) && isAnimationOn)
            b.Color = Color.Aqua;

        g.FillRectangle(b, floorRect);

        if (Player.position == indexCoords && state == GameState.Playing)
        {
            float size = Cell.wallSize / 2;
            b.Color = Color.Blue;
            RectangleF playerRect = new RectangleF(x+(size/2), y+(size/2), size, size);

            g.FillRectangle(b, playerRect);

        }

        bool doesCellHaveKey = GameMaster.maker.keys.Exists(k => k.indexCoords == indexCoords);
        bool canSeeKey = isDisableFOWOn || isNeighbour;

        if (doesCellHaveKey && canSeeKey)
        {
            Color oldC = b.Color;

            b.Color = Color.Blue;
            float cX = Cell.wallSize * (indexCoords.X + 1);
            float cY = Cell.wallSize * (indexCoords.Y + 1) - (Cell.wallSize / 2);

            float size = Cell.wallSize / 3;

            g.FillEllipse(b, cX, cY, size, size);
            b.Color = oldC;
        }

        Array.ForEach(sides, s => s.Draw(g, indexCoords, c));
    }

    private bool UpdateIfPlayerNeighbour(Graphics g, ref SolidBrush b, ref Color c)
    {
        Color floor = Color.Orange;
        Color brush = Color.White;

        Point playerPos = Player.position;
        Cell playerCell = MazeMakerHelper.GetCellByCoordinate(GameMaster.maze, playerPos);

        if(indexCoords == playerPos)
        {
            b.Color = brush;
            c = floor;
            return true;
        }

        Point nA = playerPos;
        nA.Y++;
        if(indexCoords == nA && playerCell.sides[0].state == SideType.Opening)
        {
            b.Color = brush;
            c = floor;
            return true;
        }

        Point nB = playerPos;
        nB.Y--;
        if (indexCoords == nB && playerCell.sides[2].state == SideType.Opening)
        {
            b.Color = brush;
            c = floor;
            return true;
        }

        Point nC = playerPos;
        nC.X++;
        if (indexCoords == nC && playerCell.sides[1].state == SideType.Opening)
        {
            b.Color = brush;
            c = floor;
            return true;
        }

        Point nD = playerPos;
        nD.X--;
        if (indexCoords == nD && playerCell.sides[3].state == SideType.Opening)
        {
            b.Color = brush;
            c = floor;
            return true;
        }

        return false;
    }
}