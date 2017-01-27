﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Navigator : MonoBehaviour {
    // singleton
    public static Navigator Instance;
	public AudioClip clickS;
    public AudioClip highlightAudio;

    void Awake() {
        if (!Instance) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(string action) {
		// Loads the scene based on the given string, or exits the game if the string is Quit
		AudioSource.PlayClipAtPoint (clickS, new Vector2(0.0f, 0.0f), 100.0f);
        if (action == "Quit") {
            Application.Quit();
        } else {
            SceneManager.LoadScene(action);
        }
    }
}
