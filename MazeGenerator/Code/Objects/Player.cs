using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Player
{
    public static string gameOverMsg =
        "It was too much. You couldn't bear the darkness anymore and thus you fall onto your knees. \n" +
        "Left in the dark you only had your own devices and they weren't good enough." +
        "They never did find you. \n GAME OVER";

    public static int view = 3;
    private static int _sanity = 10;
    public static int sanity
    {
        get { return _sanity; }
        set
        {
            bool negateSanityDrain = MazeMakerDebugger.HasFeature(DebugFeatures.DisableAllSanityDrain);

            if(value < 0 && negateSanityDrain)
                return;
            
            _sanity = value;

            if (_sanity <= 0)
            {
                Console.WriteLine(gameOverMsg);
                GameMaster.state = GameState.GameOver;
            }
        }
    } 

    public static Point position;
    public static Direction facing;

    private static string lastID;

    public static List<Cell> viewRange = new List<Cell>();

    public static void Move(Direction dir)
    {
        bool negateWalkingDrain = MazeMakerDebugger.HasFeature(DebugFeatures.DisableWalkingSanityDrain);

        int nm = GameMaster.maker.rng.Next(0, 100);
        if(nm <= GameMaster.sanityLossChance && !negateWalkingDrain)
        {
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[The darkness shallows your sanity.]");
            Console.WriteLine("["+sanity+" sanity left.]");
            Console.ForegroundColor = c;
            sanity--;
        }

        facing = dir;
        List<Cell> maze = GameMaster.maze;
        Cell currCell = maze.Find(x => x.indexCoords == position);
        CellSide side = Array.Find(currCell.sides, s => s.dir == dir);

        Cell newCell = MazeMakerHelper.GetCellInDirection(maze, currCell, dir);
        GameMaster.master.Invalidate();

        if (newCell == null)
            return;

        viewRange.Clear();
        viewRange.Add(newCell);

        Cell lastCell = newCell;
        for (int i = 0; i < view; i++)
        {
            Cell c = MazeMakerHelper.GetCellInDirection(maze, lastCell, facing);
            if (c != null)
            {
                viewRange.Add(c);
                lastCell = c;
            }
        }

        if (MazeMakerDebugger.HasFeature(DebugFeatures.NoClip))
        {
            position = newCell.indexCoords;

        }
        else
        {
            switch (side.state)
            {
                case SideType.Opening:
                    position = newCell.indexCoords;
                    break;
                case SideType.Door:
                    string id = side.roomID;
                    Key key = GameMaster.maker.keys.Find(k => k.roomID == id);
                    if (key.heldByPlayer)
                        position = newCell.indexCoords;
                    else
                        Console.WriteLine("Door is locked!");
                    break;
            }
        }

        if (lastID != newCell.roomID)
        {
            lastID = newCell.roomID;

            if (currCell.room != null)
                currCell.room.OnLeave();

            if (newCell.room != null)
                newCell.room.OnEnter();
        }

        if (GameMaster.maker.keys.Exists(k => k.indexCoords == position))
        {
            Key key = GameMaster.maker.keys.Find(k => k.indexCoords == position);
            if (!key.heldByPlayer)
            {
                key.heldByPlayer = true;
                Console.WriteLine("You picked up a key I wonder where it goes!");
            }
        }
    }
}
