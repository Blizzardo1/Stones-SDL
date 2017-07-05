using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using SdlDotNet.Graphics.Sprites;
using SdlDotNet.Input;
using Stones_SDL.Game.Objects;
using Stones_SDL.Menu;
using Tao.Sdl;
using static Stones_SDL.Game.Objects.SpriteHelper;
using SystemTimer = System.Timers.Timer;
using SdlTimer = SdlDotNet.Core.Timer;
using SdlFont = SdlDotNet.Graphics.Font;

namespace Stones_SDL.Game
{
    // Winning Message Box
    // Title:    STONES: YOU WON!
    // Caption:  Congratulations, all stones have been placed!
    // Button:   OK
    
    using static PrimitiveHelper;
    
    [Flags]
    public enum GameState
    {
        Stopped = 0x00,
        Splash = 0x01,
        Running = 0x02,
        Win = 0x010,
        Lose = 0x100
        
    }

    public class Stones {
        internal const int Scale = 2;
        internal const int Width = 378 * Scale;
        internal const int Height = 380 * Scale;
        
        internal const int TileWidth = 32 * Scale;
        internal const int TileHeight = 32 * Scale;

        internal const int MenuHeight = 20;

        internal readonly int GridOffsetX = 10;
        internal int GridOffsetY = 10;

        internal readonly int BaseOffsetX = 0;
        internal int BaseOffsetY = 0;
        
        internal const bool Resizeable = true;
        internal const bool Opengl = false;
        internal const bool Fullscreen = false;

        public static readonly Color Background = Color.FromArgb( 195, 195, 195 );

        private int _count = 90; // start with 90 stones; 1 in hand
        private int _bWidth = 10; // Board Width is 10
        private int _bHeight = 10; // Board Height is 10
        private int _score;
        private TimeSpan _timer;
        private static Random _rand;

        private SystemTimer _gTimer;

        private TextSprite _timerSprite;
        private TextSprite _currentStoneTextSprite;

        private Stack<Stone> _stones;
        private Stone _currentStone;

        private GameMenu _gameMenu;

        private Surface _board;
        private Stone[,] _grid;
        private Box3D _gridPlane;
        private Box3D _basePlane;

        private StonePreview _stonePreview;

        private GameState _gameState;

        private bool _debug;

#if LINUX
        public static Random Rand { get { return _rand; } }
#else
        public static Random Rand => _rand;
#endif
        public Stones ( )
        {
            _rand = new Random ( );
            InitVideo ( );
            InitTimer ( );
            InitRules( );
            InitMenu ( );

            CheckMenuSanity( );

            _gameState = GameState.Running;
            
            AllocateStones ( );
            
            PopulateGrid( );
            PopulateBorderRectangles(  );
            SetupStones( );

            InitEvents ( );
        }

        private void SetupStones()
        {
            _currentStone = GrabStone ( );
            for ( int i = 0; i < 6; i++ ) {
                PlaceStone ( Rand.Next( 0, _bHeight - 1 ), Rand.Next( 0, _bWidth - 1 ) );
                _currentStone = GrabStone( );
            }
        }

        private void InitRules( ) {
            /** RULES
    0,0 1,0 2,0 3,0 Joker
    
    0,1 1,1 2,1 3,1 \
    0,2 1,2 2,2 3,2 /
    
    0,3 1,3 2,3 3,3 \
    0,4 1,4 2,4 3,4 /
    
    0,5 1,5 2,5 3,5 \
    0,6 1,6 2,6 3,6 /
    
    0,1 1,1 2,1 3,1 \
    0,3 1,3 2,3 3,3 /
    
    0,2 1,2 2,2 3,2 \
    0,6 1,6 2,6 3,6 /
    
    0,4 1,4 2,4 3,4 \
    0,5 1,5 2,5 3,5 /
*/

            Rule.Add( new Rule( 1, 2, 1, 3 ) );
            Rule.Add( new Rule( 3, 4, 2, 6 ) );
            Rule.Add( new Rule( 5, 6, 4, 5 ) );
            // Rule.Add( new Rule( 1, 3 ) );
            // Rule.Add( new Rule( 2, 6 ) );
            // Rule.Add( new Rule( 4, 5 ) );
        }

        private void CheckMenuSanity()
        {

            if ( _gameMenu != null )
            {
                GridOffsetY += MenuHeight;
                BaseOffsetY += MenuHeight;
            }
        }

        private void InitMenu() {
            _gameMenu = new GameMenu( _board, new[] {new MenuItem {Text = @"&Game"}} );
        }

        private void PopulateGrid( ) {
            _grid = new Stone[_bHeight, _bWidth];
            for ( int y = 0; y < _bHeight; y++ ) {
                for ( int x = 0; x < _bWidth; x++ ) {
                    if ( _grid[ y, x ] == DefaultStone ) {
                        _grid[ y, x ] =
                            new Stone( new Point( x * TileWidth + GridOffsetX - 1, y * TileHeight + GridOffsetY - 1 ),
                                new Size( TileWidth, TileHeight ) );
                    }
                }
            }
        }

        /// <summary>
        ///     A Completely BOTCHED Function, SOSUMI!
        /// </summary>
        private void PopulateBorderRectangles( ) {
            
            int xD = _grid.GetUpperBound( 1 );
            int yD = _grid.GetUpperBound( 0 );
            Stone b1 = _grid[ 0, 0 ];
            Stone b2 = _grid[ yD, xD ];

            int x = b1.Box.XPosition1;
            int y = b1.Box.YPosition1;
            int w = b2.Box.XPosition2 - x;
            int h = b2.Box.YPosition2 - y;
            _gridPlane = new Box3D( new Rectangle( x, y, w, h ), 3 );
            _basePlane =
                new Box3D(
                    new Rectangle( BaseOffsetX, BaseOffsetY, Width - BaseOffsetX - 1, Height - BaseOffsetY - 1 ) );
            _stonePreview = new StonePreview( _gridPlane.Right + (6 * Scale), _gridPlane.Y - (2 * Scale) );

            /**
            _borderBoxes = new Box3D[4];

            int bBox = _grid.GetUpperBound( 0 );
            int tBox = _grid.GetUpperBound( 1 );

            Stone b1 = _grid[ 0, tBox ];
            Stone b2 = _grid[ bBox, 0 ];

            // TopBox
            int tX = 0;
            int tY = 0;
            int tW = Width - 1;
            int tH = GridOffsetY - 1;

            // Bottom Stone
            int bX = 0;
            int bY = b2.YPosition2 + 1;
            int bW = tW;
            int bH = Height - bY - 1;

            // Left Stone
            int lX = 0;
            int lY = tH;
            int lW = GridOffsetX - 1;
            int lH = bY - tH;

            // Right Stone
            int rX = b1.XPosition2 + 1;
            int rY = lY;
            int rW = Width - rX - 1;
            int rH = lH;


            _borderBoxes [ 0 ] = new Box3D ( new Rectangle ( lX, lY, lW, lH ) );
            _borderBoxes [ 1 ] = new Box3D( new Rectangle( tX, tY, tW, tH ) );
            _borderBoxes [ 2 ] = new Box3D ( new Rectangle ( bX, bY, bW, bH ) );
            _borderBoxes [ 3 ] = new Box3D( new Rectangle( rX, rY, rW, rH ) );
            
            // Bullfucking shit
            _borderBoxes [ 0 ] = new Box3D( new Rectangle( 0, 0, Width, GridOffsetY - 1 ) );
            _borderBoxes[ 1 ] =
                new Box3D( new Rectangle( 0, _borderBoxes[ 0 ].Bottom, GridOffsetX - 1,
                    Height - _grid[ _grid.GetUpperBound( 0 ), 0 ].YPosition2 - 1 ) );
            _borderBoxes[ 2 ] =
                new Box3D(
                    new Rectangle( _grid[ 0, _grid.GetUpperBound( 1 ) ].XPosition2, _borderBoxes[ 0 ].Bottom + 1,
                        Width - _grid[ 0, _grid.GetUpperBound( 1 ) ].XPosition2, Height - _borderBoxes[ 0 ].Bottom - 1 ) );
            _borderBoxes[ 3 ] =
                new Box3D( new Rectangle( 0, _borderBoxes[ 1 ].Bottom - 1, Width,
                    Height - _grid[ _grid.GetUpperBound( 0 ), 0 ].YPosition2 ) );
                    */
        }

        private void AllocateStones ( )
        {
            LoadSpriteSheet("tileset00.png");
            _stones = new Stack<Stone> ( );
            // _stones.Push( new Stone( new Sprite( ) ) );
            for ( int i = 0; i < _count - 2; i++ )
            {
                Stone stone = Stone.GetTile ( _rand.Next(0,3), _rand.Next ( 0, 6 ) );
                _stones.Push ( stone );
            }
            _stones.Shuffle( );
            _stones.Push( Stone.GetTile( _rand.Next( 0, 3 ), 0 ) );
            _stones.Push ( Stone.GetTile ( _rand.Next ( 0, 3 ), 0 ) );
        }

        private Stone GrabStone( ) => _stones.Pop( );

        private void InitVideo ( )
        {
            _board = Video.SetVideoMode ( Width, Height, Resizeable, Opengl, Fullscreen );
        }

        private void InitEvents ( )
        {
            Events.Tick += TickingEvents;
            Events.KeyboardDown += KeyboardDown;
            Events.MouseMotion += MouseMotion;
            Events.MouseButtonDown += MouseButtonDown;
            Events.Quit += LolQuit;
            Events.Run ( );
        }
        
        /// <summary>
        ///     Places your Current Stone on the Board
        /// </summary>
        /// <param name="x">Places your current hand into Grid X</param>
        /// <param name="y">Places your current hand into Grid Y</param>
        private void PlaceStone(int x, int y) {
            Stone stone = _grid[ y, x ];
            _currentStone.Seated = true;
            _currentStone.X = stone.X;
            _currentStone.Y = stone.Y;
            _grid [ y, x ] = _currentStone;
        }

        /// <summary>
        ///     Checks to see if a Stone is Legal against Neighbouring Stones
        /// </summary>
        /// <param name="idx">Id X</param>
        /// <param name="idy">Id Y</param>
        /// <param name="hand"></param>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        private bool IsLegal(Stone hand, Stone neighbour) {
            /** RULES
                0,0 1,0 2,0 3,0 Joker
                
                0,1 1,1 2,1 3,1 \
                0,2 1,2 2,2 3,2 /
                
                0,3 1,3 2,3 3,3 \
                0,4 1,4 2,4 3,4 /
                
                0,5 1,5 2,5 3,5 \
                0,6 1,6 2,6 3,6 /
                
                0,1 1,1 2,1 3,1 \
                0,3 1,3 2,3 3,3 /
                
                0,2 1,2 2,2 3,2 \
                0,6 1,6 2,6 3,6 /
                
                0,4 1,4 2,4 3,4 \
                0,5 1,5 2,5 3,5 /
            */

            // Update Rules Checking..... Add Score? Follow Directions from original Help file

            foreach ( Rule r in Rule.Rules ) {

                if ( hand.IdY == 0 || neighbour.IdY == 0 ) return true; // Wild Stone Check
                // if ( hand.IdY == r.C1 || neighbour.IdY == r.C1 ) return true; // Hand's Y Matches Neighbour's Y
                // if ( hand.IdY == r.C2 || neighbour.IdY == r.C2 ) return true; // Hand's Y Matches Neighbour's Y
                // if ( hand.IdX == neighbour.IdX && hand.IdY == neighbour.IdY ) return true;
                Point f = r.ForegroundRule;
                Point b = r.BackgroundRule;

                // Separate the Xs and Ys from the conditions...

                // Attribute One
                if ( hand.IdY == f.X || neighbour.IdY == f.X || hand.IdY == f.Y || neighbour.IdY == f.Y ) return true; // Hand's Foreground matches the given rules

                // Attribute Two
                if ( hand.IdY == b.X || neighbour.IdY == b.X || hand.IdY == b.Y || neighbour.IdY == b.Y ) return true; // Hand's Background matches the given rules
            }
            return false;
        }

        private void MouseButtonDown ( object sender, MouseButtonEventArgs e ) {
            if ( _stones.Count < 0 ) return;
            // Rules here
            for(int y = 0; y < _bHeight; y++)
            {
                for ( int x = 0; x < _bWidth; x++ ) {
                    var hit = new Rectangle( e.Position, new Size( 1, 1 ) );
                    Stone stone = _grid[ y, x ];
                    if ( !hit.IntersectsWith( stone.Rectangle ) ) continue;
                    if ( stone.Seated ) {
                        // Do Audio Stuff Here
                        continue;
                    }

                    bool legal = true;
                    int nnX = x - 1;
                    int nnY = y - 1;
                    int nX = x + 1;
                    int nY = y + 1;
                    if ( nnX < 0 ) nnX = 0;
                    if ( nX >= _bWidth ) nX = _bWidth - 1;
                    if ( nnY < 0 ) nnY = 0;
                    if ( nY >= _bHeight ) nY = _bHeight - 1;

                    Stone nls = _grid[ y, nnX ];
                    Stone nts = _grid[ nnY, x ];
                    Stone nbs = _grid[ nY, x ];
                    Stone nrs = _grid[ y, nX ];

                    // To iterate through IDs
                    for ( int sY = 0; sY < DefaultSheetHeight; sY++ ) {
                        for ( int sX = 0; sX < DefaultSheetWidth; sX++ ) {
                            legal = IsLegal( _currentStone, nls )
                                    && IsLegal( _currentStone, nts )
                                    && IsLegal( _currentStone, nbs )
                                    && IsLegal( _currentStone, nrs );
                        }
                    }
                    if ( !legal ) continue;
                    if ( !nls.Seated && !nts.Seated && !nbs.Seated && !nrs.Seated ) continue;

                    PlaceStone( x, y );
                    if ( _stones.Count == 0 ) {
                        _gameState = GameState.Win;
                        return;
                    }
                    _currentStone = GrabStone( );
                }
            }
            // ----------
                
            // _currentStone = GrabStone( );
            // _currentStone.X = e.X;
            // _currentStone.Y = e.Y;
            _currentStone.CenterOnStone( e.X, e.Y );
        }

        private void MouseMotion ( object sender, MouseMotionEventArgs e ) {

            _currentStone.CenterOnStone( e.X, e.Y );

            for ( int y = 0; y < _bHeight; y++ )
            {
                for ( int x = 0; x < _bWidth; x++ ) {
                    var hit = new Rectangle( e.Position, new Size( 1, 1 ) );
                    Stone stone = _grid[ y, x ];
                    stone.Highlight = hit.IntersectsWith( stone.Rectangle );
                }
            }

            // _currentStone.X = e.X - _currentStone.TileWidth / 2;
            // _currentStone.Y = e.Y - _currentStone.TileHeight / 2;
        }

        private void InitTimer ( )
        {
            SdlTimer.Initialize ( );
            _gTimer = new SystemTimer ( 256 );
            _gTimer.Elapsed += _gTimer_Elapsed;
            _gTimer.Start ( );
            _timerSprite = new TextSprite ( "Base", new SdlFont ( "arial.ttf", 10 ) );
            _currentStoneTextSprite = new TextSprite( "Base", _timerSprite.Font );
        }

        private void _gTimer_Elapsed ( object sender, ElapsedEventArgs e )
        {
            _timer = _timer.Add ( TimeSpan.FromMilliseconds ( _gTimer.Interval ) );
        }

        private void LolQuit ( object sender, QuitEventArgs e )
        {
            Events.QuitApplication ( );
        }

        private void KeyboardDown ( object sender, KeyboardEventArgs e )
        {
            if(e.Key == Key.F3) {
                _debug = !_debug;
            }
        }

        private void TickingEvents ( object sender, TickEventArgs e )
        {
            switch ( _gameState ) {
                case GameState.Running: {
                    Draw( );
                    Update( e );
                    break;
                }

                case GameState.Win:
                    Console.WriteLine ( @"YOU WIN!" );
                    Console.ReadKey( true );
                    break;
                case GameState.Lose:
                    Console.WriteLine(@"The next stone will not fit on the board!");
                    Console.ReadKey( true );
                    break;
                case GameState.Stopped:
                    // Hold Events
                    break;
                case GameState.Splash:
                    break;
                default:
                    throw new ArgumentOutOfRangeException( );
            }

        }

        private void Draw ( ) {
            // _board.Fill( Color.FromArgb( 23, 23, 23 ) );
            // _board.Fill( Color.Gray );
            _basePlane.Draw3D( _board, Background, false );
            for(int y = 0; y < _bHeight; y++)
            {
                for(int x = 0; x < _bWidth; x++) {
                    Stone s = _grid[ y, x ];
                    if ( s == DefaultStone ) continue;
                    if ( s.Seated ) {
                        _board.Blit( s );
                    }
                    else {
                        if(_debug)
                            _board.Draw( ( Box ) s, s.Highlight ? Color.Red : Color.Gray );
                    }
                }
            }

            _board.Blit( _currentStone );
            _gridPlane.Draw3D( _board, Background, false, false, true );

            _stonePreview.Draw( _board );

            _gameMenu.Draw();

            if ( !_debug ) return;
            // All Debug stuff here Except Grid
            _board.Blit( _timerSprite, new Point( 2, Width - 14 ) );
            _board.Blit( _currentStoneTextSprite, new Point( 2, Width - 24 ) );
        }

        private void Update ( TickEventArgs args )
        {
            _board.Update ( );
            _currentStone.Update( args );
            _currentStoneTextSprite.Text = $"{_currentStone.Id} | {_stones.Count}";
            _timerSprite.Text = $"Time Elapsed: {_timer.TotalSeconds}";
        }

    }
}
