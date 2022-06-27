using Godot;

public class BoardRenderer : Node2D {

	private static Color RED 			= new Color(1, 0.4f, 0.4f);
	private static Color WHITE 			= new Color(1, 1, 1);
	private static Color GREY 			= new Color(0.5f, 0.5f, 0.5f, 0.8f);
	private static float SPACER_HEIGHT 	= 35;
	private static float SPACER_WIDTH 	= 72;
	private static Vector2 PADDING 		= new Vector2(11.5f, 0);
	private static Vector2 CELL_SIZE 	= new Vector2(90, 90);
	private static Vector2 BOARD_SIZE 	= new Vector2(12 * (CELL_SIZE.x + PADDING.x) + SPACER_WIDTH, 10 * (CELL_SIZE.y + PADDING.y) + SPACER_HEIGHT);
	private static Texture CHECKER		= ResourceLoader.Load<Texture>("res://assets/images/checker/checker.png") as Texture;
	private Board _board;
	private Sprite[] _checkerSprites 	= new Sprite[140];

	public override void _Ready()
	{
		_board = GetParent<Board>() as Board;
		_board.Connect("OnBoardUpdate", this, "_UpdateBoard");

		_Setup(_board.BoardPoints, _board.Wall);
		Update();
	}

	private void _Setup(int[,] points, int[] wall)
	{
		int spriteWidth						= CHECKER.GetWidth();
		int spriteHeight					= CHECKER.GetHeight();
		Vector2 spriteScale					= new Vector2(CELL_SIZE.x / spriteWidth, CELL_SIZE.y / spriteHeight);

		for (int column = 0; column < points.GetLength(0); column++)
		{
			for (int position = 0; position < points.GetLength(1); position++) 
			{
				Sprite checkerSprite 					= new Sprite();
				checkerSprite.Texture 					= CHECKER;
				checkerSprite.Visible 					= false;
				checkerSprite.Offset					= new Vector2(spriteWidth * 0.5f, spriteHeight * 0.5f);
				checkerSprite.Scale						= spriteScale;
				_checkerSprites[column * 5 + position]  = checkerSprite;
				this.AddChild(checkerSprite);

				float xOffset 	= SPACER_WIDTH;

				if (column < 12)
				{	
					if (column < 6)
					{
						xOffset = 0;
					}

					checkerSprite.Position 		= new Vector2(BOARD_SIZE.x - (column + 1) * (CELL_SIZE.x + PADDING.x) - xOffset, BOARD_SIZE.y - (position + 1) * (CELL_SIZE.y + PADDING.y));
					continue;
				}

				if (column > 17) 
				{
					xOffset = 2 * SPACER_WIDTH;
				}

				checkerSprite.Position 			= new Vector2(column * (CELL_SIZE.x + PADDING.x) - BOARD_SIZE.x + xOffset, position * (CELL_SIZE.y + PADDING.y));
			}
		}
		
		for (int piece = 0; piece < wall.GetLength(0); piece++)
		{
			Sprite checkerSprite 					= new Sprite();
			checkerSprite.Texture 					= CHECKER;
			checkerSprite.Visible 					= false;
			checkerSprite.Offset					= new Vector2((spriteWidth * 0.8f) * 0.5f, (spriteHeight * 0.8f) * 0.5f);
			checkerSprite.Scale						= spriteScale * 1.2f;
			_checkerSprites[120 + piece]  			= checkerSprite;
			this.AddChild(checkerSprite);

			if (piece < 10)
			{
				checkerSprite.Position 				= new Vector2(BOARD_SIZE.x * 0.5f - CELL_SIZE.x * 0.5f - PADDING.x * 0.5f, BOARD_SIZE.y - CELL_SIZE.y - piece * (CELL_SIZE.y + PADDING.y));
				continue;
			}

			checkerSprite.Position					= new Vector2(BOARD_SIZE.x * 0.5f - CELL_SIZE.x * 0.5f - PADDING.x * 0.5f, (piece - 10) * (CELL_SIZE.y + PADDING.y));
		}
	}

	private void _UpdateBoard()
	{
		for (int column = 0; column < _board.BoardPoints.GetLength(0); column++)
		{
			for (int position = 0; position < _board.BoardPoints.GetLength(1); position++)
			{
				Sprite checkerSprite 				= _checkerSprites[column * 5 + position];
				checkerSprite.Visible				= _board.BoardPoints[column, position] > 0;
				checkerSprite.SelfModulate			= _board.BoardPoints[column, position] == 1 ? RED : WHITE;
			}
		}

		for (int piece = 0; piece < _board.Wall.GetLength(0); piece++)
		{
			Sprite checkerSprite 					= _checkerSprites[120 + piece];
			checkerSprite.Visible 					= _board.Wall[piece] > 0;
			checkerSprite.SelfModulate				= _board.Wall[piece] == 1 ? RED : WHITE;
		}
	}

	public void RenderBoardCells(int[,] points, int[] wall) 
	{
		for (int column = 0; column < points.GetLength(0); column++)
		{
			for (int position = 0; position < points.GetLength(1); position++) 
			{
				Color color 	= points[column, position] == 1 ? RED : WHITE;
				bool isHelper 	= _board.IsHelperMove(column, position);
				if (points[column, position] == 0)
				{
					if (!isHelper) continue;
					color = GREY; // For drawing the helper ghost piece. Placeholder for now.
				}
				
				float xOffset 	= SPACER_WIDTH;

				if (column < 12)
				{	
					if (column < 6)
					{
						xOffset = 0;
					}

					DrawRect(new Rect2(new Vector2(BOARD_SIZE.x - (column + 1) * (CELL_SIZE.x + PADDING.x) - xOffset, BOARD_SIZE.y - (position + 1) * (CELL_SIZE.y + PADDING.y)), CELL_SIZE), color);
					
					if (isHelper)
					{
						DrawRect(new Rect2(new Vector2(BOARD_SIZE.x - (column + 1) * (CELL_SIZE.x + PADDING.x) - xOffset, BOARD_SIZE.y - (position + 1) * (CELL_SIZE.y + PADDING.y)), CELL_SIZE), GREY);
					}
					
					continue;
				}

				if (column > 17) 
				{
					xOffset = 2 * SPACER_WIDTH;
				}

				DrawRect(new Rect2(new Vector2(column * (CELL_SIZE.x + PADDING.x) - BOARD_SIZE.x + xOffset, position * (CELL_SIZE.y + PADDING.y)), CELL_SIZE), color);

				if (isHelper)
				{
					DrawRect(new Rect2(new Vector2(column * (CELL_SIZE.x + PADDING.x) - BOARD_SIZE.x + xOffset, position * (CELL_SIZE.y + PADDING.y)), CELL_SIZE), GREY);
				}
			}
		}

		for (int piece = 0; piece < wall.GetLength(0); piece++)
		{
			if (wall[piece] == 0) continue;
			Color color = wall[piece] == 1 ? RED : WHITE;
			
			if (piece < 10)
			{
				DrawRect(new Rect2(new Vector2(BOARD_SIZE.x * 0.5f - CELL_SIZE.x * 0.5f - PADDING.x * 0.5f, BOARD_SIZE.y - CELL_SIZE.y - piece * (CELL_SIZE.y + PADDING.y)), CELL_SIZE), color);
				continue;
			}

			DrawRect(new Rect2(new Vector2(BOARD_SIZE.x * 0.5f - CELL_SIZE.x * 0.5f - PADDING.x * 0.5f, (piece - 10) * (CELL_SIZE.y + PADDING.y)), CELL_SIZE), color);
		}
	}

	public bool IsMouseInsideWall(float mouseX, float mouseY)
	{
		float leftBorder 	= _board.Position.x + BOARD_SIZE.x * 0.5f - SPACER_WIDTH * 0.5f;
		float rightBorder 	= _board.Position.x + BOARD_SIZE.x * 0.5f + SPACER_WIDTH * 0.5f;
		float bottomBorder 	= _board.Position.y + BOARD_SIZE.y;

		return mouseX > leftBorder && mouseX < rightBorder && mouseY > _board.Position.y && mouseY < bottomBorder; 
	}

	public int getColumn(float mouseX, float mouseY)
	{
		int column;
		float halfHeight 	= BOARD_SIZE.y * 0.5f;
		float halfWidth 	= BOARD_SIZE.x * 0.5f;
		float xOffset 		= 0;

		if (mouseX > _board.Position.x + halfWidth)
		{
			xOffset = SPACER_WIDTH;
		}

		column = 12 - Mathf.CeilToInt((( Mathf.Max(mouseX - xOffset - _board.Position.x, 0)) / (BOARD_SIZE.x - SPACER_WIDTH)) * 12);

		if (mouseY < _board.Position.y + halfHeight)
		{	
			column = 12 + Mathf.FloorToInt((( Mathf.Max(mouseX - xOffset - _board.Position.x, 0)) / (BOARD_SIZE.x - SPACER_WIDTH)) * 12);
		}

		return Mathf.Clamp( column, 0, 23 );
	}
}
