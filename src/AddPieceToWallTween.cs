using Godot;

public class AddPieceToWallTween : Tween
{
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
