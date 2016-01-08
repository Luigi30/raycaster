using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycaster
{
    class GameTileMap
    {
        public List<List<GameTile>> TileMap = new List<List<GameTile>>();

        public GameTileMap(int height, int width)
        {
            for(int i = 0; i < height; i++)
            {
                var row = new List<GameTile>();
                for(int j= 0; j < width; j++)
                {
                    row.Add(new GameTile(0));
                }

                TileMap.Add(row);
            }
        }
    }
}
