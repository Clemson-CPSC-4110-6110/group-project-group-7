using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Collections; 

public class LiftLevelController : MonoBehaviour
{
    [Header("Level Configuration")]
    public Transform[] levels;
    public WindowCleaner[] windows;

    [Header("Movement settings")]
    public float moveSpeed = 2f;
    public int currentLevel = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI percentageText; 
    public Image progressBarFill;
    public TextMeshProUGUI tipText; //Testing a $ tip UI for completing windows based on speed

    [Header("End Game UI")]
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;

    [Header("Audio - SFX")]
    public AudioSource sfxSource; // For Dings, Game Over, Victory
    public AudioClip dingSound;
    public AudioClip gameOverSound;
    public AudioClip victorySound;

    [Header("Audio - Movement")]
    public AudioSource movementSource; // For the looping engine hum

    [Header("Fall Recovery UI")]
    public Button resetPositionButton;

    [Header("Tip Settings")]
    public float baseTipPerLevel = 50f;
    public float maxSpeedBonus = 30f;
    public float maxCleanlinessBonus = 20f;
    public float speedBonusTime = 60f; // Clean within this time for full speed bonus amount

    private int targetLevel;
    private bool isMoving = false;
    private bool isTransitioning = false; 
    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;

    private EquipmentTracker equipmentTracker;

    //Tip tracking
    private float totalTips = 0f;
    private float levelStartTime = 0f; 

    private void Start()
    {
        Time.timeScale = 1f;

        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("No levels assigned to LiftLevelController.");
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 0, levels.Length - 1);
        targetLevel = currentLevel;
        transform.position = levels[currentLevel].position;

        UpdateUI(currentLevel);

        if (movementSource != null) movementSource.Stop();

        if (resetPositionButton != null)
        {
            resetPositionButton.gameObject.SetActive(false);
        }

        UpdateTipUI();
        levelStartTime = Time.time;
    }

    private void Update()
    {
        if (levels == null || levels.Length == 0) return;

        Vector3 targetPosition = levels[targetLevel].position;

        if (Vector3.Distance(transform.position, targetPosition) > 0.001f)
        {
            isMoving = true;

            // START MOVEMENT SOUND
            if (movementSource != null && !movementSource.isPlaying)
            {
                movementSource.Play();
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            if (isMoving) 
            {
                currentLevel = targetLevel;
                isMoving = false;

                // STOP MOVEMENT SOUND
                if (movementSource != null)
                {
                    movementSource.Stop();
                }

                UpdateUI(currentLevel);

                levelStartTime = Time.time; //Start timing for new level
            }

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
        float progress = windows[currentLevel].GetCleanPercentage();
        progressBarFill.fillAmount = progress;
        int percent = Mathf.Clamp(Mathf.RoundToInt(progress * 100f), 0, 100);
        percentageText.text = percent + "%";

        if (progress >= 0.95f)
        {
            StartCoroutine(HandleLevelComplete());
        }
    }

    IEnumerator HandleLevelComplete()
    {
        isTransitioning = true;

        // Calculate tips for the level
        float timeSpent = Time.time - levelStartTime;
        float cleanPercent = windows[currentLevel].GetCleanPercentage();

        //Speed bonus
        float speedRatio = Mathf.Clamp01(1f - (timeSpent / (speedBonusTime * 2f)));
        float speedBonus = Mathf.Round(speedRatio * maxSpeedBonus);

        //Cleanliness bonus scales 
        float cleanRatio = Mathf.Clamp01((cleanPercent - 0.95f) / 0.05f);
        float cleanBonus = Mathf.Round(cleanRatio * maxCleanlinessBonus);

        float levelTip = baseTipPerLevel + speedBonus + cleanBonus;
        totalTips += levelTip;

        if (sfxSource != null && dingSound != null) sfxSource.PlayOneShot(dingSound);

        levelText.text = "LEVEL COMPLETE!";
        progressBarFill.fillAmount = 1f;
        percentageText.text = "100%";
        percentageText.color = Color.green;

        // shows how much was earned this level
        if (tipText != null)
        {
            tipText.text = $"Tips: ${totalTips:0}\n<size=70%><color=green>+${levelTip:0} this level!</color></size>";
        }
      
        yield return new WaitForSeconds(2f);

        percentageText.color = Color.white;
        UpdateTipUI();

        if (currentLevel < levels.Length - 1)
        {
            isTransitioning = false; 
            MoveDown();
        }
        else
        {
            GameComplete();
        }
    }

    void UpdateTipUI()
    {
        if (tipText != null)
        {
            tipText.text = $"Tips: ${totalTips:0}";
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
        int gameLevel = 1 + levelIndex;
        levelText.text = "LEVEL " + gameLevel;
        progressBarFill.fillAmount = 0f;
        percentageText.text = "0%";
    }

    public void ShowFallPanel(EquipmentTracker tracker)
    {
        equipmentTracker = tracker;
 
        if (movementSource != null) movementSource.Stop();
 
        // Show panel with a warning colour (yellow) and the reset option
        endGamePanel.SetActive(true);
        endGameText.color = Color.yellow;
        endGameText.text = "YOU FELL!\n<size=50%>Reset your position to continue.</size>";
 
        // Wire up and show the reset button
        if (resetPositionButton != null)
        {
            resetPositionButton.gameObject.SetActive(true);
            resetPositionButton.onClick.RemoveAllListeners();
            resetPositionButton.onClick.AddListener(ResetPlayerPosition);
        }
 
        // Do NOT freeze time — the lift stays put and the player can look around
    }
 
    // Called by the Reset Position button
    public void ResetPlayerPosition()
    {
        // Hide the panel and button
        endGamePanel.SetActive(false);
 
        if (resetPositionButton != null)
            resetPositionButton.gameObject.SetActive(false);
 
        // Tell the tracker to move the player back and re-enable monitoring
        if (equipmentTracker != null)
            equipmentTracker.ResetPlayerPosition();
 
        equipmentTracker = null;
    }

    public void GameOver(string reason)
    {
        if (sfxSource != null && gameOverSound != null)
        {
            sfxSource.PlayOneShot(gameOverSound);
        }
        
        if (movementSource != null) movementSource.Stop();

        isMoving = false; 
        isTransitioning = true; 

        endGamePanel.SetActive(true);
        endGameText.color = Color.red;
        endGameText.text = "GAME OVER\n<size=50%>" + reason + "</size>";

        Time.timeScale = 0f; 
    }

    public void GameComplete()
    {
        if (sfxSource != null && victorySound != null)
        {
            sfxSource.PlayOneShot(victorySound);
        }

        if (movementSource != null) movementSource.Stop();

        isMoving = false;
        isTransitioning = true;

        endGamePanel.SetActive(true);
        endGameText.color = Color.green;

        // Final tip breakdown
        float maxPossible = (baseTipPerLevel + maxSpeedBonus + maxCleanlinessBonus) * levels.Length;
        endGameText.text = $"GAME COMPLETE!\n<size=50%>YOU'RE HIRED!\n\nTotal Tips: ${totalTips:0}  /  ${maxPossible:0} possible</size>";
        
        levelText.gameObject.SetActive(false); 
        percentageText.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }
}