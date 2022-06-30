using Godot;
using System;

public class PausePopup : Popup
{
	[Signal]
	delegate void OnPause();
	[Signal]
	delegate void OnContinue();

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (!Visible)
			{
				EmitSignal("OnPause");
			}
			else
			{
				EmitSignal("OnContinue");
			}
		}	
	}

	private void _on_ReturnToMenuButton_pressed()
	{
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	private void _on_ContinueButton_pressed()
	{
		EmitSignal("OnContinue");
	}

}
