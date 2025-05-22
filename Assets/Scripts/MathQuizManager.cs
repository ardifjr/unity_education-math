using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    
    [Header("Game Settings")]
    public int minNumber = 1;
    public int maxNumber = 10;
    
    private int jawaban_benar;
    private int posisi_jawaban_benar; // 0=kiri, 1=tengah, 2=kanan
    private string soal_sekarang;
    private int skor = 0;
    
    void Start()
    {
        // Setup collision detection untuk setiap box
        SetupCollisionDetection();
        
        // Generate soal pertama
        GenerateSoal();
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
        
        // Update soal di UI
        soalText.text = soal_sekarang;
        
        // Generate jawaban salah
        List<int> semua_jawaban = GenerateJawabanSalah();
        
        // Acak posisi jawaban benar
        posisi_jawaban_benar = Random.Range(0, 3);
        semua_jawaban[posisi_jawaban_benar] = jawaban_benar;
        
        // Update jawaban di UI
        jawabanKiriText.text = semua_jawaban[0].ToString();
        jawabanTengahText.text = semua_jawaban[1].ToString();
        jawabanKananText.text = semua_jawaban[2].ToString();
        
        Debug.Log($"Soal: {soal_sekarang}");
        Debug.Log($"Jawaban benar: {jawaban_benar} (posisi: {GetPosisiNama(posisi_jawaban_benar)})");
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
        if (boxIndex == posisi_jawaban_benar)
        {
            // Jawaban benar
            skor++;
            Debug.Log($"Benar! Skor: {skor}");
            
            // Generate soal baru
            GenerateSoal();
        }
        else
        {
            // Jawaban salah - Game Over
            Debug.Log("GAME OVER!");
            Debug.Log($"Jawaban yang benar adalah: {jawaban_benar} ({GetPosisiNama(posisi_jawaban_benar)})");
            Debug.Log($"Skor akhir: {skor}");
            
            // Bisa tambahkan logic game over di sini
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
        
        // Bisa tambahkan UI game over, restart button, dll
        soalText.text = $"GAME OVER! Skor: {skor}";
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
        skor = 0;
        
        // Enable car movement
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = true;
        }
        
        GenerateSoal();
    }
}