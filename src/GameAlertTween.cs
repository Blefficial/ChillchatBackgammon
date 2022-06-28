using Godot;
using System;

public class GameAlertTween : Tween
{
	public void ShowGameAlert(Label label, string alert, float duration)
	{
		label.Text 		= alert;
		label.Visible 	= true;

		if (this.IsActive()) return;
		
		InterpolateProperty(
			label,
			"rect_scale",
			null,
			new Vector2(1, 1),
			0.25f,
			TransitionType.Linear,
			EaseType.InOut
		);

		InterpolateProperty(
			label,
			"rect_scale",
			null,
			new Vector2(1, 0),
			0.25f,
			TransitionType.Cubic,
			EaseType.InOut,
			duration
		);

		InterpolateCallback(
			this,
			duration + 0.25f,
			"_HideGameAlert",
			label
		);

		Start();
	}

	private void _HideGameAlert(Label label)
	{
		label.Visible = false;
	}
}
