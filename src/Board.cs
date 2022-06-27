using Godot;
using System;
using System.Collections.Generic;

public class Board : Node2D
{
	BoardRenderer _renderer;
	Label _diceLabel;
	RandomNumberGenerator _rng;

	private int[,] _boardPoints = new int[24, 5];
	public int[,] BoardPoints 	{ get { return _boardPoints; } }
	private int[] _wall			= new int[20];
	public int[] Wall 			{ get {return _wall; } }
	private List<int> _moves 	= new List<int>();
	private int activeColumn 	= -1;
	private int _activePlayer 	= 1;
	private int[] _helperMove	= new int[2] { -1, -1 };

	[Signal]
	delegate void OnBoardUpdate();

	public override void _Ready()
	{
		_AddPiece(1, 11);
		_AddPiece(1, 11);
		_AddPiece(1, 11);
		_AddPiece(1, 11);
		_AddPiece(1, 11);

		_AddPiece(1, 18);
		_AddPiece(1, 18);
		_AddPiece(1, 18);
		_AddPiece(1, 18);
		_AddPiece(1, 18);

		_AddPiece(1, 16);
		_AddPiece(1, 16);
		_AddPiece(1, 16);

		_AddPiece(1, 0);
		_AddPiece(1, 0);

		_AddPiece(2, 5);
		_AddPiece(2, 5);
		_AddPiece(2, 5);
		_AddPiece(2, 5);
		_AddPiece(2, 5);

		_AddPiece(2, 12);
		_AddPiece(2, 12);
		_AddPiece(2, 12);
		_AddPiece(2, 12);
		_AddPiece(2, 12);

		_AddPiece(2, 7);
		_AddPiece(2, 7);
		_AddPiece(2, 7);

		_AddPiece(2, 23);
		_AddPiece(2, 23);

		_renderer 	= GetNode<BoardRenderer>("BoardRenderer") as BoardRenderer;
		_diceLabel 	= GetNode<Label>("DiceLabel") as Label;
		_rng		= new RandomNumberGenerator();

		_wall[0] = 1;
		_wall[10] = 2;

		ThrowDice();
		HasValidMove(this._activePlayer, _moves.ToArray());
		EmitSignal("OnBoardUpdate");
	}

	private int _GetTopPiece(int column) 
	{
		for (int i = 4; i >= 0; i--) 
		{
 			if (_boardPoints[column, i] != 0) return i;
		}

		return -1;
	}

	private int _GetNextFreeSlot(int column) 
	{
		for (int i = 0; i < 5; i++)
		{
 			if (_boardPoints[column, i] == 0) return i;
		}

		return -1;
	}

	private int _GetTopWallPiece(int player) 
	{
		int count = player == 1 ? 0 : 10;
		for (int i = count + 9; i >= count; i--) 
		{
 			if (_wall[i] != 0) return i;
		}

		return -1;
	}

	private int _GetNextFreeWallSlot(int player)
	{
		if (player == 1) {
			for (int slot = 0; slot < 10; slot++) {
				if (_wall[slot] == 0) return slot;
			}
		}
		else
		{
			for (int slot = 10; slot < 20; slot++) {
				if (_wall[slot] == 0) return slot;
			}
		}

		return -1;
	}

	private void _SetHelperMove(int column, int position)
	{
		_helperMove[0] = column;
		_helperMove[1] = position;
		GD.Print("Set helper move " + column + ", " + position);
		_renderer.Update();
	}

	public bool IsHelperMove(int column, int position)
	{
		return _helperMove[0] == column && _helperMove[1] == position;
	}

	private void _AddPiece(int player, int column)
	{
		if (column > 23)
		{
			GD.Print("Column position exceeds board size");
			return;
		}

		int freeSlot = _GetNextFreeSlot(column);
		if (freeSlot == -1) return;

		_boardPoints[column, freeSlot] = player;
	}

	private void _AddPieceToWall(int player)
	{
		int nextFreeWallSlot 	= _GetNextFreeWallSlot(player);
		_wall[nextFreeWallSlot] = player;
		_renderer.Update();
	}

	private void _ReplacePiece(int fromColumn, int toColumn)
	{
		int topPiece = _GetTopPiece(fromColumn);
		int piece 	 = _boardPoints[fromColumn, topPiece];

		_boardPoints[toColumn, 0] 			= piece;
		_boardPoints[fromColumn, topPiece] 	= 0;
		_AddPieceToWall(3 - _activePlayer);
	}

	private void _MovePiece(int fromColumn, int toColumn)
	{
		int topPiece = _GetTopPiece(fromColumn);
		int freeSlot = _GetNextFreeSlot(toColumn);
		int piece 	 = _boardPoints[fromColumn, topPiece];

		_boardPoints[toColumn, freeSlot] 	= piece;
		_boardPoints[fromColumn, topPiece] 	= 0;
	}

	private void _UpdateDiceLabel() {
		String rolls = "Player" + _activePlayer + ": ";
		foreach(int move in _moves) 
			{
				rolls+= String.Format(" [{0,1:D}] ", move);
			}
		_diceLabel.Text = rolls;
	}
	
	public void ThrowDice()
	{
		_rng.Randomize();
		int dice1 = _rng.RandiRange(1, 6);

		_rng.Randomize();
		int dice2 = _rng.RandiRange(1, 6);

		if (dice1 == dice2) 
		{
			_moves.Add(dice1);
			_moves.Add(dice1);
			_moves.Add(dice1);
			_moves.Add(dice1);

			GD.Print(String.Format("Player {0,1:D} rolled the dice [{1,1:D}], [{2,1:D}], [{3,1:D}], [{4,1:D}]", _activePlayer, _moves[0], _moves[1], _moves[2], _moves[3]));
			_UpdateDiceLabel();
			return;
		} 

		_moves.Add(dice1);
		_moves.Add(dice2);

		GD.Print(String.Format("Player {0,1:D} rolled the dice [{1,1:D}], [{2,1:D}]", _activePlayer, _moves[0], _moves[1]));
		_UpdateDiceLabel();
	}

	private bool _IsWallOccupied(int player)
	{
		return player == 1 ? _GetNextFreeWallSlot(player) > 0 : _GetNextFreeWallSlot(player) > 10;
	}

	public int[] GetFirstValidMove(int player, int[] moves)
	{
		int direction = player == 1 ? 1 : -1;

		if (_IsWallOccupied(player))
		{
			foreach (int amount in moves)
			{
				int column  = player == 1 ? amount - 1 : 24 - amount;
				int topPieceIndex 	= _GetTopPiece(column);
				if (topPieceIndex == -1)
				{
					return new int[] { column, 0 };
				}
				
				if (topPieceIndex >= 4) continue;

				if (_boardPoints[column, topPieceIndex] == player)
				{
					return new int[] { column, topPieceIndex + 1 };
				}

				if (topPieceIndex == 0)
				{
					return new int[] { column, 0 };
				}
			}

			return new int[2] { -1, -1 };
		}

		foreach	(int amount in moves)
		 {
			if (amount == 0) continue;

			for (int column = 0; column < _boardPoints.GetLength(0); column++)
			{
				int topPieceIndex = _GetTopPiece(column);
				if (topPieceIndex == -1) continue;

				if (_boardPoints[column, topPieceIndex] != player) continue;

				int move = amount * direction;
				int targetColumn = column + move;
				if (targetColumn > _boardPoints.GetLength(0) || targetColumn < 0) continue; // TODO: Add bearing off feature.

				topPieceIndex = _GetTopPiece(targetColumn);
				if (topPieceIndex < 0) 
				{
					return new int[] { targetColumn, 0 };
				} 

				if (_boardPoints[targetColumn, topPieceIndex] == player)
				{
					if (topPieceIndex >= 4) continue;

					return new int[] { targetColumn, topPieceIndex + 1 };
				} 
			}
		}
		
		return new int[2] { -1, -1 };
	}

	public bool HasValidMove(int player, int[] moves)
	{
		int direction = player == 1 ? 1 : -1;

		if (_IsWallOccupied(player)) // Player has pieces on the wall and cannot move any other pieces.
		{
			foreach (int amount in moves)
			{
				int column  = player == 1 ? amount - 1 : 24 - amount;
				int topPieceIndex 	= _GetTopPiece(column);
				if (topPieceIndex == -1)
				{
					GD.Print(String.Format("Found a valid move WALL->{0:D}", column));
					return true;
				}
				
				if (topPieceIndex >= 4) continue;

				if (_boardPoints[column, topPieceIndex] == player)
				{
					GD.Print(String.Format("Found a valid move WALL->{0:D}", column));
					return true;
				}

				if (topPieceIndex == 0)
				{
					GD.Print(String.Format("Found a valid move WALL->{0:D}", column));
					return true;
				}
			}

			GD.Print("Failed to find a valid move");
			return false;
		}

		foreach	(int amount in moves)
		 {
			if (amount == 0) continue;

			for (int column = 0; column < _boardPoints.GetLength(0); column++)
			{
				int topPieceIndex = _GetTopPiece(column);
				if (topPieceIndex == -1) continue; // Move onto next column if this column contains no pieces.

				if (_boardPoints[column, topPieceIndex] != player) continue; // Move onto next column if the piece is not owned by player.

				int move = amount * direction;
				if (column + move > _boardPoints.GetLength(0) || column + move < 0) continue; // TODO: Add bearing off feature.

				topPieceIndex = _GetTopPiece(column + move);
				if (topPieceIndex < 0)
				{
					GD.Print(String.Format("Found a valid move {0:D}->{1:D}", column, column + move));
					return true; // If the target column is empty or contains only one opponent piece, in which case the target piece can be moved there.
				} 

				if (_boardPoints[column + move, topPieceIndex] == player)
				{
					if (topPieceIndex >= 4) continue; // Target column is full.

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
		_UpdateDiceLabel();
	}

	private void _OnSuccessfulMove(int distance)
	{
		_SetHelperMove(-1, -1);
		_renderer.Update();
		_UseMove(distance);
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

		EmitSignal("OnBoardUpdate");
	}

	private void _OnSuccessfulMove(int fromColumn, int toColumn) 
	{
		int pointsMoved = Math.Abs(toColumn - fromColumn);
		_OnSuccessfulMove(pointsMoved);
	}

	public bool CanReach(int fromColumn, int toColumn)
	{
		if (this._activePlayer == 1)
		{
			if (fromColumn > toColumn) 
			{
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

	private void _MoveFromWall(int toColumn)
	{
		int opposingPlayer = 3 - _activePlayer;
		if (_boardPoints[toColumn, 0] == opposingPlayer)
		{
			_boardPoints[toColumn, 0] = 0;
			_AddPieceToWall(opposingPlayer);
		}
		
		int freeSlot = _GetNextFreeSlot(toColumn);

		_boardPoints[toColumn, freeSlot] 		= _activePlayer;
		_wall[_GetTopWallPiece(_activePlayer)] 	= 0;
	}

	private void _AttemptMoveFromWall(int toColumn)
	{
		int distanceToColumn = Mathf.Abs(_activePlayer == 1 ? toColumn + 1 : toColumn - 24);
		bool canReach 		 = false;

		foreach (int move in _moves)
		{
			if (distanceToColumn == move)
			{
				canReach = true;
				break;
			}
		}

		if (!canReach)
		{
			GD.Print("Unable to move from wall, no moves left matched the attempted move.");
			return;
		}

		int topPieceIndex = _GetTopPiece(toColumn);
		if (topPieceIndex > 4)
		{
			GD.Print("Unable to move from wall, target column is full.");
			return;
		};

		if (topPieceIndex == -1)
		{
			GD.Print("Move success: WALL->" + toColumn);
			_MoveFromWall(toColumn);
			_OnSuccessfulMove(distanceToColumn);
			return;
		}

		if (topPieceIndex == 0)
		{
			GD.Print("Move success: WALL->" + toColumn);
			_MoveFromWall(toColumn);
			_OnSuccessfulMove(distanceToColumn);
			return;
		}

		if (_boardPoints[toColumn, topPieceIndex] == _activePlayer)
		{
			GD.Print("Move success: WALL->" + toColumn);
			_MoveFromWall(toColumn);
			_OnSuccessfulMove(distanceToColumn);
			return;
		}

		GD.Print("Failed to move from wall");
	}

	public void AttemptMove(int fromColumn, int toColumn) 
	{
		if (fromColumn == toColumn) return;
		
		if (!CanReach(fromColumn, toColumn))
		{
			GD.Print("Move failed: No fitting moves.");
			return;
		}

		int topPieceIndex = _GetTopPiece(fromColumn);
		if (topPieceIndex == -1) {
			GD.Print("Move failed: No piece in column: " + fromColumn);
			return;
		}

		if (_boardPoints[fromColumn, topPieceIndex] != this._activePlayer)
		{
			GD.Print("Move failed: Tried to move opponets piece from column: " + fromColumn + "->" + toColumn);
			return;
		}

		topPieceIndex = _GetTopPiece(toColumn);
		if (topPieceIndex < 0)
		{
			GD.Print("Move success: " + fromColumn + "->" + toColumn);
			_MovePiece(fromColumn, toColumn);
			_OnSuccessfulMove(fromColumn, toColumn);
			return;
		}

		if (_boardPoints[toColumn, topPieceIndex] != this._activePlayer)
		{
			if (topPieceIndex == 0)
			{
				GD.Print("Move success: " + fromColumn + "->" + toColumn);
				_ReplacePiece(fromColumn, toColumn);
				_OnSuccessfulMove(fromColumn, toColumn);
				return;
			}
		}

		if (_boardPoints[toColumn, topPieceIndex] == this._activePlayer)
		{
			if (topPieceIndex < 4)
			{
				GD.Print("Move success: " + fromColumn + "->" + toColumn);
				_MovePiece(fromColumn, toColumn);
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
			Vector2 mousePosition = GetViewport().GetMousePosition();

			if (_IsWallOccupied(_activePlayer))
			{
				if (_renderer.IsMouseInsideWall(mousePosition.x, mousePosition.y))
				{
					this.activeColumn = -2;
					return;
				}
			}

			this.activeColumn = _renderer.getColumn(mousePosition.x, mousePosition.y);
		}

		if (Input.IsActionJustReleased("mouse_left_button"))
		{
			Vector2 mousePosition 	= GetViewport().GetMousePosition();
			int targetColumn 		= _renderer.getColumn(mousePosition.x, mousePosition.y);

			if (_IsWallOccupied(_activePlayer))
			{
				if (this.activeColumn == -2)
				{
					_AttemptMoveFromWall(targetColumn);
					return;
				}

				GD.Print("Player must move a piece from the wall!");
				return;
			}

			AttemptMove(this.activeColumn, targetColumn);
		}

		if (Input.IsActionJustPressed("show_help"))
		{
			int[] move = GetFirstValidMove(this._activePlayer, _moves.ToArray());
			_SetHelperMove(move[0], move[1]);
		}
	}
}
