using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrashCanBehavior : MonoBehaviour {

    private InteractionState interactionState;
    public GameObject canvas;
    public GameObject mainCanvas;
    private InventoryBehavior trash;
    private string stage;
    private GameObject dumpSpawn;
    private InventoryBehavior inventory;
	public Text binMess;
    // Use this for initialization
    void Start()
    {
        stage = "available";
        trash = canvas.GetComponentInChildren<InventoryBehavior>();
        dumpSpawn = GameObject.FindGameObjectWithTag("DumpSpawnPoint");
        mainCanvas.SetActive(false);
    }

    // When user press e in front of the station
    public void gotInteracted()
    {
        mainCanvas.SetActive(true);
        inventory = interactionState.startState(canvas);
        trash.setother(inventory);
        inventory.setother(trash);
        inventory.makeAvailableToTransfer(true);
        trash.makeAvailableToTransfer(true);
        stage = "watingForPickedItems";
    }

    // Called when PROCESS BUTTON clicked
    public void processClick()
    {
        mainCanvas.SetActive(false);
        // Collect all ids
        trash.deleteAllSlots();
        inventory.makeAvailableToTransfer(true);
        trash.makeAvailableToTransfer(true);
        trash.setother(null);
        inventory.setother(null);
        interactionState.finsihState(canvas);
        stage = "available";
    }

    // Called when LEAVE BUTTON clicked
    public void leaveClick()
    {
        mainCanvas.SetActive(false);
        trash.transferAll();
        inventory.makeAvailableToTransfer(true);
        trash.makeAvailableToTransfer(true);
        trash.setother(null);
        inventory.setother(null);
        interactionState.finsihState(canvas);
        stage = "available";
    }

    public void setInteractacState(InteractionState iState)
    {
        interactionState = iState;
        inventory = iState.inventory;
    }

}
