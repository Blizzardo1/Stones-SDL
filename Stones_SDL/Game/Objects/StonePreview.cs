using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdlDotNet.Graphics;

namespace Stones_SDL.Game.Objects
{
    public class StonePreview {
        private const int PWidth = 34;
        private const int PHeight = 162;

        public const int Width = PWidth - 2;
        public const int Height = PHeight - 2;

        private Box3D _base;
        
        private static StonePreview _inst;
        public static StonePreview Instance => _inst;

        public StonePreview(int x, int y) {
            if ( _inst != null ) return;
            _base = new Box3D( new Rectangle( x, y, PWidth * Stones.Scale, PHeight * Stones.Scale ) );
            _inst = this;
        }

        public void Draw(Surface surface) {
            _base.Draw3D( surface, Stones.Background, false, true, true );
        }
        
    }
}
