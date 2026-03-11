using UnityEngine;

public class LiftLevelController : MonoBehaviour
{
    [Header("Level markers in order from top to bottom or bottom to top")]
    public Transform[] levels;

    [Header("Movement settings")]
    public float moveSpeed = 2f;

    [Header("Current level index")]
    public int currentLevel = 0;

    private int targetLevel;
    private bool isMoving = false;

    private void Start()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("No levels assigned to LiftLevelController.");
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 0, levels.Length - 1);
        targetLevel = currentLevel;

        transform.position = levels[currentLevel].position;
    }

    private void Update()
    {
        if (levels == null || levels.Length == 0) return;

        Vector3 targetPosition = levels[targetLevel].position;

        if (Vector3.Distance(transform.position, targetPosition) > 0.001f)
        {
            isMoving = true;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = targetPosition;
            currentLevel = targetLevel;
            isMoving = false;
        }
    }

    public void MoveUp()
    {
        if (isMoving) return;

        if (targetLevel > 0)
        {
            targetLevel--;
        }
    }

    public void MoveDown()
    {
        if (isMoving) return;

        if (targetLevel < levels.Length - 1)
        {
            targetLevel++;
        }
    }
}