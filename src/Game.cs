using Godot;
using System;

public class Game : Node2D
{
	
	private Board _board;

	private int[] _moves 		= new int[4];
	private int _activePlayer;

	public override void _Ready()
	{
		_board 			= GetNode<Board>("Board") as Board;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
