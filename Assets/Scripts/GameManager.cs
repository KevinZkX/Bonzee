using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public enum Move { none, up, down, left, right, upLeft, upRight, downLeft, downRight };
	public GameObject[,] cellMatrix;
	public Text turnText, win, draw;
    public Button playAgain;

	Move move;

	GameObject CellSelected, TokenSelected;

    GameObject[] redTokenRemaining, greenTokenRemaining;

	Token.Color turn, AI_Color;
	
	int turnNo, roundsWithNoKill;

	bool isMoving, kill;

	AIAgent AI;

	void Awake()
	{
		cellMatrix = new GameObject[10,6];
        win.enabled = false;
        playAgain.gameObject.SetActive(false);
		AI_Color = (PlayerPrefs.GetInt("AI_Color") == 1) ? Token.Color.green : Token.Color.red;
    }

	void Start ()
	{
		move = Move.none;
        kill = false;
		isMoving = false;
		turn = Token.Color.green;
		turnNo = 0;
        turnText.text = "Green Turn";
        turnText.color = Color.green;
        redTokenRemaining = GameObject.FindGameObjectsWithTag("RedToken");
        greenTokenRemaining = GameObject.FindGameObjectsWithTag("GreenToken");
		AI = new AIAgent();
    }
	
	void Update ()
	{
		redTokenRemaining = GameObject.FindGameObjectsWithTag("RedToken");
		greenTokenRemaining = GameObject.FindGameObjectsWithTag("GreenToken");

		//Check if a player wins
		if (redTokenRemaining.Length == 0)
		{
			win.enabled = true;
			win.text = "Green Wins!";
			win.color = Color.green;
			playAgain.gameObject.SetActive(true);
		}
		if (greenTokenRemaining.Length == 0)
		{
			win.enabled = true;
			win.text = "Red Wins!";
			win.color = Color.red;
			playAgain.gameObject.SetActive(true);
		}

		//Checks if there is a draw
		if (!win.enabled)
			checkDraw();

		if (turn == Token.Color.green)
		{
			turnText.text = "Green Turn";
			turnText.color = Color.green;
		}
		else
		{
			turnText.text = "Red Turn";
			turnText.color = Color.red;
		}

		if (!win.enabled && !isMoving)
		{
			if (((CellSelected && TokenSelected) || turn == AI_Color) && redTokenRemaining.Length > 0 && greenTokenRemaining.Length > 0)
				MoveToken(turn);
		}

		draw.text = "Moves until draw: " + (10 - roundsWithNoKill).ToString();
	}

	public void checkDraw ()
	{
		if (roundsWithNoKill > 9 && !kill)
		{
			win.enabled = true;
			win.text = "Draw!";
			playAgain.gameObject.SetActive(true);
		}
	}

	public void MoveToken(Token.Color tu)
	{
		move = Move.none;

		if (tu == AI_Color)
		{
			//AI turn
			turnNo++;
			Debug.Log("Turn " + turnNo);
			float time = Time.realtimeSinceStartup;

			int color = (AI_Color == Token.Color.green) ? 1 : 2;
			AI.setGameState(cellMatrix);
			AI.setPlyNumber(4);
			AI.setNextState(AI.getGameState(), AI.getPlyNumber(), color);

			Vector2 orig = AI.getNextMoveStart(AI.getNextMove());
			Vector2 dest = AI.getNextMoveEnd(AI.getNextMove());

			Debug.Log("Origin: " + (char)(64 + orig.y) + orig.x);
			Debug.Log("Destination: " + (char)(64 + dest.y) + dest.x);

			TokenSelected = cellMatrix[(int)orig.x, (int)orig.y].GetComponent<Cell>().Token;
			CellSelected = cellMatrix[(int)dest.x, (int)dest.y];

			Debug.Log("Computation time: " + (Time.realtimeSinceStartup - time) + "s");
		}

		if (!CellSelected.GetComponent<Cell>().Token && CellSelected.GetComponent<Cell>().Token != TokenSelected && TokenSelected.GetComponent<Token>().getColor() == tu)
		{
			//Lateral move on all cells
			if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().up)
			{
				move = Move.up;
				isMoving = true;
			}
			else if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().down)
			{
				move = Move.down;
				isMoving = true;
			}
			if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().right)
			{
				move = Move.right;
				isMoving = true;
			}
			else if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().left)
			{
				move = Move.left;
				isMoving = true;
			}

			//Diagonal move on black cells only
			if (TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().getColor() == Cell.Color.black)
			{
				if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().upRight)
				{
					move = Move.upRight;
					isMoving = true;
				}
				else if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().downRight)
				{
					move = Move.downRight;
					isMoving = true;
				}
				if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().upLeft)
				{
					move = Move.upLeft;
					isMoving = true;
				}
				else if (CellSelected == TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().downLeft)
				{
					move = Move.downLeft;
					isMoving = true;
				}
			}

			if ((CellSelected.GetComponent<Cell>().getColor() == Cell.Color.white && (move == Move.left || move == Move.right || move == Move.up || move == Move.down)) ||
				CellSelected.GetComponent<Cell>().getColor() == Cell.Color.black && move != Move.none)
			{
				if (tu == Token.Color.green)
					turn = Token.Color.red;
				else
					turn = Token.Color.green;
			}
		}

		if (move != Move.none)
		{
            StartCoroutine(MoveToPosition(TokenSelected, CellSelected, 1));
			TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>().Token = null;
			TokenSelected.GetComponent<Token>().Cell = CellSelected;
			CellSelected.GetComponent<Cell>().Token = TokenSelected;
			RemoveTokens(move, TokenSelected.GetComponent<Token>().Cell.GetComponent<Cell>());
		}

    }

    IEnumerator MoveToPosition(GameObject initialPos, GameObject endPos, float time)
    {
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            initialPos.transform.position = Vector3.Slerp(initialPos.transform.position, endPos.transform.position, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

		isMoving = false;
    }

    public void RemoveTokens(Move mo, Cell ce)
    {
        kill = false;
        Token.Color col = TokenSelected.GetComponent<Token>().getColor();
		
        if (mo == Move.up)
		{
			if (ce.up && ce.up.GetComponent<Cell>().Token && ce.up.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.up && ce.up.GetComponent<Cell>().Token && ce.up.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.up.GetComponent<Cell>().Token);
					ce = ce.up.GetComponent<Cell>();
				}
			}
			else if (ce.down.GetComponent<Cell>().down && ce.down.GetComponent<Cell>().down.GetComponent<Cell>().Token && ce.down.GetComponent<Cell>().down.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.down.GetComponent<Cell>();
				RemoveTokens(Move.down, ce);
			}
		}
		else if (mo == Move.down)
		{
			if (ce.down && ce.down.GetComponent<Cell>().Token && ce.down.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.down && ce.down.GetComponent<Cell>().Token && ce.down.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.down.GetComponent<Cell>().Token);
					ce = ce.down.GetComponent<Cell>();
				}
			}
			else if (ce.up.GetComponent<Cell>().up && ce.up.GetComponent<Cell>().up.GetComponent<Cell>().Token && ce.up.GetComponent<Cell>().up.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.up.GetComponent<Cell>();
				RemoveTokens(Move.up, ce);
			}
		}
		else if (mo == Move.left)
		{
			if (ce.left && ce.left.GetComponent<Cell>().Token && ce.left.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.left && ce.left.GetComponent<Cell>().Token && ce.left.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.left.GetComponent<Cell>().Token);
					ce = ce.left.GetComponent<Cell>();
				}
			}
			else if (ce.right.GetComponent<Cell>().right && ce.right.GetComponent<Cell>().right.GetComponent<Cell>().Token && ce.right.GetComponent<Cell>().right.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.right.GetComponent<Cell>();
				RemoveTokens(Move.right, ce);
			}
		}
		else if (mo == Move.right)
		{
			if (ce.right && ce.right.GetComponent<Cell>().Token && ce.right.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.right && ce.right.GetComponent<Cell>().Token && ce.right.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.right.GetComponent<Cell>().Token);
					ce = ce.right.GetComponent<Cell>();
				}
			}
			else if (ce.left.GetComponent<Cell>().left && ce.left.GetComponent<Cell>().left.GetComponent<Cell>().Token && ce.left.GetComponent<Cell>().left.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.left.GetComponent<Cell>();
				RemoveTokens(Move.left, ce);
			}
		}
		else if (mo == Move.upLeft)
		{
			if (ce.upLeft && ce.upLeft.GetComponent<Cell>().Token && ce.upLeft.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.upLeft && ce.upLeft.GetComponent<Cell>().Token && ce.upLeft.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.upLeft.GetComponent<Cell>().Token);
					ce = ce.upLeft.GetComponent<Cell>();
				}
			}
			else if (ce.downRight.GetComponent<Cell>().downRight && ce.downRight.GetComponent<Cell>().downRight.GetComponent<Cell>().Token && ce.downRight.GetComponent<Cell>().downRight.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.downRight.GetComponent<Cell>();
				RemoveTokens(Move.downRight, ce);
			}
		}
		else if (mo == Move.upRight)
		{
			if (ce.upRight && ce.upRight.GetComponent<Cell>().Token && ce.upRight.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.upRight && ce.upRight.GetComponent<Cell>().Token && ce.upRight.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.upRight.GetComponent<Cell>().Token);
					ce = ce.upRight.GetComponent<Cell>();
				}
			}
			else if (ce.downLeft.GetComponent<Cell>().downLeft && ce.downLeft.GetComponent<Cell>().downLeft.GetComponent<Cell>().Token && ce.downLeft.GetComponent<Cell>().downLeft.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.downLeft.GetComponent<Cell>();
				RemoveTokens(Move.downLeft, ce);
			}
		}
		else if (mo == Move.downLeft)
		{
			if (ce.downLeft && ce.downLeft.GetComponent<Cell>().Token && ce.downLeft.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.downLeft && ce.downLeft.GetComponent<Cell>().Token && ce.downLeft.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.downLeft.GetComponent<Cell>().Token);
					ce = ce.downLeft.GetComponent<Cell>();
				}
			}
			else if (ce.upRight.GetComponent<Cell>().upRight && ce.upRight.GetComponent<Cell>().upRight.GetComponent<Cell>().Token && ce.upRight.GetComponent<Cell>().upRight.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.upRight.GetComponent<Cell>();
				RemoveTokens(Move.upRight, ce);
			}
		}
		else if (mo == Move.downRight)
		{
			if (ce.downRight && ce.downRight.GetComponent<Cell>().Token && ce.downRight.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				kill = true;

				while (ce.downRight && ce.downRight.GetComponent<Cell>().Token && ce.downRight.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
				{
					Destroy(ce.downRight.GetComponent<Cell>().Token);
					ce = ce.downRight.GetComponent<Cell>();
				}
			}
			else if (ce.upLeft.GetComponent<Cell>().upLeft && ce.upLeft.GetComponent<Cell>().upLeft.GetComponent<Cell>().Token && ce.upLeft.GetComponent<Cell>().upLeft.GetComponent<Cell>().Token.GetComponent<Token>().getColor() != col)
			{
				ce = ce.upLeft.GetComponent<Cell>();
				RemoveTokens(Move.upLeft, ce);
			}
		}

		if (kill)
			roundsWithNoKill = 0;
		else
			roundsWithNoKill++;
    }

	public void setSelected(GameObject obj)
	{
        if (obj.GetComponent<Cell>())
        {
            CellSelected = obj;
        }
        else if (obj.GetComponent<Token>() && obj.GetComponent<Token>().getColor() == Token.Color.red && turn == Token.Color.red)
        {
            CellSelected = null;
            TokenSelected = obj;
        }

        else if (obj.GetComponent<Token>() && obj.GetComponent<Token>().getColor() == Token.Color.green && turn == Token.Color.green)
		{ 
            CellSelected = null;
            TokenSelected = obj;
        }
    }
}
