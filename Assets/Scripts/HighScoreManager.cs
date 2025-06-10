using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    private const string HIGH_SCORE_KEY = "MathQuizHighScore";
    
    // Singleton pattern untuk mudah diakses dari scene manapun
    public static HighScoreManager Instance { get; private set; }
    
    [Header("Current Session")]
    public int currentScore = 0;
    public int highScore = 0;
    
    void Awake()
    {
        // Singleton pattern - pastikan hanya ada satu instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadHighScore();
        Debug.Log($"High Score loaded: {highScore}");
    }
    
    /// <summary>
    /// Simpan high score ke PlayerPrefs
    /// </summary>
    public void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
        Debug.Log($"High Score saved: {highScore}");
    }
    
    /// <summary>
    /// Load high score dari PlayerPrefs
    /// </summary>
    public void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        Debug.Log($"High Score loaded: {highScore}");
    }
    
    /// <summary>
    /// Update score saat ini dan cek apakah ini high score baru
    /// </summary>
    /// <param name="newScore">Score baru yang akan dicek</param>
    /// <returns>True jika ini high score baru</returns>
    public bool UpdateScore(int newScore)
    {
        currentScore = newScore;
        
        if (newScore > highScore)
        {
            highScore = newScore;
            SaveHighScore();
            Debug.Log($"NEW HIGH SCORE! {highScore}");
            return true; // Mengembalikan true jika ini high score baru
        }
        
        return false; // Bukan high score baru
    }
    
    /// <summary>
    /// Get high score (untuk ditampilkan di UI)
    /// </summary>
    /// <returns>High score saat ini</returns>
    public int GetHighScore()
    {
        return highScore;
    }
    
    /// <summary>
    /// Get current score
    /// </summary>
    /// <returns>Score saat ini</returns>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// Reset high score (untuk testing atau reset game)
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        currentScore = 0;
        SaveHighScore();
        Debug.Log("High Score reset to 0");
    }
    
    /// <summary>
    /// Cek apakah score tertentu adalah high score
    /// </summary>
    /// <param name="score">Score yang akan dicek</param>
    /// <returns>True jika score tersebut adalah high score</returns>
    public bool IsHighScore(int score)
    {
        return score > highScore;
    }
}