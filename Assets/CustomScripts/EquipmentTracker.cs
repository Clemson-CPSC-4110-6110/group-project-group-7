using UnityEngine;

public class EquipmentTracker : MonoBehaviour
{
    public LiftLevelController liftController;
    public Transform liftCenter;
    public Transform playerHead;
    public Transform playerRig;
    public Transform respawnPoint;
    public Transform[] tools;
    public float maxAllowedDistance = 4f;

    private bool gameIsOver = false;
    private float startupDelay = 4f;

    private Vector3[] toolSpawnPositions;
    private Quaternion[] toolSpawnRotations;

    void Start()
    {
        toolSpawnPositions = new Vector3[tools.Length];
        toolSpawnRotations = new Quaternion[tools.Length];

        for (int i = 0; i < tools.Length; i++)
        {
            if (tools[i] != null)
            {
                toolSpawnPositions[i] = tools[i].position;
                toolSpawnRotations[i] = tools[i].rotation;
            }
        }
    }

    void Update()
    {
        if (gameIsOver) return;

        if (startupDelay > 0)
        {
            startupDelay -= Time.deltaTime;
            return;
        }

        if (liftCenter == null || playerHead == null) return;

        float playerDist = Vector3.Distance(liftCenter.position, playerHead.position);

        if (playerDist > maxAllowedDistance)
        {
            TriggerGameOver("You fell off the lift!");
            //TriggerPlayerFall();
            return;
        }

        for (int i = 0; i < tools.Length; i++)
        {
            if (tools[i] != null)
            {
                float toolDist = Vector3.Distance(liftCenter.position, tools[i].position);

                if (toolDist > maxAllowedDistance)
                {
                    SnapToolBack(i);
                    //TriggerGameOver("You dropped your equipment!");
                    //return;
                }
            }
        }
    }

    void SnapToolBack(int index)
    {
        if (tools[index] == null) return;
 
        tools[index].position = toolSpawnPositions[index];
        tools[index].rotation = toolSpawnRotations[index];
 
        Rigidbody rb = tools[index].GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
 
        Debug.Log($"[EquipmentTracker] {tools[index].name} snapped back to spawn.");
    }
 
    // Call this from UI button to snap all tools back manually
    public void ResetEquipment()
    {
        for (int i = 0; i < tools.Length; i++)
        {
            SnapToolBack(i);
        }
    }
 
    void TriggerPlayerFall()
    {
        // Show the fall panel but don't end the game — let the player reset
        gameIsOver = true; // Pause checking while panel is visible
 
        if (liftController != null)
        {
            liftController.ShowFallPanel(this);
        }
    }
 
    // Called by LiftLevelController when player hits the Reset button
    public void ResetPlayerPosition()
    {
        if (playerRig != null && respawnPoint != null && playerHead != null)
        {
            Vector3 headOffset = playerHead.position - playerRig.position;
            headOffset.y = 0;
            playerRig.position = respawnPoint.position - headOffset;
        }
 
        // Also snap tools back on reset
        ResetEquipment();
 
        // Re-enable tracking after a short moment so the teleport doesn't immediately re-trigger
        gameIsOver = false;
        startupDelay = 1.5f;
    }
    void TriggerGameOver(string reason)
    {
        gameIsOver = true;

        if (playerRig != null && respawnPoint != null && playerHead != null)
        {
            Vector3 headOffset = playerHead.position - playerRig.position;
            headOffset.y = 0; 

            playerRig.position = respawnPoint.position - headOffset;
        }

        for (int i = 0; i < tools.Length; i++)
        {
            if (tools[i] != null && respawnPoint != null)
            {
                tools[i].position = respawnPoint.position;

                Rigidbody rb = tools[i].GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        if (liftController != null) liftController.GameOver(reason);
    }
}