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

    private int targetLevel;
    private bool isMoving = false;
    private bool isTransitioning = false; 
    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;

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
        if (sfxSource != null && dingSound != null) sfxSource.PlayOneShot(dingSound);

        levelText.text = "LEVEL COMPLETE!";
        progressBarFill.fillAmount = 1f;
        percentageText.text = "100%";
      
        yield return new WaitForSeconds(2f);

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
        endGameText.text = "GAME COMPLETE!\n<size=50%>YOU'RE HIRED!</size>";
        
        levelText.gameObject.SetActive(false); 
        percentageText.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }
}