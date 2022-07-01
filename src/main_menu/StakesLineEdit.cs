using Godot;
using System;

public class StakesLineEdit : LineEdit
{
	RegEx _regEx = new RegEx();

	string _previousText = "";

	public override void _Ready()
	{
		_regEx.Compile("^[0-9]*$");
	}

	private void _on_StakesLineEdit_text_changed(String newText)
	{
		if (_regEx.Search(newText) != null)
		{
			Text = newText;
			_previousText = Text;
		}
		else 
		{
			Text = _previousText;
		}

		CaretPosition = Text.Length;
	}

	public int GetStakes()
	{
		if (Text == null || Text == "") return 1;
		
		return Text.ToInt();
	}
}
