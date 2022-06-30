using System;
using Godot;

public class OptionsPopup : Popup
{
	Slider _masterVolumeSlider;
	Slider _sfxVolumeSlider;
	Label _masterVolumeDBLabel;
	Label _sfxVolumeDBLabel;

	public override void _Ready()
	{
		_masterVolumeSlider 		= GetNode<Slider>("Panel/MarginContainer/TabContainer/Audio/VBoxContainer/MasterVolumeSlider") as Slider;
		_sfxVolumeSlider 			= GetNode<Slider>("Panel/MarginContainer/TabContainer/Audio/VBoxContainer/SoundEffectVolumeSlider") as Slider;
		_masterVolumeDBLabel 		= GetNode<Label>("Panel/MarginContainer/TabContainer/Audio/VBoxContainer/MasterVolumeLabels/DesibelLabel") as Label;
		_sfxVolumeDBLabel 			= GetNode<Label>("Panel/MarginContainer/TabContainer/Audio/VBoxContainer/SoundEffectVolumeLabels/DesibelLabel") as Label;

		float masterVolume 			= (float) PlayerData.SETTINGS["master_volume"];
		float sfxVolume				= (float) PlayerData.SETTINGS["sfx_volume"];

		_masterVolumeSlider.Value	= masterVolume;
		_sfxVolumeSlider.Value		= sfxVolume;

		_sfxVolumeDBLabel.Text 		= String.Format("{0:N0} dB", masterVolume);
		_masterVolumeDBLabel.Text 	= String.Format("{0:N0} dB", sfxVolume);

		AudioServer.SetBusVolumeDb(0, masterVolume);
		AudioServer.SetBusVolumeDb(1, sfxVolume);
	}

	private void _on_SoundEffectVolumeSlider_value_changed(float value)
	{
		PlayerData.SetSFXVolume(value);
		_sfxVolumeDBLabel.Text = String.Format("{0:N0} dB", value);
	}

	private void _on_MasterVolumeSlider_value_changed(float value)
	{
		_masterVolumeDBLabel.Text = String.Format("{0:N0} dB", value);
		PlayerData.SetMasterVolume(value);
	}
}


