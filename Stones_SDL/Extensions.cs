using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stones_SDL.Game;

namespace Stones_SDL
{
    public static class Extensions
    {
        public static void Shuffle<T>(this Stack<T> stack ) {
            T[] a = stack.ToArray( );
            lock ( a ) {
                stack.Clear( );
                foreach ( T t in a.OrderBy( x => Stones.Rand.Next( ) ).ToArray( ) ) {
                    stack.Push( t );
                }
            }
        }
    }
}
