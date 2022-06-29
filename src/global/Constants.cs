using Godot;

public class Constants
{
	public static Color RED 					= new Color(1, 0.4f, 0.4f);
	public static Color WHITE 					= new Color(1, 1, 1);
	public static Color GREY 					= new Color(0.5f, 0.5f, 0.5f, 0.8f);
	public static float BOARD_SPACER_HEIGHT 	= 35;
	public static float BOARD_SPACER_WIDTH 		= 72;
	public static int BOARD_WALL_HALF_SIZE 		= 15;
	public static int BOARD_POINT_COUNT 		= 24;
	public static int BOARD_POINT_SIZE 			= 5;
	public static int BOARD_POINTS_PER_SIDE		= BOARD_POINT_COUNT / 2;
	public static Vector2 CELL_SIZE 			= new Vector2(90, 90);
	public static Vector2 CELL_PADDING 			= new Vector2(11.5f, 0);
	public static Vector2 BOARD_SIZE 			= new Vector2(BOARD_POINTS_PER_SIDE * CELL_SIZE.x + BOARD_SPACER_WIDTH + (BOARD_POINTS_PER_SIDE - 1) * CELL_PADDING.x, BOARD_POINT_SIZE * 2 * (CELL_SIZE.y + CELL_PADDING.y) + BOARD_SPACER_HEIGHT);
	public static Texture CHECKER_TEXTURE		= ResourceLoader.Load<Texture>("res://assets/images/checker/checker.png") as Texture;
	public static Vector2 SPRITE_SCALE			= new Vector2(CELL_SIZE.x / CHECKER_TEXTURE.GetWidth(), CELL_SIZE.y / CHECKER_TEXTURE.GetHeight());
	public static string PLAYER_1 				= "";
	public static string PLAYER_2 				= "";
}