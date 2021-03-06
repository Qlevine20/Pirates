﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using Prototype.NetworkLobby;

public enum DamageType {
    CANNON,
    RAM
}
public class Player : NetworkBehaviour {

    // camera shake stuff
    Vector3 originalPos;
    bool CameraShakeBool = false;
    public float Shake = 0.9f;

    private ParticleSystem explosion;

    // const vars
    public const float BASE_PROJECTILE_SPEED = 70.0f;
    public const float BASE_PROJECTILE_STRENGTH = 10.0f;
    public const float BASE_FIRING_DELAY = 1.0f;
    public const int MAX_UPGRADES = 3;
    public const int UPGRADE_COST = 100;
    public const float MAX_SHOTS = 4.0f;
    public static int[] UPGRADE_SCALE = { 1, 5, 10 };

	public static float[] SCALE_MAX_HEALTH = { 100f, 150f, 250f, 400f };
	public static float[] SCALE_RAM_DAMAGE = { 15f, 20f, 30f, 50f };
	public static float[] SCALE_MOVE_SPEED = { 25f, 35f, 45f, 60f };
	public static float[] SCALE_ROTATION_SPEED = { 90f, 100f, 110f, 150f };
	public static float[] SCALE_BOOST_LENGTH = { 1.0f, 1.25f, 1.5f, 1.75f };
	public static float[] SCALE_BOOST_DELAY = { 5.0f, 4.0f, 3.0f, 2.0f };
	public static float[] SPEED_LEVELS = { 20f, 40f, 100f, 140f, 200f };
	public static float[] SCALE_RAM_SPEED_MOD = { 0.0f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f };


    [SyncVar]
    public int ID = -2;
    [SyncVar]
    public int lowUpgrades = 0;
    [SyncVar]
    public int midUpgrades = 0;
    [SyncVar]
    public int highUpgrades = 0;

    [SyncVar(hook = "OnChangePlayer")]
	public float currentHealth = SCALE_MAX_HEALTH[0];
    [SyncVar(hook = "OnChangeResources")]
    public int resources;
    [SyncVar]
    public int kills = 0;
    [SyncVar]
    public int deaths = 0;
    [SyncVar]
    public int streak = 0;
    public Sprite[] bases;
    public Sprite[] sails;
    public Sprite[] rudders;
    public Sprite[] cannons;
    public Sprite[] rams;
    public GameObject boatBase;
    public GameObject sail;
    public GameObject rudder;
    public GameObject cannon;
    public GameObject ram;
    // keybinds and prefabs
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
    public KeyCode menu;
    public KeyCode upgrade;
    public Transform leftSpawn;
    public Transform rightSpawn;
    public Transform[] leftSpawners;
    public Transform[] rightSpawners;
    public Transform[] frontSpawners;
    public GameObject projectile;
    public GameObject resourceObj;
    public GameObject deathExplode;

    public GameObject leaderArrow;

    private Camera playerCamera;
    private Canvas canvas;
    private GameObject healthBar;
    private RectTransform healthBarRect;
    private GameObject purpleCannon;
    private RectTransform purpleCannonRect;
    private GameObject redCannon;
    private RectTransform redCannonRect;
    private float purpleCannonRectAnchorMin;
    private float purpleCannonRectWidth;
    private float redCannonRectAnchorMax;
    private float redCannonRectWidth;
    private float healthBarRectAnchorMax;
    private float healthBarRectWidth;
    private Font font;
    private Text resourcesText;
    private Text killsText;
    private Text deathsText;
    private Text bountyText;
    //private FogOfWar fogOfWar;
    private MapGenerator mapGenerator;
    // GameObject references
    private GameObject inGameUI;
    private Quaternion uiQuat;
    private RectTransform inGameHealthBar;
    private RectTransform inGameHealthBarRed;
    private RectTransform inGameHealthBarBlack;
    private UpgradePanel upgradePanel;
    private GameObject respawnTimerText;
    private Image sprintCooldownImage;
    private Rigidbody2D rb;
    private Collider2D pColl;
    // upgrade menu ranks
    public SyncListInt upgradeRanks = new SyncListInt();
    // base stats
    public float currMoveSpeed = SCALE_MOVE_SPEED[0];
	public float currRotationSpeed = SCALE_ROTATION_SPEED[0];
    public float currFiringDelay = BASE_FIRING_DELAY;
    public float currBoostDelay = SCALE_BOOST_DELAY[0];
    //public float currProjectileSpeed = BASE_PROJECTILE_SPEED;
	public float currRamDamage = SCALE_RAM_DAMAGE[0];
    public float currProjectileStrength = BASE_PROJECTILE_STRENGTH;
    public float firingTimerLeft = BASE_FIRING_DELAY;
    public float firingTimerRight = BASE_FIRING_DELAY;
    public float boostTimer = SCALE_BOOST_DELAY[0];
	public float currBoostLength = SCALE_BOOST_LENGTH[0];
	public float currMaxHealth = SCALE_MAX_HEALTH [0];
    public float currVelocity = 0.0f;
    public float numPurpleShots = MAX_SHOTS;
	public float numRedShots = MAX_SHOTS;
	private float savedMoveSpeed = 0f;
	private float savedRotationSpeed = 0f;
    [SyncVar]
    public float appliedRamDamage = 0.0f;
    // menu checks
    private bool inGameMenuActive = false;
    // ayy it's dem seagulls
    public AudioClip seagullS;
    private float seagullTimer = 10;
    // other sounds
    public AudioClip shotS1;
	public AudioClip shotS2;
	public AudioClip shotS3;
	private AudioClip currentShotS;
    public AudioClip turnS1;
	public AudioClip turnS2;
	public AudioClip turnS3;
	private AudioClip currentTurnS;
    private float creakTimer = 0;
    public AudioClip ramS;
    public AudioClip deathS;
	public AudioClip boostS1;
	public AudioClip boostS2;
	public AudioClip boostS3;
    private AudioClip currentBoostS;
    public AudioClip coinS0;
	public AudioClip coinS1;
	public AudioClip coinS2;
	private AudioClip currentCoinS;
	private int coinIndex;
	private int[] MHALL;
    private float oldHealth;

    //other UI sounds
    public AudioClip sfx_upgradeMenuOpen;
    public AudioClip sfx_upgradeMenuClose;

	// PLAYER INFO //
	[SyncVar]
	public string playerName;
	[SyncVar]
	public Color playerColor;

    

    [SyncVar]
    public bool dead;
    public bool gofast = false;
	public float boost = SCALE_BOOST_LENGTH[0];
    [SyncVar]
    public int pSpawned = 0;

	[SyncVar]
	public float score = 0f;

    //ramming cooldown
    private IEnumerator coroutine;
    private bool invuln = false;
    public bool inHill = false;
    //private bool registered = false;
    private RectTransform minimapRect;
    private RectTransform playerRect;

    // Use this for initialization
    void Start() {
        Debug.Log("Spawn");
		MHALL = new int[26]{2,1,0,1,2,2,2,1,1,1,2,2,2,2,1,0,1,2,2,2,2,1,1,2,1,0};
		coinIndex = 0;
        //StartCoroutine(BoatRepairs());

		for (int i = 0; i < (int)Upgrade.COUNT; ++i) {
            upgradeRanks.Add(0);
        }
        dead = false;
		score = 0f;

        explosion = gameObject.GetComponentInChildren<ParticleSystem>();
        playerCamera = GameObject.Find("Camera").GetComponent<Camera>();
        canvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        rb = GetComponent<Rigidbody2D>();
        font = Resources.Load<Font>("Art/Fonts/SHOWG");
        inGameUI = transform.Find("Canvas").gameObject;
        inGameHealthBar = inGameUI.transform.GetChild(2).GetComponent<RectTransform>();
        inGameHealthBarRed = inGameUI.transform.GetChild(1).GetComponent<RectTransform>();
        inGameHealthBarBlack = inGameUI.transform.GetChild(0).GetComponent<RectTransform>();
        Text t = inGameUI.transform.GetChild(3).GetComponent<Text>();
        t.text = playerName;
        t.color = playerColor;
        uiQuat = Quaternion.identity;
        sfx_upgradeMenuOpen = Resources.Load<AudioClip>("Sound/SFX/UI/Paper");
        sfx_upgradeMenuClose = Resources.Load<AudioClip>("Sound/SFX/UI/PaperReverse");
        oldHealth = currentHealth;

        if (!isLocalPlayer) {
            return;
        }
        pColl = GetComponent<Collider2D>();
        leaderArrow = GameObject.Find("MainCanvas/UI/HUD/Compass");

        upgradePanel = FindObjectOfType<UpgradePanel>();
        upgradePanel.player = this;
        upgradePanel.UpdateUI();
        upgradePanel.Hide();
        healthBar = GameObject.Find("MainCanvas/UI/HUD/Health Bar");
        healthBarRect = healthBar.GetComponent<RectTransform>();
        healthBar.GetComponent<Canvas>().sortingOrder = 10;
        purpleCannon = GameObject.Find("MainCanvas/UI/HUD/Purple Cannon");
        purpleCannonRect = purpleCannon.GetComponent<RectTransform>();
        redCannon = GameObject.Find("MainCanvas/UI/HUD/Red Cannon");
        redCannonRect = redCannon.GetComponent<RectTransform>();
        redCannonRectAnchorMax = redCannonRect.anchorMax.x;
        redCannonRectWidth = redCannonRect.anchorMax.x - redCannonRect.anchorMin.y;
        purpleCannonRectAnchorMin = purpleCannonRect.anchorMin.x;
        purpleCannonRectWidth = purpleCannonRect.anchorMax.x - purpleCannonRect.anchorMin.y;
        sprintCooldownImage = GameObject.Find("MainCanvas/UI/HUD/Sprint Cooldown").GetComponent<Image>();
        resourcesText = GameObject.Find("MainCanvas/UI/HUD/Button/BountyDisplayText").GetComponent<Text>();
        killsText = GameObject.Find("MainCanvas/UI/HUD/KDR/Kills Text").GetComponent<Text>();
        deathsText = GameObject.Find("MainCanvas/UI/HUD/KDR/Deaths Text").GetComponent<Text>();
        bountyText = GameObject.Find("MainCanvas/UI/HUD/KDR/Bounty Text").GetComponent<Text>();
        minimapRect = GameObject.Find("MainCanvas/UI/Minimap").GetComponent<RectTransform>();
        playerRect = GameObject.Find("MainCanvas/UI/Minimap/Player").GetComponent<RectTransform>();
        healthBarRectAnchorMax = healthBarRect.anchorMax.x;
        healthBarRectWidth = healthBarRect.anchorMax.x - healthBarRect.anchorMin.x;
        respawnTimerText = UI.CreateText("Respawn Timer Text", "10", font, Color.black, 200, canvas.transform,
            Vector3.zero, new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.7f), TextAnchor.MiddleCenter, true);
        respawnTimerText.SetActive(false);
        Text timerText = respawnTimerText.GetComponent<Text>();
        timerText.resizeTextMaxSize = timerText.fontSize;

        mapGenerator = FindObjectOfType<MapGenerator>();
        /*fogOfWar = FindObjectOfType<FogOfWar>();
        fogOfWar.player = this;
        fogOfWar.transform.localScale = new Vector3(mapGenerator.width, mapGenerator.height, 1);*/

        lowUpgrades = 0; midUpgrades = 0; highUpgrades = 0;
		if (isServer && BountyManager.Instance && ID < 0) {
            ID = BountyManager.Instance.RegisterPlayer(this);
            //registered = true;
        }

        SoundManager.Instance.SwitchBGM((int)TrackID.BGM_FIELD, 1.0f);
        InvokeRepeating("EnemyDetection", 1f, 0.5f);
    }
    void UpdateMinimapPlayer() {
        if (playerRect)
        {
			playerRect.anchoredPosition = new Vector3(minimapRect.rect.width * transform.position.x, minimapRect.rect.height * transform.position.y, 1) / (mapGenerator.width * mapGenerator.tileSize);
        }
        

    }
    void Update() {

        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    RpcRespawn();
        //}
        if(oldHealth > currentHealth)
        {
			oldHealth = Mathf.Max (oldHealth - 20 * Time.deltaTime, currentHealth);
			inGameHealthBarRed.anchorMax = new Vector2(0.8f - 0.6f * (SCALE_MAX_HEALTH[0] - oldHealth) / currMaxHealth, inGameHealthBar.anchorMax.y);
        }
        else if(currentHealth > oldHealth)
        {
            oldHealth = currentHealth;
        }


		if (isServer && BountyManager.Instance && ID < 0) {
            ID = BountyManager.Instance.RegisterPlayer(this);
            //registered = true;
		}

        UpdateSprites();
        // networking check
        if (!isLocalPlayer || dead) {
            return;
        }
        if (dead && pColl.enabled)
        {
            pColl.enabled = false;
        }

        if(!dead && !pColl.enabled)
        {
            pColl.enabled = true;
        }

        // update the camera's position
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, playerCamera.transform.position.z);

        if (CameraShakeBool)
        {
            Shake = currVelocity * 0.02f;
            playerCamera.transform.localPosition = originalPos + Random.insideUnitSphere * 0.7f;

            Shake -= Time.deltaTime * 1.0f;

        }


        //fogOfWar.position = new Vector3(-transform.position.x / mapGenerator.width, -transform.position.y / mapGenerator.height, 0);
        HandleBoost();
        // get player's movement
        GetCannonFire();
        UpdateInterface();
        UpdateVariables();
        //CmdDisplayHealth ();

        DrawLineToLeader();

        //SOUND - SoundManager reposition
        if (GameObject.Find("SoundManager") != null) {
            GameObject.Find("SoundManager").transform.position = GameObject.Find("Camera").transform.position;
        }

    }

    void DrawLineToLeader() {
        //For all players who are not in the lead, draws a local onscreen arrow pointing to the leader.
        //Encourages constant fighting due to all non-leader players having a location they can go to
        //To find a player

        //TODO: hide at beginning, hide until certain bounty is earned, hide when leader near
        //TODO: make this a compass on the corner of the screen instead of under the boat
        if (!isLocalPlayer || dead || currentHealth <= 0) {
            leaderArrow.SetActive(false);
            return;
        }
        if (BountyManager.Instance) {
			GameObject leader = BountyManager.Instance.GetLeader();
			if (leader == null || leader == gameObject) {
                leaderArrow.SetActive(false);
                return;
            }

            leaderArrow.SetActive(true);

            leaderArrow.transform.up = (leader.transform.position - transform.position).normalized;
        }
    }

    void HandleBoost() {
		int boostNum = Random.Range (1, 4);
		switch(boostNum) {
		case 1:
			currentBoostS = boostS1;
			break;
		case 2:
			currentBoostS = boostS2;
			break;
		case 3:
			currentBoostS = boostS3;
			break;
		}
        if (boostTimer > 0) {
            boostTimer -= Time.deltaTime;
        } else {
			sprintCooldownImage.color = Color.green;
			if (Input.GetKeyDown(KeyCode.LeftShift)) {
                SpeedBoost();
                SoundManager.Instance.PlaySFX(currentBoostS, 1.0f);
                gofast = true;
                boostTimer = currBoostDelay;
				sprintCooldownImage.color = Color.red;
            }
        }

        if (gofast == true && boost > 0) {
            boost -= Time.deltaTime;
            //currMoveSpeed -= Time.deltaTime * currMoveSpeed;
            //currRotationSpeed += Time.deltaTime * currRotationSpeed;
			currMoveSpeed = savedMoveSpeed + savedMoveSpeed * 4f * Mathf.Max(0f, boost) / currBoostLength;
			currRotationSpeed = savedRotationSpeed / (1f + 4f * Mathf.Max(0f, boost) / currBoostLength);
        } else if (boost < 0) {
            gofast = false;
			boost = currBoostLength;
        }
    }
	void GetCannonFire() {
		int shotNum = Random.Range (1, 4);
		switch(shotNum) {
		case 1:
			currentShotS = shotS1;
			break;
		case 2:
			currentShotS = shotS2;
			break;
		case 3:
			currentShotS = shotS3;
			break;
		}
        if (firingTimerLeft > 0) {
            firingTimerLeft -= Time.deltaTime;
        } else {
            // fire cannons
			if (((Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject ()) || Input.GetKeyDown(KeyCode.LeftArrow)) && !upgradePanel.gameObject.activeSelf && numPurpleShots >= 1) {
				// left cannon
				SoundManager.Instance.PlaySFX(currentShotS, 1.0f);
                CmdFireLeft((int)currProjectileStrength);
				numPurpleShots = Mathf.Floor(numPurpleShots-1);
                // reset timer
				if (numPurpleShots == 0) {
					firingTimerLeft = currFiringDelay * 13;
				} else {
					firingTimerLeft = currFiringDelay;
				}
            }
        }

        if (firingTimerRight > 0) {
            firingTimerRight -= Time.deltaTime;
        } else {
			if (((Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject ()) || Input.GetKeyDown(KeyCode.RightArrow)) && !upgradePanel.gameObject.activeSelf && numRedShots >= 1) {
                // right cannon
                SoundManager.Instance.PlaySFX(currentShotS, 1.0f);
                CmdFireRight((int)currProjectileStrength);
				numRedShots = Mathf.Floor(numRedShots-1);
				// reset timer
				if (numRedShots == 0) {
					firingTimerRight = currFiringDelay * 13;
				} else {
					firingTimerRight = currFiringDelay;
				}
            }
        }
        if (firingTimerLeft > 0 && firingTimerRight > 0) {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !upgradePanel.gameObject.activeSelf) {
                // triple volley - fire all at once
                SoundManager.Instance.PlaySFX(currentShotS, 1.0f);
                CmdFireLeftVolley((int)currProjectileStrength);
                // reset timer
                firingTimerLeft = currFiringDelay; //+2.0f;
                firingTimerRight = currFiringDelay; //+2.0f;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && !upgradePanel.gameObject.activeSelf) {
                // triple shotgun spray
                SoundManager.Instance.PlaySFX(currentShotS, 1.0f);
                CmdFireLeftTriple((int)currProjectileStrength);
                // reset timer
                firingTimerLeft = currFiringDelay; //+2.0f;
                firingTimerRight = currFiringDelay; //+2.0f;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && !upgradePanel.gameObject.activeSelf) {
                // front shot
                SoundManager.Instance.PlaySFX(currentShotS, 1.0f);
                CmdFireBowChaser((int)currProjectileStrength);
                // reset timer
                firingTimerLeft = currFiringDelay; //+2.0f;
                firingTimerRight = currFiringDelay; //+2.0f;
            }
        }
    }

    void FixedUpdate() {
        UpdateSeagulls();

        creakTimer -= Time.deltaTime;
        if (!isLocalPlayer || dead)
        {
            return;
        }
        GetMovement();
    }

    void SpeedBoost() {
		savedMoveSpeed = currMoveSpeed;
		savedRotationSpeed = currRotationSpeed;
		currMoveSpeed *= 5;
        currRotationSpeed /= 5;
    }

    [Command]
    void CmdFireLeft(int damageStrength) {
        //print (damageStrength);
        for (int i = 0; i < damageStrength / 10; i++) {
            GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, leftSpawners[i].position, Quaternion.identity);
            instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = leftSpawners[i].up * BASE_PROJECTILE_SPEED;
			Projectile newProjectile = instantiatedProjectile.GetComponent<Projectile> ();
			newProjectile.assignedID = ID;
			newProjectile.playerVel = transform.up * currVelocity;
            NetworkServer.Spawn(instantiatedProjectile);
        }
    }
    [Command]
    void CmdFireRight(int damageStrength) {
        for (int i = 0; i < damageStrength / 10; i++) {
            GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, rightSpawners[i].position, Quaternion.identity);
            instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = rightSpawners[i].up * BASE_PROJECTILE_SPEED;
			Projectile newProjectile = instantiatedProjectile.GetComponent<Projectile> ();
			newProjectile.assignedID = ID;
			newProjectile.playerVel = transform.up * currVelocity;
            NetworkServer.Spawn(instantiatedProjectile);
        }
    }
    [Command]
    void CmdFireLeftVolley(int damageStrength) {
        //triple volley shot
        for (int i = 0; i < 3; i++) {
            GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, leftSpawners[i].position, Quaternion.identity);
            instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = leftSpawners[i].up * BASE_PROJECTILE_SPEED / 2;
			Projectile newProjectile = instantiatedProjectile.GetComponent<Projectile> ();
			newProjectile.assignedID = ID;
			newProjectile.playerVel = transform.up * currVelocity;
            NetworkServer.Spawn(instantiatedProjectile);
        }
    }
    [Command]
    void CmdFireLeftTriple(int damageStrength) {
        //triple spread shot

        GameObject instantiatedProjectile1 = (GameObject)Instantiate(projectile, leftSpawners[0].position, Quaternion.identity);
        instantiatedProjectile1.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, 45) * leftSpawners[0].up * BASE_PROJECTILE_SPEED;
		Projectile newProjectile1 = instantiatedProjectile1.GetComponent<Projectile> ();
		newProjectile1.assignedID = ID;
		newProjectile1.playerVel = transform.up * currVelocity;
        NetworkServer.Spawn(instantiatedProjectile1);

        //angled 45 degrees backward
        GameObject instantiatedProjectile2 = (GameObject)Instantiate(projectile, leftSpawners[1].position, Quaternion.identity);
        instantiatedProjectile2.GetComponent<Rigidbody2D>().velocity = leftSpawners[1].up * BASE_PROJECTILE_SPEED;
		Projectile newProjectile2 = instantiatedProjectile2.GetComponent<Projectile> ();
		newProjectile2.assignedID = ID;
		newProjectile2.playerVel = transform.up * currVelocity;
        NetworkServer.Spawn(instantiatedProjectile2);

        GameObject instantiatedProjectile3 = (GameObject)Instantiate(projectile, leftSpawners[2].position, Quaternion.identity);
        instantiatedProjectile3.GetComponent<Rigidbody2D>().velocity = Quaternion.Euler(0, 0, -45) * leftSpawners[2].up * BASE_PROJECTILE_SPEED;
		Projectile newProjectile3 = instantiatedProjectile3.GetComponent<Projectile> ();
		newProjectile3.assignedID = ID;
		newProjectile3.playerVel = transform.up * currVelocity;
        NetworkServer.Spawn(instantiatedProjectile3);
    }
    [Command]
    void CmdFireBowChaser(int damageStrength) {
        //forward firing cannon
        for (int i = 0; i < damageStrength / 10; i++) {
            GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, frontSpawners[0].position, Quaternion.identity);
            instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = frontSpawners[0].up * BASE_PROJECTILE_SPEED;
			Projectile newProjectile = instantiatedProjectile.GetComponent<Projectile> ();
			newProjectile.assignedID = ID;
			newProjectile.playerVel = transform.up * currVelocity;
            NetworkServer.Spawn(instantiatedProjectile);
        }
    }
    [Command]
    void CmdSpawnResources(Vector3 pos) {
        GameObject instantiatedResource = Instantiate(resourceObj, pos, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(instantiatedResource);
    }
    [Command]
    void CmdUpgrade(Upgrade upgrade, bool positive) {
        if (positive && (int)upgrade < upgradeRanks.Count) {
            int upgradePrice = UPGRADE_COST * UPGRADE_SCALE[upgradeRanks[(int)upgrade]];

            if ((upgradeRanks[(int)upgrade] < MAX_UPGRADES) && (resources >= upgradePrice)) {
                upgradeRanks[(int)upgrade]++;
                resources -= upgradePrice;
                switch (upgradeRanks[(int)upgrade]) {
                    case 1:
                        lowUpgrades++;
                        break;
                    case 2:
                        midUpgrades++;
                        break;
                    case 3:
                        highUpgrades++;
                        break;
                    default:
                        print("Erm, unexpected upgrade rank");
                        break;
                }
            }
        }
    }
    [Command]
    void CmdChangeHealth(float setHealth, bool flatSet) {
        if (flatSet) {
            currentHealth = setHealth;
        } else {
            currentHealth += setHealth;

        }
    }
    [Command]
	void CmdDeath(bool isDead) {
		dead = isDead;
		if (isDead) {
			RpcSetSpriteActive (false);
			GameObject instantiatedResource = Instantiate(deathExplode, transform.position, Quaternion.identity) as GameObject;
			NetworkServer.Spawn(instantiatedResource);
		} else {
			RpcSetSpriteActive (true);
			SoundManager.Instance.PlaySFX_Respawn();
			RpcFinishRespawn();
		}
	}
    [Command]
    void CmdApplyDamage(float damage, int enemyID) {
        if (dead || currentHealth <= 0) {
            return;
        }
        print("DAMAGE! " + damage);
        currentHealth -= damage;
        // respawn the player if they are dead
        if (currentHealth <= 0.0f) {
            SoundManager.Instance.PlaySFX(deathS, 1.0f);
            RpcRespawn();
            BountyManager.Instance.CmdReportKill(ID, enemyID);
            deaths++;
            if (streak < 0) {
                streak--;
            } else {
                streak = -1;
            }
        }
    }
    [Command]
    void CmdSetRamDamage(float ramDam) {
        appliedRamDamage = ramDam;
    }
	[ClientRpc]
	void RpcSetSpriteActive(bool setActive)
	{
		GetComponent<Collider2D>().enabled = setActive;
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(setActive);
		}
	}
    [ClientRpc]
    void RpcRespawn() {
        gameObject.transform.FindChild("Sprite").gameObject.SetActive(false);
        if (!isLocalPlayer) {
            return;
        }
        CmdDeath(true);
        StartCoroutine(Death());
    }
    [ClientRpc]
    void RpcFinishRespawn() {
        currentHealth = currMaxHealth;
        oldHealth = currMaxHealth;
        gameObject.transform.FindChild("Sprite").gameObject.SetActive(true);
    }
    void UpdateSprites() {
        boatBase.GetComponent<SpriteRenderer>().sprite = bases[upgradeRanks[(int)Upgrade.HULL]];
        sail.GetComponent<SpriteRenderer>().sprite = sails[upgradeRanks[(int)Upgrade.SPEED]];
        rudder.GetComponent<SpriteRenderer>().sprite = rudders[upgradeRanks[(int)Upgrade.AGILITY]];
        cannon.GetComponent<SpriteRenderer>().sprite = cannons[upgradeRanks[(int)Upgrade.CANNON]];
        ram.GetComponent<SpriteRenderer>().sprite = rams[upgradeRanks[(int)Upgrade.RAM]];
        inGameUI.transform.position = new Vector3(transform.position.x, -10 + transform.position.y, transform.position.z);
        inGameUI.transform.rotation = uiQuat;

    }
    private void UpdateInterface() {
        if (Input.GetKeyDown(menu)) {
            if (upgradePanel.gameObject.activeSelf) {
                upgradePanel.gameObject.SetActive(false);
            }
        }
        if (Input.GetKeyDown(upgrade) && !inGameMenuActive) {
            upgradePanel.gameObject.SetActive(!upgradePanel.gameObject.activeSelf);

            if (upgradePanel.gameObject.activeSelf)
                SoundManager.Instance.PlaySFX(sfx_upgradeMenuOpen, 0.15f);
            else
                SoundManager.Instance.PlaySFX(sfx_upgradeMenuClose, 0.15f);
        }
        purpleCannonRect.anchorMin = new Vector2(purpleCannonRectAnchorMin + purpleCannonRectWidth * (MAX_SHOTS - numPurpleShots) / MAX_SHOTS, purpleCannonRect.anchorMin.y);
        redCannonRect.anchorMax = new Vector2(redCannonRectAnchorMax - redCannonRectWidth * (MAX_SHOTS - numRedShots) / MAX_SHOTS, redCannonRect.anchorMax.y);
        sprintCooldownImage.fillAmount = 1f - boostTimer / currBoostDelay;
        killsText.text = "" + kills;
        deathsText.text = "" + deaths;
		bountyText.text = "" + (int)BountyManager.CalculateWorth(this);
    }
	void OnChangePlayer(float newHealth) {
        currentHealth = newHealth;
		inGameHealthBar.anchorMax = new Vector2(0.8f - 0.6f * (SCALE_MAX_HEALTH[0] - currentHealth) / SCALE_MAX_HEALTH[0], inGameHealthBar.anchorMax.y);
		inGameHealthBarBlack.anchorMax = new Vector2(0.8f - 0.6f * (SCALE_MAX_HEALTH[0] - currMaxHealth) / SCALE_MAX_HEALTH[0], inGameHealthBar.anchorMax.y);
        if (!isLocalPlayer) {
            return;
        }
        healthBarRect.anchorMax = new Vector2(healthBarRectAnchorMax - healthBarRectWidth * (currMaxHealth - currentHealth) / currMaxHealth, healthBarRect.anchorMax.y);
    }
    void OnChangeResources(int newResources) {
        if (!isLocalPlayer) {
            return;
        }
        resources = newResources;
        resourcesText.text = resources + "g";
        upgradePanel.UpdateUI();
    }

    private void GetMovement() {
		int turnNum = Random.Range (1, 4);
		switch(turnNum) {
		case 1:
			currentTurnS = turnS1;
			break;
		case 2:
			currentTurnS = turnS2;
			break;
		case 3:
			currentTurnS = turnS3;
			break;
		}
        if (Input.GetKey(left)) { // click left
            if (creakTimer <= 0 && Input.GetKeyDown(left)) {
                creakTimer = 4.0f;
                SoundManager.Instance.PlaySFX(currentTurnS, 0.7f);
            }
            rb.MoveRotation(rb.rotation + currRotationSpeed * Time.deltaTime);
			rb.angularVelocity = 0f;
        }

        if (Input.GetKey(right)) {
            if (creakTimer <= 0 && Input.GetKeyDown(right)) {
                creakTimer = 4.0f;
                SoundManager.Instance.PlaySFX(currentTurnS, 0.7f);
            }
            rb.MoveRotation(rb.rotation - currRotationSpeed * Time.deltaTime);
			rb.angularVelocity = 0f;
        }
        if (Input.GetKey(up)) {
            currVelocity = Mathf.Min(currMoveSpeed, currVelocity + currMoveSpeed * Time.deltaTime);
        } else if (Input.GetKey(down)) {
            currVelocity = Mathf.Max(-currMoveSpeed * (1 + (currRotationSpeed / SCALE_ROTATION_SPEED[0] / 4)) / 2f, currVelocity - currMoveSpeed * .85f * Time.deltaTime);
        } else {
            if (currVelocity > 0) {
                currVelocity = Mathf.Max(0f, currVelocity - currMoveSpeed / 2f * Time.deltaTime);
            } else if (currVelocity < 0) {
                currVelocity = Mathf.Min(0f, currVelocity + currMoveSpeed / 2f * Time.deltaTime);
            }
        }
        rb.MovePosition(transform.position + transform.up * Time.deltaTime * currVelocity);

		float ramDam = -1f;
		for (int i = 0; i < SPEED_LEVELS.Length; i++) {
			if (currVelocity < SPEED_LEVELS [i]) {
				ramDam = currRamDamage * SCALE_RAM_SPEED_MOD [i];
				break;
			}
		}
		if (ramDam == -1f) {
			ramDam = currRamDamage * SCALE_RAM_SPEED_MOD [SCALE_RAM_SPEED_MOD.Length - 1];
		}
		CmdSetRamDamage (ramDam);
        //CmdSetRamDamage(currRamDamage * currVelocity / SCALE_MOVE_SPEED[0]);
        UpdateMinimapPlayer();
    }

    IEnumerator Death() {
        if (inHill)
        {
            inHill = false;
        }
        yield return new WaitForSeconds(2f);

        Text timerText = respawnTimerText.GetComponent<Text>();
        respawnTimerText.SetActive(true);
        for (int i = 5; i > 0; i--) {
            timerText.text = i + "";
            yield return new WaitForSeconds(1f);
        }
        respawnTimerText.SetActive(false);

        GameObject[] sl = GameObject.FindGameObjectsWithTag("spawner");
        GameObject farthestSpawn = sl[0];
        float maxDistSum = 0;
        foreach (GameObject g in sl) {
            float distSum = 0;
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player")) {
                if (p != this.gameObject) {
                    distSum += (g.transform.position - p.transform.position).sqrMagnitude;
                }

            }
            if (distSum > maxDistSum) {
                maxDistSum = distSum;
                farthestSpawn = g;
            }
        }
        transform.position = farthestSpawn.transform.position;
        Vector3 dir = -transform.position;
        dir = dir.normalized;
        transform.up = dir;
        CmdChangeHealth(currMaxHealth, true);

        CmdDeath(false);
    }



    IEnumerator BoatRepairs() {
        while (true) {

            yield return new WaitForSeconds(2);
            if (currentHealth != currMaxHealth && !dead) {
                if (GetComponent<Rigidbody2D>().velocity.SqrMagnitude() >= 0 && GetComponent<Rigidbody2D>().velocity.SqrMagnitude() <= .5) {
                    CmdChangeHealth(5, false);
                }
            }
        }

    }

    public void ApplyDamage(float damage, int enemyID) {
        CmdApplyDamage(damage, enemyID);
    }

    public void CamShake()
    {
        originalPos = playerCamera.transform.localPosition;
        CameraShakeBool = true;
        StartCoroutine(ShakeTime());

    }

    IEnumerator ShakeTime()
    {
        print(Time.time);
        yield return new WaitForSeconds(0.4f);
        print(Time.time);
        CameraShakeBool = false;
    }


    public void OnCollisionEnter2D(Collision2D collision) {

        if (!isLocalPlayer) {
            return;
        }


        //if rammed
        Player otherPlayer = collision.collider.gameObject.GetComponent<Player>();
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up);
        RaycastHit2D hit = Physics2D.Raycast(collision.gameObject.transform.position, collision.gameObject.transform.up);

        if (otherPlayer != null && collision.collider.gameObject.CompareTag("Player") && hit.collider != null && hit.collider.tag == "Player" && !invuln) {
        
            CamShake();


            var emitParams = new ParticleSystem.EmitParams();
            explosion.Emit(emitParams, 90);



            ApplyDamage(otherPlayer.appliedRamDamage, otherPlayer.ID);
            SoundManager.Instance.PlaySFX(ramS, 1.0f);
            //3 second invulnerability before you can take ram damage again
            StartCoroutine(RamInvuln());
        }
    }
    private IEnumerator RamInvuln() {
        //make player invulnerable to ramming for X seconds (currently 3)
        invuln = true;
        yield return new WaitForSeconds(3);
        invuln = false;
    }

    public void AddGold(int gold) {
        if (!isServer) {
            return;
        }
        resources += gold;
        //upgradePanel.UpdateUI();
    }

    public void UpgradePlayer(Upgrade upgrade, bool positive) {
        if (isLocalPlayer) {
            CmdUpgrade(upgrade, positive);

        }
        UpdateVariables();
        //UpdateSprites ();
    }

    public override void OnStartLocalPlayer() {
    }

    private void UpdateVariables() {
        if (!gofast) // only update movement speed if not in boost mode
        {
			currMoveSpeed = SCALE_MOVE_SPEED [upgradeRanks[(int)Upgrade.SPEED]];
			currRotationSpeed = SCALE_ROTATION_SPEED [upgradeRanks [(int)Upgrade.AGILITY]];
        }
		currRamDamage = SCALE_RAM_DAMAGE [upgradeRanks[(int)Upgrade.RAM]];
		currBoostDelay = SCALE_BOOST_DELAY [upgradeRanks[(int)Upgrade.AGILITY]];
		currBoostLength = SCALE_BOOST_LENGTH [upgradeRanks[(int)Upgrade.SPEED]];

		currProjectileStrength = BASE_PROJECTILE_STRENGTH * (1 + (upgradeRanks[(int)Upgrade.CANNON] / 1.0f));

        float oldMaxHealth = currMaxHealth;
		currMaxHealth = SCALE_MAX_HEALTH [upgradeRanks[(int)Upgrade.HULL]];
        if (oldMaxHealth != currMaxHealth) {
            CmdChangeHealth(currMaxHealth - oldMaxHealth, false);
        }
        if (firingTimerLeft <= 0) {
			if (numPurpleShots == 0) {
				numPurpleShots = MAX_SHOTS;
			} else {
				numPurpleShots += Time.deltaTime * 1.5f;
				numPurpleShots = Mathf.Clamp (numPurpleShots, 0, MAX_SHOTS);
			}
        }
		if (firingTimerRight <= 0) {
			if (numRedShots == 0) {
				numRedShots = MAX_SHOTS;
			} else {
				numRedShots += Time.deltaTime * 1.5f;
				numRedShots = Mathf.Clamp (numRedShots, 0, MAX_SHOTS);
			}
		}
    }

    private void UpdateSeagulls() {
        seagullTimer -= Time.deltaTime;
        if (seagullTimer <= 0) {
            seagullTimer = 15 + Random.value * 15;
            SoundManager.Instance.PlaySFX(seagullS, 1.0f);
        }
    }

    //SOUND - battle bgm
    private void EnemyDetection() {
        LayerMask detectionLayer;
        Collider2D[] detectionResult = new Collider2D[10];

        detectionLayer = 1 << 9; // layerName: Player
        int detectionBattle = Physics2D.OverlapCircleNonAlloc(transform.position, 60.0f, detectionResult, detectionLayer);
        int detectionEscape = Physics2D.OverlapCircleNonAlloc(transform.position, 90.0f, detectionResult, detectionLayer);

        if (detectionBattle > 1 && SoundManager.Instance.NowPlaying() == (int)TrackID.BGM_FIELD)
            SoundManager.Instance.SwitchBGM((int)TrackID.BGM_BATTLE, 2.0f);
        if (detectionEscape <= 1)
            SoundManager.Instance.SwitchBGM((int)TrackID.BGM_FIELD, 2.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
	{
        if (isLocalPlayer && collision.CompareTag("Resource"))
		{
			if (coinIndex >= 26)
				coinIndex = 0;
			int coinNum = MHALL[coinIndex++];
			switch(coinNum) {
			case 0:
				currentCoinS = coinS0;
				break;
			case 1:
				currentCoinS = coinS1;
				break;
			case 2:
				currentCoinS = coinS2;
				break;
			}
            SoundManager.Instance.PlaySFX(currentCoinS, 0.15f);
            //SoundManager.Instance.PlaySFXTransition (coinS, 1.0f);
        }
        if (isLocalPlayer && collision.CompareTag("Hill"))
        {
            SoundManager.Instance.PlayCaptureSFX();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        SoundManager.Instance.StopCaptureSFX();
    }

	public void PlayPointSFX() {
		SoundManager.Instance.StopCaptureSFX ();
		SoundManager.Instance.PlayPointSFX ();
	}
	public void StopPointSFX() {
		SoundManager.Instance.StopPointSFX ();
	}
}
