using Godot;

public class BoardRenderer : Node2D {

	private Board _board;
	private Sprite[] _checkerSprites 	= new Sprite[140];
	private MovePieceTween _movePieceTween;

	public static Color _colorRed 		= Constants.RED;
	public static Color _colorWhite 	= Constants.WHITE;
	public static Color _colorGrey 		= Constants.GREY;
	public static Vector2 _cellPadding 	= Constants.CELL_PADDING;
	public static Vector2 _cellSize 	= Constants.CELL_SIZE;
	private Vector2 _boardSize			= Constants.BOARD_SIZE;
	private Texture _checkerTexture 	= Constants.CHECKER;
	private float _spacerWidth			= Constants.BOARD_SPACER_WIDTH;
	private Vector2 _spriteScale 		= Constants.SPRITE_SCALE;

	public override void _Ready()
	{
		_board = GetParent<Board>() as Board;
		_board.Connect("OnBoardUpdate", this, "_UpdateBoard");
		_board.Connect("OnMovePiece", this, "_MovePiece");
		_board.Connect("OnMovePieceToWall", this, "_MovePieceToWall");
		_board.Connect("OnMovePieceFromWall", this, "_MovePieceFromWall");
		
		_movePieceTween = GetNode<Tween>("MovePieceTween") as MovePieceTween;
		_movePieceTween.Connect("tween_all_completed", this, "_UpdateBoard");

		_Setup(_board.BoardPoints, _board.Wall);
		Update();
	}

	private void _Setup(int[,] points, int[] wall)
	{
		float relativeTextureWidth 	= _spriteScale.x * _checkerTexture.GetWidth() * 0.5f;
		float relativeTextureHeight = _spriteScale.y * _checkerTexture.GetHeight() * 0.5f;
		
		for (int column = 0; column < points.GetLength(0); column++)
		{
			for (int position = 0; position < points.GetLength(1); position++) 
			{
				Sprite checkerSprite 					= new Sprite();
				checkerSprite.Texture 					= _checkerTexture;
				checkerSprite.Visible 					= false;
				checkerSprite.Scale						= _spriteScale;
				_checkerSprites[column * 5 + position]  = checkerSprite;
				this.AddChild(checkerSprite);

				float xOffset 		= _spacerWidth;

				if (column < 12)
				{	
					if (column < 6)
					{
						xOffset = 0;
					}

					checkerSprite.Position = new Vector2(_boardSize.x - (column + 1) * (_cellSize.x + _cellPadding.x) - xOffset + relativeTextureWidth, _boardSize.y - (position + 1) * (_cellSize.y + _cellPadding.y) + relativeTextureHeight);
					continue;
				}

				if (column > 17)
				{
					xOffset = 2 * _spacerWidth;
				}

				checkerSprite.Position = new Vector2(column * (_cellSize.x + _cellPadding.x) - _boardSize.x + xOffset + relativeTextureWidth, position * (_cellSize.y + _cellPadding.y) + relativeTextureHeight);
			}
		}
		
		for (int piece = 0; piece < wall.GetLength(0); piece++)
		{
			Sprite checkerSprite 			= new Sprite();
			checkerSprite.Texture 			= _checkerTexture;
			checkerSprite.Visible 			= false;
			checkerSprite.Scale				= _spriteScale * 1.2f;
			checkerSprite.SelfModulate		= piece < 10 ? _colorRed : _colorWhite;
			_checkerSprites[120 + piece]	= checkerSprite;
			this.AddChild(checkerSprite);

			if (piece < 10)
			{
				checkerSprite.Position = new Vector2((_boardSize.x - _cellSize.x - _cellPadding.x) * 0.5f + relativeTextureWidth, _boardSize.y - _cellSize.y - piece * (_cellSize.y + _cellPadding.y) + relativeTextureHeight);
				continue;
			}

			checkerSprite.Position	= new Vector2((_boardSize.x - _cellSize.x - _cellPadding.x) * 0.5f + relativeTextureWidth, (piece - 10) * (_cellSize.y + _cellPadding.y) + relativeTextureHeight);
		}
	}

	private void _MovePiece(int[] from, int[] to)
	{
		Sprite startSprite 		 = _checkerSprites[from[0] * 5 + from[1]];
		Sprite destinationSprite = _checkerSprites[to[0] * 5 + to[1]];
		Vector2 startPosition 	 = startSprite.Position;
		Vector2 endPosition 	 = destinationSprite.Position;
		_movePieceTween.MovePiece(startSprite, destinationSprite, startPosition, endPosition, 0.45f);
	}

	private void _MovePieceFromWall(int wallIndex, int[] to)
	{
		Sprite startSprite 		 = _checkerSprites[120 + wallIndex];
		Sprite destinationSprite = _checkerSprites[to[0] * 5 + to[1]];
		Vector2 startPosition 	 = startSprite.Position;
		Vector2 endPosition 	 = destinationSprite.Position;
		_movePieceTween.MovePiece(startSprite, destinationSprite, startPosition, endPosition, 0.45f);
	}

	private void _MovePieceToWall(int wallIndex)
	{
		Sprite sprite = _checkerSprites[120 + wallIndex];
		sprite.SelfModulate = wallIndex < 10 ? _colorRed : _colorWhite;
		_movePieceTween.AddPieceToWall(sprite);
	}

	private void _UpdateBoard()
	{
		for (int column = 0; column < _board.BoardPoints.GetLength(0); column++)
		{
			for (int position = 0; position < _board.BoardPoints.GetLength(1); position++)
			{
				Sprite checkerSprite 				= _checkerSprites[column * 5 + position];
				checkerSprite.ZIndex 				= 0;
				checkerSprite.Visible				= _board.BoardPoints[column, position] > 0;
				Color color							= _board.BoardPoints[column, position] == 1 ? _colorRed : _colorWhite;
				if (_board.IsHelperMove(column, position))
				{
					color = _colorGrey;
					checkerSprite.Visible = true;
				}
				
				checkerSprite.SelfModulate			= color;
			}
		}

		for (int piece = 0; piece < _board.Wall.GetLength(0); piece++)
		{
			Sprite checkerSprite 					= _checkerSprites[120 + piece];
			checkerSprite.Scale						= _spriteScale * 1.2f;
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
