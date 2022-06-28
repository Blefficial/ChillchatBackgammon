using Godot;
using System;

public class MovePieceTween : Tween
{
	public void MovePiece(Sprite startSprite, Sprite destinationSprite, Vector2 startPosition, Vector2 endPosition, float duration)
	{
		float delay = IsActive() ? GetRuntime() : 0;
		_MovePiece(startSprite, destinationSprite, startPosition, endPosition, duration, delay);
	}
	
	private void _MovePiece(Sprite startSprite, Sprite destinationSprite, Vector2 startPosition, Vector2 endPosition, float duration, float delay)
	{
		startSprite.ZIndex = 1;

		InterpolateProperty(
			startSprite,
			"position",
			startPosition,
			endPosition,
			duration,
			TransitionType.Cubic,
			EaseType.InOut,
			delay
		);

		InterpolateProperty(
			startSprite,
			"scale",
			null,
			destinationSprite.Scale,
			duration,
			TransitionType.Cubic,
			EaseType.InOut,
			delay
		);

		InterpolateProperty(
			startSprite,
			"position",
			null,
			startPosition,
			duration,
			TransitionType.Linear,
			EaseType.InOut,
			duration + delay
		);

		InterpolateProperty(
			startSprite,
			"visible",
			null,
			false,
			0,
			TransitionType.Linear,
			EaseType.InOut,
			duration + delay
		);
		
		InterpolateProperty(
			destinationSprite,
			"self_modulate",
			startSprite.SelfModulate,
			startSprite.SelfModulate,
			0,
			TransitionType.Linear,
			EaseType.InOut,
			duration + delay
		);

		InterpolateProperty(
			destinationSprite,
			"visible",
			null,
			true,
			0,
			TransitionType.Linear,
			EaseType.InOut,
			duration + delay
		);

		Start();
	}

	public void AddPieceToWall(Sprite sprite)
	{
		InterpolateProperty(
			sprite,
			"scale",
			Vector2.Zero,
			sprite.Scale,
			1,
			TransitionType.Cubic,
			EaseType.InOut
		);
		
		sprite.Scale 	= Vector2.Zero;
		Start();
		sprite.Visible 	= true;
	}
}
