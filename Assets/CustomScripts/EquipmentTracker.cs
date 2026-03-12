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
            return;
        }

        for (int i = 0; i < tools.Length; i++)
        {
            if (tools[i] != null)
            {
                float toolDist = Vector3.Distance(liftCenter.position, tools[i].position);

                if (toolDist > maxAllowedDistance)
                {
                    TriggerGameOver("You dropped your equipment!");
                    return;
                }
            }
        }
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