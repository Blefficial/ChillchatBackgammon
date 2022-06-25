using Godot;
using System;
using System.Collections.Generic;

public class Board : Node2D
{
	BoardRenderer _renderer;
	RandomNumberGenerator _rng;

	private int[,] _boardPoints = new int[24, 5];
	public int[,] BoardPoints {
		get { return _boardPoints; }
	}
	//private int[] _moves 		= new int[4];
	private List<int> _moves 	= new List<int>();
	private int activeColumn 	= -1;
	private int _activePlayer = 1;

	public override void _Ready()
	{
		AddPiece(1, 11);
		AddPiece(1, 11);
		AddPiece(1, 11);
		AddPiece(1, 11);
		AddPiece(1, 11);

		AddPiece(1, 18);
		AddPiece(1, 18);
		AddPiece(1, 18);
		AddPiece(1, 18);
		AddPiece(1, 18);

		AddPiece(1, 16);
		AddPiece(1, 16);
		AddPiece(1, 16);

		AddPiece(1, 0);
		AddPiece(1, 0);

		AddPiece(2, 5);
		AddPiece(2, 5);
		AddPiece(2, 5);
		AddPiece(2, 5);
		AddPiece(2, 5);

		AddPiece(2, 12);
		AddPiece(2, 12);
		AddPiece(2, 12);
		AddPiece(2, 12);
		AddPiece(2, 12);

		AddPiece(2, 7);
		AddPiece(2, 7);
		AddPiece(2, 7);

		AddPiece(2, 23);
		AddPiece(2, 23);

		_renderer 	= GetNode<BoardRenderer>("BoardRenderer") as BoardRenderer;
		_rng		= new RandomNumberGenerator();

		ThrowDice();
		HasValidMove(this._activePlayer, _moves.ToArray());
	}

	private int GetTopPiece(int column) 
	{
		for (int i = 4; i >= 0; i--) 
		{
 			if (_boardPoints[column, i] != 0) return i;
		}

		return -1;
	}

	public int GetNextFreeSlot(int column) 
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

		int freeSlot = GetNextFreeSlot(column);

		if (freeSlot == -1) return;

		_boardPoints[column, freeSlot] = player;
	}

	public void ReplacePiece(int fromColumn, int toColumn) {
		int topPiece = GetTopPiece(fromColumn);
		int piece 	 = _boardPoints[fromColumn, topPiece];

		_boardPoints[toColumn, 0] 			= piece;
		_boardPoints[fromColumn, topPiece] 	= 0;
		_renderer.Update();
	}

	public void MovePiece(int fromColumn, int toColumn)
	{
		int topPiece = GetTopPiece(fromColumn);
		int freeSlot = GetNextFreeSlot(toColumn);
		int piece 	 = _boardPoints[fromColumn, topPiece];

		_boardPoints[toColumn, freeSlot] 	= piece;
		_boardPoints[fromColumn, topPiece] 	= 0;
		_renderer.Update();
	}
	public bool IsValidMove(int player, int fromColumn, int destinationColumn) 
	{
		return false;
	}
	
	public void ThrowDice() {
		_rng.Randomize();
		int dice1 = _rng.RandiRange(1, 6);

		_rng.Randomize();
		int dice2 = _rng.RandiRange(1, 6);

		if (dice1 == dice2) {
			_moves.Add(dice1);
			_moves.Add(dice1);
			_moves.Add(dice1);
			_moves.Add(dice1);

			GD.Print(String.Format("Player {0,1:D} rolled the dice [{1,1:D}], [{2,1:D}], [{3,1:D}], [{4,1:D}]", _activePlayer, _moves[0], _moves[1], _moves[2], _moves[3]));
			return;
		} 

		_moves.Add(dice1);
		_moves.Add(dice2);

		GD.Print(String.Format("Player {0,1:D} rolled the dice [{1,1:D}], [{2,1:D}]", _activePlayer, _moves[0], _moves[1]));
	}

	public bool HasValidMove(int player, int[] moves) {
		int direction = player == 1 ? 1 : -1;

		foreach	(int amount in moves) {
			if (amount == 0) continue;

			for (int column = 0; column < _boardPoints.GetLength(0); column++)
			{
				int topPieceIndex = GetTopPiece(column);
				if (topPieceIndex == -1) continue; // Move onto next column if this column contains no pieces.

				if (_boardPoints[column, topPieceIndex] != player) continue; // Move onto next column if the piece is not owned by player.

				int move = amount * direction;
				if (column + move > _boardPoints.GetLength(0) || column + move < 0) continue; // TODO: Add bearing off feature.

				topPieceIndex = GetTopPiece(column + move);
				if (topPieceIndex < 0) 
				{
					GD.Print(String.Format("Found a valid move {0:D}->{1:D}", column, column + move));
					return true; // If the target column is empty or contains only one opponent piece, in which case the target piece can be moved there.
				} 

				if (_boardPoints[column + move, topPieceIndex] == player)
				{
					GD.Print(String.Format("Found a valid move {0:D}->{1:D}", column, column + move));
					return true; // If the top piece is owned by the player, the piece can be moved there.
				} 
			}
		}
		
		GD.Print("Failed to find a valid move");
		return false;
	}

	private void _ChangeTurn()
	{
		this._activePlayer = this._activePlayer == 1 ? 2 : 1;
		GD.Print("Player " + _activePlayer + "'s turn!");
		_moves.Clear();
		ThrowDice();
		if (!HasValidMove(this._activePlayer, _moves.ToArray())) _ChangeTurn();
	}

	private void _UseMove(int pointsMoved) 
	{
		_moves.Remove(pointsMoved);
	}

	private void _OnSuccessfulMove(int fromColumn, int toColumn) 
	{
		int pointsMoved = Math.Abs(toColumn - fromColumn);
		_UseMove(pointsMoved);

		string movesLeft = "Player" + this._activePlayer + " has " + _moves.Count + " move(s) left:";

		foreach (int move in _moves)
		{
			movesLeft += String.Format(" [{0:D}] ", move);
		}

		GD.Print(movesLeft);

		if (_moves.Count == 0|| !HasValidMove(this._activePlayer, _moves.ToArray()))
		{
			_ChangeTurn();
		}
	}

	public bool CanReach(int fromColumn, int toColumn)
	{

		if (this._activePlayer == 1)
		{
			if (fromColumn > toColumn) {
				GD.Print("Unable to reach: Player 1 must move clockwise.");
				return false;
			}
		} 
		else if (fromColumn < toColumn)
		{
			GD.Print("Unable to reach: Player 2 must move counter-clockwise.");
			return false;
		}

		int distance = Mathf.Abs(fromColumn - toColumn);

		foreach (int move in _moves) 
		{
			if (move == distance) return true;
		}

		return false;
	}

	public void AttemptMove(int fromColumn, int toColumn) 
	{
		if (fromColumn == toColumn) return;
		
		if (!CanReach(fromColumn, toColumn))
		{
			GD.Print("Move failed: No fitting moves.");
			return;
		}

		int topPieceIndex = GetTopPiece(fromColumn);
		if (topPieceIndex == -1) {
			GD.Print("Move failed: No piece in column: " + fromColumn);
			return;
		}

		if (_boardPoints[fromColumn, topPieceIndex] != this._activePlayer)
		{
			GD.Print("Move failed: Tried to move opponets piece from column: " + fromColumn + "->" + toColumn);
			return;
		}

		topPieceIndex = GetTopPiece(toColumn);
		if (topPieceIndex < 0)
		{
			GD.Print("Move success: " + fromColumn + "->" + toColumn);
			MovePiece(fromColumn, toColumn);
			_OnSuccessfulMove(fromColumn, toColumn);
			return;
		}

		if (_boardPoints[toColumn, topPieceIndex] != this._activePlayer)
		{
			if (topPieceIndex == 0)
			{
				GD.Print("Move success: " + fromColumn + "->" + toColumn);
				ReplacePiece(fromColumn, toColumn);
				_OnSuccessfulMove(fromColumn, toColumn);
				return;
			}
		}

		if (_boardPoints[toColumn, topPieceIndex] == this._activePlayer)
		{
			if (topPieceIndex < 4)
			{
				GD.Print("Move success: " + fromColumn + "->" + toColumn);
				MovePiece(fromColumn, toColumn);
				_OnSuccessfulMove(fromColumn, toColumn);
				return;
			}

			GD.Print("Move failed: toColumn is full " + fromColumn + "->" + toColumn);
		}
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("mouse_left_button"))
		{
			float mouseX 		= GetViewport().GetMousePosition().x;
			float mouseY 		= GetViewport().GetMousePosition().y;
			this.activeColumn 	= _renderer.getColumn(mouseX, mouseY);
		}

		if (Input.IsActionJustReleased("mouse_left_button"))
		{
			float mouseX 		= GetViewport().GetMousePosition().x;
			float mouseY 		= GetViewport().GetMousePosition().y;
			int targetColumn 	= _renderer.getColumn(mouseX, mouseY);
			
			AttemptMove(this.activeColumn, targetColumn);
		}
	}
}
