using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour {

	public enum Color { red, green };
	public GameObject Cell { get; set; }
	
	Color color;
	GameObject GameManager;

	void Awake ()
	{
		if (GetComponent<Renderer>().material.color == UnityEngine.Color.green)
			color = Color.green;

	}

    private void Start()
    {
        GameManager = GameObject.Find("GameManager");

    }

    void Update ()
	{
		
	}

	public Color getColor ()
	{
		return color;
	}

	private void OnMouseDown()
	{
		GameManager.GetComponent<GameManager>().setSelected(gameObject);
	}
}
