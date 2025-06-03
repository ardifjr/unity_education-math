using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MathQuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI soalText;
    public TextMeshProUGUI skorText; // Menampilkan skor selama permainan
    public TextMeshPro jawabanKiriText;
    public TextMeshPro jawabanTengahText;
    public TextMeshPro jawabanKananText;

    [Header("Answer Box Objects")]
    public GameObject boxKiri;
    public GameObject boxTengah;
    public GameObject boxKanan;

    [Header("Game Settings")]
    public int minNumber = 1;
    public int maxNumber = 20;

    private int jawaban_benar;
    private int posisi_jawaban_benar;
    private string soal_sekarang;
    private int skor = 0;
    private List<int> semua_jawaban_current;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI skorAkhirText; // Tambahan untuk menampilkan skor akhir

    void Start()
    {
        SetupCollisionDetection();
        GenerateSoal();
        UpdateSkorUI();
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
            box.AddComponent<BoxCollider>();

        box.GetComponent<BoxCollider>().isTrigger = true;

        AnswerBox answerBox = box.GetComponent<AnswerBox>();
        if (answerBox == null)
            answerBox = box.AddComponent<AnswerBox>();

        answerBox.boxIndex = boxIndex;
        answerBox.quizManager = this;
    }

    public void GenerateSoal()
    {
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

        soalText.text = soal_sekarang;

        List<int> semua_jawaban = GenerateJawabanSalah();
        posisi_jawaban_benar = Random.Range(0, 3);
        semua_jawaban[posisi_jawaban_benar] = jawaban_benar;
        semua_jawaban_current = new List<int>(semua_jawaban);

        jawabanKiriText.text = semua_jawaban[0].ToString();
        jawabanTengahText.text = semua_jawaban[1].ToString();
        jawabanKananText.text = semua_jawaban[2].ToString();
    }

    List<int> GenerateJawabanSalah()
    {
        List<int> jawaban_salah = new List<int>();

        while (jawaban_salah.Count < 3)
        {
            int range = Mathf.Max(5, jawaban_benar / 2);
            int jawaban_random = Random.Range(
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
        int jawaban_dipilih = semua_jawaban_current[boxIndex];

        if (boxIndex == posisi_jawaban_benar)
        {
            skor++;
            UpdateSkorUI();
            GenerateSoal();
        }
        else
        {
            GameOver();
        }
    }

    void UpdateSkorUI()
    {
        if (skorText != null)
            skorText.text = $"Skor: {skor}";
    }

    void GameOver()
    {
        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
        {
            carScript.enabled = false;
        }

        soalText.text = $"GAME OVER!";

        if (skorAkhirText != null)
            skorAkhirText.text = $"Skor Akhir: {skor}";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        skor = 0;

        Car carScript = FindFirstObjectByType<Car>();
        if (carScript != null)
            carScript.enabled = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateSkorUI();
        GenerateSoal();
    }

    public void BackToHome()
    {
        SceneManager.LoadScene("home"); // Ganti dengan nama scene kamu
    }
}
