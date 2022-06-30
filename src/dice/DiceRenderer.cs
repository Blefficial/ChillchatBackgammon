using Godot;
using System.Collections.Generic;

public class DiceRenderer : Node2D
{
	private Game _game;
	private Sprite _sprite;
	private Viewport _viewport;
	private Dice _dice;

	public override void _Ready()
	{
		_game 		= GetParent<Node2D>().GetParent<Game>();
		_sprite 	= GetNode<Sprite>("Background") 	as Sprite;
		_viewport 	= GetNode<Viewport>("Viewport") 	as Viewport;
		_dice 		= GetNode<Dice>("Viewport/Dice") 	as Dice;
	}

	public void OnDiceThrow(List<int> moves)
	{
		_dice.ThrowDice(moves[0], moves[1]);
	}

  	public override void _Process(float delta)
  	{
		_sprite.Texture = _viewport.GetTexture();
  	}
}
