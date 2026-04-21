using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LiftLevelController : MonoBehaviour
{
    public Transform[] levels;
    public WindowCleaner[] windows;

    public float moveSpeed = 2f;
    public int currentLevel = 0;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI percentageText;
    public Image progressBarFill;
    public TextMeshProUGUI tipText;

    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;

    public AudioSource sfxSource;
    public AudioClip dingSound;
    public AudioClip gameOverSound;
    public AudioClip victorySound;

    public AudioSource movementSource;

    public float baseTipPerLevel = 50f;
    public float maxSpeedBonus = 30f;
    public float maxCleanlinessBonus = 20f;
    public float speedBonusTime = 60f;

    private int targetLevel;
    private bool isMoving = false;
    private bool isTransitioning = false;
    private float checkInterval = 0.5f;
    private float nextCheckTime = 0f;

    private float totalTips = 0f;
    private float levelStartTime = 0f;

    private void Start()
    {
        Time.timeScale = 1f;

        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("No levels assigned.");
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 0, levels.Length - 1);
        targetLevel = currentLevel;
        transform.position = levels[currentLevel].position;

        UpdateUI(currentLevel);

        if (movementSource != null) movementSource.Stop();

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

                if (movementSource != null)
                {
                    movementSource.Stop();
                }

                UpdateUI(currentLevel);
                levelStartTime = Time.time;
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

        if (progress >= 0.80f)
        {
            StartCoroutine(HandleLevelComplete());
        }
    }

    IEnumerator HandleLevelComplete()
    {
        isTransitioning = true;

        float timeSpent = Time.time - levelStartTime;
        float cleanPercent = windows[currentLevel].GetCleanPercentage();

        float speedRatio = Mathf.Clamp01(1f - (timeSpent / (speedBonusTime * 2f)));
        float speedBonus = Mathf.Round(speedRatio * maxSpeedBonus);

        float cleanRatio = Mathf.Clamp01((cleanPercent - 0.95f) / 0.05f);
        float cleanBonus = Mathf.Round(cleanRatio * maxCleanlinessBonus);

        float levelTip = baseTipPerLevel + speedBonus + cleanBonus;
        totalTips += levelTip;

        if (sfxSource != null && dingSound != null) sfxSource.PlayOneShot(dingSound);

        levelText.text = "LEVEL COMPLETE!";
        progressBarFill.fillAmount = 1f;
        percentageText.text = "100%";
        percentageText.color = Color.green;

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

        float maxPossible = (baseTipPerLevel + maxSpeedBonus + maxCleanlinessBonus) * levels.Length;
        endGameText.text = $"GAME COMPLETE!\n<size=50%>YOU'RE HIRED!\n\nTotal Tips: ${totalTips:0}  /  ${maxPossible:0} possible</size>";

        levelText.gameObject.SetActive(false);
        percentageText.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }
}