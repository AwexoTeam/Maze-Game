using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

public partial class MazeMaker
{
    public delegate void _OnPaintTickFinished();
    public event _OnPaintTickFinished OnPaintTickFinished;

    public static int paintTimer = 50;
    public static Cell lastCell;
    public static List<Cell> drawnN = new List<Cell>();
    public static bool shouldDraw;
    public void InvokeFinishedPainting()
    {
        OnPaintTickFinished.Invoke();
    }

    public void OnPaintFinished()
    {
        if (!shouldDraw)
            return;

        Timer t = new Timer(paintTimer);
        t.Elapsed += OnDelayReached;
        t.AutoReset = false;
        t.Start();
    }

    private void OnDelayReached(object sender, ElapsedEventArgs e)
        => Step();

    private void Step()
    {
        MakePath(lastCell);
        MazePainter.MazePaintTick();
    }
}