using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Raycaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int WALL_HEIGHT = 256; //When we're 64 units away from the wall, it should be 256 pixels tall
        const int COLUMN_COUNT = 320; //320 rays
        const int VIEWPORT_WIDTH = 640; //so each one is 2px wide
        const int VIEWPORT_HEIGHT = 480;
        const int VIEWPORT_CENTER_HEIGHT = VIEWPORT_HEIGHT / 2;
        int RAY_WIDTH = VIEWPORT_WIDTH / COLUMN_COUNT; //2px
        const int MAP_WIDTH = 16; //tiles
        const int MAP_HEIGHT = 16; //tiles
        const int FOV_DEGREES = 90;
        const int GRID_WIDTH = 64; //each map tile is 64x64 units

        public MainWindow()
        {
            var mwVm = new MainWindowViewModel();
            mwVm.ImageBuffer = new WriteableBitmap(VIEWPORT_WIDTH, VIEWPORT_HEIGHT, 96, 96, PixelFormats.Bgra32, null);

            var GameMap = new GameTileMap(MAP_WIDTH, MAP_HEIGHT);
            GameMap.TileMap[1][0].WallHere = 1; //a blank map with a wall at (1,0)
            GameMap.TileMap[2][0].WallHere = 1; //a blank map with a wall at (2,0)
            GameMap.TileMap[3][0].WallHere = 1; //a blank map with a wall at (3,0)
            GameMap.TileMap[4][0].WallHere = 1; //a blank map with a wall at (4,0)

            var PlayerActor = new Player();
            PlayerActor.X = 96.0M;
            PlayerActor.Y = 224.0M;
            PlayerActor.rotation = 180.0M;

            InitializeComponent();
            DataContext = mwVm;

            mwVm.ImageBuffer.Clear(Colors.Black);

            //initialize the column height map
            List<Decimal> WallHeights = new List<Decimal>();
            for (int i = 0; i < COLUMN_COUNT; i++)
            {
                WallHeights.Add(0);
            }

            Decimal increment = (Decimal)FOV_DEGREES / (Decimal)COLUMN_COUNT; //one column covers (90/320) degrees
            Decimal startingAngle = PlayerActor.rotation - (increment * COLUMN_COUNT) / 2;
            int columnIndex = 0;
            //Draw a column on the screen. 
            for (int i = -COLUMN_COUNT / 2; i < (COLUMN_COUNT / 2) - 1; i++)
            {
                columnIndex = i + (COLUMN_COUNT / 2);

                Decimal angle = startingAngle + increment * columnIndex; //sweeping from rotation-45 to rotation+45
                if(angle < 0)
                {
                    angle += 360;
                }
                else if(angle > 360)
                {
                    angle -= 360;
                }

                var distance = CastRay(PlayerActor, GameMap, angle, PlayerActor.rotation - angle);

                if (distance == -1)
                {
                    WallHeights[columnIndex] = 0;
                }
                else
                {
                    WallHeights[columnIndex] = WALL_HEIGHT / (distance / GRID_WIDTH); //WALL_HEIGHT units tall when GRID_WIDTH units away
                }
            }

            //Draw the actual column on the screen bitmap.
            for (int i = 0; i < COLUMN_COUNT; i++)
            {
                if (WallHeights[i] > 0)
                {
                    for (int j = 0; j < RAY_WIDTH; j++)
                    {
                        for (int k = 0; k < WallHeights[i]; k++)
                        {
                            int startingHeight = VIEWPORT_CENTER_HEIGHT - (int)Math.Floor(WallHeights[i] / 2);
                            mwVm.ImageBuffer.SetPixel(j + (i * RAY_WIDTH), startingHeight + k, Colors.White);
                        }

                    }
                }
            }
        }

        Decimal CastRay(Player player, GameTileMap map, Decimal rayAngle, Decimal beta)
        {
            Decimal rayX = player.X;
            Decimal rayY = player.Y;
            //rayAngle is in degrees
            Decimal rayAngleRadians = (rayAngle * (decimal)(Math.PI / 180)); //forward angle
            Decimal betaRadians = beta * (decimal)(Math.PI / 180);

            bool RAY_FACES_UP = (rayAngle - 180) <= 0;

            double Xa, Ya;
            if(rayAngleRadians > 0)
            {
                Xa = 64 / Math.Tan((double)rayAngleRadians);
            }
            else
            {
                Xa = 0;
            }

            Ya = GRID_WIDTH;

            double Ax, Ay;
            double wallInterceptX, wallInterceptY = 0;

            //Horizontal interception
            if (RAY_FACES_UP)
            {
                Ay = (double)Math.Floor(player.Y / 64) * 64 - 1;
            }
            else
            {
                Ay = (double)Math.Floor(player.Y / 64) * 64 + 64;
            }

            if(rayAngleRadians > 0)
            {
                Ax = (double)player.X + ((double)player.Y - Ay) / Math.Tan((double)rayAngleRadians);
            } else
            {
                Ax = 0;
            }

            wallInterceptX = Ax;
            wallInterceptY = Ay;

            int gridX, gridY;
            gridX = Convert.ToInt16(Ax / 64) - 1;
            gridY = Convert.ToInt16(Ay / 64) - 1;

            if (gridX < 0 || gridY < 0 || gridX > MAP_WIDTH - 1 || gridY > MAP_HEIGHT - 1)
            {
                return -1.0M;
            }

            bool foundWall = map.TileMap[gridX][gridY].WallHere == 1;
            int tooFarCount = 0;

            while (!foundWall || tooFarCount > 10)
            {
                if (RAY_FACES_UP)
                {
                    Ya -= 64;
                }
                else
                {
                    Ya += 64;
                }
                Xa = 64 / Math.Tan((double)rayAngleRadians);

                wallInterceptX = Ax + Xa;
                wallInterceptY = Ay + Ya;
                gridX = Convert.ToInt16(wallInterceptX / 64) - 1;
                gridY = Convert.ToInt16(wallInterceptY / 64) - 1;

                if (gridX < 0 || gridY < 0 || gridX > MAP_WIDTH - 1 || gridY > MAP_HEIGHT - 1)
                {
                    return -1.0M;
                }

                foundWall = map.TileMap[gridX][gridY].WallHere == 1;
                tooFarCount++;
            }

            if (foundWall)
            {
                var distortedDistance = Convert.ToDecimal(Math.Sqrt(Math.Pow((double)player.X - wallInterceptX, 2) + Math.Pow((double)player.Y - wallInterceptY, 2)));
                var correctedDistance = Convert.ToDecimal(Math.Cos((double)betaRadians)) * distortedDistance;
                return correctedDistance;
            }

            return -1.0M;

        }
    }
}
