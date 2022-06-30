using Godot;
using System.Collections.Generic;

public class DiceResultIndicator : Control
{
	private static Texture _diceAltasTexture = ResourceLoader.Load("res://assets/images/dice/dice_atlas.png") as Texture;
	private TextureRect[] _indicators;
	private DiceResultTween _diceResultTween;

	public override void _Ready()
	{
		VBoxContainer moves 	= GetNode<VBoxContainer>("Moves") 				as VBoxContainer;
		_diceResultTween		= GetNode<DiceResultTween>("DiceResultTween") 	as DiceResultTween;
		_indicators 			= new TextureRect[4];

		for (int i = 0; i < _indicators.Length; i++)
		{
			AtlasTexture diceAtlas 		= new AtlasTexture();
			diceAtlas.Atlas 			= _diceAltasTexture;
			diceAtlas.Region 			= new Rect2(0, 0, 128, 128);

			TextureRect textureRect 	= new TextureRect();
			textureRect.RectMinSize 	= new Vector2(128, 128);
			textureRect.RectPivotOffset = textureRect.RectMinSize * 0.5f;
			textureRect.RectScale		= Vector2.Zero;
			textureRect.Texture 		= diceAtlas;
			_indicators[i] 				= textureRect;

			moves.AddChild(textureRect);
		}
	}

	public void OnMoveUsed(int index)
	{
		TextureRect textureRect = _indicators[index];
		AtlasTexture texture 	= textureRect.Texture as AtlasTexture;
		_diceResultTween.HideResult(textureRect, 0);
	}

	public void UpdateIndicator(List<int> moves)
	{
		foreach (TextureRect textureRect in _indicators)
		{
			_diceResultTween.HideResult(textureRect, 0);
		}

		for (int i = 0; i < moves.Count; i++)
		{
			TextureRect textureRect = _indicators[i];
			_diceResultTween.UpdateResult(moves[i], textureRect, 0);
			_diceResultTween.ShowResult(textureRect, 2);
		}
	}
}
