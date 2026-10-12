using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField, Min(0)] private int startingTimeSeconds = 60;
    [SerializeField] private bool startOnPlay = true;

    [Header("Display")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private string displayPrefix = "";

    private float remainingSeconds;
    private bool isRunning;

    private void Start()
    {
        ResetTimer();

        if (startOnPlay)
            StartTimer();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        remainingSeconds -= Time.deltaTime;

        if (remainingSeconds <= 0f)
        {
            remainingSeconds = 0f;
            isRunning = false;
        }

        RefreshDisplay();
    }

    public void StartTimer()
    {
        isRunning = true;
        RefreshDisplay();
    }

    public void StopTimer()
    {
        isRunning = false;
        RefreshDisplay();
    }

    public void ResetTimer()
    {
        remainingSeconds = startingTimeSeconds;
        isRunning = false;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{displayPrefix}{minutes:00}:{seconds:00}";
    }
}
