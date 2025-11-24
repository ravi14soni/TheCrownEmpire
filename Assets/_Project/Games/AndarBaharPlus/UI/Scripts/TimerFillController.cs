using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerFillController : MonoBehaviour
{
    public Image timerImage; // Reference to the sliced image.
    public float totalTime = 15f; // The total duration of the timer.
    private float remainingTime; // The remaining time.
    private Coroutine blinkCoroutine;

    // Call this method to initialize or update the timer.
    public void SetTimer(float currentTime)
    {
        // Update the remaining time based on the input.
        remainingTime = Mathf.Clamp(currentTime, 0, totalTime);

        // Normalize the fill amount and update the image.
        UpdateFillAmount();

        if (remainingTime <= 5f)
        {
            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(BlinkImage());
        }
        else
        {
            // Stop blinking if the time goes back above 5 seconds.
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
                timerImage.color = Color.white; // Reset to the original color.
            }
        }
    }

    private void UpdateFillAmount()
    {
        // Calculate the normalized fill amount (1 to 0 based on remaining time).
        float fill = Mathf.Clamp01(remainingTime / totalTime);
        timerImage.fillAmount = fill;
    }

    private IEnumerator BlinkImage()
    {
        Color originalColor = timerImage.color; // Store the original color.
        Color redColor = Color.red; // Define the red color.

        while (remainingTime > 0)
        {
            // Alternate between the original color and red.
            timerImage.color = redColor;
            yield return new WaitForSeconds(0.5f); // Half a second delay.
            timerImage.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }

        // Ensure the image resets to its original color when blinking stops.
        timerImage.color = originalColor;
    }
}
