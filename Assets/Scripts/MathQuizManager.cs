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

    [Header("Audio Settings")]
    public AudioSource backgroundMusicSource; // AudioSource untuk background music
    public AudioSource gameOverSoundSource; // AudioSource untuk sound effect game over
    public AudioClip gameOverClip; // AudioClip untuk game over sound

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

    [Header("Initial Positions")]
    public Vector3 initialCarPosition;
    public Vector3 initialBoxKiriPosition;
    public Vector3 initialBoxTengahPosition;
    public Vector3 initialBoxKananPosition;

    private int jawaban_benar;
    private int posisi_jawaban_benar; // 0=kiri, 1=tengah, 2=kanan
    private string soal_sekarang;
    private int skor = 0;
    private int totalEarnings = 0; // Total pendapatan
    private List<int> semua_jawaban_current; // Menyimpan jawaban saat ini
    private bool isProcessingAnswer = false; // Flag untuk mencegah multiple hits
    private bool gameStarted = false; // Flag untuk mengetahui apakah game sudah dimulai
    private bool isGameOver = false; // Flag untuk status game over

    // Speed control variables
    private int consecutiveCorrectAnswers = 0; // Jawaban benar berturut-turut
    private float currentSpeed; // Kecepatan saat ini

    void SaveInitialPositions()
    {
        // Simpan posisi awal mobil
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            initialCarPosition = carScript.transform.position;
        }

        // Simpan posisi awal box jawaban
        if (boxKiri != null)
            initialBoxKiriPosition = boxKiri.transform.position;
        if (boxTengah != null)
            initialBoxTengahPosition = boxTengah.transform.position;
        if (boxKanan != null)
            initialBoxKananPosition = boxKanan.transform.position;

        Debug.Log("Initial positions saved");
    }

    void Start()
    {
        SetupUI();
        SetupCollisionDetection();
        InitializeSpeedControl();
        ResetGameVariables();
        SaveInitialPositions(); // TAMBAHKAN INI

        if (countdownManager != null)
        {
            StartCoroutine(AutoStartCountdown());
        }
        else
        {
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
        isGameOver = false;
        Debug.Log("Math Quiz Game Started!");
        InitializeSpeedControl();

        // Mulai background music jika ada
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Play();
        }

        // Enable car control
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = true;
        }

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
        if (roadAnimator != null && !isGameOver)
        {
            roadAnimator.SetFloat(speedParameterName, currentSpeed);
            roadAnimator.speed = 1f; // Pastikan animator speed = 1 agar parameter bisa bekerja

            Debug.Log($"Animator speed updated - Parameter: {speedParameterName} = {currentSpeed}, Speed property = 1f");

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
        else if (isGameOver)
        {
            Debug.Log("Game over - tidak mengupdate animator speed");
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

    void ResetGameVariables()
    {
        // Reset semua variable game
        jawaban_benar = 0;
        posisi_jawaban_benar = 0;
        soal_sekarang = "";
        skor = 0;
        totalEarnings = 0;
        semua_jawaban_current = null;
        isProcessingAnswer = false;
        consecutiveCorrectAnswers = 0;
        currentSpeed = baseSpeed;

        Debug.Log("All game variables reset");
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
        // Pastikan game sudah dimulai dan tidak game over
        if (!gameStarted || isGameOver)
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
        // Pastikan game sudah dimulai dan tidak game over
        if (!gameStarted || isGameOver) return;

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
        // Pastikan game sudah dimulai dan tidak game over
        if (!gameStarted || isGameOver) return;

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
            isProcessingAnswer = false;
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        gameStarted = false;

        // Stop background music
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
            Debug.Log("Background music stopped");
        }

        // Play game over sound
        if (gameOverSoundSource != null && gameOverClip != null)
        {
            gameOverSoundSource.PlayOneShot(gameOverClip);
            Debug.Log("Game over sound played");
        }

        // Disable car control
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = false;
            Debug.Log("Car control disabled");
        }

        // Stop road animation completely
        if (roadAnimator != null)
        {
            roadAnimator.speed = 0f;
            roadAnimator.SetFloat(speedParameterName, 0f);
            Debug.Log("Road animation stopped completely");
        }

        // Stop all other animations in scene
        StopAllAnimations();

        // UBAH INI: Gunakan animasi untuk show game over panel
        StartCoroutine(ShowGameOverPanel());

        if (soalText != null)
        {
            soalText.text = $"GAME OVER! Skor: {skor}";
            soalText.ForceMeshUpdate();
        }

        Debug.Log("=== GAME OVER - ALL ACTIVITIES STOPPED ===");
    }
    void ResetCarPosition()
    {
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            // Gunakan posisi awal yang tersimpan
            carScript.transform.position = initialCarPosition;

            // Jika ada Rigidbody, reset velocity
            Rigidbody carRb = carScript.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                carRb.linearVelocity = Vector3.zero;
                carRb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"Car position reset to initial: {initialCarPosition}");
        }
    }
    void ResetAnswerBoxPositions()
    {
        if (boxKiri != null)
        {
            boxKiri.transform.position = initialBoxKiriPosition;
            Debug.Log($"Box Kiri reset to: {initialBoxKiriPosition}");
        }

        if (boxTengah != null)
        {
            boxTengah.transform.position = initialBoxTengahPosition;
            Debug.Log($"Box Tengah reset to: {initialBoxTengahPosition}");
        }

        if (boxKanan != null)
        {
            boxKanan.transform.position = initialBoxKananPosition;
            Debug.Log($"Box Kanan reset to: {initialBoxKananPosition}");
        }
    }

    void StopAllAnimations()
    {
        // Stop semua animator di scene kecuali UI
        Animator[] allAnimators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator animator in allAnimators)
        {
            // Skip UI animators (biasanya ada di Canvas)
            if (animator.gameObject.GetComponentInParent<Canvas>() == null)
            {
                animator.speed = 0f;
                Debug.Log($"Stopped animator on: {animator.gameObject.name}");
            }
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
        // Reset semua variable
        ResetGameVariables();
        ResetSpeed();
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Stop game over sound jika masih playing
        if (gameOverSoundSource != null && gameOverSoundSource.isPlaying)
        {
            gameOverSoundSource.Stop();
        }

        // Reset posisi mobil ke posisi awal
        ResetCarPosition();

        // Reset posisi box jawaban ke posisi awal
        ResetAnswerBoxPositions();

        // Clear UI
        if (soalText != null)
        {
            soalText.text = "";
            soalText.ForceMeshUpdate();
        }

        if (jawabanKiriText != null)
        {
            jawabanKiriText.text = "";
            jawabanKiriText.ForceMeshUpdate();
        }

        if (jawabanTengahText != null)
        {
            jawabanTengahText.text = "";
            jawabanTengahText.ForceMeshUpdate();
        }

        if (jawabanKananText != null)
        {
            jawabanKananText.text = "";
            jawabanKananText.ForceMeshUpdate();
        }

        // Disable car initially
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = false;
        }

        // Stop road animation temporarily
        if (roadAnimator != null)
        {
            roadAnimator.speed = 0f;
            roadAnimator.SetFloat(speedParameterName, 0f);
        }

        // Restart all animations to initial state
        RestartAllAnimations();

        // Restart countdown
        if (countdownManager != null)
        {
            countdownManager.StartCountdown();
        }
        else
        {
            StartGame();
        }

        Debug.Log("=== GAME RESTARTED - ALL OBJECTS RESET TO INITIAL POSITIONS ===");
    }

    void RestartAllAnimations()
    {
        Animator[] allAnimators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator animator in allAnimators)
        {
            // Skip UI animators
            if (animator.gameObject.GetComponentInParent<Canvas>() == null)
            {
                // Reset animator ke state awal
                animator.Rebind();
                animator.speed = 1f;

                // Jika ini road animator, set ke base speed
                if (animator == roadAnimator)
                {
                    animator.speed = 0f; // Akan diatur ulang saat game start
                }

                Debug.Log($"Restarted animator on: {animator.gameObject.name}");
            }
        }
    }

    IEnumerator ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            // Set scale ke 0 terlebih dahulu
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.SetActive(true);

            // Animasi scale dari 0 ke 1 dalam 0.5 detik dengan bounce effect
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Bounce effect dengan overshoot
                float scale = Mathf.Lerp(0f, 1.1f, progress);
                if (progress > 0.8f)
                {
                    scale = Mathf.Lerp(1.1f, 1f, (progress - 0.8f) / 0.2f);
                }

                gameOverPanel.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            gameOverPanel.transform.localScale = Vector3.one;

            // Mulai animasi score setelah panel muncul
            StartCoroutine(AnimateScoreText());
        }
    }
    IEnumerator AnimateScoreText()
    {
        if (finalScoreText != null && earningsText != null)
        {
            float duration = 2f;
            float elapsed = 0f;
            int startScore = 0;
            int targetScore = skor;
            int startEarnings = 0;
            int targetEarnings = totalEarnings;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Interpolasi dengan smooth curve
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                int currentScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, smoothProgress));
                int currentEarnings = Mathf.RoundToInt(Mathf.Lerp(startEarnings, targetEarnings, smoothProgress));

                finalScoreText.text = $"Score: {currentScore}";
                earningsText.text = $"Total Score: {currentEarnings}";

                yield return null;
            }

            // Pastikan nilai akhir tepat
            finalScoreText.text = $"Score: {targetScore}";
            earningsText.text = $"Total Score: {targetEarnings}";
        }
    }

    public void GoToHome()
    {
        // Stop all sounds before changing scene
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
        }

        if (gameOverSoundSource != null)
        {
            gameOverSoundSource.Stop();
        }

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
        Debug.Log($"Is Game Over: {isGameOver}");
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

    [ContextMenu("Test Game Over")]
    public void TestGameOver()
    {
        GameOver();
    }
}
