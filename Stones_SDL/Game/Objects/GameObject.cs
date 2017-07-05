using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Stones_SDL.Game.Objects
{
    public class GameObject : Sprite {
        protected GameObject(Sprite sprite) {
            Setup( sprite );
        }

        private void Setup( Sprite sprite ) {
            try {
                Surface = sprite.Surface;
                AllowDrag = sprite.AllowDrag;
                Alpha = sprite.Alpha;
                AlphaBlending = sprite.AlphaBlending;
                BeingDragged = sprite.BeingDragged;
                BoundingBox = sprite.BoundingBox;
                Height = sprite.Height;
                Width = sprite.Width;
                LastBlitRectangle = sprite.LastBlitRectangle;
                Position = sprite.Position;
                Transparent = sprite.Transparent;
                TransparentColor = sprite.TransparentColor;
                X = sprite.X;
                Y = sprite.Y;
                Z = sprite.Z;
            }
            catch ( Exception ) {
                Surface = new Surface( Stones.TileWidth, Stones.TileHeight );
                AllowDrag = false;
                Alpha = 0;
                AlphaBlending = true;
                BeingDragged = false;
                Position = Point.Empty;
                X = 0;
                Y = 0;
                Z = 0;
            }
        }

        public virtual void Draw() {
            
        }

        public override void Update( TickEventArgs args) {
            
        }
    }
}
