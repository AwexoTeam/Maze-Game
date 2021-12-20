using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Dungeon : Room
{
    public int sanityDebuff = 5;
    public override string roomName => "Dungeon";

    public override string roomDescription => "A pungent smell fills the room. \n" +
        "It is far from pleasent and you can feel it tearing some old memories. \n" +
        "-" + sanityDebuff + " in sanity";

    public override void OnEnter()
    {
        base.OnEnter();
        Player.sanity -= sanityDebuff;
    }
}
