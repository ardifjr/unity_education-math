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

    [Header("Scene Names")]
    public string homeSceneName = "HomeScene"; // Nama scene home

    private int jawaban_benar;
    private int posisi_jawaban_benar; // 0=kiri, 1=tengah, 2=kanan
    private string soal_sekarang;
    private int skor = 0;
    private int totalEarnings = 0; // Total pendapatan
    private List<int> semua_jawaban_current; // Menyimpan jawaban saat ini
    private bool isProcessingAnswer = false; // Flag untuk mencegah multiple hits

    void Start()
    {
        // Setup UI
        SetupUI();
        
        // Setup collision detection untuk setiap box
        SetupCollisionDetection();

        // Generate soal pertama dengan delay
        StartCoroutine(GenerateSoalCoroutine());
    }

    void SetupUI()
    {
        // Hide game over panel di awal
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Setup button listeners
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
        // Pastikan setiap box memiliki collider dan rigidbody
        SetupBoxCollider(boxKiri, 0);
        SetupBoxCollider(boxTengah, 1);
        SetupBoxCollider(boxKanan, 2);
    }

    void SetupBoxCollider(GameObject box, int boxIndex)
    {
        // Tambahkan BoxCollider jika belum ada
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

    // Coroutine untuk generate soal dengan delay
    IEnumerator GenerateSoalCoroutine()
    {
        // Reset flag
        isProcessingAnswer = false;
        
        // Wait satu frame untuk memastikan semua update selesai
        yield return null;
        
        GenerateSoal();
        
        // Wait lagi untuk memastikan UI ter-update
        yield return null;
        
        Debug.Log("Soal baru siap!");
    }

    public void GenerateSoal()
    {
        // Generate dua angka random
        int angka1 = Random.Range(minNumber, maxNumber + 1);
        int angka2 = Random.Range(minNumber, maxNumber + 1);

        // Pilih operator random
        string[] operators = { "+", "-", "×", "÷" };
        string operator_terpilih = operators[Random.Range(0, operators.Length)];

        // Hitung jawaban benar
        switch (operator_terpilih)
        {
            case "+":
                jawaban_benar = angka1 + angka2;
                soal_sekarang = $"{angka1} + {angka2} = ?";
                break;
            case "-":
                // Pastikan hasil tidak negatif
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
                // Pastikan pembagian bersisa 0
                angka1 = angka1 * angka2; // Membuat angka1 bisa dibagi angka2
                jawaban_benar = angka1 / angka2;
                soal_sekarang = $"{angka1} ÷ {angka2} = ?";
                break;
        }

        // Generate jawaban salah
        List<int> semua_jawaban = GenerateJawabanSalah();

        // Acak posisi jawaban benar
        posisi_jawaban_benar = Random.Range(0, 3);
        semua_jawaban[posisi_jawaban_benar] = jawaban_benar;

        // Simpan jawaban saat ini untuk referensi
        semua_jawaban_current = new List<int>(semua_jawaban);

        // Update UI dengan force refresh
        UpdateUI();

        Debug.Log($"Soal: {soal_sekarang}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");
        Debug.Log($"Semua jawaban: [{semua_jawaban_current[0]}, {semua_jawaban_current[1]}, {semua_jawaban_current[2]}]");
    }

    void UpdateUI()
    {
        // Update soal di UI
        if (soalText != null)
        {
            soalText.text = soal_sekarang;
            soalText.ForceMeshUpdate(); // Force update untuk TMP
        }

        // Update jawaban di UI dengan null check
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

        // Force canvas update jika menggunakan Canvas
        Canvas.ForceUpdateCanvases();
    }

    List<int> GenerateJawabanSalah()
    {
        List<int> jawaban_salah = new List<int>();

        // Generate 2 jawaban salah yang berbeda dari jawaban benar
        while (jawaban_salah.Count < 3)
        {
            int jawaban_random;

            // Generate jawaban salah dengan range yang masuk akal
            int range = Mathf.Max(5, jawaban_benar / 2);
            jawaban_random = Random.Range(
                Mathf.Max(0, jawaban_benar - range),
                jawaban_benar + range + 1
            );

            // Pastikan tidak sama dengan jawaban benar dan belum ada dalam list
            if (jawaban_random != jawaban_benar && !jawaban_salah.Contains(jawaban_random))
            {
                jawaban_salah.Add(jawaban_random);
            }
        }

        return jawaban_salah;
    }

    public void OnAnswerSelected(int boxIndex)
    {
        // Cegah multiple hits dalam waktu bersamaan
        if (isProcessingAnswer)
        {
            Debug.Log("Masih memproses jawaban sebelumnya, mengabaikan hit ini");
            return;
        }

        // Validasi data
        if (semua_jawaban_current == null || boxIndex < 0 || boxIndex >= semua_jawaban_current.Count)
        {
            Debug.LogError($"Data tidak valid! boxIndex: {boxIndex}, jawaban count: {(semua_jawaban_current?.Count ?? 0)}");
            return;
        }

        // Set flag processing
        isProcessingAnswer = true;

        // Dapatkan nilai jawaban yang dipilih
        int jawaban_dipilih = semua_jawaban_current[boxIndex];
        string posisi_nama = GetPosisiNama(boxIndex);

        // Print informasi box yang ditabrak dan nilainya
        Debug.Log($"=== HIT DETECTED ===");
        Debug.Log($"Menabrak box {posisi_nama} (index: {boxIndex}), jawaban: {jawaban_dipilih}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");

        if (boxIndex == posisi_jawaban_benar)
        {
            // Jawaban benar
            skor++;
            totalEarnings += coinPerCorrectAnswer; // Tambah pendapatan
            Debug.Log($"BENAR! Skor: {skor}, Pendapatan: {totalEarnings}");

            // Generate soal baru dengan delay
            StartCoroutine(GenerateSoalCoroutine());
        }
        else
        {
            // Jawaban salah - Game Over
            Debug.Log("SALAH! GAME OVER!");
            Debug.Log($"Jawaban yang benar adalah: {jawaban_benar} ({GetPosisiNama(posisi_jawaban_benar)})");
            Debug.Log($"Skor akhir: {skor}");
            Debug.Log($"Pendapatan akhir: {totalEarnings}");

            // Reset flag
            isProcessingAnswer = false;
            
            // Game over
            GameOver();
        }
    }

    void GameOver()
    {
        // Disable car movement atau logic game over lainnya
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = false;
        }

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update final score text
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score: {skor}";
            }
            
            // Update earnings text
            if (earningsText != null)
            {
                earningsText.text = $"Total Score: {totalEarnings}";
            }
        }

        // Update soal text juga sebagai backup
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

    // Method untuk restart game
    public void RestartGame()
    {
        // Reset semua variable
        skor = 0;
        totalEarnings = 0;
        isProcessingAnswer = false;

        // Hide game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Enable car movement
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = true;
        }

        // Generate soal baru
        StartCoroutine(GenerateSoalCoroutine());
        
        Debug.Log("Game restarted!");
    }

    // Method untuk kembali ke home
    public void GoToHome()
    {
        Debug.Log("Going to home scene...");
        
        // Load scene home
        SceneManager.LoadScene(homeSceneName);
    }

    // Method untuk debugging - panggil dari inspector atau script lain
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"=== DEBUG STATE ===");
        Debug.Log($"Soal: {soal_sekarang}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");
        
        if (semua_jawaban_current != null)
        {
            Debug.Log($"Jawaban di UI: [{semua_jawaban_current[0]}, {semua_jawaban_current[1]}, {semua_jawaban_current[2]}]");
        }
        
        Debug.Log($"UI Texts: [{jawabanKiriText?.text}, {jawabanTengahText?.text}, {jawabanKananText?.text}]");
        Debug.Log($"Is Processing: {isProcessingAnswer}");
        Debug.Log($"Skor: {skor}, Pendapatan: {totalEarnings}");
    }

    // Method untuk testing (bisa dihapus jika tidak diperlukan)
    [ContextMenu("Test Game Over")]
    public void TestGameOver()
    {
        skor = 5; // Set score untuk testing
        totalEarnings = 50; // Set earnings untuk testing
        GameOver();
    }
}