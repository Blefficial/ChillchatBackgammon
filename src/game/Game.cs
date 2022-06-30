using Godot;

public class Game : CanvasLayer
{
	private Board				_board;
	private BoardRenderer 		_boardRenderer;
	private DiceRenderer 		_diceRenderer;
	private HomeRenderer		_homeRenderer;
	private Label 				_gameAlertLabel;
	private Label				_player1Label;
	private Label				_player2Label;
	private Label				_activePlayerLabel;
	private GameAlertTween		_gameAlertTween;
	private DiceResultIndicator _resultIndicator;
	private PausePopup			_pausePopup;
	private GameFinishedPopup	_gameFinishedPopup;
	private SoundEffectsPlayer 	_soundEffectsPlayer;

	public override void _Ready()
	{
		_board 				= GetNode<Board>("Game/Board") 								as Board;
		_diceRenderer 		= GetNode<DiceRenderer>("Game/DiceRenderer") 				as DiceRenderer;
		_homeRenderer		= GetNode<HomeRenderer>("HomeRenderer")						as HomeRenderer;
		_gameAlertLabel 	= GetNode<Label>("Game/MarginContainer/GameAlert") 			as Label;
		_player1Label 		= GetNode<Label>("Game/Player1Label") 						as Label;
		_player2Label 		= GetNode<Label>("Game/Player2Label") 						as Label;
		_activePlayerLabel 	= GetNode<Label>("Game/ActivePlayerLabel") 					as Label;
		_gameAlertTween 	= GetNode<GameAlertTween>("Game/GameAlertTween") 			as GameAlertTween;
		_resultIndicator	= GetNode<DiceResultIndicator>("DiceResultIndicator") 		as DiceResultIndicator;
		_pausePopup			= GetNode<PausePopup>("PausePopup")							as PausePopup;
		_gameFinishedPopup 	= GetNode<GameFinishedPopup>("GameFinishedPopup")			as GameFinishedPopup;
		_soundEffectsPlayer = GetNode<SoundEffectsPlayer>("SoundEffectsPlayer")			as SoundEffectsPlayer;

		_board.Connect("OnDiceThrow", 			_diceRenderer, 		 "OnDiceThrow");
		_board.Connect("OnDiceThrow", 			_resultIndicator, 	 "UpdateIndicator");
		_board.Connect("OnChangeTurn", 			this, 				 "_OnChangeTurn");
		_board.Connect("OnGameAlert", 			this, 				 "_OnGameAlert");
		_board.Connect("MoveUsed", 				_resultIndicator, 	 "OnMoveUsed");
		_board.Connect("OnGameFinished",		this, 				 "_OnGameFinished");
		_board.Connect("OnBearOff",				_homeRenderer,		 "OnBearOff");
		_board.Connect("OnMovePiece",			_soundEffectsPlayer, "OnMovePiece");
		_board.Connect("OnMovePieceFromWall",	_soundEffectsPlayer, "OnMovePieceFromWall");
		_board.Connect("OnDiceThrow",			_soundEffectsPlayer, "OnDiceThrow");
		_board.Connect("OnBearOff",				_soundEffectsPlayer, "OnBearOff");
		_board.Initialize();

		_pausePopup.Connect("OnPause", 			 	 this, 	"_OnPause");
		_pausePopup.Connect("OnContinue", 			 this, 	"_OnContinue");
		_gameFinishedPopup.Connect("RequestRematch", this, 	"_OnRequestRematch");

		_player1Label.Text = Constants.PLAYER_1;
		_player2Label.Text = Constants.PLAYER_2;
	}

	private void _OnPause()
	{
		_pausePopup.PopupCentered();
		_board.OnPause();
	}

	private void _OnContinue()
	{
		_pausePopup.Hide();
		_board.OnPause();
	}

	private void _OnRequestRematch()
	{
		_gameFinishedPopup.Hide();
		_homeRenderer.Reset();
		_board.Reset();
		_board.Initialize();
	}

	private void _OnGameFinished(string player, string victoryTitle, int scoreGained)
	{
		_gameFinishedPopup.UpdateWinnerLabel(player, victoryTitle, scoreGained);
		_gameFinishedPopup.PopupCentered();
	}

	private void _OnChangeTurn(int activePlayer)
	{
		_activePlayerLabel.Text = activePlayer == 1 ? "RED" : "WHITE";
	}

	private void _OnGameAlert(string alert)
	{
		_gameAlertTween.ShowGameAlert(_gameAlertLabel, alert, 2);
	}
}
