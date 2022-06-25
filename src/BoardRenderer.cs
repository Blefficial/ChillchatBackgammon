using Godot;

public class BoardRenderer : Node2D {

	private static Color RED 			= new Color(1, 0, 0);
	private static Color BLACK 			= new Color(0, 0, 0);
	private static float PADDING 		= 1;
	private static float SPACER_HEGIHT 	= 25;
	private static float SPACER_WIDTH 	= 50;
	private static Vector2 CELL_SIZE 	= new Vector2(70, 70);
	private static Vector2 BOARD_SIZE 	= new Vector2(12 * (CELL_SIZE.x + PADDING) + SPACER_WIDTH, 10 * (CELL_SIZE.y + PADDING) + SPACER_HEGIHT);
	private Board _board;

	public override void _Ready()
	{
		_board = GetParent<Board>() as Board;
	}

	public void RenderBoard(int[,] _points) 
	{
		for (int column = 0; column < _points.GetLength(0); column++) 
		{
			for (int position = 0; position < _points.GetLength(1); position++) 
			{
				if (_points[column, position] == 0) continue;

				Color color = _points[column, position] == 1 ? RED : BLACK;

				float xOffset = SPACER_WIDTH;

				if (column < 12)
				{	
					if (column < 6) 
					{
						xOffset = 0;
					}

					DrawRect(new Rect2(new Vector2(BOARD_SIZE.x - (column + 1) * (CELL_SIZE.x + PADDING) - xOffset, BOARD_SIZE.y - (position + 1) * (CELL_SIZE.y + PADDING) + SPACER_HEGIHT), CELL_SIZE), color);
					continue;
				}

				if (column > 17) 
				{
					xOffset = 2 * SPACER_WIDTH;
				}

				DrawRect(new Rect2(new Vector2(column * (CELL_SIZE.x + PADDING) - BOARD_SIZE.x + xOffset, position * (CELL_SIZE.y + PADDING)), CELL_SIZE), color);
			}
		}
	}

	public override void _Draw()
	{
		RenderBoard(_board.BoardPoints);
	}

	public bool isPositionOnBoard(float mouseX, float mouseY) 
	{
		return this.Position.x < mouseX && mouseX < this.Position.x + BOARD_SIZE.x && this.Position.y < mouseY && mouseY < this.Position.y + BOARD_SIZE.y;
	}

	public int getColumn(float mouseX, float mouseY)
	{
		int column;
		float halfHeight 	= BOARD_SIZE.y * 0.5f;
		float halfWidth 	= BOARD_SIZE.x * 0.5f;
		float xOffset 		= 0;

		if (mouseX >  halfWidth) {
			xOffset = SPACER_WIDTH;
		}

		column = 12 - Mathf.CeilToInt(((mouseX - xOffset) / (BOARD_SIZE.x - SPACER_WIDTH)) * 12);

		if (mouseY < halfHeight)
		{	
			column = 12 + Mathf.FloorToInt(((mouseX - xOffset) / (BOARD_SIZE.x - SPACER_WIDTH)) * 12);
		}

		return Mathf.Clamp( column, 0, 23 );
	}
}
