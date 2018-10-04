using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour {

	public void playAgain()
    {
        SceneManager.LoadScene(1);
    }

	public void play(int color)
	{
		SceneManager.LoadScene(0);
		PlayerPrefs.SetInt("AI_Color", color);
	}
}
