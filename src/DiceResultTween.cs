using Godot;
using System;

public class DiceResultTween : Tween
{
	public void ShowResult(TextureRect textureRect, float delay)
	{
		InterpolateProperty(
			textureRect,
			"rect_scale",
			Vector2.Zero,
			new Vector2(1, 1),
			0.5f,
			TransitionType.Cubic,
			EaseType.InOut,
			delay + 0.1f
		);

		Start();
	}

	public void HideResult(TextureRect textureRect, float delay)
	{
		InterpolateProperty(
			textureRect,
			"rect_scale",
			null,
			Vector2.Zero,
			0.5f,
			TransitionType.Cubic,
			EaseType.InOut,
			delay
		);

		Start();
	}

	public void UpdateResult(int result, TextureRect textureRect, float delay)
	{
		InterpolateProperty(
			textureRect,
			"rect_scale",
			null,
			Vector2.Zero,
			0.5f,
			TransitionType.Cubic,
			EaseType.InOut,
			delay
		);

		InterpolateCallback(
			this,
			delay + 0.5f,
			"OnUpdateResult",
			result,
			textureRect
		);

		Start();
	}

	public void OnUpdateResult(int result, TextureRect textureRect)
	{
		AtlasTexture texture 	= textureRect.Texture as AtlasTexture;
		texture.Region 		 	= new Rect2((result - 1) * 128, 0, 128, 128);
	}
}
