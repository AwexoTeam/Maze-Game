using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CellSide
{
    public static float openingSize = 10;
    public Direction dir;
    public SideType state;
    public Cell owner;

    public string roomID;

    public bool isWall { get { return (state != SideType.Wall); } }

    public PointF a = new PointF();
    private PointF b = new PointF();
    private PointF c = new PointF();
    public PointF d = new PointF();

    public CellSide(Cell owner, Direction dir)
    {
        this.owner = owner;
        this.dir = dir;
    }

    public void Draw(Graphics g, Point indexCoords, Color color)
    {
        Pen pen = new Pen(color, 3);
        bool shoulDrawDoor = owner.isNeighbour || MazeMakerDebugger.HasFeature(DebugFeatures.DisableFOW);
        if (state == SideType.Door && shoulDrawDoor)
            pen.Color = Color.Green;

        g.DrawLine(pen, a, b);
        g.DrawLine(pen, c, d);

        if (state == SideType.Opening)
            return;

        Color newColor = color;

        if (state == SideType.Door && shoulDrawDoor)
            newColor = Color.LightGreen;

        pen.Color = newColor;
        g.DrawLine(pen, b, c);
    }

    public void CalculateSides(Cell owner)
    {
        PointF indexCoords = owner.indexCoords;
        float halfSize = Cell.wallSize / 2;

        float divider = state == SideType.Opening ? CellSide.openingSize : 3;
        float thirdSize = Cell.wallSize / divider;
        if (owner.type == RoomType.Room && state == SideType.Opening)
            thirdSize = 0;

        PointF firstPoint = MazePainter.startingPoint;
        firstPoint.X += MazePainter.offset + halfSize;
        firstPoint.Y += MazePainter.offset + halfSize;

        PointF center = new PointF(0, 0);
        center.X = firstPoint.X + (Cell.wallSize * indexCoords.X);
        center.Y = firstPoint.Y + (Cell.wallSize * indexCoords.Y);

        float offset = 0;

        switch (dir)
        {
            case Direction.North:
                offset = center.Y + halfSize;
                a = new PointF(center.X - halfSize, offset);
                d = new PointF(center.X + halfSize, offset);
                b = new PointF(a.X + thirdSize, offset);
                c = new PointF(d.X - thirdSize, offset);

                break;
            case Direction.East:
                offset = center.X + halfSize;
                a = new PointF(offset, center.Y + halfSize);
                d = new PointF(offset, center.Y - halfSize);
                b = new PointF(offset, a.Y - thirdSize);
                c = new PointF(offset, d.Y + thirdSize);

                break;
            case Direction.South:
                offset = center.Y - halfSize;
                a = new PointF(center.X + halfSize, offset);
                d = new PointF(center.X - halfSize, offset);
                b = new PointF(a.X - thirdSize, offset);
                c = new PointF(d.X + thirdSize, offset);

                break;
            case Direction.West:
                offset = center.X - halfSize;
                a = new PointF(offset, center.Y - halfSize);
                d = new PointF(offset, center.Y + halfSize);
                b = new PointF(offset, a.Y + thirdSize);
                c = new PointF(offset, d.Y - thirdSize);

                break;
        }
    }

    public override string ToString()
    {
        string output = "A{0},\n B{1},\n C{2},\n D{3}";
        return string.Format(output, a, b, c, d);
    }
}
