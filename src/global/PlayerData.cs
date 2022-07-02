using Godot;
using Godot.Collections;

public class PlayerData : Node {
	private static string _SETTINGS_PATH 		= "user://settings";
	private static string _PLAYER_STATS_PATH	= "user://stats";
	public static Dictionary SETTINGS;
	public static Dictionary<string, Dictionary<string, int>> PLAYER_STATS;

	public override void _Ready()
	{
		LoadSettings();
		LoadPlayerStats();
	}

	public static void LoadSettings()
	{
		var file = new File();
		if (!file.FileExists(_SETTINGS_PATH))
		{
			SETTINGS = new Dictionary() {
				{ "master_volume", -12f },
				{ "sfx_volume", -12f }
			};

			SaveSettings();
			return;
		}

		file.Open(_SETTINGS_PATH, File.ModeFlags.Read);
		SETTINGS = new Dictionary((Dictionary)JSON.Parse(file.GetLine()).Result);
		file.Close();
	}
	
	public static void SaveSettings() 
	{
		var file = new File();
		file.Open(_SETTINGS_PATH, File.ModeFlags.Write);
		file.StoreLine(JSON.Print(SETTINGS));
		file.Close();
	}

	public static void SetMasterVolume(float volume)
	{
		SETTINGS["master_volume"] = volume;
		AudioServer.SetBusVolumeDb(0, volume);
		PlayerData.SaveSettings();
	}

	public static void SetSFXVolume(float volume)
	{
		SETTINGS["sfx_volume"] = volume;
		AudioServer.SetBusVolumeDb(1, volume);
		PlayerData.SaveSettings();
	}

	public static void LoadPlayerStats()
	{
		var file = new File();
		if (!file.FileExists(_PLAYER_STATS_PATH))
		{
			PLAYER_STATS = new Dictionary<string, Dictionary<string, int>>();
			SavePlayerStats();
			return;
		}

		file.Open(_PLAYER_STATS_PATH, File.ModeFlags.Read);
		PLAYER_STATS = new Dictionary<string, Dictionary<string, int>>((Dictionary)JSON.Parse(file.GetLine()).Result);
		file.Close();
	}

	public static void SavePlayerStats()
	{
		var file = new File();
		file.Open(_PLAYER_STATS_PATH, File.ModeFlags.Write);
		file.StoreLine(JSON.Print(PLAYER_STATS));
		file.Close();
	}

	public static void CreatePlayer(string playerName)
	{
		PLAYER_STATS.Add(
			playerName, 
				new Dictionary<string, int>() {
					{ "wins", 0 },
					{ "losses", 0 },
					{ "total_points", 0 }
				}
		);
	}

	public static void IncrementWinCount(string player)
	{
		if (!PLAYER_STATS.ContainsKey(player))
		{
			CreatePlayer(player);
		}
		
		Dictionary<string, int> entry = (Dictionary<string, int>) PlayerData.PLAYER_STATS[player];
		int wins = (int) entry["wins"];
		entry["wins"] = wins + 1;
		SavePlayerStats();
	}

	public static void IncrementLossCount(string player)
	{
		if (!PLAYER_STATS.ContainsKey(player))
		{
			CreatePlayer(player);
		}
		
		Dictionary<string, int> entry = (Dictionary<string, int>) PlayerData.PLAYER_STATS[player];
		int losses = (int) entry["losses"];
		entry["losses"] = losses + 1;
		SavePlayerStats();
	}

	public static void IncreaseTotalPoints(string player, int scoreGained)
	{
		if (!PLAYER_STATS.ContainsKey(player))
		{
			CreatePlayer(player);
		}
		
		Dictionary<string, int> entry = (Dictionary<string, int>) PlayerData.PLAYER_STATS[player];
		int points = (int) entry["total_points"];
		entry["total_points"] = points + scoreGained;
		SavePlayerStats();
	}

	public static void DecreaseTotalPoints(string player, int scoreLost)
	{
		if (!PLAYER_STATS.ContainsKey(player))
		{
			CreatePlayer(player);
		}
		
		Dictionary<string, int> entry = (Dictionary<string, int>) PlayerData.PLAYER_STATS[player];
		int points = (int) entry["total_points"];
		entry["total_points"] = Mathf.Max(0, points - scoreLost);
		SavePlayerStats();
	}
}
