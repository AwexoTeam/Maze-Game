using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//This was partial class cause i debated to add a modloader.
//Do not ask why.
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

    private bool hasPlacedStairs;
    public MazeMaker(int width, int height, int roomCount, string _seed = "NILL")
    {
        //If no seed was set then generate a new one.
        if (_seed == "NILL")
            _seed = Guid.NewGuid().ToString();

        //An event to deal with framerate if i wanted to and should lock movement
        OnPaintTickFinished += OnPaintFinished;

        //Set the instance variables.
        this.width = width;
        this.height = height;
        this.roomCount = roomCount;

        maze = new List<Cell>();
        visited = new List<Cell>();
        roomIDs = new List<string>();
        roomStates = new Dictionary<string, bool>();
        keys = new List<Key>();

        //Random takes in an int so get hashcode
        seed = _seed.GetHashCode();
        rng = new Random(seed);

        //Fill in our maze with empty cells.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell c = new Cell(new Point(x, y));
                maze.Add(c);
            }
        }

        //If there is no rooms theres no win conditions.
        //Therefore there always have to have 1 room.
        if (roomCount <= 0)
            roomCount = 1;

        //Fill in the empty rooms.
        FillInRoom(roomCount);
        
        //Take the first cell and make path from there!
        Cell first = maze.First();
        MakePath(first);

    }

    #region Starting out
    private void MakePath(Cell currCell)
    {
        //If this was called outside of playing then return.
        if (state == GameState.Playing)
            return;

        //Get the cell's neighbour without the diagonals
        List<Cell> neighbours = MazeMakerHelper.GetCellNeighbours(maze, currCell);

        //If its been visted then ignore it.
        neighbours.RemoveAll(x => visited.Contains(x));

        //Add the cell since its been visted
        visited.Add(currCell);

        //If we have no neighbor we reached the path's dead end
        if(neighbours.Count <= 0)
        {
            //Artifacts are places that are left to make paths out of.
            //Essentially we get all the not visited cells.
            var artifacts = maze.Where(c => !visited.Contains(c)).ToList();

            //If theres artifacts then we aren't done!
            if(artifacts.Count > 0)
            {
                //Basically this part just go through ALL of the cells.
                //Find the first one that have a non visited neighbour
                //And then take that cell and call this method.
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

        //If there is neighbours then take a random one.
        int index = rng.Next(0, neighbours.Count);
        Cell chosenNeighbour = neighbours[index];
        
        //Make a path between the two cells.
        MakePathBetween(currCell, chosenNeighbour);

        //If we use animation we should let the paint function do its thing else recursively call itself.
        bool isAnimationOn = (MazeMakerDebugger.HasFeature(DebugFeatures.Animation));
        if (!isAnimationOn)
            MakePath(chosenNeighbour);
        else
            lastCell = chosenNeighbour;
    }

    //When things aren't working and i dunno where to put it.
    //It gets through here
    //TODO: put these things the right places.
    private void CleanUpLazyCode()
    {
        List<Cell> deadEnds = new List<Cell>();
        List<Cell> otherRooms = new List<Cell>();
        
        deadEnds = maze.FindAll(c => c.type == RoomType.Dead_End);
        otherRooms = maze.FindAll(c => c.type != RoomType.Dead_End);
        otherRooms.RemoveAll(c => c.type == RoomType.Room);
        
        //For each rooms add a key to a random room.
        //We prefer dead ends but else we gotta add to normal rooms.
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
        //Lazy code to see if its within bound.

        Point p = c.indexCoords;
        
        //it is -2 due to it being 0 indexed so first maze pos is 0.0
        //Therefore highest x is width - 1
        //We want a border away 1 so thats also -1
        //Therefore in total -2
        if (p.X >= width - 2)
            return false;

        if (p.Y <= 1)
            return false;

        return true;
    }

    //Fills in the rooms needed.
    private void FillInRoom(int cnt)
    {
        List<Cell> cellsLeft = new List<Cell>();
        cellsLeft.AddRange(maze);

        //Removing all that isn't within the border.
        cellsLeft.RemoveAll(c => !IsValidRoomOrigin(c));

        //This is to control the loop. Alternatively we could do reverse for loop
        //But i think while loop is more sensible
        bool shoulContinue = true;
        int maxIter = 150;

        while (shoulContinue)
        {
            //If we have no rooms left to place then stop.
            if (cnt <= 0)
                return;

            //If we tried to place room too many times stop.
            if (maxIter <= 0)
                return;

            
            maxIter--;
            
            //Take a random room position in the maxe.
            int index = rng.Next(0, cellsLeft.Count);
            Point p = cellsLeft[index].indexCoords;
            int choosenX = p.X;
            int choosenY = p.Y;

            //If we can change the room then do it.
            if (ChangeToRoom(ref cellsLeft, choosenX, choosenY))
                cnt--;
        }
    }

    private bool ChangeToRoom(ref List<Cell> maze, int x, int y)
    {
        List<Cell> chkMaze = maze;

        int width = maze.Max(s => s.indexCoords.X);
        int height = maze.Max(s => s.indexCoords.Y);

        /*
         There are 4 tiles to this room.
            a is always the origin.
            b is always the one to the left of a.
            c is always the one above to a.
            d is the one above and left to a.
         */

        //TODO: make this into a method.
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

        //Index is used throughtout so i didnt have to keep initializing new variables with confusing names.
        int index = 0;

        Room r = null;

        //If we havent placed staircase then place it.
        if (!hasPlacedStairs)
        {
            r = new Staircase();
            hasPlacedStairs = true;
        }

        //Else we pick a random room
        else
        {
            //TODO: consider to make sure it picks only ONE of each rooms.
            index = rng.Next(0, GameMaster.possibleRooms.Count);
            r = GameMaster.possibleRooms[index];
        }

        //Theres some of my math thats wrong so we just ignore nulls and pretend it didnt happen.
        roomsCells.RemoveAll(cell => cell == null);

        //Get a room id so we know which cells belong together.
        string roomID = Guid.NewGuid().ToString();
        roomsCells.ForEach(cell => cell.roomID = roomID);
        roomsCells.ForEach(cell => cell.room = r);

        //If any of them are null this is true.
        bool isAnyNull =
            a == null ||
            b == null ||
            c == null ||
            d == null;

        //If so return false so we know we can.
        if (isAnyNull)
            return false;

        MazeMaker.lastCell = a;
        MazeMaker.drawnN.Clear();

        //TODO: is this why it returns null in get neighbours?
        maze.Remove(a);
        maze.Remove(b);
        maze.Remove(c);
        maze.Remove(d);

        UpdateCell(ref sides, a, 0, 3);
        UpdateCell(ref sides, b, 0, 1);
        UpdateCell(ref sides, c, 2, 3);
        UpdateCell(ref sides, d, 2, 1);

        //Set everything to walls
        sides.ForEach(s => s.state = SideType.Wall);
        index = 0;

        //From here on down a lot is in just lots of testing.
        //I dont know if i can fully explain it due to some explaining problems i have.
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

        //Here we get the outer neighbours and process them too.
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

