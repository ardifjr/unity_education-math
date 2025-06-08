using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MathQuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI soalText; // TMP UI untuk soal
    public TextMeshPro jawabanKiriText; // TMP 3D object untuk jawaban kiri
    public TextMeshPro jawabanTengahText; // TMP 3D object untuk jawaban tengah  
    public TextMeshPro jawabanKananText; // TMP 3D object untuk jawaban kanan

    [Header("Answer Box Objects")]
    public GameObject boxKiri; // 3D square object untuk box kiri
    public GameObject boxTengah; // 3D square object untuk box tengah
    public GameObject boxKanan; // 3D square object untuk box kanan

    [Header("Game Over Panel")]
    public GameObject gameOverPanel; // Panel Game Over
    public TextMeshProUGUI finalScoreText; // Text untuk score akhir
    public TextMeshProUGUI earningsText; // Text untuk pendapatan
    public Button restartButton; // Tombol mulai ulang
    public Button homeButton; // Tombol home

    [Header("Game Settings")]
    public int minNumber = 1;
    public int maxNumber = 20;
    public int coinPerCorrectAnswer = 10; // Koin per jawaban benar

    [Header("Speed Control Settings")]
    public Animator roadAnimator; // Reference ke Animator untuk jalan/loop
    public string speedParameterName = "Speed"; // Nama parameter speed di Animator
    public float baseSpeed = 0.3f; // Kecepatan dasar
    public float speedIncrement = 0.1f; // Peningkatan speed per jawaban benar berturut-turut
    public float maxSpeed = 2.0f; // Kecepatan maksimum
    public int correctAnswersForSpeedIncrease = 1; // Berapa jawaban benar untuk menaikkan speed
    
    [Header("Speed UI (Optional)")]
    public TextMeshProUGUI speedDisplayText; // Text untuk menampilkan kecepatan saat ini

    [Header("Scene Names")]
    public string homeSceneName = "HomeScene"; // Nama scene home

    [Header("Countdown Integration")]
    public CountdownManager countdownManager; // Reference ke CountdownManager

    private int jawaban_benar;
    private int posisi_jawaban_benar; // 0=kiri, 1=tengah, 2=kanan
    private string soal_sekarang;
    private int skor = 0;
    private int totalEarnings = 0; // Total pendapatan
    private List<int> semua_jawaban_current; // Menyimpan jawaban saat ini
    private bool isProcessingAnswer = false; // Flag untuk mencegah multiple hits
    private bool gameStarted = false; // Flag untuk mengetahui apakah game sudah dimulai
    
    // Speed control variables
    private int consecutiveCorrectAnswers = 0; // Jawaban benar berturut-turut
    private float currentSpeed; // Kecepatan saat ini

    void Start()
    {
        SetupUI();
        SetupCollisionDetection();
        InitializeSpeedControl();
        
        // Jangan langsung mulai generate soal
        // Tunggu countdown selesai atau trigger manual
        if (countdownManager != null)
        {
            // Auto start countdown setelah delay singkat
            StartCoroutine(AutoStartCountdown());
        }
        else
        {
            // Jika tidak ada countdown manager, langsung mulai
            StartGame();
        }
    }

    IEnumerator AutoStartCountdown()
    {
        yield return new WaitForSeconds(1f); // Delay 1 detik
        countdownManager.StartCountdown();
    }

    // Method ini dipanggil oleh CountdownManager
    public void StartGame()
    {
        gameStarted = true;
        Debug.Log("Math Quiz Game Started!");
        InitializeSpeedControl();
        StartCoroutine(GenerateSoalCoroutine());
    }

    void InitializeSpeedControl()
    {
        currentSpeed = baseSpeed;
        UpdateAnimatorSpeed();
        UpdateSpeedDisplay();
    }

    void UpdateAnimatorSpeed()
{
    if (roadAnimator != null)
    {
        // PERBAIKAN: Hapus kondisi gameStarted agar bisa dipanggil kapan saja
        roadAnimator.SetFloat(speedParameterName, currentSpeed);
        
        // TAMBAHAN: Set speed property juga
        roadAnimator.speed = 1f; // Pastikan animator speed = 1 agar parameter bisa bekerja
        
        Debug.Log($"Animator speed updated - Parameter: {speedParameterName} = {currentSpeed}, Speed property = 1f");
        
        // TAMBAHAN: Debug animator state untuk troubleshooting
        if (roadAnimator.gameObject.activeInHierarchy)
        {
            Debug.Log($"Animator is active and enabled: {roadAnimator.enabled}");
            Debug.Log($"Animator controller: {roadAnimator.runtimeAnimatorController?.name}");
        }
        else
        {
            Debug.LogWarning("Animator GameObject is not active in hierarchy!");
        }
    }
    else
    {
        Debug.LogError("roadAnimator is null!");
    }
}

    void UpdateSpeedDisplay()
    {
        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"Speed: {currentSpeed:F1}x";
        }
    }

    void IncreaseSpeed()
    {
        consecutiveCorrectAnswers++;
        
        // Hitung speed baru berdasarkan jawaban benar berturut-turut
        if (consecutiveCorrectAnswers % correctAnswersForSpeedIncrease == 0)
        {
            float newSpeed = baseSpeed + (speedIncrement * (consecutiveCorrectAnswers / correctAnswersForSpeedIncrease));
            currentSpeed = Mathf.Min(newSpeed, maxSpeed);
            
            UpdateAnimatorSpeed();
            UpdateSpeedDisplay();
            
            Debug.Log($"Speed increased! Consecutive correct: {consecutiveCorrectAnswers}, New speed: {currentSpeed}");
        }
    }

    void ResetSpeed()
    {
        consecutiveCorrectAnswers = 0;
        currentSpeed = baseSpeed;
        UpdateAnimatorSpeed();
        UpdateSpeedDisplay();
        Debug.Log("Speed reset to base speed");
    }

    void SetupUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(GoToHome);
        }
    }

    void SetupCollisionDetection()
    {
        SetupBoxCollider(boxKiri, 0);
        SetupBoxCollider(boxTengah, 1);
        SetupBoxCollider(boxKanan, 2);
    }
    
    void SetupBoxCollider(GameObject box, int boxIndex)
    {
        if (box.GetComponent<BoxCollider>() == null)
        {
            box.AddComponent<BoxCollider>();
        }

        // Set sebagai trigger
        box.GetComponent<BoxCollider>().isTrigger = true;

        // Tambahkan script AnswerBox
        AnswerBox answerBox = box.GetComponent<AnswerBox>();
        if (answerBox == null)
        {
            answerBox = box.AddComponent<AnswerBox>();
        }
        answerBox.boxIndex = boxIndex;
        answerBox.quizManager = this;
    }
    
    IEnumerator GenerateSoalCoroutine()
    {
        // Pastikan game sudah dimulai
        if (!gameStarted)
        {
            yield break;
        }
        
        isProcessingAnswer = false;
        yield return null;
        GenerateSoal();
        
        yield return null;
        
        Debug.Log("Soal baru siap!");
    }
    
    public void GenerateSoal()
    {
        // Pastikan game sudah dimulai
        if (!gameStarted) return;
        
        int angka1 = Random.Range(minNumber, maxNumber + 1);
        int angka2 = Random.Range(minNumber, maxNumber + 1);
        string[] operators = { "+", "-", "×", "÷" };
        string operator_terpilih = operators[Random.Range(0, operators.Length)];
        switch (operator_terpilih)
        {
            case "+":
                jawaban_benar = angka1 + angka2;
                soal_sekarang = $"{angka1} + {angka2} = ?";
                break;
            case "-":
                if (angka1 < angka2)
                {
                    int temp = angka1;
                    angka1 = angka2;
                    angka2 = temp;
                }
                jawaban_benar = angka1 - angka2;
                soal_sekarang = $"{angka1} - {angka2} = ?";
                break;
            case "×":
                jawaban_benar = angka1 * angka2;
                soal_sekarang = $"{angka1} × {angka2} = ?";
                break;
            case "÷":
                angka1 = angka1 * angka2;
                jawaban_benar = angka1 / angka2;
                soal_sekarang = $"{angka1} ÷ {angka2} = ?";
                break;
        }
        List<int> semua_jawaban = GenerateJawabanSalah();
        posisi_jawaban_benar = Random.Range(0, 3);
        semua_jawaban[posisi_jawaban_benar] = jawaban_benar;
        semua_jawaban_current = new List<int>(semua_jawaban);
        UpdateUI();
        Debug.Log($"Soal: {soal_sekarang}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");
        Debug.Log($"Semua jawaban: [{semua_jawaban_current[0]}, {semua_jawaban_current[1]}, {semua_jawaban_current[2]}]");
    }
    
    void UpdateUI()
    {
        if (soalText != null)
        {
            soalText.text = soal_sekarang;
            soalText.ForceMeshUpdate(); 
        }
        if (jawabanKiriText != null && semua_jawaban_current != null && semua_jawaban_current.Count > 0)
        {
            jawabanKiriText.text = semua_jawaban_current[0].ToString();
            jawabanKiriText.ForceMeshUpdate();
        }
        if (jawabanTengahText != null && semua_jawaban_current != null && semua_jawaban_current.Count > 1)
        {
            jawabanTengahText.text = semua_jawaban_current[1].ToString();
            jawabanTengahText.ForceMeshUpdate();
        }
        if (jawabanKananText != null && semua_jawaban_current != null && semua_jawaban_current.Count > 2)
        {
            jawabanKananText.text = semua_jawaban_current[2].ToString();
            jawabanKananText.ForceMeshUpdate();
        }
        Canvas.ForceUpdateCanvases();
    }

    List<int> GenerateJawabanSalah()
    {
        List<int> jawaban_salah = new List<int>();
        while (jawaban_salah.Count < 3)
        {
            int jawaban_random;
            int range = Mathf.Max(5, jawaban_benar / 2);
            jawaban_random = Random.Range(
                Mathf.Max(0, jawaban_benar - range),
                jawaban_benar + range + 1
            );
            if (jawaban_random != jawaban_benar && !jawaban_salah.Contains(jawaban_random))
            {
                jawaban_salah.Add(jawaban_random);
            }
        }
        return jawaban_salah;
    }

    public void OnAnswerSelected(int boxIndex)
    {
        // Pastikan game sudah dimulai
        if (!gameStarted) return;
        
        if (isProcessingAnswer)
        {
            return;
        }
        if (semua_jawaban_current == null || boxIndex < 0 || boxIndex >= semua_jawaban_current.Count)
        {
            return;
        }
        isProcessingAnswer = true;
        int jawaban_dipilih = semua_jawaban_current[boxIndex];
        string posisi_nama = GetPosisiNama(boxIndex);
        Debug.Log($"=== HIT DETECTED ===");
        Debug.Log($"Menabrak box {posisi_nama} (index: {boxIndex}), jawaban: {jawaban_dipilih}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");

        if (boxIndex == posisi_jawaban_benar)
        {
            skor++;
            totalEarnings += coinPerCorrectAnswer;
            IncreaseSpeed(); // Tingkatkan kecepatan untuk jawaban benar
            Debug.Log($"BENAR! Skor: {skor}, Pendapatan: {totalEarnings}");
            StartCoroutine(GenerateSoalCoroutine());
        }
        else
        {
            Debug.Log("SALAH! GAME OVER!");
            Debug.Log($"Jawaban yang benar adalah: {jawaban_benar} ({GetPosisiNama(posisi_jawaban_benar)})");
            Debug.Log($"Skor akhir: {skor}");
            Debug.Log($"Pendapatan akhir: {totalEarnings}");
            ResetSpeed(); // Reset kecepatan saat game over
            isProcessingAnswer = false;
            GameOver();
        }
    }

    void GameOver()
    {
        gameStarted = false; // Stop game
        
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = false;
        }
        
        // Stop road animation
        if (roadAnimator != null)
        {
            roadAnimator.speed = 0f;
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score: {skor}";
            }
            if (earningsText != null)
            {
                earningsText.text = $"Total Score: {totalEarnings}";
            }
        }
        if (soalText != null)
        {
            soalText.text = $"GAME OVER! Skor: {skor}";
            soalText.ForceMeshUpdate();
        }
    }

    string GetPosisiNama(int posisi)
    {
        switch (posisi)
        {
            case 0: return "Kiri";
            case 1: return "Tengah";
            case 2: return "Kanan";
            default: return "Unknown";
        }
    }
    
    public void RestartGame()
    {
        skor = 0;
        totalEarnings = 0;
        isProcessingAnswer = false;
        gameStarted = false; // Reset game state
        ResetSpeed(); // Reset kecepatan saat restart
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = false; // Disable sampai countdown selesai
        }
        
        // Stop road animation
        if (roadAnimator != null)
        {
            roadAnimator.speed = 0f;
        }
        
        // Restart countdown
        if (countdownManager != null)
        {
            countdownManager.StartCountdown();
        }
        else
        {
            // Jika tidak ada countdown, langsung mulai
            StartGame();
        }
    }

    public void GoToHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }

    // Method untuk trigger manual countdown (jika diperlukan)
    public void TriggerCountdown()
    {
        if (countdownManager != null)
        {
            countdownManager.StartCountdown();
        }
    }
    
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"=== DEBUG STATE ===");
        Debug.Log($"Game Started: {gameStarted}");
        Debug.Log($"Soal: {soal_sekarang}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");
        
        if (semua_jawaban_current != null)
        {
            Debug.Log($"Jawaban di UI: [{semua_jawaban_current[0]}, {semua_jawaban_current[1]}, {semua_jawaban_current[2]}]");
        }
        
        Debug.Log($"UI Texts: [{jawabanKiriText?.text}, {jawabanTengahText?.text}, {jawabanKananText?.text}]");
        Debug.Log($"Is Processing: {isProcessingAnswer}");
        Debug.Log($"Skor: {skor}, Pendapatan: {totalEarnings}");
        Debug.Log($"Current Speed: {currentSpeed}, Consecutive Correct: {consecutiveCorrectAnswers}");
    }

    [ContextMenu("Test Increase Speed")]
    public void TestIncreaseSpeed()
    {
        IncreaseSpeed();
    }

    [ContextMenu("Test Reset Speed")]
    public void TestResetSpeed()
    {
        ResetSpeed();
    }
}