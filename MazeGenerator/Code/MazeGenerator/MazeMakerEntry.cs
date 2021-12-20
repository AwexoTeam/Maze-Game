using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum GameState
{
    Generating,
    Playing,
    Done,
    GameOver,
}

public partial class MazeMaker
{
    public List<Cell> maze;

    public int seed;
    
    private List<Cell> visited;
    public Random rng;

    public int width;
    public int height;

    public int roomCount;

    public GameState state;
    private List<string> roomIDs;
    private Dictionary<string, bool> roomStates;

    public List<Key> keys;

    public MazeMaker(int width, int height, int roomCount, string _seed = "NILL")
    {
        if (_seed == "NILL")
            _seed = Guid.NewGuid().ToString();

        OnPaintTickFinished += OnPaintFinished;

        this.width = width;
        this.height = height;
        this.roomCount = roomCount;

        seed = _seed.GetHashCode();
        rng = new Random(seed);

        maze = new List<Cell>();
        visited = new List<Cell>();
        roomIDs = new List<string>();
        roomStates = new Dictionary<string, bool>();
        keys = new List<Key>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell c = new Cell(new Point(x, y));
                maze.Add(c);
            }
        }

        shouldDraw = MazeMakerDebugger.HasFeature(DebugFeatures.Animation);
        if(roomCount > 0)
            FillInRoom(roomCount);
        
        Cell first = maze.First();
        MakePath(first);

    }

    #region Starting out
    private void MakePath(Cell currCell)
    {
        if (state == GameState.Playing)
            return;

        List<Cell> neighbours = MazeMakerHelper.GetCellNeighbours(maze, currCell);
        neighbours.RemoveAll(x => visited.Contains(x));
        visited.Add(currCell);

        if(neighbours.Count <= 0)
        {
            var artifacts = maze.Where(c => !visited.Contains(c)).ToList();
            if(artifacts.Count > 0)
            {
                for (int i = 0; i < maze.Count; i++)
                {
                    Cell curr = maze[i];

                    neighbours.Clear();
                    neighbours = MazeMakerHelper.GetCellNeighbours(maze, curr);
                    neighbours.RemoveAll(x => visited.Contains(x));
                    
                    if(neighbours.Count > 0)
                    {
                        MakePath(curr);
                        return;
                    }
                }
                return;
            }
            CleanUpLazyCode();
            state = GameState.Playing;
            return;
        }

        int index = rng.Next(0, neighbours.Count);
        Cell chosenNeighbour = neighbours[index];
        
        MakePathBetween(currCell, chosenNeighbour);

        bool isAnimationOn = (MazeMakerDebugger.HasFeature(DebugFeatures.Animation));
        if (!isAnimationOn)
            MakePath(chosenNeighbour);
        else
            lastCell = chosenNeighbour;
    }

    private void CleanUpLazyCode()
    {
        List<Cell> deadEnds = new List<Cell>();
        List<Cell> otherRooms = new List<Cell>();
        
        deadEnds = maze.FindAll(c => c.type == RoomType.Dead_End);
        otherRooms = maze.FindAll(c => c.type != RoomType.Dead_End);
        otherRooms.RemoveAll(c => c.type == RoomType.Room);
        
        for (int i = 0; i < roomIDs.Count; i++)
        {
            int index = 0;

            Key key = new Key();
            key.roomID = roomIDs[i];
            key.heldByPlayer = false;

            if (deadEnds.Count > 0)
            {
                index = rng.Next(0, deadEnds.Count);
                key.indexCoords = deadEnds[index].indexCoords;
                deadEnds.RemoveAt(index);
            
            }
            else
            {
                index = rng.Next(0, otherRooms.Count);
                key.indexCoords = otherRooms[index].indexCoords;
                otherRooms.RemoveAt(index);
            }

            keys.Add(key);
        }
    }

    private void MakePathBetween(Cell a, Cell b)
    {
        Direction dir = MazeMakerHelper.GetDirectionBetweenTwoCells(a, b);
        Direction oppsiteDir = MazeMakerHelper.GetOppsiteDirection(dir);

        Array.Find(a.sides, s => s.dir == dir).state = SideType.Opening;
        Array.Find(b.sides, s => s.dir == oppsiteDir).state = SideType.Opening;
    }
    #endregion

    #region RoomFilling

    private bool IsValidRoomOrigin(Cell c)
    {
        Point p = c.indexCoords;
        if (p.X >= width - 2)
            return false;

        if (p.Y <= 1)
            return false;

        return true;
    }
    private void FillInRoom(int cnt)
    {
        List<Cell> cellsLeft = new List<Cell>();
        cellsLeft.AddRange(maze);
        cellsLeft.RemoveAll(c => !IsValidRoomOrigin(c));

        bool shoulContinue = true;
        int maxIter = 150;

        while (shoulContinue)
        {
            if (cnt <= 0)
                return;

            if (maxIter <= 0)
                return;

            maxIter--;
            int index = rng.Next(0, cellsLeft.Count);
            Point p = cellsLeft[index].indexCoords;
            int choosenX = p.X;
            int choosenY = p.Y;

            if (ChangeToRoom(ref cellsLeft, choosenX, choosenY))
                cnt--;
        }
    }

    private bool hasPlacedStairs;
    private bool ChangeToRoom(ref List<Cell> maze, int x, int y)
    {
        List<Cell> chkMaze = maze;

        int width = maze.Max(s => s.indexCoords.X);
        int height = maze.Max(s => s.indexCoords.Y);

        Point pa = new Point(x, y);
        Point pb = new Point(x + 1, y);
        Point pc = new Point(x, y - 1);
        Point pd = new Point(x + 1, y - 1);

        List<CellSide> sides = new List<CellSide>();
        List<Cell> roomsCells = new List<Cell>();

        Cell a = MazeMakerHelper.GetCellByCoordinate(maze, pa);
        Cell b = MazeMakerHelper.GetCellByCoordinate(maze, pb);
        Cell c = MazeMakerHelper.GetCellByCoordinate(maze, pc);
        Cell d = MazeMakerHelper.GetCellByCoordinate(maze, pd);

        roomsCells.Add(a);
        roomsCells.Add(b);
        roomsCells.Add(c);
        roomsCells.Add(d);



        int index = 0;

        Room r = null;
        if (!hasPlacedStairs)
        {
            r = new Staircase();
            hasPlacedStairs = true;
        }
        else
        {
            index = rng.Next(0, GameMaster.possibleRooms.Count);
            r = GameMaster.possibleRooms[index];
        }
        roomsCells.RemoveAll(cell => cell == null);

        string roomID = Guid.NewGuid().ToString();
        roomsCells.ForEach(cell => cell.roomID = roomID);
        roomsCells.ForEach(cell => cell.room = r);

        bool isAnyNull =
            a == null ||
            b == null ||
            c == null ||
            d == null;

        if (isAnyNull)
            return false;

        MazeMaker.lastCell = a;
        MazeMaker.drawnN.Clear();

        //maze.Add(b);
        //maze.Add(c);
        //maze.Add(d);

        maze.Remove(a);
        maze.Remove(b);
        maze.Remove(c);
        maze.Remove(d);

        UpdateCell(ref sides, a, 0, 3);
        UpdateCell(ref sides, b, 0, 1);
        UpdateCell(ref sides, c, 2, 3);
        UpdateCell(ref sides, d, 2, 1);

        sides.ForEach(s => s.state = SideType.Wall);
        index = 0;

        sides.RemoveAll(xc => MazeMakerHelper.GetCellInDirection(chkMaze, xc.owner, xc.dir) == null);

        index = rng.Next(sides.Count);
        CellSide choosenSide = sides[index];
        choosenSide.state = SideType.Door;
        choosenSide.roomID = roomID;

        Cell doorWayCell = choosenSide.owner;
        Direction doorwayDir = choosenSide.dir;

        Cell doorwayNeigh = MazeMakerHelper.GetCellInDirection(maze, doorWayCell, doorwayDir);
        Direction oppsiteDir = MazeMakerHelper.GetOppsiteDirection(doorwayDir);

        doorwayNeigh.sides[(int)oppsiteDir].state = SideType.Door;
        doorwayNeigh.sides[(int)oppsiteDir].roomID = roomID;

        Point npoint = a.indexCoords;
        npoint.X--;
        npoint.Y -= 2;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Point neighbor = new Point(npoint.X + i, npoint.Y + j);
                Cell n = MazeMakerHelper.GetCellByCoordinate(maze, neighbor);

                if (n != null && maze.Contains(n))
                    maze.Remove(n);
            }
        }

        roomIDs.Add(roomID);
        return true;
    }

    private void UpdateCell(ref List<CellSide> sides, Cell c, int wall1, int wall2)
    {
        Array.ForEach(c.sides, s => s.state = SideType.Opening);
        sides.Add(c.sides[wall1]);
        sides.Add(c.sides[wall2]);
        c.type = RoomType.Room;
        visited.Add(c);
    }
    #endregion
}

