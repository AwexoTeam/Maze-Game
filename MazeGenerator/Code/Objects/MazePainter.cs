using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MazeGenerator;

public static class MazePainter
{
    public static Form1 master;
    public static float offset = 10;
    public static float width;
    public static float height;
    public static float textboxHeight = 200;
    public static PointF startingPoint;
    public static PictureBox renderer;

    private static float x;
    private static float y;
    private static float w;
    private static float h;

    public static void Initialize(Form1 form)
    {
        master = form;
        renderer = new PictureBox();
        renderer.Click += OnClick;
        renderer.Dock = DockStyle.None;

        SetBoarder(form.Width, form.Height);
        renderer.SetBounds((int)x, (int)y, (int)width, (int)height);
        master.Controls.Add(renderer);
    }

    private static void OnClick(object sender, EventArgs e)
    {

    }

    private static void SetBoarder(float _width, float _height)
    {
        width = _width;
        height = _height;

        x = offset;
        y = offset;
        w = width - (2 * offset);
        h = height - offset - textboxHeight;

        startingPoint = new PointF(x, y);

        float mW = w / GameMaster.mazeWidth;
        float mH = h / GameMaster.mazeHeight;

        bool isWidthSmallest = mW < mH;
        float average = isWidthSmallest ? mW : mH;
        Cell.wallSize = average;

        startingPoint.Y -= offset;
    }

    public static void MazePaintTick()
    {
        List<Cell> maze = GameMaster.maze;

        PixelFormat pf = PixelFormat.Format32bppArgb;
        Bitmap render = new Bitmap((int)width, (int)height, pf);
        Graphics g = Graphics.FromImage(render);

        Cell playerCell = MazeMakerHelper.GetCellByCoordinate(maze, Player.position);
        List<Cell> playerNeighbour = MazeMakerHelper.GetCellNeighbours(maze, playerCell);
        playerNeighbour.Add(playerCell);

        List<Cell> lowPriorityCell = new List<Cell>();
        lowPriorityCell.AddRange(maze);
        lowPriorityCell.RemoveAll(c => playerNeighbour.Contains(c));

        lowPriorityCell.ForEach(c => c.Draw(g));
        playerNeighbour.ForEach(c => c.Draw(g));

        renderer.Image = render;
        GameMaster.maker.InvokeFinishedPainting();
    }
}
