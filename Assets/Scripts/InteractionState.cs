using UnityEngine;
using UnityEngine.UI;

public class InteractionState: MonoBehaviour
{

    public InventoryBehavior inventory;
    private string stage;
    public Text pernamentInstruction;
    private GameObject switchTable;
    public BasicBehaviour bb;
    private static bool isStarted;
   
    // Enter the system, return the inventory table
    public InventoryBehavior startState(GameObject table)
    {
        controlTransision(true, table);
        return inventory;
    }

    // Leave the system
    public void finsihState(GameObject table)
    {
        controlTransision(false, table);
    }

    // Entering the item processing system (isConnected == true) or leave the system (isConnected == false)
    void controlTransision(bool isConverted, GameObject tableCanvas)
    {
        if (isConverted)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // Show/Hide mouse cursor, movement, camera, and the craft table
        Cursor.visible = isConverted;
        bb.onPlayerMovement = !isConverted;
        Camera.main.GetComponent<ThirdPersonOrbitCam>().enabled = !isConverted;
        tableCanvas.SetActive(isConverted);
        // Show/hide text, inventory table and craft table
        pernamentInstruction.enabled = !isConverted;
        inventory.clickable = isConverted;
        tableCanvas.GetComponentInChildren<InventoryBehavior>().clickable = isConverted;
    }
}
