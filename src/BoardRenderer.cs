using Godot;

public class BoardRenderer : Node2D {

	private Board _board;
	private static int _boardPointCount = Constants.BOARD_POINT_COUNT;
	private static int _boardPointSize 	= Constants.BOARD_POINT_SIZE;
	private static int _boardPieceCount = _boardPointCount * _boardPointSize;
	private static int _pointsPerSide	= Constants.BOARD_POINTS_PER_SIDE;
	private static int _pointsPerQuarter = _pointsPerSide / 2;
	private static int _wallHalfSize 	= Constants.BOARD_WALL_HALF_SIZE;
	private static int _wallSpriteCount = 10;
	private static int _halfWallSprites = _wallSpriteCount / 2;
	private Sprite[] _checkerSprites 	= new Sprite[_boardPieceCount + _wallSpriteCount];
	private MovePieceTween _movePieceTween;
	private AddPieceToWallTween _wallPieceTween;

	public static Color _colorRed 		= Constants.RED;
	public static Color _colorWhite 	= Constants.WHITE;
	public static Color _colorGrey 		= Constants.GREY;
	public static Vector2 _cellPadding 	= Constants.CELL_PADDING;
	public static Vector2 _cellSize 	= Constants.CELL_SIZE;
	private Vector2 _boardSize			= Constants.BOARD_SIZE;
	private Texture _checkerTexture 	= Constants.CHECKER_TEXTURE;
	private float _spacerWidth			= Constants.BOARD_SPACER_WIDTH;
	private Vector2 _spriteScale 		= Constants.SPRITE_SCALE;

	public override void _Ready()
	{
		_board = GetParent<Board>() as Board;
		_board.Connect("OnBoardUpdate", this, "_UpdateBoard");
		_board.Connect("OnMovePiece", this, "_MovePiece");
		_board.Connect("OnMovePieceToWall", this, "_MovePieceToWall");
		_board.Connect("OnMovePieceFromWall", this, "_MovePieceFromWall");
		
		_movePieceTween = GetNode<Tween>("MovePieceTween") 		as MovePieceTween;
		_wallPieceTween = GetNode<Tween>("AddPieceToWallTween") as AddPieceToWallTween;
		_movePieceTween.Connect("tween_all_completed", this, "_UpdateBoard");

		_Setup(_board.BoardPoints, _board.Wall);
		Update();
	}

	public override void _Draw()
	{
		DrawCircle(_boardSize, 5, _colorRed);
	}

	private void _Setup(int[,] points, int[] wall)
	{
		float relativeTextureWidth 	= _spriteScale.x * _checkerTexture.GetWidth() * 0.5f;
		float relativeTextureHeight = _spriteScale.y * _checkerTexture.GetHeight() * 0.5f;
		Vector2 relativeTextureSize = new Vector2(_checkerTexture.GetSize() * 0.5f * _spriteScale);
		Vector2 cellPositionOffset = new Vector2(_cellSize + _cellPadding);

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

				Vector2 xOffset 		= new Vector2(_GetColumnXOffset(column), 0);
				Vector2 padding			= column > 0 ? column * _cellPadding : Vector2.Zero;

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
		Sprite startSprite 		 = _checkerSprites[from[0] * 5 + from[1]];
		Sprite destinationSprite = _checkerSprites[to[0] * 5 + to[1]];
		Vector2 startPosition 	 = startSprite.Position;
		Vector2 endPosition 	 = destinationSprite.Position;
		_movePieceTween.MovePiece(startSprite, destinationSprite, startPosition, endPosition, 0.3f);
	}

	private void _MovePieceFromWall(int wallIndex, int[] to)
	{
		Sprite startSprite 		 = _checkerSprites[120 + wallIndex];
		Sprite destinationSprite = _checkerSprites[to[0] * 5 + to[1]];
		Vector2 startPosition 	 = startSprite.Position;
		Vector2 endPosition 	 = destinationSprite.Position;
		_movePieceTween.MovePiece(startSprite, destinationSprite, startPosition, endPosition, 0.3f);
	}

	private void _MovePieceToWall(int wallIndex)
	{
		Sprite sprite = _checkerSprites[120 + wallIndex];
		sprite.SelfModulate = wallIndex < 10 ? _colorRed : _colorWhite;
		_wallPieceTween.AddPieceToWall(sprite);
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
				//checkerSprite.Visible = true;

				if (_board.IsHelperMove(column, position))
				{
					color = _colorGrey;
					checkerSprite.Visible = true;
				}
				
				checkerSprite.SelfModulate = color;
			}
		}

		for (int piece = 0; piece < _wallSpriteCount; piece++)
		{
			Sprite checkerSprite 					= _checkerSprites[_boardPieceCount + piece];
			checkerSprite.Scale				= _spriteScale * 1.2f;
			checkerSprite.ZIndex 					= 0;
			checkerSprite.Visible 					= _board.Wall[piece] > 0;
			checkerSprite.SelfModulate				= _board.Wall[piece] == 1 ? _colorRed : _colorWhite;
		}
	}

	public void RenderBoardCells(int[,] points, int[] wall)
	{
		for (int column = 0; column < points.GetLength(0); column++)
		{
			for (int position = 0; position < points.GetLength(1); position++) 
			{
				Color color 	= points[column, position] == 1 ? _colorRed : _colorWhite;
				bool isHelper 	= _board.IsHelperMove(column, position);
				if (points[column, position] == 0)
				{
					if (!isHelper) continue;
					color = _colorGrey;
				}
				
				float xOffset 	= _spacerWidth;

				if (column < 12)
				{	
					if (column < 6)
					{
						xOffset = 0;
					}

					DrawRect(new Rect2(new Vector2(_boardSize.x - (column + 1) * (_cellSize.x + _cellPadding.x) - xOffset, _boardSize.y - (position + 1) * (_cellSize.y + _cellPadding.y)), _cellSize), color);
					
					if (isHelper)
					{
						DrawRect(new Rect2(new Vector2(_boardSize.x - (column + 1) * (_cellSize.x + _cellPadding.x) - xOffset, _boardSize.y - (position + 1) * (_cellSize.y + _cellPadding.y)), _cellSize), _colorGrey);
					}
					
					continue;
				}

				if (column > 17) 
				{
					xOffset = 2 * _spacerWidth;
				}

				DrawRect(new Rect2(new Vector2(column * (_cellSize.x + _cellPadding.x) - _boardSize.x + xOffset, position * (_cellSize.y + _cellPadding.y)), _cellSize), color);

				if (isHelper)
				{
					DrawRect(new Rect2(new Vector2(column * (_cellSize.x + _cellPadding.x) - _boardSize.x + xOffset, position * (_cellSize.y + _cellPadding.y)), _cellSize), _colorGrey);
				}
			}
		}

		for (int piece = 0; piece < wall.GetLength(0); piece++)
		{
			if (wall[piece] == 0) continue;
			Color color = wall[piece] == 1 ? _colorRed : _colorWhite;
			
			if (piece < 10)
			{
				DrawRect(new Rect2(new Vector2(_boardSize.x * 0.5f - _cellSize.x * 0.5f - _cellPadding.x * 0.5f, _boardSize.y - _cellSize.y - piece * (_cellSize.y + _cellPadding.y)), _cellSize), color);
				continue;
			}

			DrawRect(new Rect2(new Vector2(_boardSize.x * 0.5f - _cellSize.x * 0.5f - _cellPadding.x * 0.5f, (piece - 10) * (_cellSize.y + _cellPadding.y)), _cellSize), color);
		}
	}
}
