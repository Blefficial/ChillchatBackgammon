using Godot;
using System;

public class HomeRenderer : Control
{
	private VBoxContainer		_player1HomeSprites;
	private VBoxContainer		_player2HomeSprites;

	public override void _Ready()
	{
		_player1HomeSprites = GetNode<VBoxContainer>("Player1HomeSprites") as VBoxContainer;
		_player2HomeSprites = GetNode<VBoxContainer>("Player2HomeSprites") as VBoxContainer;
		
		for (int i = 0; i < Constants.PIECES_PER_PLAYER; i++)
		{
			TextureRect P1_textureRect 	= new TextureRect();
			P1_textureRect.Texture 		= Constants.CHECKER_TEXTURE_SIDE;
			P1_textureRect.Expand		= true;
			P1_textureRect.RectMinSize	= Constants.CHECKER_TEX_MINSIZE;
			P1_textureRect.SelfModulate = Constants.RED;
			P1_textureRect.Visible		= false;
			_player1HomeSprites.AddChild(P1_textureRect);

			TextureRect P2_textureRect 	= new TextureRect();
			P2_textureRect.Texture 		= Constants.CHECKER_TEXTURE_SIDE;
			P2_textureRect.Expand		= true;
			P2_textureRect.RectMinSize	= Constants.CHECKER_TEX_MINSIZE;
			P2_textureRect.Visible		= false;
			_player2HomeSprites.AddChild(P2_textureRect);
		}
	}

	public void Reset()
	{
		for (int i = 0; i < _player1HomeSprites.GetChildCount(); i++)
		{
			Node node = _player1HomeSprites.GetChild(i);
			if (node is TextureRect)
			{
				TextureRect textureRect = (TextureRect)node;
				textureRect.Visible 	= false;
			}
		}

		for (int i = 0; i < _player2HomeSprites.GetChildCount(); i++)
		{
			Node node = _player2HomeSprites.GetChild(i);
			if (node is TextureRect)
			{
				TextureRect textureRect = (TextureRect)node;
				textureRect.Visible 	= false;
			}
		}
	}

	private TextureRect _GetFirstHiddenTexureRect(VBoxContainer container)
	{
		for (int i = 0; i < container.GetChildCount(); i++)
		{
			Node node = container.GetChild(i);
			if (node is TextureRect)
			{
				TextureRect textureRect = (TextureRect)node;
				
				if (!textureRect.Visible) return textureRect;
			}
		}

		return null;
	}

	public void OnBearOff(int player, int fromColumn, int pieceIndex)
	{
		if (player == 1)
		{
			TextureRect rect = _GetFirstHiddenTexureRect(_player1HomeSprites);

			if (rect == null) return;

			rect.Visible = true;
		}
		else
		{
			TextureRect rect = _GetFirstHiddenTexureRect(_player2HomeSprites);

			if (rect == null) return;

			rect.Visible = true;
		}
	}
}
