using Godot;
using System;

public class PlayPopup : Popup
{
	LineEdit 		_player1LineEdit;
	LineEdit 		_player2LineEdit;
	StakesLineEdit 	_stakesLineEdit;
	
	public override void _Ready()
	{
		_player1LineEdit = GetNode<LineEdit>("Panel/VBoxContainer/NameContainer/HBoxContainer/Player1LineEdit") 		as LineEdit; 
		_player2LineEdit = GetNode<LineEdit>("Panel/VBoxContainer/NameContainer/HBoxContainer/Player2LineEdit") 		as LineEdit; 
		_stakesLineEdit	 = GetNode<StakesLineEdit>("Panel/VBoxContainer/VBoxContainer/MarginContainer/StakesLineEdit") 	as StakesLineEdit;
	}

	private void _on_PlayButton_pressed()
	{
		if (_player1LineEdit.Text == "" || _player2LineEdit.Text == "") return;
		if (_player1LineEdit.Text == _player2LineEdit.Text) return;
		if (_stakesLineEdit.Text == "") return;

		Constants.PLAYER_1 	= _player1LineEdit.Text;
		Constants.PLAYER_2 	= _player2LineEdit.Text;
		Constants.STAKES 	= _stakesLineEdit.GetStakes();

		GetTree().ChangeScene("res://scenes/Game.tscn");
	}
}
