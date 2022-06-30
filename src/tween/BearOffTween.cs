using Godot;

public class BearOffTween : Tween
{
	
	public void BearOffPiece(Sprite sprite, float duration)
	{
		InterpolateProperty(
			sprite,
			"scale",
			sprite.Scale,
			Vector2.Zero,
			duration
		);

		InterpolateCallback(
			this,
			duration,
			"_ResetSprite",
			sprite,
			sprite.Scale
		);

		Start();
	}

	private void _ResetSprite(Sprite sprite, Vector2 scale)
	{
		sprite.Visible = false;
		sprite.Scale = scale;
	}
}
