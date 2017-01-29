﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class BountyManager : NetworkBehaviour {

	public const int BASE_BOUNTY = 100;

	public SyncListInt playerBounties = new SyncListInt();
	public SyncListInt killStreak = new SyncListInt();

	//public int localID;
	private GameObject bountyPanel;
	private List<GameObject> bountyTexts = new List<GameObject>();
	private Canvas canvas;
	private Font font;
    public GameObject spawnPoint;
    public int maxResources = 40;
    public GameObject resourcePrefab;
    private GameObject MapGen;

	public bool victoryUndeclared;

	// Use this for initialization
	void Start () {
		victoryUndeclared = true;
		MapGen = GameObject.FindGameObjectWithTag("mapGen");
        //maxResources = Mathf.RoundToInt((width + height) / 50);
        
		font = Resources.Load<Font>("Art/Fonts/riesling");

        Random.InitState(System.DateTime.Now.Millisecond);
		for (int i = 0; i < maxResources; i++) {
			CmdSpawnResource ();
		}

        if (!isLocalPlayer) {
			return;
		}

		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		if (canvas != null) {
			CreateBountyPanel ();
			//print ("makin' a bounty board");
		}
	}


    [Command]
    void CmdSpawnResource()
    {
        if (!isServer)
        {
            return;
        }
        ClientScene.RegisterPrefab(resourcePrefab);
        GameObject instantiatedResource = Instantiate(resourcePrefab, new Vector2(Random.Range(-MapGen.GetComponent<MapGenerator>().width / 2, MapGen.GetComponent<MapGenerator>().width / 2), Random.Range(-MapGen.GetComponent<MapGenerator>().height / 2, MapGen.GetComponent<MapGenerator>().height / 2)), Quaternion.identity) as GameObject;
        NetworkServer.Spawn(instantiatedResource);
    }


	/*[Command]
	void CmdCreateID ()
	{
		int newID = playerBounties.Count;
		playerBounties.Add (100);
		killStreak.Add (0);
		if (bountyPanel != null) {
			int playerCount = playerBounties.Count;
			bountyTexts.Add (UI.CreateText ("Bounty Text " + newID, "Player " + newID + " | " + playerBounties [newID] + "g", font, Color.black, 24, bountyPanel.transform,
				Vector3.zero, new Vector2 (0.1f, 1f/playerCount * (playerCount-(newID+1))), new Vector2 (0.9f, 1f/playerCount * (playerCount-newID)), TextAnchor.UpperLeft, true));
		}
	}*/


    
	// Update is called once per frame
    void Update () {
		if (canvas == null) {
			canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
			if (canvas != null) {
				CreateBountyPanel ();
				//print ("makin' a bounty board (late)");
			}
		}

		if (playerBounties.Count > 0) {
			Player[] playerList = FindObjectsOfType<Player> ();

			for (int i = 0; i < playerList.Length; i++) {
				if (isServer) {
					int upgradeBounty = 10 * (int)Mathf.Floor (playerList [i].lowUpgrades / 2)
					                   + 25 * playerList [i].midUpgrades
					                   + 100 * playerList [i].highUpgrades;
					int killStreakBounty = 100 * killStreak [playerList [i].playerID];
					float bonusMod = 1f;
					if (playerList [i].playerID == GetHighestBounty ()) {
						bonusMod = 1.2f;
					}
					playerBounties [playerList [i].playerID] = (int)((BASE_BOUNTY + upgradeBounty + killStreakBounty)*bonusMod);
				}

				if (victoryUndeclared && playerBounties [playerList [i].playerID] >= 1000) {
					StartCoroutine(DeclareVictory (playerList [i].playerID));
					victoryUndeclared = false;
				}

				if (bountyTexts.Count <= i) {
					int playerCount = playerBounties.Count;
					if (bountyPanel != null) {
						//print ("postin' a bounty (late)");
						/*bountyTexts.Add (UI.CreateText ("Bounty Text " + i, "Player " + i + " | " + playerBounties [i] + "g", font, Color.black, 24, bountyPanel.transform,
							Vector3.zero, new Vector2 (0.1f, 1.0f / 4f * (3-i)), new Vector2 (0.9f, 1.0f / 4f * (4-i)), TextAnchor.MiddleCenter, true));*/
						bountyTexts.Add (UI.CreateText ("Bounty Text " + i, "Player " + (i+1) + " | " + playerBounties [i] + "g", font, Color.black, 24, bountyPanel.transform,
							Vector3.zero, new Vector2 (0.1f, 1f/playerCount * (playerCount-(i+1))), new Vector2 (0.9f, 1f/playerCount * (playerCount-i)), TextAnchor.UpperLeft, true));
					}
				} else {
					bountyTexts [i].GetComponent<Text> ().text = "Player " + (i+1) + "  |  " + playerBounties [i];
					/*if (i == localID) {
						bountyTexts [i].GetComponent<Text> ().color = Color.red;
					}*/
				}
			}
		}

		/*if (Input.GetKeyDown (KeyCode.Q)) {
			StartCoroutine(DeclareVictory (0));
		}*/
	}

	public int AddID () {
		//localID = CmdCreateID ();
		//return localID;

		int newID = playerBounties.Count;
		playerBounties.Add (100);
		killStreak.Add (0);
		if (bountyPanel != null) {
			int playerCount = playerBounties.Count;
			bountyTexts.Add (UI.CreateText ("Bounty Text " + newID, "Player " + (newID+1) + " | " + playerBounties [newID] + "g", font, Color.black, 24, bountyPanel.transform,
				Vector3.zero, new Vector2 (0.1f, 1f / playerCount * (playerCount - (newID + 1))), new Vector2 (0.9f, 1f / playerCount * (playerCount - newID)), TextAnchor.UpperLeft, true));
			/*bountyTexts.Add (UI.CreateText ("Bounty Text " + newID, "Player " + newID + " | " + playerBounties [newID] + "g", font, Color.black, 24, bountyPanel.transform,
				Vector3.zero, new Vector2 (0.1f, 1.0f / 5f * newID), new Vector2 (0.9f, 1.0f / 5f * (newID+1)), TextAnchor.MiddleCenter, true));*/
		}
		return newID;
	}

	public void ReportHit (int loser, int winner) {
		if (killStreak [loser] >= 5) {
			killStreak [winner] += 2;
		} else {
			killStreak [winner] += 1;
		}
		killStreak [loser] = 0;
		//playerBounties [loser] = 100;

		Player[] playerList = FindObjectsOfType<Player> ();
		for (int i = 0; i < playerList.Length; i++) {
			if (playerList [i].playerID == winner) {
				playerList[i].AddGold(playerBounties[loser]);
			}
		}
	}


	private void CreateBountyPanel() {
		bountyPanel = UI.CreatePanel("Bounty Panel", null, new Color(1.0f, 1.0f, 1.0f, 0.65f), canvas.transform,
			Vector3.zero, new Vector2(0.02f, 0.95f-0.1f*playerBounties.Count), new Vector3(0.18f, 0.95f));
	}

	private int GetHighestBounty() {
		int highestID = 0;
		for (int i = 1; i < playerBounties.Count; i++) {
			if (playerBounties [i] > playerBounties [highestID]) {
				highestID = i;
			}
		}
		return highestID;
	}


	private IEnumerator DeclareVictory(int playerID) {
		// delcare the winning player to be the pirate king
		print("Victory has been declared... in theory");
		GameObject lastText = (UI.CreateText ("Victory Text", "Player " + (playerID+1) + " is the Pirate King!", font, Color.black, 100, canvas.transform,
			Vector3.zero, new Vector2 (0.1f, 0.1f), new Vector2 (0.9f, 0.9f), TextAnchor.MiddleCenter, true));
		//lastText.GetComponent<Text> ().resizeTextForBestFit = true;

		Player[] playerList = FindObjectsOfType<Player> ();

		if (isServer) {
			for (int i = 0; i < playerList.Length; i++) {	
				playerList [i].dead = true;
			}
		}

		yield return new WaitForSeconds(5.0f);
		Navigator.Instance.LoadLevel("Menu");
	}
}
