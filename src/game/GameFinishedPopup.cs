using Godot;
using System;

public class GameFinishedPopup : Popup
{
	private Label _winnerLabel;
	private Label _player1PointsLabel;
	private Label _player2PointsLabel;
	
	[Signal]
	delegate void RequestRematch();

	public override void _Ready()
	{
		_winnerLabel 		= GetNode<Label>("VBoxContainer/WinnerLabel") 			as Label;
		_player1PointsLabel = GetNode<Label>("VBoxContainer/Player1PointsLabel") 	as Label;
		_player2PointsLabel = GetNode<Label>("VBoxContainer/Player2PointsLabel") 	as Label;
	}

	public void UpdateWinnerLabel(string winner, string victoryTitle, int scoreGained)
	{
		string player1 = Constants.PLAYER_1;
		string player2 = Constants.PLAYER_2;
		
		_winnerLabel.Text = String.Format("{0} won and scored {1} (+{2:D}) points!", winner, victoryTitle, scoreGained);
		_player1PointsLabel.Text = String.Format("{0} now has {1:D} points in total!", player1, PlayerData.PLAYER_STATS[player1]["total_points"]);
		_player2PointsLabel.Text = String.Format("{0} now has {1:D} points in total!", player2, PlayerData.PLAYER_STATS[player2]["total_points"]);
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("ui_cancel") && this.Visible)
		{
			GetTree().ChangeScene("res://scenes/MainMenu.tscn");
		}	
	}

	private void _on_ReturnToMenuButton_pressed()
	{
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	private void _on_RematchButton_pressed()
	{
		EmitSignal("RequestRematch");
	}
}
