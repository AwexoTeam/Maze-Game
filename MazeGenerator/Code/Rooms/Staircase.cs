using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Staircase : Room
{
    public override string roomName => "Staircase";

    public override string roomDescription => "Finally a way out of this maze! I am finally free again.";

    public override void OnEnter()
    {
        base.OnEnter();
        GameMaster.state = GameState.Done;
    }
}
