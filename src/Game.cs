using Godot;
using System.Collections.Generic;

public class Game : CanvasLayer
{
	private Board _board;
	private BoardRenderer _boardRenderer;
	private DiceRenderer _diceRenderer;
	private Label _gameAlertLabel;
	private GameAlertTween _gameAlertTween;
	private DiceResultIndicator _resultIndicator;
	public override void _Ready()
	{
		_board 				= GetNode<Board>("Game/Board") 								as Board;
		_diceRenderer 		= GetNode<DiceRenderer>("Game/DiceRenderer") 				as DiceRenderer;
		_gameAlertLabel 	= GetNode<Label>("Game/MarginContainer/GameAlert") 			as Label;
		_gameAlertTween 	= GetNode<GameAlertTween>("Game/GameAlertTween") 			as GameAlertTween;
		_resultIndicator	= GetNode<DiceResultIndicator>("DiceResultIndicator") 		as DiceResultIndicator;

		_board.Connect("OnDiceThrow", _diceRenderer, "OnDiceThrow");
		_board.Connect("OnDiceThrow", _resultIndicator, "UpdateIndicator");
		_board.Connect("OnGameAlert", this, "_OnGameAlert");
		_board.Connect("MoveUsed", _resultIndicator, "OnMoveUsed");
		_board.Initialize();
	}

	private void _OnGameAlert(string alert)
	{
		_gameAlertTween.ShowGameAlert(_gameAlertLabel, alert, 2);
	}
}
