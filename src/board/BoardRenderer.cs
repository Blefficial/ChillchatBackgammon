using Godot;

public class BoardRenderer : Node2D {

	private Board 				_board;
	private MovePieceTween 		_movePieceTween;
	private AddPieceToWallTween _wallPieceTween;
	private BearOffTween		_bearOffTween;

	private static int 	_boardPointCount 	= Constants.BOARD_POINT_COUNT;
	private static int 	_boardPointSize 	= Constants.BOARD_POINT_SIZE;
	private static int 	_boardPieceCount 	= _boardPointCount * _boardPointSize;
	private static int 	_pointsPerSide		= Constants.BOARD_POINTS_PER_SIDE;
	private static int 	_pointsPerQuarter 	= _pointsPerSide / 2;
	private static int 	_wallHalfSize 		= Constants.BOARD_WALL_HALF_SIZE;
	private static int 	_wallSpriteCount 	= 10;
	private static int 	_halfWallSprites 	= _wallSpriteCount / 2;

	private static Sprite[] _checkerSprites = new Sprite[_boardPieceCount + _wallSpriteCount];
	private static Color 	_colorRed 		= Constants.RED;
	private static Color 	_colorWhite 	= Constants.WHITE;
	private static Vector2 	_cellPadding 	= Constants.CELL_PADDING;
	private static Vector2 	_cellSize 		= Constants.CELL_SIZE;
	private static Vector2 	_boardSize		= Constants.BOARD_SIZE;
	private static Texture 	_checkerTexture = Constants.CHECKER_TEXTURE;
	private static Vector2 	_spriteScale 	= Constants.SPRITE_SCALE;
	private static float 	_spacerWidth	= Constants.BOARD_SPACER_WIDTH;

	public override void _Ready()
	{
		_board = GetParent<Board>() as Board;
		_board.Connect("OnBoardUpdate", 		this, 	"_UpdateBoard");
		_board.Connect("OnMovePiece", 			this, 	"_MovePiece");
		_board.Connect("OnMovePieceToWall", 	this, 	"_MovePieceToWall");
		_board.Connect("OnMovePieceFromWall",	this, 	"_MovePieceFromWall");
		_board.Connect("OnBearOff",				this,	"_BearOff");
		
		_movePieceTween = GetNode<Tween>("MovePieceTween") 		as MovePieceTween;
		_wallPieceTween = GetNode<Tween>("AddPieceToWallTween") as AddPieceToWallTween;
		_bearOffTween	= GetNode<Tween>("BearOffTween")		as BearOffTween;
		_movePieceTween.Connect("tween_all_completed", this, "_UpdateBoard");

		_Setup(_board.BoardPoints, _board.Wall);
		Update();
	}

	private void _Setup(int[,] points, int[] wall)
	{
		Vector2 relativeTextureSize = new Vector2(_checkerTexture.GetSize() * 0.5f * _spriteScale);
		Vector2 cellPositionOffset 	= new Vector2(_cellSize + _cellPadding);

		for (int column = 0; column < _boardPointCount; column++)
		{
			for (int position = 0; position < _boardPointSize; position++) 
			{
				Sprite checkerSprite 					= new Sprite();
				checkerSprite.Texture 					= _checkerTexture;
				checkerSprite.Visible 					= false;
				checkerSprite.Scale						= _spriteScale;
				_checkerSprites[column * _boardPointSize + position]  = checkerSprite;
				this.AddChild(checkerSprite);

				Vector2 xOffset = new Vector2(_GetColumnXOffset(column), 0);
				Vector2 padding	= column > 0 ? column * _cellPadding : Vector2.Zero;

				if (column < _pointsPerSide)
				{	
					checkerSprite.Position = _boardSize - new Vector2(column + 1, position + 1) * _cellSize + relativeTextureSize + xOffset - padding;
					continue;
				}

				checkerSprite.Position = new Vector2(column - _pointsPerSide, position) * cellPositionOffset + relativeTextureSize + xOffset;
			}
		}
		
		for (int piece = 0; piece < _wallSpriteCount; piece++)
		{
			Sprite checkerSprite 			= new Sprite();
			checkerSprite.Texture 			= _checkerTexture;
			checkerSprite.Visible 			= false;
			checkerSprite.Scale				= _spriteScale * 1.05f;
			checkerSprite.SelfModulate		= piece < _halfWallSprites ? _colorRed : _colorWhite;
			_checkerSprites[_boardPieceCount + piece] = checkerSprite;
			this.AddChild(checkerSprite);

			if (piece < _halfWallSprites)
			{
				checkerSprite.Position = new Vector2(_boardSize.x * 0.5f, _boardSize.y - (piece + 1) * _cellSize.y * 0.75f);
				continue;
			}

			checkerSprite.Position = new Vector2(_boardSize.x * 0.5f, (piece - _halfWallSprites + 1) * _cellSize.y * 0.75f);
		}
	}

	private float _GetColumnXOffset(int column) 
	{
		int finalQuarter = _boardPointCount - _pointsPerQuarter;

		if (column >= finalQuarter)
		{
			return _spacerWidth;
		}
		else if (column >= _pointsPerQuarter && column < _pointsPerSide)
		{
			return -_spacerWidth;
		}

		return 0;
	}

	private void _MovePiece(int[] from, int[] to)
	{
		Sprite startSprite 		 = _checkerSprites[from[0] * _boardPointSize + from[1]];
		Sprite destinationSprite = _checkerSprites[to[0] * _boardPointSize + to[1]];
		Vector2 startPosition 	 = startSprite.Position;
		Vector2 endPosition 	 = destinationSprite.Position;
		_movePieceTween.MovePiece(startSprite, destinationSprite, startPosition, endPosition, 0.3f);
	}

	private void _MovePieceFromWall(int player, int wallIndex, int[] to)
	{
		if (player == 1 && wallIndex >= _halfWallSprites) return;

		int absoluteWallIndex = player == 1 ? wallIndex : wallIndex - _wallSpriteCount;
		
		if (absoluteWallIndex >= _wallSpriteCount) return;
		
		Sprite startSprite 		 = _checkerSprites[_boardPieceCount + absoluteWallIndex];
		Sprite destinationSprite = _checkerSprites[to[0] * _boardPointSize + to[1]];
		Vector2 startPosition 	 = startSprite.Position;
		Vector2 endPosition 	 = destinationSprite.Position;
		_movePieceTween.MovePiece(startSprite, destinationSprite, startPosition, endPosition, 0.3f);
	}

	private void _MovePieceToWall(int player, int wallIndex)
	{
		if (player == 1 && wallIndex >= _halfWallSprites) return;

		int absoluteWallIndex = player == 1 ? wallIndex : wallIndex - _wallSpriteCount;

		if (absoluteWallIndex >= _wallSpriteCount) return;

		Sprite sprite = _checkerSprites[_boardPieceCount + absoluteWallIndex];
		sprite.SelfModulate = wallIndex < _wallHalfSize - 1 ? _colorRed : _colorWhite;
		_wallPieceTween.AddPieceToWall(sprite);
	}

	private void _BearOff(int player, int fromColumn, int pieceIndex)
	{
		Sprite sprite 		  = _checkerSprites[fromColumn * _boardPointSize + pieceIndex];
		Vector2 startPosition = sprite.Position;
		_bearOffTween.BearOffPiece(sprite, 0.3f);
	}

	private void _UpdateBoard()
	{
		for (int column = 0; column < _boardPointCount; column++)
		{
			for (int position = 0; position < _boardPointSize; position++)
			{
				int piece 			 = _board.BoardPoints[column, position];
				Sprite checkerSprite = _checkerSprites[column * _boardPointSize + position];
				Color color			 = piece == 1 ? _colorRed : _colorWhite;
				
				checkerSprite.ZIndex  = 0;
				checkerSprite.Visible = piece > 0;
				checkerSprite.Scale   = _spriteScale;

				checkerSprite.SelfModulate = color;
			}
		}

		for (int piece = 0; piece < _wallSpriteCount; piece++)
		{
			Sprite checkerSprite 		= _checkerSprites[_boardPieceCount + piece];
			checkerSprite.Scale			= _spriteScale * 1.2f;
			checkerSprite.ZIndex 		= 0;
			//checkerSprite.Visible 		= _board.Wall[piece] > 0;
			checkerSprite.SelfModulate	= _board.Wall[piece] == 1 ? _colorRed : _colorWhite;
		}
	}
}
