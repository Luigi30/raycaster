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
            GameMap.TileMap[1][0].WallHere = 1;

            var PlayerActor = new Player();
            PlayerActor.X = 0.0M;
            PlayerActor.Y = 32.0M;
            PlayerActor.rotation = 0.0M;

            InitializeComponent();
            DataContext = mwVm;

            mwVm.ImageBuffer.Clear(Colors.Black);

            List<Decimal> WallHeights = new List<Decimal>();
            for (int i = 0; i < COLUMN_COUNT; i++)
            {
                WallHeights.Add(0);
            }

            for(int i = -COLUMN_COUNT / 2; i < (COLUMN_COUNT / 2) - 1; i++)
            {
                Decimal increment = (Decimal)FOV_DEGREES / (Decimal)COLUMN_COUNT;
                Decimal angle = i * increment;
                if(angle < 0)
                {
                    angle += 360;
                }

                var distance = CastRay(PlayerActor, GameMap, angle, PlayerActor.rotation + angle);
                if (distance == -1)
                {
                    WallHeights[i + (COLUMN_COUNT / 2)] = 0;
                } else
                {
                    WallHeights[i + (COLUMN_COUNT / 2)] = WALL_HEIGHT / distance;
                }

            }

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
            Decimal rayAngleRadians = rayAngle * (decimal)(Math.PI / 180); //forward angle

            double Xa = 64 / Math.Tan((double)rayAngleRadians);
            double Ya = GRID_WIDTH;

            return -1.0M;

        }
    }
}
