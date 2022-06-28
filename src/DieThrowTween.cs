using Godot;
using System;

public class DieThrowTween : Tween
{
	public void ThrowTo(Spatial die, Vector3 rotation)
	{
		InterpolateProperty(
			die,
			"rotation",
			die.Rotation,
			new Vector3(Mathf.Pi * 4, 0, Mathf.Pi * 4),
			1f
		);

		InterpolateProperty(
			die,
			"translation",
			die.Translation,
			die.Translation + new Vector3(0, 5, 0),
			1,
			TransitionType.Cubic,
			EaseType.InOut
		);

		InterpolateCallback(
			this,
			1,
			"LandTo",
			die,
			rotation
		);

		Start();
	}

	private void LandTo(Spatial die, Vector3 rotation)
	{
		InterpolateProperty(
			die,
			"translation",
			die.Translation,
			die.Translation - new Vector3(0, 5, 0),
			1.2f,
			TransitionType.Cubic,
			EaseType.InOut
		);
		
		InterpolateProperty(
			die,
			"rotation",
			null,
			rotation,
			1,
			TransitionType.Linear,
			EaseType.InOut
		);

		Start();
	}
}
