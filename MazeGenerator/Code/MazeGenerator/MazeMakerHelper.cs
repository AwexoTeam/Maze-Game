using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MazeMakerHelper
{
    //I really wanted there to be a smart way of doing this.
    //I couldnt find one so hard coded it is :/
    public static Direction GetOppsiteDirection(Direction dir)
    {
        if (dir == Direction.North)
            return Direction.South;

        if (dir == Direction.East)
            return Direction.West;

        if (dir == Direction.South)
            return Direction.North;

        if (dir == Direction.West)
            return Direction.East;

        return Direction.North;
    }

    public static List<Cell> GetCellNeighbours(List<Cell> maze, Cell c)
    {
        List<Cell> rtn = new List<Cell>();

        rtn.Add(GetCellInDirection(maze, c, Direction.North));
        rtn.Add(GetCellInDirection(maze, c, Direction.East));
        rtn.Add(GetCellInDirection(maze, c, Direction.South));
        rtn.Add(GetCellInDirection(maze, c, Direction.West));

        rtn.RemoveAll(x => x == null);
        rtn.RemoveAll(x => x.type == RoomType.Room);
        return rtn;
    }

    public static Cell GetCellByCoordinate(List<Cell> maze, Point chk)
    {
        Cell rtn = maze.Find(c => c.indexCoords == chk);

        bool isVerbose = MazeMakerDebugger.HasFeature(DebugFeatures.VerboseDebug);
        if(rtn == null && isVerbose)
            Console.WriteLine("Tried to get value at: " + chk);

        return rtn;
    }

    //Again i wanted this to be coded well but for now its hardcoded.
    public static Cell GetCellInDirection(List<Cell> maze, Cell start, Direction dir)
    {
        Point p = start.indexCoords;
        switch (dir)
        {
            case Direction.North:
                p.Y++;
                break;
            case Direction.East:
                p.X++;
                break;
            case Direction.South:
                p.Y--;
                break;
            case Direction.West:
                p.X--;
                break;
        }

        if (p.Y > GameMaster.mazeHeight - 1 || p.Y < 0)
            return null;

        if (p.X > GameMaster.mazeWidth -1 || p.X < 0)
            return null;

        return GetCellByCoordinate(maze,p);
    }

    public static Direction GetDirectionBetweenTwoCells(Cell a, Cell b)
    {
        //TODO: included vector math to make really cool longer stretches.

        int x = b.indexCoords.X - a.indexCoords.X;
        int y = b.indexCoords.Y - a.indexCoords.Y;

        Point diff = new Point(x, y);
        if (diff == new Point(0, 1))
            return Direction.North;

        if (diff == new Point(1, 0))
            return Direction.East;

        if (diff == new Point(0, -1))
            return Direction.South;

        return Direction.West;
    }

    public static void UpdateCellInfo(ref Cell cell)
    {
        //Hard to explain over code comments.
        //Ask me about how we define each rooms in the updatecell info.
        //Keep in mind the index of the sides relates the direction enum's index too :) (eg. north = 0, east = 1...)

        bool[] w = new bool[]
        {
            cell.sides[0].isWall,
            cell.sides[1].isWall,
            cell.sides[2].isWall,
            cell.sides[3].isWall,
        };

        if (cell.type == RoomType.Room)
            return;

        int wallCnt = Array.FindAll(w, x => x).Length;
        int index = 0;

        if (wallCnt == 1)
        {
            index = Array.FindIndex(w, x => x);
            cell.type = RoomType.Nexus;
            cell.originalType = RoomType.Nexus;
            cell.dir = (Direction)index;
            return;
        }

        else if (wallCnt == 3)
        {
            index = Array.FindIndex(w, x => !x);
            cell.type = RoomType.Dead_End;
            cell.originalType = RoomType.Dead_End;
            cell.dir = (Direction)index;
            return;
        }

        else if (wallCnt == 2) {
            List<CellSide> openWalls = Array.FindAll(cell.sides, x => x.isWall).ToList();
            CellSide a = openWalls.First();
            CellSide b = openWalls.Last();

            if (b.dir == GetOppsiteDirection(a.dir))
            {
                cell.type = RoomType.Corridor;
                cell.dir = a.dir;
                return;
            }

            Direction dirA = a.dir;
            Direction dirB = b.dir;

            Direction endDir = Direction.East;

            if (w[0])
            {
                Direction other = dirA == Direction.North ? dirB : dirA;
                endDir = other == Direction.East ? Direction.West : Direction.South;
                cell.dir = endDir;
                cell.type = RoomType.Corner;
                return;
            }

            else if (w[2])
            {
                Direction other = dirA == Direction.South ? dirB : dirA;
                endDir = other == Direction.East ? Direction.North : Direction.East;
                cell.dir = endDir;
                cell.type = RoomType.Corner;
                return;
            }
        }

        if(wallCnt == 4)
            Console.WriteLine("We got a weird one at: " + cell.indexCoords);
    }

    /// <param name="maze">A reference to the maze we run this on.</param>
    /// <param name="start">The start cell</param>
    /// <param name="dir">The direction to look in</param>
    /// <param name="end">This get out which is the cell at the end of the corridor</param>
    /// <returns>The list of cooridors between start and end</returns>
    public static List<Cell> GetCellByEndOfCooridor(List<Cell> maze, Cell start, Direction dir, out Cell end)
    {
        List<Cell> rtn = new List<Cell>();
        end = GetCellInDirection(maze, start, dir);
        RoomType type = end.type;

        if(end.type == RoomType.Corridor)
        {
            rtn.Add(end);
            rtn.AddRange(GetCellByEndOfCooridor(maze, end, dir, out end));
        }

        return rtn;
    }
}