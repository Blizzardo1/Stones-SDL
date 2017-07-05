using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;

namespace Stones_SDL.Menu
{
    public class GameMenu : System.Windows.Forms.Menu {
        private IntPtr _hwnd;
        private Surface _surface;
        private int x, y, w, h;
        private Line[] _menuLines;

        public GameMenu( Surface surface, MenuItem[] items ) : base( items ) {
            _surface = surface;
            x = 0;
            y = 0;
            w = surface.Width;
            h = 20;

            _menuLines = new Line[2];
            _menuLines[ 0 ] = new Line( ( short ) x, ( short ) y, ( short ) w, (short)y );
            _menuLines[ 1 ] = new Line( ( short ) x, ( short ) ( h - 1 ), ( short ) w, (short)(h - 1) );
        }

        public void Draw() {
            _surface.Fill( new Rectangle( x, y, w, h ), Color.White );

            foreach ( Line line in _menuLines ) {
                line.Draw( _surface, Color.Black );
            }

        }
    }
}
