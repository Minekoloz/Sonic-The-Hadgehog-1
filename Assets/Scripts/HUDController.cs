using UnityEngine;
using TMPro;

public class SonicTimerHUD : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public SonicController sonicController; // Ссылка на SonicController

    public bool isRunning = true;
    private float timer = 0f;

    void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";

        // Если достигнуто 99:99 — убить Соника
        if (minutes >= 99 && seconds >= 59)
        {
            isRunning = false;
            sonicController?.Die();
        }
    }

    public void ResetTimer() => timer = 0f;
    public float GetCurrentTime() => timer;
    public void StopTimer() => isRunning = false;
    public void StartTimer() => isRunning = true;
}
