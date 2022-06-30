using Godot;
using System;

public class MainMenu : Control
{
	private PlayPopup 		_playPopup;
	private OptionsPopup	_optionsPopup;
	private StatisticsPopup	_statisticsPopup;

	public override void _Ready()
	{
		_playPopup 			= GetNode<PlayPopup>("PlayPopup") 				as PlayPopup;
		_optionsPopup 		= GetNode<OptionsPopup>("OptionsPopup")			as OptionsPopup;
		_statisticsPopup 	= GetNode<StatisticsPopup>("StatisticsPopup")	as StatisticsPopup;
		
		GetNode<Button>("ButtonContainer/PlayButton").GrabFocus();
		_statisticsPopup.PopulateElementContainer();
	}

	private void _on_PlayButton_pressed()
	{
		_playPopup.PopupCentered();
	}

	private void _on_OptionsButton_pressed()
	{
		_optionsPopup.PopupCentered();
	}

	private void _on_StatisticsButton_pressed()
	{
		_statisticsPopup.PopupCentered();
	}

	private void _on_QuitButton_pressed()
	{
		GetTree().Quit();
	}

}
