using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandBoxController : MonoBehaviour {

    public bool restart;
    public char conditionMet;

	private Vector3 moveDirection = Vector3.zero;
	
    public class Instruction
	{
        public char colorChoice { get; set; } 
        public char layerChoice { get; set; } 
	};

    Instruction instruction = new Instruction { colorChoice = '0', layerChoice = '0' };

    void Start()
	{
		restart = false;
		conditionMet = '0';
	}

    public void changeInstructions(char color, char layer)
	{
		instruction.colorChoice = color;
		instruction.layerChoice = layer;
	}

    void CheckCorrectColor(char colorTouched, GameObject sphereTouched)
	{
		// check for the correct color and layer
		if (instruction.colorChoice == colorTouched
			&& instruction.layerChoice == sphereTouched.ToString()[1])
		{
			sphereTouched.GetComponent<Renderer>().material.color = Color.white;
			conditionMet = '!';
			Debug.Log("Correct sphere touched");
		}
	}

    void OnTriggerEnter(Collider other)
	{
        switch(other.gameObject.tag)
		{
			case "Red Sphere":
		        CheckCorrectColor('r', other.gameObject);
				break;

			case "Blue Sphere":
		        CheckCorrectColor('b', other.gameObject);
				break;

			case "Green Sphere":
		        CheckCorrectColor('g', other.gameObject);
				break;

			case "Yellow Sphere":
		        CheckCorrectColor('y', other.gameObject);
				break;

            case "Restart Button":
			    // restart the game
				restart = true;
                break;

            case "Self":
			    break;

			default:
				Debug.Log("This is not a sphere, it is a " + other.gameObject.ToString());
				break;
		}
    }    

    void OnTriggerExit(Collider other)
	{
        switch(other.gameObject.tag)
	    {
			case "Red Sphere":
				other.gameObject.GetComponent<Renderer>().material.color = Color.red;
				if (conditionMet == '!') { conditionMet = '0'; }
				break;

			case "Blue Sphere":
				other.gameObject.GetComponent<Renderer>().material.color = Color.blue;
				if (conditionMet == '!') { conditionMet = '0'; }
				break;

			case "Green Sphere":
				other.gameObject.GetComponent<Renderer>().material.color = Color.green;
				if (conditionMet == '!') { conditionMet = '0'; }
				break;

			case "Yellow Sphere":
				other.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
				if (conditionMet == '!') { conditionMet = '0'; }
				break;

            case "Restart Button":
			    break;

            case "Self":
			    break;

			default:
				Debug.Log("This is not a sphere, it is a " + other.gameObject.ToString());
				break;
		}
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    void Update()
	{
	}

    // used to test the code - moves the spheres
    void FixedUpdate()
	{
		CharacterController controller = GetComponent<CharacterController>();
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        if (Input.GetKeyDown("e")) { moveDirection.y = 20f; }
		if (Input.GetKeyDown("q")) { moveDirection.y = -20f; }

        controller.Move(moveDirection * Time.deltaTime);
	}

}
