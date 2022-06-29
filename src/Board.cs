using Godot;
using System;
using System.Collections.Generic;

public class Board : Node2D
{
	RandomNumberGenerator _rng;

	private int[,] _boardPoints = new int[Constants.BOARD_POINT_COUNT, Constants.BOARD_POINT_SIZE];
	public int[,] BoardPoints 	{ get { return _boardPoints; } }

	private static int _boardHalfSize 	= Constants.BOARD_WALL_HALF_SIZE;
	private int[] _wall					= new int[_boardHalfSize * 2];
	public int[] Wall 					{ get { return _wall; } }

	private List<int> _moves 	= new List<int>();
	private int activeColumn 	= -1;
	private int _activePlayer 	= 1;
	private int[] _helperMove	= new int[2] { -1, -1 };
	private Vector2 _boardSize	= Constants.BOARD_SIZE;
	private float _spacerWidth 	= Constants.BOARD_SPACER_WIDTH;

	[Signal]
	delegate void OnBoardUpdate();
	[Signal]
	delegate void OnMovePiece(int[] from, int[] to);
	[Signal]
	delegate void OnMovePieceToWall(int wallIndex);
	[Signal]
	delegate void OnMovePieceFromWall(int wallIndex, int[] to);
	[Signal]
	delegate void OnDiceThrow(List<int> result);
	[Signal]
	delegate void MoveUsed(int index);
	[Signal]
	delegate void UpdateMovesLeft(List<int> movesLeft);
	[Signal]
	delegate void OnBearOff(int player, int fromColumn, int pieceIndex);
	[Signal]
	delegate void OnGameAlert(string alert);

	public override void _Ready()
	{
		_rng		= new RandomNumberGenerator();
	}

	public void Initialize()
	{
		//_AddDefaultPieces();

		_AddPiece(1, 18);
		_AddPiece(2, 0);
		_AddPiece(2, 1);
		_AddPiece(2, 2);
		_AddPiece(2, 3);
		_AddPiece(2, 4);
		_AddPiece(2, 5);

		_wall[0] = 1;
		_wall[1] = 1;
		_wall[2] = 1;
		_wall[3] = 1;
		_wall[4] = 1;
		_wall[5] = 1;
		_wall[6] = 1;
		_wall[7] = 1;
		_wall[8] = 1;
		_wall[9] = 1;

		ThrowDice();
		HasValidMove(this._activePlayer, _moves.ToArray());
		EmitSignal("OnBoardUpdate");
	}

	private void _AddDefaultPieces()
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
	}

	private void _SendGameAlert(string alert)
	{
		EmitSignal("OnGameAlert", alert);
	}

	private int _GetTopPieceIndex(int column) 
	{
		if (column < 0 || column >= Constants.BOARD_POINT_COUNT) return -1;
		
		for (int i = 4; i >= 0; i--)
		{
 			if (_boardPoints[column, i] != 0) return i;
		}

		return -1;
	}

	private int _GetNextFreeSlot(int column) 
	{
		for (int i = 0; i < Constants.BOARD_POINT_SIZE; i++)
		{
 			if (_boardPoints[column, i] == 0) return i;
		}

		return -1;
	}

	private int _GetTopWallPiece(int player) 
	{
		int startIndex 	= player == 1 ? 0 : _boardHalfSize - 1;
		int endIndex 	= player == 1 ? _boardHalfSize : _boardHalfSize * 2;

		for (int i = startIndex; i < endIndex; i++) 
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
		//EmitSignal("OnBoardUpdate");
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
		EmitSignal("OnMovePieceToWall", nextFreeWallSlot);
	}

	private void _ReplacePiece(int fromColumn, int toColumn)
	{
		int topPiece = _GetTopPieceIndex(fromColumn);
		int piece 	 = _boardPoints[fromColumn, topPiece];

		_boardPoints[toColumn, 0] 			= piece;
		_boardPoints[fromColumn, topPiece] 	= 0;
		_AddPieceToWall(3 - _activePlayer);
		
		EmitSignal("OnMovePiece", new int[]{ fromColumn, topPiece }, new int[]{ toColumn, 0 });
	}

	private void _MovePiece(int fromColumn, int toColumn)
	{
		int topPiece = _GetTopPieceIndex(fromColumn);
		int freeSlot = _GetNextFreeSlot(toColumn);
		int piece 	 = _boardPoints[fromColumn, topPiece];

		_boardPoints[toColumn, freeSlot] 	= piece;
		_boardPoints[fromColumn, topPiece] 	= 0;

		EmitSignal("OnMovePiece", new int[]{ fromColumn, topPiece }, new int[]{ toColumn, freeSlot });
	}

	public void ThrowDice()
	{
		_rng.Randomize();
		int dice1 = _rng.RandiRange(1, 6);

		_rng.Randomize();
		int dice2 = _rng.RandiRange(1, 6);

		_moves.Add(dice1);
		_moves.Add(dice2);

		if (dice1 == dice2) 
		{
			_moves.Add(dice1);
			_moves.Add(dice1);
		}

		string result = String.Format("Player {0,1:D} rolled the dice: ", _activePlayer);

		foreach (int move in _moves)
		{
			result += String.Format("[{0:D}]", move);
		}

		GD.Print(result);
		EmitSignal("OnDiceThrow", _moves);
	}

	private void _OnGameFinished()
	{
		_SendGameAlert(String.Format("Game Finished! " + (_activePlayer == 1 ? "RED" : "WHITE") + " is the winner!"));
	}

	private void _OnBearOff(int player, int move)
	{
		_UseMove(move);
		EmitSignal("OnBearOff");

		for (int i = 0; i < _boardPoints.GetLength(0); i++)
		{
			if (_boardPoints[i, 0] == player) break;

			_OnGameFinished();
		}

		if (_moves.Count == 0)
		{
			_ChangeTurn();
		}
	}

	private void _BearOff(int player, int fromColumn, int pieceIndex)
	{
		_boardPoints[fromColumn, pieceIndex] = 0;
	}
	
	private void _AttemptBearOff(int player, int fromColumn)
	{
		int direction = player == 1 ? 1 : -1;
		int homeOffset = player == 1 ? 24 : -1;
		foreach (int move in _moves)
		{
			int targetColumn = homeOffset - (move * direction);
			int topPieceIndex = _GetTopPieceIndex(fromColumn);
			if (targetColumn == fromColumn && topPieceIndex > -1)
			{
				_OnBearOff(player, move);
				_BearOff(player, fromColumn, topPieceIndex);
				return;
			}
		}

		GD.Print("Cannot bear off attempted piece");
	}

	private bool _CanBearOff(int player)
	{
		for (int column = 6; column < 18; column++)
		{
			if (_boardPoints[column, 0] == player) return false;
		}
		
		return true;
	}

	private bool _IsWallOccupied(int player)
	{
		return player == 1 ? _GetNextFreeWallSlot(player) > 0 : _GetNextFreeWallSlot(player) > 10;
	}

	public int[] GetFirstValidMove(int player, int[] moves) // TODO: Perhaps rework to return a int[,,,] instead, containing int[fromColumn, fromIndex, toColumn, toIndex];
	{
		int direction = player == 1 ? 1 : -1;

		if (_IsWallOccupied(player))
		{
			foreach (int amount in moves)
			{
				int column  = player == 1 ? amount - 1 : 24 - amount;
				int topPieceIndex 	= _GetTopPieceIndex(column);
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

		if (_CanBearOff(player))
		{
			int homeOffset = player == 1 ? 24 : -1;
			
			foreach (int move in moves)
			{
				int fromColumn = homeOffset - (move * direction);
				if (_boardPoints[fromColumn, 0] == player)
				{
					GD.Print(String.Format("Found a valid move {0:D}->HOME", fromColumn));
					return new int[] { fromColumn, 0 };
				}
			}
		}

		foreach	(int amount in moves)
		 {
			if (amount == 0) continue;

			for (int column = 0; column < _boardPoints.GetLength(0); column++)
			{
				int topPieceIndex = _GetTopPieceIndex(column);
				if (topPieceIndex == -1) continue;

				if (_boardPoints[column, topPieceIndex] != player) continue;

				int move = amount * direction;
				int targetColumn = column + move;
				if (targetColumn > _boardPoints.GetLength(0) - 1 || targetColumn < 0) continue; // TODO: Add bearing off feature.

				topPieceIndex = _GetTopPieceIndex(targetColumn);
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
				int column  		= player == 1 ? amount - 1 : 24 - amount;
				int topPieceIndex 	= _GetTopPieceIndex(column);
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

		if (_CanBearOff(player))
		{
			int homeOffset = player == 1 ? 24 : -1;
			
			foreach (int move in moves)
			{
				int fromColumn = homeOffset - (move * direction);
				if (_boardPoints[fromColumn, 0] == player)
				{
					GD.Print(String.Format("Found a valid move {0:D}->HOME", fromColumn));
					return true;
				}
			}
		}

		foreach	(int amount in moves)
		 {
			if (amount == 0) continue;

			for (int column = 0; column < _boardPoints.GetLength(0); column++)
			{
				int topPieceIndex = _GetTopPieceIndex(column);
				if (topPieceIndex == -1) continue; // Move onto next column if this column contains no pieces.

				if (_boardPoints[column, topPieceIndex] != player) continue; // Move onto next column if the piece is not owned by player.

				int move = amount * direction;
				if (column + move > _boardPoints.GetLength(0) - 1 || column + move < 0) continue; // TODO: Add bearing off feature.

				topPieceIndex = _GetTopPieceIndex(column + move);
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
		this._activePlayer 	= this._activePlayer == 1 ? 2 : 1;
		string player 		= this._activePlayer == 1 ? "RED" : "WHITE";
		GD.Print("Player " + _activePlayer + "'s turn!");
		_moves.Clear();
		ThrowDice();
		if (!HasValidMove(this._activePlayer, _moves.ToArray()))
		{
			_ChangeTurn();
			return;
		}

		_SendGameAlert(player + "'s turn!");
	}

	private void _UseMove(int pointsMoved) 
	{
		EmitSignal("MoveUsed", _moves.FindLastIndex((match) => match == pointsMoved));
		_moves.Remove(pointsMoved);
	}

	private void _OnSuccessfulMove(int distance)
	{
		_SetHelperMove(-1, -1);
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
				_SendGameAlert("RED must move clockwise");
				return false;
			}
		} 
		else if (fromColumn < toColumn)
		{
			GD.Print("Unable to reach: Player 2 must move counter-clockwise.");
			_SendGameAlert("WHITE must move counter-clockwise");
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
		
		int freeSlot 	= _GetNextFreeSlot(toColumn);
		int wallIndex 	= _GetTopWallPiece(_activePlayer);

		_boardPoints[toColumn, freeSlot] = _activePlayer;
		_wall[wallIndex] = 0;

		EmitSignal("OnMovePieceFromWall", wallIndex, new int[] { toColumn, freeSlot });
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
			_SendGameAlert("Unable to move from wall, no moves left matched the attempted move.");
			return;
		}

		int topPieceIndex = _GetTopPieceIndex(toColumn);
		if (topPieceIndex > 4)
		{
			GD.Print("Unable to move from wall, target column is full.");
			_SendGameAlert("Unable to move from wall, target column is full.");
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

	private void _AttemptMove(int fromColumn, int toColumn) 
	{
		if (fromColumn == toColumn) return;
		
		if (!CanReach(fromColumn, toColumn))
		{
			GD.Print("Move failed: No fitting moves.");
			return;
		}

		int topPieceIndex = _GetTopPieceIndex(fromColumn);
		if (topPieceIndex == -1) {
			GD.Print("Move failed: No piece in column: " + fromColumn);
			return;
		}

		if (_boardPoints[fromColumn, topPieceIndex] != this._activePlayer)
		{
			GD.Print("Move failed: Tried to move opponets piece from column: " + fromColumn + "->" + toColumn);
			return;
		}

		topPieceIndex = _GetTopPieceIndex(toColumn);
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

	public bool IsMouseInsideHome(float mouseX, float mouseY)
	{
		float leftBorder 	= this.Position.x + _boardSize.x + 10;
		float rightBorder 	= this.Position.x + _boardSize.x + 110;
		float bottomBorder 	= this.Position.y + _boardSize.y;

		return mouseX > leftBorder && mouseX < rightBorder && mouseY > this.Position.y && mouseY < bottomBorder;
	}

	public bool IsMouseInsideWall(float mouseX, float mouseY)
	{
		float leftBorder 	= this.Position.x + _boardSize.x * 0.5f - _spacerWidth * 0.5f;
		float rightBorder 	= this.Position.x + _boardSize.x * 0.5f + _spacerWidth * 0.5f;
		float bottomBorder 	= this.Position.y + _boardSize.y;

		return mouseX > leftBorder && mouseX < rightBorder && mouseY > this.Position.y && mouseY < bottomBorder;
	}

	public int GetColumn(float mouseX, float mouseY)
	{
		int column;
		float halfHeight 	= _boardSize.y * 0.5f;
		float halfWidth 	= _boardSize.x * 0.5f;
		float xOffset 		= 0;

		if (mouseX > this.Position.x + halfWidth)
		{
			xOffset = _spacerWidth;
		}

		column = 12 - Mathf.CeilToInt((( Mathf.Max(mouseX - xOffset - this.Position.x, 0)) / (_boardSize.x - _spacerWidth)) * 12);

		if (mouseY < this.Position.y + halfHeight)
		{	
			column = 12 + Mathf.FloorToInt((( Mathf.Max(mouseX - xOffset - this.Position.x, 0)) / (_boardSize.x - _spacerWidth)) * 12);
		}

		return Mathf.Clamp( column, 0, 23 );
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("mouse_left_button"))
		{
			Vector2 mousePosition = GetViewport().GetMousePosition();

			if (_IsWallOccupied(_activePlayer))
			{
				if (IsMouseInsideWall(mousePosition.x, mousePosition.y))
				{
					this.activeColumn = -2;
					return;
				}
			}

			this.activeColumn = GetColumn(mousePosition.x, mousePosition.y);
		}

		if (Input.IsActionJustReleased("mouse_left_button"))
		{
			Vector2 mousePosition 	= GetViewport().GetMousePosition();
			int targetColumn 		= GetColumn(mousePosition.x, mousePosition.y);

			if (_IsWallOccupied(_activePlayer))
			{
				if (this.activeColumn == -2)
				{
					_AttemptMoveFromWall(targetColumn);
					return;
				}

				GD.Print("Player must move a piece from the wall!");
				_SendGameAlert("Player must move a piece from the wall!");
				return;
			}

			if (IsMouseInsideHome(mousePosition.x, mousePosition.y) && _CanBearOff(_activePlayer))
			{
				_AttemptBearOff(this._activePlayer, this.activeColumn);
				return;
			}

			_AttemptMove(this.activeColumn, targetColumn);
		}

		if (Input.IsActionJustPressed("show_help"))
		{
			int[] move = GetFirstValidMove(this._activePlayer, _moves.ToArray());
			_SetHelperMove(move[0], move[1]);
		}
	}
}
