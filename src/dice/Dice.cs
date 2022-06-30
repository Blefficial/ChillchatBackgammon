using Godot;

public class Dice : Spatial
{
	private static Vector3[] _diceRotations = new Vector3[6] {  
		new Vector3(0, 0, 0),
		new Vector3(-Mathf.Pi * 0.5f, 0, 0),
		new Vector3(0, 0, Mathf.Pi * 0.5f),
		new Vector3(0, 0, -Mathf.Pi * 0.5f),
		new Vector3(Mathf.Pi * 0.5f, 0, 0),
		new Vector3(0, Mathf.Pi * 0.5f, Mathf.Pi)
	};
	
	Spatial _die1;
	Spatial _die2;
	DieThrowTween _dieThrowTween;

	public override void _Ready()
	{
		_dieThrowTween 	= GetNode<DieThrowTween>("DieThrowTween") 	as DieThrowTween;
		_die1 			= GetNode<Spatial>("Die1") 					as Spatial;
		_die2 			= GetNode<Spatial>("Die2") 					as Spatial;
	}

	public void ThrowDice(int dice1, int dice2)
	{
		_dieThrowTween.ThrowTo(_die1, _diceRotations[dice1 - 1]);
		_dieThrowTween.ThrowTo(_die2, _diceRotations[dice2 - 1]);
	}
}
