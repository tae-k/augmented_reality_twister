using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

public class GameManager : MonoBehaviour {

	//private uint activeId;

    private int MAX_WAIT_TIME = 11;

	private int points;
	private int health;
    private char conditionMet;
    private float elapsedTime;

	private bool restart;
    private bool gameOver;
    private bool instructionGiven;

    public Text gameText;
    public Text timerText;
    public Text pointsText;

    private GameObject[] hearts;
	public HandBoxController handBox;

    private char[] colorChoices = {'r', 'b', 'g', 'y'};
	private char[] layerChoices = {'1', '2', '3', '4'};

    class Instruction
	{
        public char colorChoice { get; set; } 
        public char layerChoice { get; set; } 
	};

	Instruction instruction = new Instruction { colorChoice = '0', layerChoice = '0' };

	private HashSet<uint> trackedHands = new HashSet<uint>();
	private Dictionary<uint, HandBoxController> trackedObjects = new Dictionary<uint, HandBoxController>();

    // Speech and Shuffling Initialization

    private string speech;
    private TextToSpeech textToSpeech;


    private GameObject[][] layers;
    private Vector3[][] shuffledLs;
    // original positions of the spheres 
    private Vector3[][] fixedLs = new Vector3[][] {
            new Vector3[] { new Vector3(-0.7f,0,0), new Vector3(-0.7f, 0, 1.4f), new Vector3(0.7f,0,1.4f), new Vector3(0.7f,0,0f) },
            new Vector3[] { new Vector3(-0.7f,0.4f,0), new Vector3(-0.7f,0.4f,1.4f), new Vector3(0.7f,0.4f,1.4f), new Vector3(0.7f,0.4f,0) },
            new Vector3[] { new Vector3(-0.7f,0.8f,0), new Vector3(-0.7f,0.8f,1.4f), new Vector3(0.7f,0.8f,1.4f), new Vector3(0.7f,0.8f,0) },
            new Vector3[] { new Vector3(-0.7f, 1.2f, 0), new Vector3(-0.7f, 1.2f, 1.4f), new Vector3(0.7f, 1.2f, 1.4f), new Vector3(0.7f, 1.2f, 0) } };

    void Awake()
    {
        textToSpeech = GetComponent<TextToSpeech>();

        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
	}

    void Start()
	{
		gameOver = true;
		instructionGiven = false;

        points = 0;
		health = 3;
		elapsedTime = MAX_WAIT_TIME;

        conditionMet = '0';

        gameText.text = "";
        pointsText.text = points.ToString();
        timerText.text = "00:" + MAX_WAIT_TIME.ToString("00");

        trackedHands.Add(0);
		trackedObjects.Add(0, handBox);

        hearts = new GameObject[] { GameObject.Find("H1"),
		                            GameObject.Find("H2"),
									GameObject.Find("H3") };

		// Initialize a 2D array of spheres / layer
		layers = new GameObject[][] { 
			new GameObject[] { GameObject.Find("B1"), GameObject.Find("R1"),
			                   GameObject.Find("G1"), GameObject.Find("Y1") },
			new GameObject[] { GameObject.Find("B2"), GameObject.Find("R2"),
			                   GameObject.Find("G2"), GameObject.Find("Y2") },
			new GameObject[] { GameObject.Find("B3"), GameObject.Find("R3"),
			                   GameObject.Find("G3"), GameObject.Find("Y3") },
			new GameObject[] { GameObject.Find("B4"), GameObject.Find("R4"),
			                   GameObject.Find("G4"), GameObject.Find("Y4") } };

		// Initialize a 2D array of sphere positions / layer
		shuffledLs = new Vector3[][] {
			new Vector3[] { GameObject.Find("B1").transform.position, GameObject.Find("R1").transform.position,
							GameObject.Find("G1").transform.position, GameObject.Find("Y1").transform.position },
			new Vector3[] { GameObject.Find("B2").transform.position, GameObject.Find("R2").transform.position,
							GameObject.Find("G2").transform.position, GameObject.Find("Y2").transform.position },
			new Vector3[] { GameObject.Find("B3").transform.position, GameObject.Find("R3").transform.position,
							GameObject.Find("G3").transform.position, GameObject.Find("Y3").transform.position },
			new Vector3[] { GameObject.Find("B4").transform.position, GameObject.Find("R4").transform.position,
							GameObject.Find("G4").transform.position, GameObject.Find("Y4").transform.position } };
        
        
    }

    void UpdateHealth()
    {
		string health_msg = "";

	    switch(health)
	    {
	        case 3:
    		    break;
			
			case 2:
				hearts[2].SetActive(false);
				health_msg = "You have 2 hearts left";
				break;

			case 1:
				hearts[1].SetActive(false);
				health_msg = "You have 1 heart left";
				break;
			
			case 0:
				hearts[0].SetActive(false);
				health_msg = "Game Over";
				break;
		}

		// you are told how many hearts you have left
		var msg = string.Format(health_msg, textToSpeech.Voice.ToString());
		textToSpeech.StartSpeaking(msg);
		Debug.Log(health_msg);
   }

    void UpdateTimer()
	{
        // as long as both hands are not in the correct positions, decrease the timer        
		if (conditionMet != '!')
		{
			// keep on decreasing time
			elapsedTime -= Time.deltaTime;
            float minutes = (int)(elapsedTime / 60f);
            float seconds = (int)(elapsedTime % 60f);
            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
			
            // there is no more time left
			if (elapsedTime <= 1)
			{
				if (health > 0)
				{
					health--;
					UpdateHealth();
					elapsedTime = MAX_WAIT_TIME;
					timerText.text = "00:" + elapsedTime.ToString("00");
				}
			}

            // there are no more hearts left			
			if (health <= 0)
			{
			    timerText.text = "";
                gameText.text = "GAME\nOVER";
     			gameOver = true;

				// make sure there is no premature restart
			    foreach (uint id in trackedHands)
		        {
			        trackedObjects[id].restart = false;
			    }
				restart = false;
			}
        }
	}

    void CheckGiveInstructions()
	{
		if (instructionGiven == false && conditionMet == '0')
		{
			// choose a random instruction
			int randi = Random.Range(0, 4);
			instruction.colorChoice = colorChoices[randi];
			randi = Random.Range(0, 4);
			instruction.layerChoice = layerChoices[randi];

			// update the instruction for the speech
			string color = "";
			switch(instruction.colorChoice)
			{
				case 'r':
					color = "red";
					break;
				case 'b':
					color = "blue";
					break;
				case 'g':
					color = "green";
					break;
				case 'y':
					color = "yellow";
					break;
			}
			string layer = "";
			switch (instruction.layerChoice)
			{
				case '1':
					layer = "one";
					break;
				case '2':
					layer = "two";
					break;
				case '3':
					layer = "three";
					break;
				case '4':
					layer = "four";
					break;
			}
			speech = "Instructions: " + " Color: " + color + " Layer: " + layer + ".";

            // tell the instructions
            
            var msg = string.Format(speech, textToSpeech.Voice.ToString());
			textToSpeech.StartSpeaking(msg);
			Debug.Log(speech);

			// make sure to update the instruction for all the tracked hands
			foreach (uint id in trackedHands)
			{
				trackedObjects[id].changeInstructions(instruction.colorChoice,
													instruction.layerChoice);
			}
		}
        instructionGiven = true;
	}

    // shuffles the layers
    void Shuffle(Vector3[] layer)
    {
        int n = layer.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector3 temp = layer[i];
            layer[i] = layer[j];
            layer[j] = temp;
        }
    }

    // randomize the layers of spheres
    void RandomizeLayers(GameObject[] layer, Vector3[] shuffledLayer)
    {
        Shuffle(shuffledLayer);
        for (int i = 0; i < layer.Length; i++)
        {
            layer[i].transform.position = shuffledLayer[i];
        }
    }

    void Unshuffle(GameObject[] layer, Vector3[] fixedArr)
    {
        for (int i = 0; i < layer.Length; i++)
        {
            layer[i].transform.position = fixedArr[i];
        }
    }

    void CheckInstructionsMet()
	{
		// check if any hands met the instructions
        foreach (uint id in trackedHands)
		{
			if (trackedObjects[id].conditionMet == '!')
			{
			    conditionMet = '!';
				break;
			}
		}

        if (conditionMet == '!') 
		{
			// update the points
			points += 1;
			pointsText.text = points.ToString();

            // make sure that the hands no longer have the condition met
			foreach (uint id in trackedHands)
			{
				trackedObjects[id].conditionMet = '0';
			}
			conditionMet = '0';

            // get ready to give new instructions and restart the timer
            instructionGiven = false;
			elapsedTime = MAX_WAIT_TIME;
			timerText.text = "00:" + MAX_WAIT_TIME.ToString("00");

            // randomize the layers
			for (int i = 0; i < 4; i++)
			{
				RandomizeLayers(layers[i], shuffledLs[i]);
			}
		}
	}

    void CheckRestart()
	{
		// check if any hands touched the restart button
		foreach (uint id in trackedHands)
		{
			if (trackedObjects[id].restart)
			{
				restart = true;
				break;
			}
		}

		// can only restart the game once the game is over
	    if (gameOver && restart)
		{
            // game restarts
		    gameOver = false;	
			gameText.text = "";

            // points are reset
			points = 0;
            pointsText.text = points.ToString();

            // instructions are reset
			instructionGiven = false;

            // make the hearts reappear
            health = 3;
			foreach (GameObject h in hearts)
			{
				h.SetActive(true);
			}

            // restart the timer
			elapsedTime = MAX_WAIT_TIME;
			timerText.text = "00:" + MAX_WAIT_TIME.ToString("00");

            // put the spheres back in their original positions
            for (int i = 0; i < 4; i++)
            {
                Unshuffle(layers[i], fixedLs[i]);
            }

            // make sure that the hands no longer have the condition met or restart
            foreach (uint id in trackedHands)
		    {
			    trackedObjects[id].restart = false;
			    trackedObjects[id].conditionMet = '0';
			}
			restart = false;
			conditionMet = '0';
		}
	}

    void Update ()
	{
		if (gameOver == false)
		{
		    UpdateTimer();
            CheckGiveInstructions();
			CheckInstructionsMet();
		}
        CheckRestart();
        
    }

    /* ############################################
	 * STARTING FROM HERE IS THE HAND TRACKING CODE
	 * ############################################  
	 */
    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
	{
		uint id = args.state.source.id;
		// make sure that the source is a hand
		if (args.state.source.kind != InteractionSourceKind.Hand)
		{
		    return;
		}     
		// add hand to trackedHands if not already added
        if (!trackedHands.Contains(id))
		{
            trackedHands.Add(id);
		}
       //activeId = id;

        var obj = Instantiate(handBox) as HandBoxController;
		Vector3 pos;

		if (args.state.sourcePose.TryGetPosition(out pos))
		{
			obj.transform.position = pos;
			obj.changeInstructions(instruction.colorChoice, instruction.layerChoice);
		}

		// add object to trackedObjects if not already added
        if (!trackedObjects.ContainsKey(id))
		{
            trackedObjects.Add(id, obj);
		} 
	}

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        uint id = args.state.source.id;
        Vector3 pos;
        Quaternion rot;

		// make sure that the source is a hand
		if (args.state.source.kind == InteractionSourceKind.Hand)
		{
			if (trackedObjects.ContainsKey(id))
			{
				if (args.state.sourcePose.TryGetPosition(out pos))
				{
					trackedObjects[id].transform.position = pos;
				}
				if (args.state.sourcePose.TryGetRotation(out rot))
     			{
				    trackedObjects[id].transform.rotation = rot;
			    }
		    }
	    }
    }

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
	{
		uint id = args.state.source.id;
		// make sure that the source is a hand
        if (args.state.source.kind != InteractionSourceKind.Hand)
		{
		    // make sure that the source is speach
            if (args.state.source.kind != InteractionSourceKind.Voice)
			{
			    return;
			}
		} 

		if (trackedHands.Contains(id))
		{
			trackedHands.Remove(id);
		}
		if (trackedObjects.ContainsKey(id))
		{
			var obj = trackedObjects[id];
			trackedObjects.Remove(id);
            //obj.SetActive(false);
			obj.Kill();
		}
		if (trackedHands.Count > 0)
		{
			//activeId = trackedHands.First();
		}
	}

    void OnDestroy()
	{                        
		InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
		InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
		InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
	}

}
