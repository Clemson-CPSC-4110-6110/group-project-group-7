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
        if (gameIsOver || startupDelay > 0f)
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
                    SnapToolBack(i);
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
    }

    public void ResetEquipment()
    {
        for (int i = 0; i < tools.Length; i++)
        {
            SnapToolBack(i);
        }
    }

    void SafeTeleportPlayer(Vector3 targetPosition)
    {
        if (playerRig == null || playerHead == null) return;

        Vector3 headOffset = playerHead.position - playerRig.position;
        headOffset.y = 0f;

        CharacterController cc = playerRig.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerRig.position = targetPosition - headOffset;

        if (cc != null) cc.enabled = true;
    }

    void TriggerGameOver(string reason)
    {
        gameIsOver = true;

        if (respawnPoint != null)
        {
            SafeTeleportPlayer(respawnPoint.position);
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

        if (liftController != null)
        {
            liftController.GameOver(reason);
        }
    }
}