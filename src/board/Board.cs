using Godot;
using System;
using System.Collections.Generic;

public class Board : Node2D
{
	RandomNumberGenerator _rng;
	
	private static int 	_boardPointCount 	= Constants.BOARD_POINT_COUNT;
	private static int 	_quarterPointCount 	= _boardPointCount / 4; 
	private static int 	_boardPointSize 	= Constants.BOARD_POINT_SIZE;
	private int[,] 		_boardPoints 		= new int[_boardPointCount, _boardPointSize];
	public int[,] 		BoardPoints 		{ get { return _boardPoints; } }

	private static int 	_wallHalfSize 		= Constants.BOARD_WALL_HALF_SIZE;
	private int[] 		_wall				= new int[_wallHalfSize * 2];
	public int[] 		Wall 				{ get { return _wall; } }

	private List<int> _moves 	= new List<int>();
	private int _activeColumn 	= -1;
	private int _activePlayer 	= 1;
	private Vector2 _boardSize	= Constants.BOARD_SIZE;
	private float _spacerWidth 	= Constants.BOARD_SPACER_WIDTH;
	private float _homeWidth 	= Constants.BOARD_HOME_WIDTH;

	/// States
	///<summary>
	/// (-2) 	Uninitialized,
	/// (-1) 	Paused,
	/// (0) 	Waiting to throw dice,
	/// (1) 	Waiting to move piece,
	/// (2) 	Finished
	///</summary>
	private int _boardState 		= -2;
	private int _previousBoardState = -2;
	private bool[] _hasBorne		= new bool[2];
	
	delegate bool ValidateVictory(int player, int[,] points);
	private Dictionary<string, ValidateVictory> _winConditions = new Dictionary<string, ValidateVictory>();

	private static ValidateVictory defaultWinCondition = (player, points) => 
	{
		for (int i = 0; i < points.GetLength(0); i++)
		{
			if (points[i, 0] == player) return false;
		}

		return true;
	};

	[Signal]
	delegate void OnBoardUpdate();
	[Signal]
	delegate void OnMovePiece(int[] from, int[] to);
	[Signal]
	delegate void OnMovePieceToWall(int player, int wallIndex);
	[Signal]
	delegate void OnMovePieceFromWall(int player, int wallIndex, int[] to);
	[Signal]
	delegate void OnDiceThrow(List<int> result);
	[Signal]
	delegate void OnChangeTurn(int activePlayer);
	[Signal]
	delegate void MoveUsed(int index);
	[Signal]
	delegate void UpdateMovesLeft(List<int> movesLeft);
	[Signal]
	delegate void OnBearOff(int player, int fromColumn, int pieceIndex);
	[Signal]
	delegate void OnGameFinished(string player, string victoryTitle, int scoreGained);
	[Signal]
	delegate void OnGameAlert(string alert);

	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();
		_winConditions.Add("default", defaultWinCondition);
	}

	public void Initialize()
	{
		Reset();
		EmitSignal("OnBoardUpdate");
		_ChangeBoardState(0);
	}

	public void Reset()
	{
		for (int column = 0; column < _boardPoints.GetLength(0); column++)
		{
			for (int position = 0; position < _boardPoints.GetLength(1); position++)
			{
				_boardPoints[column, position] = 0;
			}
		}

		for (int i = 0; i < _wall.GetLength(0); i++)
		{
			_wall[i] = 0;
		}

		// Red pieces

		_boardPoints[11, 0] = 1;
		_boardPoints[11, 1] = 1;
		_boardPoints[11, 2] = 1;
		_boardPoints[11, 3] = 1;
		_boardPoints[11, 4] = 1;

		_boardPoints[18, 0] = 1;
		_boardPoints[18, 1] = 1;
		_boardPoints[18, 2] = 1;
		_boardPoints[18, 3] = 1;
		_boardPoints[18, 4] = 1;

		_boardPoints[16, 0] = 1;
		_boardPoints[16, 1] = 1;
		_boardPoints[16, 2] = 1;

		_boardPoints[0, 0] = 1;
		_boardPoints[0, 1] = 1;

		// White pieces

		_boardPoints[5, 0] = 2;
		_boardPoints[5, 1] = 2;
		_boardPoints[5, 2] = 2;
		_boardPoints[5, 3] = 2;
		_boardPoints[5, 4] = 2;

		_boardPoints[12, 0] = 2;
		_boardPoints[12, 1] = 2;
		_boardPoints[12, 2] = 2;
		_boardPoints[12, 3] = 2;
		_boardPoints[12, 4] = 2;

		_boardPoints[7, 0] = 2;
		_boardPoints[7, 1] = 2;
		_boardPoints[7, 2] = 2;

		_boardPoints[23, 0] = 2;
		_boardPoints[23, 1] = 2;

		_hasBorne[0] = false;
		_hasBorne[1] = false;
		_moves.Clear();
		_activePlayer = 1;
		_ChangeBoardState(0);
		EmitSignal("OnChangeTurn", _activePlayer);
	}

	private void _ChangeBoardState(int state)
	{
		this._previousBoardState = this._boardState;
		this._boardState 	= state;
	}

	public void OnPause()
	{
		if (_boardState == -2) return;

		_ChangeBoardState(_boardState == -1 ? _previousBoardState : -1);
	}

	public void SetStakes()
	{

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
		int startIndex 	= player == 1 ? _wallHalfSize - 1 : _wallHalfSize * 2 - 1;
		int endIndex 	= player == 1 ? 0 : _wallHalfSize;

		for (int i = startIndex; i >= endIndex; i--) 
		{
 			if (_wall[i] != 0) return i;
		}

		return -1;
	}

	private int _GetNextFreeWallSlot(int player)
	{
		int startIndex 	= player == 1 ? 0 : _wallHalfSize;
		int endIndex 	= player == 1 ? _wallHalfSize - 1 : _wallHalfSize * 2;

		for (int i = startIndex; i < endIndex; i++) 
		{
 			if (_wall[i] == 0) return i;
		}

		return -1;
	}

	private void _AddPiece(int player, int column)
	{
		if (column > _boardPoints.GetLength(0))
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

		if (nextFreeWallSlot >= _wallHalfSize * 2 || nextFreeWallSlot < 0) return;

		_wall[nextFreeWallSlot] = player;
		EmitSignal("OnMovePieceToWall", player, nextFreeWallSlot);
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

	private void _OnDiceThrow()
	{
		string result = String.Format("Player {0,1:D} rolled the dice: ", _activePlayer);

		foreach (int move in _moves)
		{
			result += String.Format("[{0:D}]", move);
		}

		GD.Print(result);
		EmitSignal("OnDiceThrow", _moves);
		_ChangeBoardState(1);

		if (!_HasValidMove(_activePlayer, _moves.ToArray()))
		{
			_ChangeTurn();
		}
	}

	private void _ThrowDice()
	{
		_moves.Clear();
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
	}

	private string _GetVictoryTitle(int victoryModifier)
	{
		switch(victoryModifier) 
		{
			case 1:
				return "";
			case Constants.GAMMON_MULTIPLIER:
				return "gammon";
			case Constants.BACKGAMMON_MULTIPLIER:
				return "backgammon";
			default: return "";
		}
	}

	private int _GetVictoryModifier(int player, int opposingPlayer)
	{
		bool hasOpponentBorne		= _hasBorne[opposingPlayer - 1];
		bool opposingWallOccupied 	= _IsWallOccupied(opposingPlayer);
		bool winnerHomeOccupied 	= false;

		int startOffset = player == 1 ? 3 * _quarterPointCount : 0;
		for (int i = startOffset; i < _quarterPointCount + startOffset; i++)
		{
			if (_boardPoints[i, 0] == opposingPlayer) 
			{
				winnerHomeOccupied = true;
				break;
			}
		}
		
		if (!hasOpponentBorne && winnerHomeOccupied || !hasOpponentBorne && opposingWallOccupied) return Constants.BACKGAMMON_MULTIPLIER;

		if (!hasOpponentBorne) return Constants.GAMMON_MULTIPLIER;

		return 1;
	}

	private void _OnGameFinished(int player)
	{
		string winner 		= player == 1 ? Constants.PLAYER_1 : Constants.PLAYER_2;
		string loser 		= player != 1 ? Constants.PLAYER_1 : Constants.PLAYER_2;

		int opposingPlayer 	= 3 - player;
		int victoryModifier = _GetVictoryModifier(player, opposingPlayer);

		int score 			= Constants.STAKES * victoryModifier;
		string victoryTitle = _GetVictoryTitle(victoryModifier);

		PlayerData.IncrementWinCount(winner);
		PlayerData.IncrementLossCount(loser);
		PlayerData.IncreaseTotalPoints(winner, score);
		PlayerData.DecreaseTotalPoints(loser, score);
		
		EmitSignal("OnGameFinished", winner, victoryTitle, score);
	}

	private bool _ValidateVictory(int player)
	{
		foreach (ValidateVictory condition in _winConditions.Values)
		{
			if (condition.Invoke(player, _boardPoints)) return true;
		}

		return false;
	}

	private void _OnBearOff(int player, int move)
	{
		_UseMove(move);
		_hasBorne[player - 1] = true;

		if (_ValidateVictory(player))
		{
			_OnGameFinished(player);
			return;
		}

		if (_moves.Count == 0 || !_HasValidMove(player, _moves.ToArray()))
		{
			_ChangeTurn();
		}
	}

	private void _BearOff(int player, int fromColumn, int pieceIndex)
	{
		_boardPoints[fromColumn, pieceIndex] = 0;
		EmitSignal("OnBearOff", player, fromColumn, pieceIndex);
	}
	
	private void _AttemptBearOff(int player, int fromColumn)
	{
		int direction = player == 1 ? 1 : -1;
		int homeOffset = player == 1 ? _boardPointCount : -1;

		foreach (int move in _moves)
		{
			int targetColumn = homeOffset - (move * direction);
			int topPieceIndex = _GetTopPieceIndex(fromColumn);

			if (targetColumn == fromColumn && topPieceIndex > -1)
			{
				_BearOff(player, fromColumn, topPieceIndex);
				_OnBearOff(player, move);
				return;
			}
		}

		//GD.Print("Cannot bear off attempted piece");
	}

	private bool _CanBearOff(int player)
	{
		for (int column = _quarterPointCount; column < _boardPointCount - _quarterPointCount; column++)
		{
			if (_boardPoints[column, 0] == player) return false;
		}
		
		return true;
	}

	private bool _IsWallOccupied(int player)
	{
		return player == 1 ? _wall[0] != 0 : _wall[_wallHalfSize] != 0;
	}

	public int[] GetFirstValidMove(int player, int[] moves) // TODO: Perhaps rework to return an int[,,,] instead, containing int[fromColumn, fromIndex, toColumn, toIndex];
	{
		int direction = player == 1 ? 1 : -1;

		if (_IsWallOccupied(player))
		{
			foreach (int amount in moves)
			{
				int column  = player == 1 ? amount - 1 : _boardPointCount - amount;
				int topPieceIndex 	= _GetTopPieceIndex(column);
				if (topPieceIndex == -1)
				{
					return new int[] { column, 0 };
				}
				
				if (topPieceIndex >= _boardPointSize - 1) continue;

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
			int homeOffset = player == 1 ? _boardPointCount : -1;
			
			foreach (int move in moves)
			{
				int fromColumn = homeOffset - (move * direction);
				if (_boardPoints[fromColumn, 0] == player)
				{
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
				if (targetColumn >= _boardPointCount || targetColumn < 0) continue;

				topPieceIndex = _GetTopPieceIndex(targetColumn);
				if (topPieceIndex < 0) 
				{
					return new int[] { targetColumn, 0 };
				} 

				if (_boardPoints[targetColumn, topPieceIndex] == player)
				{
					if (topPieceIndex >= _boardPointSize - 1) continue;

					return new int[] { targetColumn, topPieceIndex + 1 };
				} 
			}
		}
		
		return new int[2] { -1, -1 };
	}

	private bool _HasValidMove(int player, int[] moves)
	{
		int direction = player == 1 ? 1 : -1;

		if (_IsWallOccupied(player))
		{
			foreach (int amount in moves)
			{
				int column  		= player == 1 ? amount - 1 : _boardPointCount - amount;
				int topPieceIndex 	= _GetTopPieceIndex(column);
				if (topPieceIndex == -1)
				{
					GD.Print(String.Format("Found a valid move WALL->{0:D}", column));
					return true;
				}
				
				if (topPieceIndex >= _boardPointSize - 1) continue;

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
			int homeOffset = player == 1 ? _boardPointCount : -1;
			
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
				if (topPieceIndex == -1) continue;

				if (_boardPoints[column, topPieceIndex] != player) continue; 

				int move = amount * direction;
				if (column + move >= _boardPointCount || column + move < 0) continue;

				topPieceIndex = _GetTopPieceIndex(column + move);
				if (topPieceIndex < 0)
				{
					GD.Print(String.Format("Found a valid move {0:D}->{1:D}", column, column + move));
					return true;
				} 

				if (_boardPoints[column + move, topPieceIndex] == player)
				{
					if (topPieceIndex >= _boardPointSize - 1) continue;

					GD.Print(String.Format("Found a valid move {0:D}->{1:D}", column, column + move));
					return true;
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
		EmitSignal("OnChangeTurn", this._activePlayer);
		_SendGameAlert(player + "'s turn! Click to roll dice.");
		_ChangeBoardState(0);
	}

	private void _UseMove(int pointsMoved) 
	{
		EmitSignal("MoveUsed", _moves.FindLastIndex((match) => match == pointsMoved));
		_moves.Remove(pointsMoved);
	}

	private void _OnSuccessfulMove(int distance)
	{
		_UseMove(distance);
		string movesLeft = "Player" + this._activePlayer + " has " + _moves.Count + " move(s) left:";

		foreach (int move in _moves)
		{
			movesLeft += String.Format(" [{0:D}] ", move);
		}

		GD.Print(movesLeft);

		if (_moves.Count == 0 || !_HasValidMove(this._activePlayer, _moves.ToArray()))
		{
			_ChangeTurn();
		}
	}

	private void _OnSuccessfulMove(int fromColumn, int toColumn)
	{
		int pointsMoved = Math.Abs(toColumn - fromColumn);
		_OnSuccessfulMove(pointsMoved);
	}

	private bool _CanReach(int fromColumn, int toColumn)
	{
		if (this._activePlayer == 1)
		{
			if (fromColumn > toColumn) 
			{
				//GD.Print("Unable to reach: Player 1 must move clockwise.");
				_SendGameAlert("RED must move clockwise");
				return false;
			}
		} 
		else if (fromColumn < toColumn)
		{
			//GD.Print("Unable to reach: Player 2 must move counter-clockwise.");
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

		EmitSignal("OnMovePieceFromWall", _activePlayer, wallIndex, new int[] { toColumn, freeSlot });
	}

	private void _AttemptMoveFromWall(int toColumn)
	{
		int distanceToColumn = Mathf.Abs(_activePlayer == 1 ? toColumn + 1 : toColumn - _boardPointCount);
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
			//GD.Print("Unable to move from wall, no moves left matched the attempted move.");
			_SendGameAlert("Unable to move from wall, no moves left matched the attempted move.");
			return;
		}

		int topPieceIndex = _GetTopPieceIndex(toColumn);
		if (topPieceIndex > _boardPointSize - 1)
		{
			//GD.Print("Unable to move from wall, target column is full.");
			_SendGameAlert("Unable to move from wall, target column is full.");
			return;
		};

		if (topPieceIndex == -1)
		{
			//GD.Print("Move success: WALL->" + toColumn);
			_MoveFromWall(toColumn);
			_OnSuccessfulMove(distanceToColumn);
			return;
		}

		if (topPieceIndex == 0)
		{
			//GD.Print("Move success: WALL->" + toColumn);
			_MoveFromWall(toColumn);
			_OnSuccessfulMove(distanceToColumn);
			return;
		}

		if (_boardPoints[toColumn, topPieceIndex] == _activePlayer)
		{
			//GD.Print("Move success: WALL->" + toColumn);
			_MoveFromWall(toColumn);
			_OnSuccessfulMove(distanceToColumn);
			return;
		}

		//GD.Print("Failed to move from wall");
	}

	private void _AttemptMove(int fromColumn, int toColumn) 
	{
		if (fromColumn == toColumn) return;
		
		if (!_CanReach(fromColumn, toColumn))
		{
			//GD.Print("Move failed: No fitting moves.");
			_SendGameAlert("Attempted move does not match any move left.");
			return;
		}

		int topPieceIndex = _GetTopPieceIndex(fromColumn);
		if (topPieceIndex == -1) {
			//GD.Print("Move failed: No piece in column: " + fromColumn);
			return;
		}

		if (_boardPoints[fromColumn, topPieceIndex] != this._activePlayer)
		{
			//GD.Print("Move failed: Tried to move opponets piece from column: " + fromColumn + "->" + toColumn);
			_SendGameAlert("Cannot move opponets pieces!");
			return;
		}

		topPieceIndex = _GetTopPieceIndex(toColumn);
		if (topPieceIndex < 0)
		{
			//GD.Print("Move success: " + fromColumn + "->" + toColumn);
			_MovePiece(fromColumn, toColumn);
			_OnSuccessfulMove(fromColumn, toColumn);
			return;
		}

		if (_boardPoints[toColumn, topPieceIndex] != this._activePlayer)
		{
			if (topPieceIndex == 0)
			{
				//GD.Print("Move success: " + fromColumn + "->" + toColumn);
				_ReplacePiece(fromColumn, toColumn);
				_OnSuccessfulMove(fromColumn, toColumn);
				return;
			}
		}

		if (_boardPoints[toColumn, topPieceIndex] == this._activePlayer)
		{
			if (topPieceIndex < _boardPointSize - 1)
			{
				//GD.Print("Move success: " + fromColumn + "->" + toColumn);
				_MovePiece(fromColumn, toColumn);
				_OnSuccessfulMove(fromColumn, toColumn);
				return;
			}

			//GD.Print("Move failed: toColumn is full " + fromColumn + "->" + toColumn);
			_SendGameAlert("That column is full!");
		}
	}

	private bool _IsMouseInsideHome(float mouseX, float mouseY)
	{
		float leftBorder 	= this.Position.x + _boardSize.x + 10;
		float rightBorder 	= this.Position.x + _boardSize.x + _homeWidth;
		float bottomBorder 	= this.Position.y + _boardSize.y;

		return mouseX > leftBorder && mouseX < rightBorder && mouseY > this.Position.y && mouseY < bottomBorder;
	}

	private bool _IsMouseInsideWall(float mouseX, float mouseY)
	{
		float leftBorder 	= this.Position.x + _boardSize.x * 0.5f - _spacerWidth * 0.5f;
		float rightBorder 	= this.Position.x + _boardSize.x * 0.5f + _spacerWidth * 0.5f;
		float bottomBorder 	= this.Position.y + _boardSize.y;

		return mouseX > leftBorder && mouseX < rightBorder && mouseY > this.Position.y && mouseY < bottomBorder;
	}

	private int _GetColumn(float mouseX, float mouseY)
	{
		int column;
		int halfBoardPoints = _boardPointCount / 2;

		float halfHeight 	= _boardSize.y * 0.5f;
		float halfWidth 	= _boardSize.x * 0.5f;
		float xOffset 		= 0;

		if (mouseX > this.Position.x + halfWidth)
		{
			xOffset = _spacerWidth;
		}

		column = halfBoardPoints - Mathf.CeilToInt((( Mathf.Max(mouseX - xOffset - this.Position.x, 0)) / (_boardSize.x - _spacerWidth)) * halfBoardPoints);

		if (mouseY < this.Position.y + halfHeight)
		{	
			column = halfBoardPoints + Mathf.FloorToInt((( Mathf.Max(mouseX - xOffset - this.Position.x, 0)) / (_boardSize.x - _spacerWidth)) * halfBoardPoints);
		}

		return Mathf.Clamp( column, 0, _boardPointCount - 1);
	}

	public override void _Process(float delta)
	{
		if (_boardState <= -1)
		{
			this._activeColumn = -1;
			return;
		}
		
		if (Input.IsActionJustPressed("mouse_left_button"))
		{
			if (_boardState == 0) return;

			Vector2 mousePosition = GetViewport().GetMousePosition();

			if (_IsWallOccupied(_activePlayer))
			{
				if (_IsMouseInsideWall(mousePosition.x, mousePosition.y))
				{
					this._activeColumn = -2;
					return;
				}
			}

			this._activeColumn = _GetColumn(mousePosition.x, mousePosition.y);
		}

		if (Input.IsActionJustReleased("mouse_left_button"))
		{
			if (_boardState == 0)
			{
				_ThrowDice();
				_OnDiceThrow();
				return;
			}
			
			Vector2 mousePosition 	= GetViewport().GetMousePosition();
			int targetColumn 		= _GetColumn(mousePosition.x, mousePosition.y);

			if (_IsWallOccupied(_activePlayer))
			{
				if (this._activeColumn == -2)
				{
					_AttemptMoveFromWall(targetColumn);
					return;
				}

				//GD.Print("Player must move a piece from the wall!");
				_SendGameAlert("Player must move a piece from the wall!");
				return;
			}

			if (_IsMouseInsideHome(mousePosition.x, mousePosition.y) && _CanBearOff(_activePlayer))
			{
				_AttemptBearOff(this._activePlayer, this._activeColumn);
				return;
			}

			_AttemptMove(this._activeColumn, targetColumn);
		}
	}
}
