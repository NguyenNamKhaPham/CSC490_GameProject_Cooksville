
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayer : Photon.MonoBehaviour {

	private PhotonVoiceRecorder voiceChat;
	public Text speakingIndicator;

	private GameObject middle;
	private Transform cam;
	public Text instructionText;

	public InventoryBehavior inventory;
    public GameObject cookBook;
    public GameObject playerCanvas;
    public GameObject instructionPanel;
    public GameObject instructionCanvas;
    public int PlayerScore = 0;

	public List<string> returnIds;
	public List<string> enterIds = new List<string>();

    private AICharacterControl MoveNPC;

    public bool onDumpIngredients = false;

    public Text pernamentInstruction;

    public InteractionState interactionState;

	public Text IngredientText;
    public Text message;

	public Text StationText;
	public Text ScoreSelf;
    public Text ScoreEnemy;
    public Image bg_top;
    public Image temp_bg;
    public Image ingr_bg;
    public Image pickup_bg;
    public Image crosshair;
    public Sprite[] allCrosshairs;

    private List<PhotonView> teamPlayers;
    private List<PhotonView> enemyPlayers;
    private GameObject[] players;
    private GameObject[] indicators;
    public string team;

    private int teamGotOrder;
    private double teamOrderScore;
    private DateTime teamOrderEndTime;
    private string teamOrderName;
    private int teamOrderPickedUp;
    private int teamTotalScore;
    public Text teamOrder;

    private int enemyGotOrder;
    private double enemyOrderScore;
    private DateTime enemyOrderEndTime;
    private string enemyOrderName;
    private int enemyTotalScore;
    public Text enemyOrder;

    private float GameCount;
    private string minutes;
    private string seconds;
    public Text countdownText;

	private GameObject hitObject;
    public GameObject won;
    public GameObject lost;
    public GameObject tied;

    // Use this for initialization
    void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		//Disable voice chat
		voiceChat = GetComponent<PhotonVoiceRecorder>();
		voiceChat.Transmit = false;
        // For capsule cast system
		cam = Camera.main.transform;
		middle = GameObject.FindGameObjectWithTag("middle");

		instructionPanel.SetActive (false);


        indicators = GameObject.FindGameObjectsWithTag("Indicators");
		// IF this is the object belong to the controlled clident, enable for the client to control
		if (photonView.isMine)
		{
			GetComponent<BasicBehaviour>().enabled = true;
			GetComponent<MoveBehaviour>().enabled = true;
			GetComponent<AudioListener>().enabled = true;
            Invoke("getAllPlayers", 1);
            cookbookShowHide(true);
        } else //IF not, disable
		{
			GetComponent<BasicBehaviour>().enabled = false;
			GetComponent<MoveBehaviour>().enabled = false;
			GetComponent<AudioListener>().enabled = false;
            //Disbale all children objects not belong to mine
            foreach (Transform child in transform)
            {
                if (child.tag == "notLocal")
                {
                    child.gameObject.SetActive(false);
                }
            }
		}

		hitObject = this.gameObject;
        GameCount = 600;
        countdownText.color = Color.yellow;

		ScoreSelf.text = "0";
		ScoreEnemy.text = "0";
        cursorAppear();
        pickup_bg.enabled = false;
    }

	void Update()
	{
		if (photonView.isMine)
		{
            showInstruction();
            showOrder();
            pushToTalk();
            showcookbook();
            //showInstructionPanel();
            pointIndicatorsToPlayer();
            gameCountDown();
            if (GameCount <= 60)
            {
                callTimesUp();
            }
        }
	}
    // Show instructions when close to an object
    // Dectect key pressed and handle the interations
    void showInstruction()
    {
        // Cast a ray
        RaycastHit[] hits;
        Vector3 offset = middle.transform.position - cam.position;
        // IF hit something
        hits = Physics.CapsuleCastAll(cam.position, middle.transform.position, 0.1f, offset, 4);
        //hits = Physics.RaycastAll(cam.position, offset, 6f);

		if (hits.Length > 0) {
			foreach (RaycastHit hit in hits) {
				GameObject hitO = hit.collider.gameObject;
				// IF it is a NPC and have not collect the recipe, collect
				switch (hitO.tag) {
				// Meet NPC
				case "NPC":
					hitObject = hitO;
					break;
				// IF hit a station on the kitchen
				case "kitchenStation":
					hitObject = hitO;
					break;
				// IF it is an ingrident
				case "ingredient":
					hitObject = hitO;
					break;
				case "Waste":
					hitObject = hitO;
					break;
                case "Storage":
                    hitObject = hitO;
                    break;
                }
			}
			switch (hitObject.tag) 
			{
				// Meet NPC
			case "NPC":
                crosshair.sprite = allCrosshairs[1];
                hitObject.GetComponent<AIControl> ().stop (transform.position);
				GiveOrder npc = hitObject.GetComponent<GiveOrder> ();
				npc.showMessage (instructionText, teamGotOrder, team);
                temp_bg.enabled = true;
				if (Input.GetKeyDown ("e")) {
					if (teamGotOrder == 1) {
						bool got = npc.gotInteracted (inventory, team);
						if (got) {
							foreach (PhotonView p in teamPlayers) {
								p.RPC ("teamDelivered", PhotonTargets.AllBuffered, teamOrderScore);
							}
                            foreach (PhotonView p in enemyPlayers) {
                                p.RPC("enemyDelivered", PhotonTargets.AllBuffered, teamOrderScore);
							}
						}
                        else
                            {
                                showMessage("No orders in inventory is valid to deliver");
                            }
					} else if (teamGotOrder == 0) {
						string order = npc.gotInteracted (team, teamOrderPickedUp);
						if (order != "") {
							foreach (PhotonView p in teamPlayers) {
								p.RPC ("addTeamRecipe", PhotonTargets.AllBuffered, order);
							}
                            foreach (PhotonView p in enemyPlayers) {
								p.RPC ("addEnemyRecipe", PhotonTargets.AllBuffered, order);
							}

						}
					}
				}
				break;
				// IF it is an ingrident
				case "ingredient":
                    crosshair.sprite = allCrosshairs[1];
                    ingr_bg.enabled = true;
					IngredientText.text = hitObject.GetComponent<Item> ().id;
                    if (Input.GetKeyDown("e"))
                        hitObject.GetComponent<respawn>().gotInteracted(inventory, message, pickup_bg);
                    break;

				// IF hit a station on the kitchen
				case "kitchenStation":
                    crosshair.sprite = allCrosshairs[1];
                    kitchenStation k = hitObject.GetComponent<kitchenStation> ();
					k.showMessgae (StationText);
                    temp_bg.enabled = true;
                    if (Input.GetKeyDown ("e")) {
						k.setInteractacState (interactionState);
                        k.setup(message, pickup_bg, teamPlayers, enemyPlayers);
						k.gotInteracted (team);
					}
					break;
			    case "Waste":
                    crosshair.sprite = allCrosshairs[1];
                    instructionText.text = "Waste Bin";
                    temp_bg.enabled = true;
                    TrashCanBehavior trash = hitObject.GetComponent<TrashCanBehavior> ();
					if (Input.GetKeyDown ("e")) {
						trash.setInteractacState (interactionState);
						trash.gotInteracted ();
					}
					break;
                case "Storage":
                    crosshair.sprite = allCrosshairs[1];
                    instructionText.text = "Storage";
                    temp_bg.enabled = true;
                    StorageBehaviour storage = hitObject.GetComponent<StorageBehaviour>();
                    if (Input.GetKeyDown("e"))
                    {
                        storage.setInteractacState(interactionState);
                        storage.gotInteracted();
                    }
                    break;
                case "Player":
                    crosshair.sprite = allCrosshairs[0];
                    temp_bg.enabled = false;
                    ingr_bg.enabled = false;
                    instructionText.text = "";
					IngredientText.text = "";
					StationText.text = "";
					break;
				default:
                    crosshair.sprite = allCrosshairs[0];
                    temp_bg.enabled = false;
                    ingr_bg.enabled = false;
                    instructionText.text = "";
					IngredientText.text = "";
					StationText.text = "";
					break;
			}
		} 
		hitObject = this.gameObject;
    }

    [PunRPC]
    void showMessage(string m)
    {
        message.text = m;
        pickup_bg.enabled = true;
        Invoke("clearMessage", 4);
    }

    void clearMessage()
    {
        message.text = "";
        pickup_bg.enabled = false;
    }

    void gameCountDown()
    {
        
        GameCount = GameCount - Time.deltaTime;
        string m = Mathf.Floor(GameCount / 60).ToString("00");
        string s = (GameCount % 60).ToString("00");
        if (s != seconds)
        {
            minutes = m;
            seconds = s;
            string time = string.Format("{0} : {1}", m,s);
            countdownText.text = time;
        }

        if (GameCount <= 0)
        {
            if (teamTotalScore == enemyTotalScore)
            {
                endGame("endGameTied", "endGameTied");
            }
            else if (teamTotalScore < enemyTotalScore)
            {
                endGame("endGameLost", "endGaneWon");
            }
            else
            {
                endGame("endGaneWon", "endGameLost");
            }
        }
    }
    
    void callTimesUp()
    {
        StartCoroutine(timesUp());
    }

    IEnumerator timesUp()
    {
        while (true)
        {
            countdownText.color = Color.yellow;
            yield return new WaitForSeconds(0.5f);
            countdownText.color = Color.red;
            yield return new WaitForSeconds(0.4f);
        }
        
    }

    void endGame(string teamResult, string enemyResult)
    {
        foreach (PhotonView p in teamPlayers)
        {
            p.RPC(teamResult, PhotonTargets.AllBuffered);
        }
        foreach (PhotonView p in enemyPlayers)
        {
            p.RPC(enemyResult, PhotonTargets.AllBuffered);
        }
    }

	void cursorAppear(){
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

    [PunRPC]
    void endGameLost()
    {
        if (photonView.isMine)
        {
			cursorAppear ();
            lost.SetActive(true);
            //Time.timeScale = 0;
        }
    }

    [PunRPC]
    void endGaneWon()
    {
        if (photonView.isMine)
        {
			
			cursorAppear ();
            won.SetActive(true);
            //Time.timeScale = 0;
        }
            
    }

    [PunRPC]
    void endGameTied()
    {
        if (photonView.isMine)
        {
			cursorAppear ();
            tied.SetActive(true);
            //Time.timeScale = 0;
        }
            
    }

    [PunRPC]
    void updateScore()
    {
        ScoreSelf.text = string.Format("{0}", teamTotalScore);
        ScoreEnemy.text = string.Format("{0}", enemyTotalScore);
    }

    void getAllPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        teamPlayers = new List<PhotonView>();
        enemyPlayers = new List<PhotonView>();
        foreach (GameObject p in players)
        {
            if (p.name == "PlayerRed(Clone)")
            {
                if (team == "red")
                {
                    teamPlayers.Add(p.GetComponent<PhotonView>());
                }
                else
                {
                    enemyPlayers.Add(p.GetComponent<PhotonView>());
                }
            }
            else
            {
                if (team == "red")
                {
                    enemyPlayers.Add(p.GetComponent<PhotonView>());
                }
                else
                {
                    teamPlayers.Add(p.GetComponent<PhotonView>());
                }
            }
        }

    }

    void showOrder()
    {
        if (teamGotOrder == 1)
        {
            if (teamOrderScore > 0)
            {
                teamOrderScore = Convert.ToInt32((teamOrderEndTime - DateTime.Now).TotalSeconds);
            }
            else
            {
                teamOrderScore = 0;
            }
            teamOrder.text = string.Format("{0}\n $50 + ${1}", teamOrderName, teamOrderScore);
        }
        else if (teamGotOrder == 0)
        {
            teamOrder.text = "No Order";
        }

        if (enemyGotOrder == 1)
        {
            if (enemyOrderScore > 0)
            {
                enemyOrderScore = Convert.ToInt32((enemyOrderEndTime - DateTime.Now).TotalSeconds);
            }
            else
            {
                enemyOrderScore = 0;
            }
            enemyOrder.text = string.Format("{0}\n $50 + ${1}", enemyOrderName, enemyOrderScore);
        }
        else if (enemyGotOrder == 0)
        {
            enemyOrder.text = "No Order";
        }
    }

    [PunRPC]
    void addTeamRecipe(string r)
    {
        string[] s = r.Split('_');
        if (s.Length == 2)
        {
            teamOrderName = s[0];
            teamOrderScore = Convert.ToInt32(s[1]);
            cookBook = transform.FindChild("Cook Book").FindChild(teamOrderName).gameObject;
            teamOrderEndTime = DateTime.Now.AddSeconds(teamOrderScore);
            teamOrderPickedUp++;
            teamGotOrder = 1;
            string m = string.Format("Got order {0}\n $50 + ${1}", teamOrderName, teamOrderScore);
            showMessage(m);
        }
    }

    [PunRPC]
    void addEnemyRecipe(string r)
    {
        string[] s = r.Split('_');
        if (s.Length == 2)
        {
            enemyOrderName = s[0];
            enemyOrderScore = Convert.ToInt32(s[1]);
            enemyOrderEndTime = DateTime.Now.AddSeconds(enemyOrderScore);
            enemyGotOrder = 1;
            string m = string.Format("Your enemy got an order\n {0} $50 + ${1}", enemyOrderName, enemyOrderScore);
            showMessage(m);
        }
    }

    [PunRPC]
    void teamDelivered(double Score)
    {
        int s = Convert.ToInt32(Score) + 50;
        teamTotalScore += s;
        teamGotOrder = 2;
        StartCoroutine(ExecuteAfterTime(3));
        cookBook = transform.FindChild("Cook Book").FindChild("none").gameObject;
        string m = string.Format("DELIVERED!!! You got ${0}", s);
        showMessage(m);
        updateScore();
    }

    [PunRPC]
    void enemyDelivered(double Score)
    {
        int s = Convert.ToInt32(Score) + 50;
        enemyTotalScore += s;
        enemyGotOrder = 2;
        string m = string.Format("Your enemy delivered their food and got ${0}", s);
        showMessage(m);
        updateScore();
    }

   
    void pointIndicatorsToPlayer()
    {
        foreach (GameObject i in indicators)
        {
            i.transform.LookAt(this.transform);
        }
    }

    void pushToTalk()
	{
		if (Input.GetKey("q"))
		{
			voiceChat.Transmit = true;
			speakingIndicator.text = "Speaking";
            ingr_bg.enabled = true;
		}
		else
		{
			voiceChat.Transmit = false;
			speakingIndicator.text = "";
		}
	}

    void showcookbook()
    {
        if (Input.GetKeyDown("tab"))
        {
            if (cookBook.GetActive())
            {
                cookbookShowHide(false);
            }
            else
            {
                cookbookShowHide(true);
            }
        }
    }

    void cookbookShowHide(bool isShow)
    {
        if (isShow)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        Cursor.visible = isShow;
        GetComponent<BasicBehaviour>().onPlayerMovement = !isShow;
        Camera.main.GetComponent<ThirdPersonOrbitCam>().enabled = !isShow;
        cookBook.SetActive(isShow);
        playerCanvas.SetActive(!isShow);
    }
    
    public void clickButtonLeaveCookBook()
    {
        cookbookShowHide(false);
    }

    void showInstructionPanel()
    {
        if (Input.GetKey("tab"))
        {
            instructionPanel.SetActive(true);
            playerCanvas.SetActive(false);

        }
        else
        {
            instructionPanel.SetActive(false);
            playerCanvas.SetActive(true);
        }
    }

    IEnumerator ExecuteAfterTime(float time){
    	yield return new WaitForSeconds(time);
    	teamGotOrder = 0;
    }
}

