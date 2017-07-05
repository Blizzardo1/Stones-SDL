using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Stones_SDL.Game.Objects {
    public static class SpriteHelper {

        internal const int DefaultSheetWidth = 4;
        internal const int DefaultSheetHeight = 7;
        private const string DefaultDir = "Resources";
        private static Sprite[,] _loadedSprites;
        private static int tX, tY;
        
        public static void LoadSpriteSheet( string sheet ) => LoadSpriteSheet( sheet, Stones.TileWidth, Stones.TileHeight, DefaultSheetWidth, DefaultSheetHeight );

        public static void LoadSpriteSheet( string sheet, int tileWidth, int tileHeight, int tilesXLen, int tilesYLen ) {
            _loadedSprites = new Sprite[tilesYLen, tilesXLen];
            tX = tilesXLen;
            tY = tilesYLen;

            var s = new Surface( Path.Combine( DefaultDir, sheet ) );
            s = s.CreateScaledSurface( Stones.Scale );

            for ( int y = 0; y < tY; y++ ) {
                for ( int x = 0; x < tX; x++ ) {
                    var sourceRectangle = new Rectangle( x * tileWidth, y * tileHeight, tileWidth, tileHeight );
                    var destRectangle = new Rectangle( 0, 0, tileWidth, tileHeight );
                    Surface a = s.CreateCompatibleSurface( tileWidth, tileHeight );
                    a.Blit( s, destRectangle, sourceRectangle );
                    _loadedSprites [ y, x ] = new Sprite( a );
                }
            }
        }

        public static Sprite GetSprite( int x, int y ) {
            if ( x >= tX || x < 0 || y >= tY || y < 0 )
                return null;

            return _loadedSprites[ y, x ];
        }
    }
}
