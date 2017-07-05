using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using SdlDotNet.Graphics.Sprites;

namespace Stones_SDL.Game.Objects
{
    //public enum StoneColor
    //{
    //    Black = 0x01,
    //    Green = 0x02,
    //    Maroon = 0x04,
    //    Yellow = 0x08,
    //    Lime = 0x10,
    //    Red = 0x20,
    //    Blue = 0x40
    //}

    //public static class SymbolHelper
    //{
    //    public const char A = 'A';
    //    public const char B = 'B';
    //    public const char C = 'C';
    //    public const char D = 'D';
    //    public const char J = 'J';

    //    public static char Rand ( )
    //    {
    //        int r = Stones.Rand.Next ( 0, 3 );
    //        switch ( r )
    //        {
    //            case 1:
    //                return B;
    //            case 2:
    //                return C;
    //            case 3:
    //                return D;
    //            case 0:
    //            default:
    //                return A;
    //        }
    //    }
    //}

    //public struct StoneType
    //{
    //    public StoneColor Foreground { get; set; }
    //    public StoneColor Background { get; set; }
    //    public char Symbol { get; set; }

    //    public StoneType AdjustSymbol ( char newSym )
    //    {
    //        Symbol = newSym;
    //        return this;
    //    }

    //    public static StoneType Joker = new StoneType { Background = StoneColor.Yellow, Foreground = StoneColor.Blue, Symbol = SymbolHelper.J };
    //    public static StoneType KoG = new StoneType { Background = StoneColor.Green, Foreground = StoneColor.Black };
    //    public static StoneType RoK = new StoneType { Background = StoneColor.Black, Foreground = StoneColor.Red };
    //    public static StoneType LoM = new StoneType { Background = StoneColor.Maroon, Foreground = StoneColor.Lime };
    //    public static StoneType LoK = new StoneType { Background = StoneColor.Black, Foreground = StoneColor.Lime };
    //    public static StoneType KoM = new StoneType { Background = StoneColor.Maroon, Foreground = StoneColor.Black };
    //    public static StoneType RoG = new StoneType { Background = StoneColor.Green, Foreground = StoneColor.Red };

    //    public static StoneType LoL = new StoneType {
    //        Background = StoneColor.Black,
    //        Foreground = StoneColor.Black,
    //        Symbol = 'L'
    //    };

    //    // public static StoneType J = new StoneType {Background = StoneColor.Yellow, Foreground = StoneColor.Blue};
    //}

    public class Stone : GameObject
    {
        private const char NullChar = '\0';

        private string _id;
        private Box _box;
        private int _idx, _idy;

        internal bool Highlight { get; set; }
        internal bool Seated { get; set; }
        
#if LINUX
        public string Id { get { return _id; } }
        public int IdX { get { return _idx; } }
        public int IdY { get { return _idy; } }
        public Box Box { get { return _box; } }
#else
        public string Id => _id;
        public int IdX => _idx;
        public int IdY => _idy;
        public Box Box => _box;
#endif

        public Stone(Point p, Size s) : base(new Sprite()) {
            _box = new Box( p, s );
            Rectangle = new Rectangle( p, s );
        }

        public Stone ( Sprite sprite) : base (sprite)
        {   
            _box = new Box( new Point( Left, Top ), new Point( Right, Bottom ) );
        }

        public static Stone GetTile(int x, int y) {
            Sprite s = SpriteHelper.GetSprite( x, y );
            
            var a = new Stone (s) {
                _idx = x,
                _idy = y,
                _id = $"stone{x}{y}",
            };
            a.Surface.Blit( s );
            a._box = new Box( new Point( a.X, a.Y ), new Size( a.Width, a.Height ) );
            return a;
        }

        public void CenterOnStone ( int sX, int sY)
        {
            var p = new Point(sX - Width / 2, sY - Height / 2);
            X = p.X;
            Y = p.Y;
        }

        public static implicit operator Box(Stone s) {
            return s.Box;
        }
    }
}
