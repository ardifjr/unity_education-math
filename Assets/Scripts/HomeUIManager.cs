using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class HomeUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI highScoreTXT; // Drag text mesh pro "highScoreTXT" ke sini

    [Header("New High Score Effect (Optional)")]
    public GameObject newHighScoreEffect; // Optional: particle effect atau animasi khusus
    public AudioSource highScoreAudioSource; // Optional: sound effect untuk high score baru
    public AudioClip newHighScoreClip;

    private bool hasShownNewHighScoreEffect = false;

    void Start()
    {
        CheckForNewHighScore();
    UpdateHighScoreDisplay();
    }

    void UpdateHighScoreDisplay()
    {
        if (highScoreTXT == null)
        {
            Debug.LogError("highScoreTXT tidak di-assign! Drag text mesh pro ke inspector.");
            return;
        }

        int currentHighScore = 0;

        // Cek apakah HighScoreManager instance ada
        if (HighScoreManager.Instance != null)
        {
            currentHighScore = HighScoreManager.Instance.GetHighScore();
        }
        else
        {
            // Jika tidak ada instance, load langsung dari PlayerPrefs
            currentHighScore = PlayerPrefs.GetInt("MathQuizHighScore", 0);
            Debug.LogWarning("HighScoreManager instance not found, loading directly from PlayerPrefs");
        }

        // Langsung set nilai tanpa animasi
        highScoreTXT.text = $"{currentHighScore}";

        Debug.Log($"High Score displayed: {currentHighScore}");
    }

    void CheckForNewHighScore()
    {
        // Cek apakah baru saja mendapat high score baru
        bool isNewHighScore = PlayerPrefs.GetInt("IsNewHighScore", 0) == 1;

        if (isNewHighScore && !hasShownNewHighScoreEffect)
        {
            ShowNewHighScoreEffect();
            hasShownNewHighScoreEffect = true;

            // Reset flag setelah ditampilkan
            PlayerPrefs.SetInt("IsNewHighScore", 0);
            PlayerPrefs.Save();
        }
    }

    void ShowNewHighScoreEffect()
    {
        Debug.Log("NEW HIGH SCORE ACHIEVED!");

        // Aktifkan particle effect atau animasi khusus
        if (newHighScoreEffect != null)
        {
            newHighScoreEffect.SetActive(true);

            // Auto-hide effect setelah beberapa detik
            StartCoroutine(HideEffectAfterDelay(3f));
        }

        // Play sound effect
        if (highScoreAudioSource != null && newHighScoreClip != null)
        {
            highScoreAudioSource.PlayOneShot(newHighScoreClip);
        }
    }

    IEnumerator HideEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (newHighScoreEffect != null)
        {
            newHighScoreEffect.SetActive(false);
        }
    }

    /// <summary>
    /// Method untuk refresh display high score (bisa dipanggil dari button atau script lain)
    /// </summary>
    public void RefreshHighScoreDisplay()
    {
        UpdateHighScoreDisplay();
    }

    /// <summary>
    /// Method untuk reset high score (untuk testing)
    /// </summary>
    public void ResetHighScore()
    {
        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.ResetHighScore();
        }
        else
        {
            PlayerPrefs.SetInt("MathQuizHighScore", 0);
            PlayerPrefs.Save();
        }

        UpdateHighScoreDisplay();
        Debug.Log("High Score reset dari Home UI");
    }

    /// <summary>
    /// Debug method untuk test high score baru
    /// </summary>
    [ContextMenu("Test New High Score Effect")]
    public void TestNewHighScoreEffect()
    {
        PlayerPrefs.SetInt("IsNewHighScore", 1);
        PlayerPrefs.Save();
        CheckForNewHighScore();
    }
}