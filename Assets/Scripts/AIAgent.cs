using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct ValidState
{
	public bool valid;
	public int[,] gs;

	public ValidState(bool val, int[,] gs)
	{
		valid = val;
		this.gs = gs;
	}
}

public class AIAgent {

	int ply_number;
	int[] bestHeur;
	int[,] gameState, nextMove;
	Stack<int[,]> searchStates;

	public AIAgent()
	{
		gameState = new int[10, 6];
		nextMove = new int[10, 6];
		bestHeur = new int[10];
		searchStates = new Stack<int[,]>(4);
	}

	public void setGameState (GameObject[,] gs)
	{
		//Variable reset
		for (int i = 0; i < gs.GetLength(0); i++)
		{
			bestHeur[i] = 0;

			for (int j = 0; j < gs.GetLength(1); j++)
			{
				gameState[i, j] = 0;
				nextMove[i, j] = 0;
			}
		}

		searchStates.Clear();

		//Game state conversion
		for (int i = 1; i < gs.GetLength(0); i++)
		{
			for (int j = 1; j < gs.GetLength(1); j++)
			{
				//Cells: White = 10, Black = 20
				if (gs[i, j].GetComponent<Cell>().getColor() == Cell.Color.white)
					gameState[i, j] += 10;
				else if (gs[i, j].GetComponent<Cell>().getColor() == Cell.Color.black)
					gameState[i, j] += 20;

				//Tokens: Green = 1, Red = 2
				if (gs[i, j].GetComponent<Cell>().Token)
				{
					if (gs[i, j].GetComponent<Cell>().Token.GetComponent<Token>().getColor() == Token.Color.green)
						gameState[i, j]++;
					else if (gs[i, j].GetComponent<Cell>().Token.GetComponent<Token>().getColor() == Token.Color.red)
						gameState[i, j] += 2;
				}
			}
		}
	}

	public int getPlyNumber ()
	{
		return ply_number;
	}

	public void setPlyNumber(int ply)
	{
		ply_number = ply;
	}

	public Vector2 getNextMoveStart (int[,] gs)
	{
		return new Vector2(gs[0, 0], gs[0, 1]);
	}

	public Vector2 getNextMoveEnd(int[,] gs)
	{
		return new Vector2(gs[0, 2], gs[0, 3]);
	}

	public void setNextState (int[,] gs, int ply, int color)
	{
		if (ply % 2 == 0)
		{
			if (color == 1)
				setNextStateEvenGreen(gs, ply, color);
			else
				setNextStateEvenRed(gs, ply, color);
		}
		else
		{
			if (color == 1)
				setNextStateOddGreen(gs, ply, color);
			else
				setNextStateOddRed(gs, ply, color);
		}
	}

	public void setNextStateOddGreen (int[,] gs, int ply, int color)
	{
		int otherCol = (color == 2) ? 1 : 2;
		//Debug.Log(ply);

		//Base case, ply = 3
		if (ply == 0)
		{
			searchStates.Push(gs);
			gs[0, 5] = getHeuristicTwo(gs);
		}
		//Odd plies
		else if (ply % 2 == 1)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? -10000 : 10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == color)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, color);
							if (temp.valid)
							{
								setNextStateOddGreen(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] > gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									if (ply == ply_number)
									{
										nextMove = searchStates.Peek();
									}
									//Alpha/beta pruning
									else if (searchStates.Peek()[0, 5] > gs[0, 5])
									{
										searchStates.Pop();
										break;
									}
								}

								if (ply != ply_number && bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
		//Even plies
		else if (ply % 2 == 0)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? 10000 : -10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == otherCol)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, otherCol);
							if (temp.valid)
							{
								setNextStateOddGreen(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] < gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									//Alpha/beta pruning
									if (bestHeur[ply] != 0 && gs[0, 5] < bestHeur[ply])
									{
										searchStates.Pop();
										break;
									}
								}

								if (bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
	}

	public void setNextStateOddRed (int[,] gs, int ply, int color)
	{
		int otherCol = (color == 2) ? 1 : 2;
		//Debug.Log(ply);

		//Base case, ply = 3
		if (ply == 0)
		{
			searchStates.Push(gs);
			gs[0, 5] = getHeuristicTwo(gs);
		}
		//Odd plies
		else if (ply % 2 == 1)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? -10000 : 10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == color)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, color);
							if (temp.valid)
							{
								setNextStateOddRed(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] < gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									if (ply == ply_number)
									{
										nextMove = searchStates.Peek();
									}
									//Alpha/beta pruning
									else if (searchStates.Peek()[0, 5] < gs[0, 5])
									{
										searchStates.Pop();
										break;
									}
								}

								if (ply != ply_number && bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
		//Even plies
		else if (ply % 2 == 0)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? 10000 : -10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == otherCol)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, otherCol);
							if (temp.valid)
							{
								setNextStateOddRed(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] > gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									//Alpha/beta pruning
									if (bestHeur[ply] != 0 && gs[0, 5] > bestHeur[ply])
									{
										searchStates.Pop();
										break;
									}
								}

								if (bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
	}

	public void setNextStateEvenGreen (int[,] gs, int ply, int color)
	{
		int otherCol = (color == 2) ? 1 : 2;
		//Debug.Log(ply);

		//Base case, ply = 3
		if (ply == 0)
		{
			searchStates.Push(gs);
			gs[0, 5] = getHeuristicTwo(gs);
		}
		//Odd plies
		else if (ply % 2 == 1)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? 10000 : -10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == otherCol)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, otherCol);
							if (temp.valid)
							{
								setNextStateEvenGreen(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] < gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									//Alpha/beta pruning
									if (bestHeur[ply] != 0 && gs[0, 5] < bestHeur[ply])
									{
										searchStates.Pop();
										break;
									}
								}

								if (bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
		//Even plies
		else if (ply % 2 == 0)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? -10000 : 10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == color)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, color);
							if (temp.valid)
							{
								setNextStateEvenGreen(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] > gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									if (ply == ply_number)
									{
										nextMove = searchStates.Peek();
									}
									//Alpha/beta pruning
									else if (searchStates.Peek()[0, 5] > gs[0, 5])
									{
										searchStates.Pop();
										break;
									}
								}

								if (ply != ply_number && bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
	}

	public void setNextStateEvenRed (int[,] gs, int ply, int color)
	{
		int otherCol = (color == 2) ? 1 : 2;
		//Debug.Log(ply);

		//Base case, ply = 3
		if (ply == 0)
		{
			searchStates.Push(gs);
			gs[0, 5] = getHeuristicTwo(gs);
		}
		//Odd plies
		else if (ply % 2 == 1)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? 10000 : -10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == otherCol)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, otherCol);
							if (temp.valid)
							{
								setNextStateEvenRed(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] > gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									//Alpha/beta pruning
									if (bestHeur[ply] != 0 && gs[0, 5] > bestHeur[ply])
									{
										searchStates.Pop();
										break;
									}
								}

								if (bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
		//Even plies
		else if (ply % 2 == 0)
		{
			searchStates.Push(gs);
			int nextPly = ply - 1;
			bestHeur[nextPly] = 0;
			gs[0, 5] = (color == 1) ? -10000 : 10000;

			for (int i = 1; i < 10; ++i)
			{
				for (int j = 1; j < 6; ++j)
				{
					if (gs[i, j] % 10 == color)
					{
						ValidState temp;

						foreach (GameManager.Move move in Enum.GetValues(typeof(GameManager.Move)))
						{
							temp = simulateMove(gs, new Vector2(i, j), move, color);
							if (temp.valid)
							{
								setNextStateEvenRed(temp.gs, nextPly, color);

								if (searchStates.Peek()[0, 5] < gs[0, 5])
								{
									gs[0, 5] = searchStates.Peek()[0, 5];

									if (ply == ply_number)
									{
										nextMove = searchStates.Peek();
									}
									//Alpha/beta pruning
									else if (searchStates.Peek()[0, 5] < gs[0, 5])
									{
										searchStates.Pop();
										break;
									}
								}

								if (ply != ply_number && bestHeur[ply] == 0)
								{
									bestHeur[ply] = gs[0, 5];
								}

								searchStates.Pop();
							}
						}
					}
				}
			}
		}
	}

	public int[,] getGameState()
	{
		return gameState;
	}

	public int[,] getNextMove()
	{
		return nextMove;
	}
	
	private ValidState simulateMove (int[,] gs, Vector2 start, GameManager.Move move, int color)
	{
		int[,] copyGs = new int[10,6];

		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				copyGs[i, j] = gs[i, j];
			}
		}

		Vector2 end = new Vector2(start.x, start.y);

		if (move == GameManager.Move.up)
		{
			if (end.y - 1 > 0 && copyGs[(int)end.x, (int)end.y - 1] % 10 == 0)
				end.y -= 1;
		}
		else if (move == GameManager.Move.down)
		{
			if (end.y + 1 < 6 && copyGs[(int)end.x, (int)end.y + 1] % 10 == 0)
				end.y += 1;
		}
		else if (move == GameManager.Move.left)
		{
			if (end.x - 1 > 0 && copyGs[(int)end.x - 1, (int)end.y] % 10 == 0)
				end.x -= 1;
		}
		else if (move == GameManager.Move.right)
		{
			if (end.x + 1 < 10 && copyGs[(int)end.x + 1, (int)end.y] % 10 == 0)
				end.x += 1;
		}
		else if (move == GameManager.Move.upLeft)
		{
			if (end.x - 1 > 0 && end.y - 1 > 0 && copyGs[(int)end.x - 1, (int)end.y - 1] % 10 == 0 && copyGs[(int)start.x, (int)start.y] >= 20)
			{
				end.x -= 1;
				end.y -= 1;
			}
		}
		else if (move == GameManager.Move.upRight)
		{
			if (end.x + 1 < 10 && end.y - 1 > 0 && copyGs[(int)end.x + 1, (int)end.y - 1] % 10 == 0 && copyGs[(int)start.x, (int)start.y] >= 20)
			{
				end.x += 1;
				end.y -= 1;
			}
		}
		else if (move == GameManager.Move.downLeft)
		{
			if (end.x - 1 > 0 && end.y + 1 < 6 && copyGs[(int)end.x - 1, (int)end.y + 1] % 10 == 0 && copyGs[(int)start.x, (int)start.y] >= 20)
			{
				end.x -= 1;
				end.y += 1;
			}
		}
		else if (move == GameManager.Move.downRight)
		{
			if (end.x + 1 < 10 && end.y + 1 < 6 && copyGs[(int)end.x + 1, (int)end.y + 1] % 10 == 0 && copyGs[(int)start.x, (int)start.y] >= 20)
			{
				end.x += 1;
				end.y += 1;
			}
		}

		if (end == start)
		{
			return new ValidState(false, null);
		}
		else
		{
			copyGs[(int)start.x, (int)start.y] -= color;
			copyGs[(int)end.x, (int)end.y] += color;
			copyGs[0, 0] = (int)start.x;
			copyGs[0, 1] = (int)start.y;
			copyGs[0, 2] = (int)end.x;
			copyGs[0, 3] = (int)end.y;

			return new ValidState(true, removeTokens(copyGs, end, move, color));
		}
	}

	private int[,] removeTokens(int[,] gs, Vector2 newPos, GameManager.Move move, int color)
	{
        int[,] copyGs = gs;
		int otherCol = (color == 1) ? 2 : 1;

		if (move == GameManager.Move.up)
		{
            //FORWARD ATTACK
			if (newPos.y - 1 > 0 && copyGs[(int)newPos.x, (int)newPos.y - 1] % 10 == otherCol)
			{
				while (newPos.y - 1 > 0 && copyGs[(int)newPos.x, (int)newPos.y - 1] % 10 == otherCol)
				{
					copyGs[(int)newPos.x, (int)newPos.y - 1] -= otherCol;
					newPos.y--;
				}
			}
            //BACKWARD ATTACK
			else if (newPos.y + 2 < 6 && copyGs[(int)newPos.x, (int)newPos.y + 2] % 10 == otherCol)
			{
				removeTokens(copyGs, new Vector2(newPos.x, newPos.y + 1), GameManager.Move.down, color);
            }
		}
		else if (move == GameManager.Move.down)
		{
            //FORWARD ATTACK
            if (newPos.y + 1 < 6 && copyGs[(int)newPos.x, (int)newPos.y + 1] % 10 == otherCol)
            {
				while (newPos.y + 1 < 6 && copyGs[(int)newPos.x, (int)newPos.y + 1] % 10 == otherCol)
				{
					copyGs[(int)newPos.x, (int)newPos.y + 1] -= otherCol;
					newPos.y++;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.y - 2 > 0 && copyGs[(int)newPos.x, (int)newPos.y - 2] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x, newPos.y - 2), GameManager.Move.up, color);
			}
        }
		else if (move == GameManager.Move.left)
		{
            //FORWARD ATTACK
            if (newPos.x - 1 > 0 && copyGs[(int)newPos.x - 1, (int)newPos.y] % 10 == otherCol)
            {
				while (newPos.x - 1 > 0 && copyGs[(int)newPos.x - 1, (int)newPos.y] % 10 == otherCol)
				{
					copyGs[(int)newPos.x - 1, (int)newPos.y] -= otherCol;
					newPos.x--;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.x + 2 < 10 && copyGs[(int)newPos.x + 2, (int)newPos.y] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x + 1, newPos.y), GameManager.Move.right, color);
			}
        }
		else if (move == GameManager.Move.right)
		{
            //FORWARD ATTACK
            if (newPos.x + 1 < 10 && copyGs[(int)newPos.x + 1, (int)newPos.y] % 10 == otherCol)
            {
				while (newPos.x + 1 < 10 && copyGs[(int)newPos.x + 1, (int)newPos.y] % 10 == otherCol)
				{
					copyGs[(int)newPos.x + 1, (int)newPos.y] -= otherCol;
					newPos.x++;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.x - 2 > 0 && copyGs[(int)newPos.x - 2, (int)newPos.y] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x - 1, newPos.y), GameManager.Move.left, color);
			}
        }
		else if (move == GameManager.Move.upLeft)
		{
            //FORWARD ATTACK
            if (newPos.x - 1 > 0 && newPos.y - 1 > 0 && copyGs[(int)newPos.x - 1, (int)newPos.y - 1] % 10 == otherCol)
            {
				while (newPos.x - 1 > 0 && newPos.y - 1 > 0 && copyGs[(int)newPos.x - 1, (int)newPos.y - 1] % 10 == otherCol)
				{
					copyGs[(int)newPos.x - 1, (int)newPos.y - 1] -= otherCol;
					newPos.x--;
					newPos.y--;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.x + 2 < 10 && newPos.y + 2 < 6 && copyGs[(int)newPos.x + 2, (int)newPos.y + 2] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x + 1, newPos.y + 1), GameManager.Move.downRight, color);
			}
        }
		else if (move == GameManager.Move.upRight)
		{
            //FORWARD ATTACK
            if (newPos.x + 1 < 10 && newPos.y - 1 > 0 && copyGs[(int)newPos.x + 1, (int)newPos.y - 1] % 10 == otherCol)
            {
				while (newPos.x + 1 < 10 && newPos.y - 1 > 0 && copyGs[(int)newPos.x + 1, (int)newPos.y - 1] % 10 == otherCol)
				{
					copyGs[(int)newPos.x + 1, (int)newPos.y - 1] -= otherCol;
					newPos.x++;
					newPos.y--;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.x - 2 > 0 && newPos.y + 2 < 6 && copyGs[(int)newPos.x - 2, (int)newPos.y + 2] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x - 1, newPos.y + 1), GameManager.Move.downLeft, color);
			}
        }
		else if (move == GameManager.Move.downLeft)
		{
            //FORWARD ATTACK
            if (newPos.x - 1 > 0 && newPos.y + 1 < 6 && copyGs[(int)newPos.x - 1, (int)newPos.y + 1] % 10 == otherCol)
            {
				while (newPos.x - 1 > 0 && newPos.y + 1 < 6 && copyGs[(int)newPos.x - 1, (int)newPos.y + 1] % 10 == otherCol)
				{
					copyGs[(int)newPos.x - 1, (int)newPos.y + 1] -= otherCol;
					newPos.x--;
					newPos.y++;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.x + 2 < 10 && newPos.y - 2 > 0 && copyGs[(int)newPos.x + 2, (int)newPos.y - 2] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x + 1, newPos.y - 1), GameManager.Move.upRight, color);
			}
        }
		else if (move == GameManager.Move.downRight)
		{
            //FORWARD ATTACK
            if (newPos.x + 1 < 10 && newPos.y + 1 < 6 && copyGs[(int)newPos.x + 1, (int)newPos.y + 1] % 10 == otherCol)
            {
				while (newPos.x + 1 < 10 && newPos.y + 1 < 6 && copyGs[(int)newPos.x + 1, (int)newPos.y + 1] % 10 == otherCol)
				{
					copyGs[(int)newPos.x + 1, (int)newPos.y + 1] -= otherCol;
					newPos.x++;
					newPos.y++;
				}
            }
            //BACKWARD ATTACK
            else if (newPos.x - 2 > 0 && newPos.y - 2 > 0 && copyGs[(int)newPos.x - 2, (int)newPos.y - 2] % 10 == otherCol)
            {
				removeTokens(copyGs, new Vector2(newPos.x - 1, newPos.y - 1), GameManager.Move.upLeft, color);
			}
        }
        return copyGs;
	}

	private int getHeuristicTwo(int[,] gs)
	{
		int redTokens = 0;
		int greenTokens = 0;

		for (int i = 1; i < gs.GetLength(0); i++)
		{
			for (int j = 1; j < gs.GetLength(1); j++)
			{
				if (gs[i, j] % 10 == 1)
					greenTokens++;
				else if (gs[i, j] % 10 == 2)
					redTokens++;
			}
		}

		if (greenTokens == 0)
			return -1000000;
		else if (redTokens == 0)
			return 1000000;

		return greenTokens - redTokens;
	}

//	private float getHeuristic(int[,] cell_temps)
//	{
//		float sum_of_green_vertical_index = 0;
//		float sum_of_green_horizontal_index = 0;
//		float sum_of_red_vertical_index = 0;
//		float sum_of_red_horizontal_index = 0;
//
//		for (int i = 1; i < cell_temps.GetLength(0); i++)
//		{
//			for (int j = 1; j < cell_temps.GetLength(1); j++)
//			{
//				if (cell_temps[i, j] % 10 == 1)
//				{
//
//					sum_of_green_vertical_index += i;
//					sum_of_green_horizontal_index += j;
//
//				}
//
//				if (cell_temps[i, j] % 10 == 2)
//				{
//
//					sum_of_red_vertical_index += i;
//					sum_of_red_horizontal_index += j;
//				}
//			}
//		}
//
//		if (sum_of_green_horizontal_index == 0 && sum_of_green_vertical_index == 0)
//		{
//			return -1000000f;
//		}
//		else if (sum_of_red_horizontal_index == 0 && sum_of_red_vertical_index == 0)
//		{
//			return 1000000f;
//		}
//
//		return (100 * sum_of_green_horizontal_index + 50 * sum_of_green_vertical_index - 100 * sum_of_red_horizontal_index - 50 * sum_of_red_vertical_index);
//	}

	private int[,] copyGs (int[,] gs)
	{
		int[,] copy = new int[gs.GetLength(0), gs.GetLength(1)];

		for (int i = 0; i < gs.GetLength(0); i++)
		{
			for (int j = 0; j < gs.GetLength(1); j++)
			{
				copy[i, j] = gs[i, j];
			}
		}

		return copy;
	}
}
