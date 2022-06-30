# ChillchatBackgammon
 Chillchat coding assignment

## Controls

This game features a simple drag-and-drop gameplay mechanic. To move pieces, click on the desired piece, keep the mouse pressed and let go once over the target location.

Controls can also be viewed via the in-game menu.

## Rules

Rules are per the classic Backgammon rules, with the exception of *higher number precedence* (where if a move can be made according to either one die, bu not both, the higher number must be played). The doubling cube is also omitted for simplicity's sake.

Before each game, players must choose a stake amount they are willing to play for. Once the game has finished, the staked amount will be subtracted from the loser and added to the winners total points.

In accordance with Backgammon rules, different win conditions may multiply the staked amount before being added or subtracted from the players. This means the players may lose or gain more depending how the game is won.

### Multipliers

Gammon = stake * 2

Backgammon = stake * 3

The value of multipliers can be changed from *main/src/global/Constants.cs*

## Player Statistics

Players' statistics can be viewed through the main menu, and after each game.
