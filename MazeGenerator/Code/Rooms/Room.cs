using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Room
{
    public abstract string roomName { get; }
    public abstract string roomDescription { get; }
    public virtual void OnEnter()
    {
        ConsoleColor c = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[" + roomName + "]");
        Console.ForegroundColor = c;

        Console.WriteLine(roomDescription);
    }
    public virtual void OnLeave()
    {
        ConsoleColor c = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("You left the " + roomName);
        Console.ForegroundColor = c;
    }
}
