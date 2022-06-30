using Godot;
using System.Collections.Generic;

public class SoundEffectsPlayer : AudioStreamPlayer
{
	RandomNumberGenerator _rng;

	private static AudioStreamOGGVorbis[] _movePieceSFX = {
		ResourceLoader.Load("res://assets/sfx/move_01.ogg") as AudioStreamOGGVorbis,
		ResourceLoader.Load("res://assets/sfx/move_02.ogg") as AudioStreamOGGVorbis,
		ResourceLoader.Load("res://assets/sfx/move_03.ogg") as AudioStreamOGGVorbis,
	};

	private static AudioStreamOGGVorbis[] _throwDiceSFX = {
		ResourceLoader.Load("res://assets/sfx/throw_dice_01.ogg") as AudioStreamOGGVorbis,
		ResourceLoader.Load("res://assets/sfx/throw_dice_02.ogg") as AudioStreamOGGVorbis,
		ResourceLoader.Load("res://assets/sfx/throw_dice_03.ogg") as AudioStreamOGGVorbis,
	};

	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();
	}

	public void OnMovePiece(int[] from, int[] to)
	{
		_rng.Randomize();
		Stream = _movePieceSFX[_rng.RandiRange(0, _movePieceSFX.Length - 1)];
		Play();
	}

	public void OnMovePieceFromWall(int player, int wallIndex, int[] to)
	{
		_rng.Randomize();
		Stream = _movePieceSFX[_rng.RandiRange(0, _movePieceSFX.Length - 1)];
		Play();
	}

	public void OnBearOff(int player, int fromColumn, int pieceIndex)
	{
		_rng.Randomize();
		Stream = _movePieceSFX[_rng.RandiRange(0, _movePieceSFX.Length - 1)];
		Play();
	}

	public void OnDiceThrow(List<int> result)
	{
		_rng.Randomize();
		Stream = _throwDiceSFX[_rng.RandiRange(0, _throwDiceSFX.Length - 1)];
		Play();
	}
}
