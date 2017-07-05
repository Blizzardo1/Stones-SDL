using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stones_SDL
{
    public class Rule {
        public static List< Rule > Rules { get; set; }

        private int _c1, _c2;
        private Point _fore, _back;

        public int C1 => _c1;
        public int C2 => _c2;

        public Point ForegroundRule => _fore;
        public Point BackgroundRule => _back;

        public static void Add(Rule rule) {
            if ( Rules == null ) Rules = new List< Rule >( );
            Rules.Add( rule );
        }

        public Rule(int c1, int c2) {
            _c1 = c1;
            _c2 = c2;
        }

        public Rule(int f1, int f2, int b1, int b2) {
            _fore = new Point( f1, f2 );
            _back = new Point( b1, b2 );
        }
    }
}
