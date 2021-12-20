using MazeGenerator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class GameMaster
{
    public static List<Room> possibleRooms = new List<Room>()
    {
        new DinnerRoom(),
    };

    public static List<Cell> maze { get { return maker.maze; } }
    public static GameState state { get { return maker.state; } set { maker.state = value; } }
    public static MazeMaker maker;
    public static Form1 master;
    public static int mazeWidth = 18;
    public static int mazeHeight = 10;
    public static int sanityLossChance = 3;
    public static int startSanity;
    public static int roomCount = 3;
    public static void Initialize(Form1 _master)
    {
        master = _master;
        MazePainter.Initialize(master);

        maker = new MazeMaker(mazeWidth, mazeHeight, roomCount);
        Cell deadEnd = maker.maze.Find(x => x.type == RoomType.Dead_End);
        if (deadEnd == null)
            deadEnd = maker.maze.First();

        Player.sanity = startSanity;
        Player.position = deadEnd.indexCoords;
    }

}
