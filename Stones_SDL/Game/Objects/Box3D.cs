using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using Tao.Sdl;

namespace Stones_SDL.Game.Objects {
    public unsafe struct Box3D : IPrimitive {
        private IntPtr _hwnd;

        private static int counter;
        private int _thickness;

        private Rectangle _rect;
        private Point _center;
        private int _sz;

        private string _id;
        
        public int X => _rect.X;
        public int Y => _rect.Y;
        public int Width => _rect.Width;
        public int Height => _rect.Height;
        public int Top => _rect.Top;
        public int Bottom => _rect.Bottom;
        public int Left => _rect.Left;
        public int Right => _rect.Right;

        public int Size => _sz;
        public string Id => _id;

        public IntPtr Handle => _hwnd;
        
        public void Draw( Surface surface, Color color ) => Draw( surface, color, false );
        public void Draw( Surface surface, Color color, bool antiAlias ) => Draw( surface, color, antiAlias, false );

        public void Draw( Surface surface, Color color, bool antiAlias, bool fill ) {
            // surface.Draw( this, color, antiAlias, fill );
            if ( surface == null ) throw new ArgumentException( nameof( surface ) );
            int result = 0;

            if ( fill ) {
                result = SdlGfx.boxRGBA( surface.Handle, ( short ) Left, ( short ) Top, ( short ) Right,
                    ( short ) Bottom, color.R, color.G, color.B,
                    color.A );
            }
            else {
                result = SdlGfx.rectangleRGBA( surface.Handle, ( short ) Left, ( short ) Top, ( short ) Right,
                    ( short ) Bottom, color.R, color.G, color.B,
                    color.A );
            }
            GC.KeepAlive ( this );
            if ( result != 0 )
                throw SdlException.Generate ( );
        }

        public void Draw3D( Surface surface, Color baseColor, bool antiAlias, bool fill = true,
            bool inverse = false ) {
            const int factor = 64;
            int rH = baseColor.R + factor;
            int gH = baseColor.G + factor;
            int bH = baseColor.B + factor;

            int rS = baseColor.R - factor;
            int gS = baseColor.G - factor;
            int bS = baseColor.B - factor;


            const int m = byte.MaxValue;

            if ( rH > m ) rH = m;
            if ( gH > m ) gH = m;
            if ( bH > m ) bH = m;
            if ( rS < 0 ) rS = 0;
            if ( gS < 0 ) gS = 0;
            if ( bS < 0 ) bS = 0;

            Color hL = Color.FromArgb( rH, gH, bH );
            Color shadow = Color.FromArgb( rS, gS, bS );

            Draw( surface, baseColor, antiAlias, fill );

            SdlDotNet.Graphics.Primitives.Box[] left = new SdlDotNet.Graphics.Primitives.Box[_thickness];
            SdlDotNet.Graphics.Primitives.Box[] top = new SdlDotNet.Graphics.Primitives.Box[_thickness];
            SdlDotNet.Graphics.Primitives.Box[] bottom = new SdlDotNet.Graphics.Primitives.Box[_thickness];
            SdlDotNet.Graphics.Primitives.Box[] right = new SdlDotNet.Graphics.Primitives.Box[_thickness];

            Rectangle rect = _rect;

            rect.X -= _thickness - 1;
            rect.Y -= _thickness - 1;
            rect.Width += _thickness - 1;
            rect.Height += _thickness - 1;

            for ( int i = 0; i < _thickness; i++ ) {
                /**
                    left[ i ] = new Stone( new Point( X - i, Y - i ), new Point( X - i, Y + Height + i ) );
                    top[ i ] = new Stone( new Point( X - i, Y - i ), new Point( X + Width + i, Y - i ) );
                    bottom[ i ] = new Stone( new Point( X - i, Y + Height + i ), new Point( X + Width + i, Y + Height + i ) );
                    right[ i ] = new Stone( new Point( X + Width + i, Y - i ), new Point( X + Width + i, Y + Height + i ) );
                 */

                left[ i ] = new SdlDotNet.Graphics.Primitives.Box( new Point( rect.X + i, rect.Y + i ),
                    new Point( rect.X + i, rect.Y + rect.Height - i ) );
                top[ i ] = new SdlDotNet.Graphics.Primitives.Box( new Point( rect.X + i, rect.Y + i ),
                    new Point( rect.X + rect.Width - i, rect.Y + i ) );
                bottom[ i ] = new SdlDotNet.Graphics.Primitives.Box( new Point( rect.X + i, rect.Y + rect.Height - i ),
                    new Point( rect.X + rect.Width - i, rect.Y + rect.Height - i ) );
                right[ i ] = new SdlDotNet.Graphics.Primitives.Box( new Point( rect.X + rect.Width - i, Y + i ),
                    new Point( rect.X + rect.Width - i, rect.Y + rect.Height - i ) );

                if ( inverse ) {

                    bottom[ i ].Draw( surface, hL, antiAlias );
                    right[ i ].Draw( surface, hL, antiAlias );
                    left[ i ].Draw( surface, shadow, antiAlias );
                    top[ i ].Draw( surface, shadow, antiAlias );
                }
                else {

                    bottom[ i ].Draw( surface, shadow, antiAlias );
                    right[ i ].Draw( surface, shadow, antiAlias );
                    left[ i ].Draw( surface, hL, antiAlias );
                    top[ i ].Draw( surface, hL, antiAlias );
                }
            }
        }

        public Box3D( Rectangle rect, int thickness = 1 ) {
            _rect = rect;
            _center = new Point( _rect.Right / 2, _rect.Bottom / 2 );
            _sz = sizeof( Rectangle );
            _id = $"Stone {counter++}";
            _thickness = thickness;
            _hwnd = Marshal.AllocHGlobal( _sz * 4 );
        }
        
        public void CloseHandle( ) {
            Marshal.FreeHGlobal( _hwnd );
        }

        Point IPrimitive.Center
        {
            get { return _center; }
            set { _center = value; }
        }

        public override string ToString( )
            =>
            $"ID: {_id}; {{Left: {_rect.Left}; Top: {_rect.Top}; Bottom: {_rect.Bottom}; Right: {_rect.Right}; X: {_rect.X}; Y: {_rect.Y}; Width: {_rect.Width}; Height: {_rect.Height}; Center: {_center.X},{_center.Y}}}";
    }
}
