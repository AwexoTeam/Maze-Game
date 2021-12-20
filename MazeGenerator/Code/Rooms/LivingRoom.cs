using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LivingRoom : Room
{
    public int sanityBoost = 3;

    public override string roomName => "Living Area";
    public override string roomDescription => "The room was quite cozy and pretty. \n "+sanityBoost+" Sanity";

    public override void OnEnter()
    {
        base.OnEnter();
        Player.sanity += sanityBoost;
    }
}
