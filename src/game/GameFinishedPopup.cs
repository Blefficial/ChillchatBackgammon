using Godot;
using System;

public class GameFinishedPopup : Popup
{
	private Label _winnerLabel;
	
	[Signal]
	delegate void RequestRematch();

	public override void _Ready()
	{
		_winnerLabel = GetNode<Label>("VBoxContainer/WinnerLabel");
	}

	public void UpdateWinnerLabel(string player, string victoryTitle, int scoreGained)
	{
		_winnerLabel.Text = String.Format("{0} won and scored {1} (+{2:D}) points!", player, victoryTitle, scoreGained);
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
