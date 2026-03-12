using UnityEngine;
using UnityEngine.UI; // Needed for UI
using TMPro; // Needed for TextMeshPro
using System.Collections; // Needed for Coroutines (pauses)

public class LiftLevelController : MonoBehaviour
{
    [Header("Level Configuration")]
    [Tooltip("Level markers in order from top to bottom")]
    public Transform[] levels;
    [Tooltip("Drag your WindowSurface objects here in the same order as the markers!")]
    public WindowCleaner[] windows;

    [Header("Movement settings")]
    public float moveSpeed = 2f;
    public int currentLevel = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI percentageText; 
    public Image progressBarFill;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dingSound;

    private int targetLevel;
    private bool isMoving = false;
    private bool isTransitioning = false; // Prevents checking while the "Level Complete" pause happens
    
    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;

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

        UpdateUI(currentLevel);
    }

    private void Update()
    {
        if (levels == null || levels.Length == 0) return;

        Vector3 targetPosition = levels[targetLevel].position;

        // --- MOVEMENT LOGIC ---
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
            // We have arrived at a floor!
            if (isMoving) 
            {
                currentLevel = targetLevel;
                isMoving = false;
                UpdateUI(currentLevel); // Reset UI for the new floor
            }

            // --- PROGRESS CHECK LOGIC ---
            // Only check the window if we are stopped, not transitioning, and it's time to check
            if (!isMoving && !isTransitioning && currentLevel < windows.Length)
            {
                if (Time.time >= nextCheckTime)
                {
                    nextCheckTime = Time.time + checkInterval;
                    CheckCurrentWindow();
                }
            }
        }
    }

    void CheckCurrentWindow()
    {
        // Ask the current window how clean it is
        float progress = windows[currentLevel].GetCleanPercentage();
        
        // Update the UI
        progressBarFill.fillAmount = progress;
        int percent = Mathf.Clamp(Mathf.RoundToInt(progress * 100f), 0, 100);
        percentageText.text = percent + "%";

        // If it's 95% clean, trigger the win sequence!
        if (progress >= 0.95f)
        {
            StartCoroutine(HandleLevelComplete());
        }
    }

    IEnumerator HandleLevelComplete()
    {
        isTransitioning = true; // Lock the script so it doesn't double-fire

        // 1. Play the Ding!
        if (audioSource != null && dingSound != null)
        {
            audioSource.PlayOneShot(dingSound);
        }

        // 2. Force UI to show 100% and success text
        levelText.text = "LEVEL COMPLETE!";
        progressBarFill.fillAmount = 1f;
        percentageText.text = "100%";

        // 3. Pause for 2 seconds to let the player read it
        yield return new WaitForSeconds(2f);

        // 4. Automatically move the lift down!
        if (currentLevel < levels.Length - 1)
        {
            isTransitioning = false; // Unlock
            MoveDown();
        }
        else
        {
            // No more levels to go down to!
            levelText.text = "YOU'RE HIRED!";
            percentageText.text = "";
        }
    }

    public void MoveUp()
    {
        if (isMoving) return;
        if (targetLevel > 0)
        {
            targetLevel--;
            isTransitioning = false;
        }
    }

    public void MoveDown()
    {
        if (isMoving) return;
        if (targetLevel < levels.Length - 1)
        {
            targetLevel++;
            isTransitioning = false;
        }
    }

    void UpdateUI(int levelIndex)
    {
        // MATH MAGIC: If we have 6 levels, and we are on Index 0 (top), Floor = 6. 
        int gameLevel = 1+levelIndex;
        
        levelText.text = "LEVEL " + gameLevel;
        progressBarFill.fillAmount = 0f;
        percentageText.text = "0%";
    }
}