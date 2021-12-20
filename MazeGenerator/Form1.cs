using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Paint += OnPaint;
            Resize += OnResize;
            KeyDown += OnKeyDown;

            string fileName = "Settings.txt";
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string[] lines = File.ReadAllLines(path + "\\" + fileName);
            Setup(lines);

            GameMaster.Initialize(this);
        }

        private void Setup(string[] lines)
        {
            if (lines.Length < 15)
            {
                Console.WriteLine("Error in the settings file.\n Using default values.");
                return;
            }

            int val = 0;
            lines[0] = lines[0].Replace("width=", string.Empty);
            lines[1] = lines[1].Replace("height=", string.Empty);
            lines[2] = lines[2].Replace("sanityLoseChance=", string.Empty);
            lines[3] = lines[3].Replace("startSanity=", string.Empty);
            lines[4] = lines[4].Replace("roomAmount=", string.Empty);
            lines[6] = lines[6].Replace("useDungeon=", string.Empty);
            lines[7] = lines[7].Replace("useLivingRoom=", string.Empty);
            lines[9] = lines[9].Replace("AnimationDebug=", string.Empty);
            lines[10] = lines[10].Replace("ColoredFlooringDebug=", string.Empty);
            lines[11] = lines[11].Replace("DisableFOWDebug=", string.Empty);
            lines[12] = lines[12].Replace("NoClipDebug=", string.Empty);
            lines[13] = lines[13].Replace("DisableWalkingSanityDrain=", string.Empty);
            lines[14] = lines[14].Replace("DisableAllSanityDrain=", string.Empty);

            if (int.TryParse(lines[0], out val))
                GameMaster.mazeWidth = val;

            if (int.TryParse(lines[1], out val))
                GameMaster.mazeHeight = val;

            if (int.TryParse(lines[2], out val))
                GameMaster.sanityLossChance = val;

            if (int.TryParse(lines[3], out val))
                GameMaster.startSanity = val;

            if (int.TryParse(lines[4], out val))
                GameMaster.roomCount = val;

            if (lines[6].ToLower() == "true")
                GameMaster.possibleRooms.Add(new Dungeon());

            if (lines[7].ToLower() == "true")
                GameMaster.possibleRooms.Add(new LivingRoom());

            if (lines[9].ToLower() == "true")
                MazeMakerDebugger.SetFeature(DebugFeatures.Animation);

            if (lines[10].ToLower() == "true")
                MazeMakerDebugger.SetFeature(DebugFeatures.ColoredFlooring);

            if (lines[11].ToLower() == "true")
                MazeMakerDebugger.SetFeature(DebugFeatures.DisableFOW);

            if (lines[12].ToLower() == "true")
                MazeMakerDebugger.SetFeature(DebugFeatures.NoClip);

            if (lines[13].ToLower() == "true")
                MazeMakerDebugger.SetFeature(DebugFeatures.DisableWalkingSanityDrain);

            if (lines[14].ToLower() == "true")
                MazeMakerDebugger.SetFeature(DebugFeatures.DisableAllSanityDrain);

        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            MazePainter.MazePaintTick();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            GameState state = GameMaster.state;

            if (state == GameState.Playing)
            {
                if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
                    Player.Move(Direction.South);

                else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
                    Player.Move(Direction.East);

                else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
                    Player.Move(Direction.North);

                else if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
                    Player.Move(Direction.West);
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            MazePainter.MazePaintTick();
        }
    }
}
