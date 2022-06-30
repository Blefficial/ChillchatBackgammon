using Godot;
using System.Collections.Generic;

public class StatisticsPopup : Popup
{
	VBoxContainer _statEntries;
	
	public override void _Ready()
	{
		_statEntries = GetNode<VBoxContainer>("Panel/VBoxContainer/MarginContainer/ScrollContainer/StatEntries");
	}

	public void PopulateElementContainer()
	{
		foreach(string playerKey in PlayerData.PLAYER_STATS.Keys)
		{
			Godot.Collections.Dictionary<string, int> entry = (Godot.Collections.Dictionary<string, int>) PlayerData.PLAYER_STATS[playerKey];
			
			int wins 			= (int) entry["wins"];
			int losses  		= (int) entry["losses"];
			int totalPoints 	= (int) entry["total_points"];
			int totalGames 		= wins + losses;
			float winPercentage = totalGames == 0 ? 0 : (float)wins / (float)totalGames;
			_statEntries.AddChild(_AddEntry(playerKey, wins, losses, winPercentage, totalPoints));
		}
	}

	private HBoxContainer _AddEntry(string name, int wins, int losses, float winPercentage, int totalPoints)
	{
		HBoxContainer elementContainer  = new HBoxContainer();
		Label nameLabel 				= new Label();
		Label winsLabel					= new Label();
		Label lossesLabel				= new Label();
		Label winPercentageLabel 		= new Label();
		Label totalPointsLabel 			= new Label();

		List<Label> nodes = new List<Label>();
		nodes.Add(nameLabel);
		nodes.Add(winsLabel);
		nodes.Add(lossesLabel);
		nodes.Add(winPercentageLabel);
		nodes.Add(totalPointsLabel);

		elementContainer.MouseFilter = MouseFilterEnum.Pass;
		
		nameLabel.Text 	 		= name;
		winsLabel.Text 	 		= wins.ToString();
		lossesLabel.Text 		= losses.ToString();
		winPercentageLabel.Text = string.Format("{0:N1}", winPercentage * 100);
		totalPointsLabel.Text 	= totalPoints.ToString();

		foreach (Label label in nodes)
		{
			label.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
			label.ClipText = true;
			label.HintTooltip = label.Text;
			label.MouseFilter = MouseFilterEnum.Pass;
			elementContainer.AddChild(label);
		}

		return elementContainer;
	}
}
