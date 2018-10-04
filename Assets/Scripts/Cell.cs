using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {

	public enum Color { white, black };

	[SerializeField]
	Color color;

	[SerializeField]
	Vector2 coordinates;

	[SerializeField]
	GameObject TokenRed;

	[SerializeField]
	GameObject TokenGreen;

	public GameObject Token { get; set; }
	public GameObject up, down, left, right, upLeft, upRight, downLeft, downRight;
	
	bool setup;
	GameObject Manager;

    //not correct
    float heuristic_value;

	void Start ()
	{
		//Instantiate the tokens
		if (coordinates.y == 1 || coordinates.y == 2)
		{
			Token = Instantiate(TokenRed, transform.position, TokenRed.transform.rotation);
		}
		else if (coordinates.y == 4 || coordinates.y == 5)
		{
			Token = Instantiate(TokenGreen, transform.position, TokenGreen.transform.rotation);
		}
		else
		{
			if (coordinates.x < 5)
			{
				Token = Instantiate(TokenGreen, transform.position, TokenGreen.transform.rotation);
			}
			else if (coordinates.x > 5)
			{
				Token = Instantiate(TokenRed, transform.position, TokenRed.transform.rotation);
			}
		}

		if (Token)
			Token.GetComponent<Token>().Cell = gameObject;

		Manager = GameObject.Find("GameManager");
		Manager.GetComponent<GameManager>().cellMatrix[(int)coordinates.x, (int)coordinates.y] = this.gameObject;
		setup = true;
	}
	
	void Update ()
	{
		setNeighbours(setup);
	}

	private void setNeighbours (bool set)
	{
		if (set)
		{
			GameObject[,] board = Manager.GetComponent<GameManager>().cellMatrix;

			//Sets the 3 cells to the left
			if (coordinates.x > 1)
			{
				left = board[(int)coordinates.x - 1, (int)coordinates.y];

				if (coordinates.y > 1)
					upLeft = board[(int)coordinates.x - 1, (int)coordinates.y - 1];

				if (coordinates.y < board.GetLength(1) - 1)
					downLeft = board[(int)coordinates.x - 1, (int)coordinates.y + 1];
			}

			//Sets the 3 cells to the right
			if (coordinates.x < board.GetLength(0) - 1)
			{
				right = board[(int)coordinates.x + 1, (int)coordinates.y];

				if (coordinates.y > 1)
					upRight = board[(int)coordinates.x + 1, (int)coordinates.y - 1];

				if (coordinates.y < board.GetLength(1) - 1)
					downRight = board[(int)coordinates.x + 1, (int)coordinates.y + 1];
			}

			//Sets cell above
			if (coordinates.y > 1)
				up = board[(int)coordinates.x, (int)coordinates.y - 1];

			//Sets cell below
			if (coordinates.y < board.GetLength(1) - 1)
				down = board[(int)coordinates.x, (int)coordinates.y + 1];

			set = false;
		}
	}

	public Color getColor()
	{
		return color;
	}

	public Vector2 getCoordinates()
	{
		return coordinates;
	}

	private void OnMouseDown()
	{
		if (!Token)
		{
			Manager.GetComponent<GameManager>().setSelected(gameObject);
		}
	}

    //not correct
    public void setHeuristicValue(float value)
    {
        heuristic_value = value;
    }

    public float getHeuristicValue()
    {
        return heuristic_value;
    }
}
