using Godot;
using System;

public class Board : Node2D
{
	BoardRenderer _renderer;

	private int[,] _boardPoints = new int[24, 5];
	public int[,] BoardPoints {
		get { return _boardPoints; }
	}
	private int activeColumn 	= -1;

	public override void _Ready()
	{
		AddPiece(1, 0);
		AddPiece(1, 0);
		AddPiece(1, 0);
		AddPiece(1, 0);
		AddPiece(1, 0);

		AddPiece(1, 12);
		AddPiece(1, 12);
		AddPiece(2, 12);
		AddPiece(1, 12);
		AddPiece(1, 12);

		_renderer = GetNode<BoardRenderer>("BoardRenderer") as BoardRenderer;
	}

	public bool isValidMove(int player, int destinationColumn) 
	{
		return false;
	}

	private int getTopPiece(int column) 
	{
		for (int i = 4; i >= 0; i--) 
		{
 			if (_boardPoints[column, i] != 0) return i;
		}

		return -1;
	}

	public int getNextFreeSlot(int column) 
	{
		for (int i = 0; i < 5; i++)
		{
 			if (_boardPoints[column, i] == 0) return i;
		}

		return -1;
	}

	public void AddPiece(int player, int column) 
	{
		if (column > 23)
		{
			GD.Print("Column position exceeds board size");
			return;
		}

		int freeSlot = getNextFreeSlot(column);

		if (freeSlot == -1) return;

		_boardPoints[column, freeSlot] = player;
	}

	public void MovePiece(int fromColumn, int toColumn) 
	{
		int topPiece = getTopPiece(fromColumn);

		if (topPiece == -1) 
		{
			GD.Print("Cannot move piece from an empty column.");
			return;
		}

		int freeSlot = getNextFreeSlot(toColumn);

		if (freeSlot == -1) 
		{
			GD.Print("Cannot move piece to a full column.");
			return;
		}

		int piece = _boardPoints[fromColumn, topPiece];
		_boardPoints[toColumn, freeSlot] = piece;
		_boardPoints[fromColumn, topPiece] = 0;

		_renderer.Update();
	}

	public override void _Process(float delta) 
	{
		if (Input.IsActionJustPressed("mouse_left_button")) 
		{
			float mouseX = GetViewport().GetMousePosition().x;
			float mouseY = GetViewport().GetMousePosition().y;
			this.activeColumn = _renderer.getColumn(mouseX, mouseY);
		}

		if (Input.IsActionJustReleased("mouse_left_button")) 
		{
			float mouseX = GetViewport().GetMousePosition().x;
			float mouseY = GetViewport().GetMousePosition().y;

			int targetColumn = _renderer.getColumn(mouseX, mouseY);
			if (this.activeColumn != targetColumn) {
				MovePiece(this.activeColumn, targetColumn);
				this.activeColumn = -1;
			}

		}
	}
}
