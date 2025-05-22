using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // untuk reload atau ke scene GameOver

public class SoalManager : MonoBehaviour
{
    public TextMeshProUGUI soalText;
    public TextMeshPro jawabanBoxKiri;
    public TextMeshPro jawabanBoxTengah;
    public TextMeshPro jawabanBoxKanan;

    private int jawabanBenar;

    public GameObject gameOverUI; // overlay Game Over (misalnya UI Canvas atau panel 3D)

    void Start()
    {
        GenerateSoalBaru();
    }

    public void CekJawaban(string jawabanStr)
    {
        if (int.TryParse(jawabanStr, out int jawaban))
        {
            if (jawaban == jawabanBenar)
            {
                Debug.Log("Jawaban Benar!");
                GenerateSoalBaru(); // generate soal baru
            }
            else
            {
                Debug.Log("Jawaban Salah. Game Over!");
                GameOver();
            }
        }
    }

    void GameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // munculkan overlay
        }
        // bisa juga: SceneManager.LoadScene("GameOverScene");
        Time.timeScale = 0f; // pause game
    }

    void GenerateSoalBaru()
    {
        int a = Random.Range(1, 11);
        int b = Random.Range(1, 11);
        int op = Random.Range(0, 4); // 0:+ 1:- 2:* 3:/
        string opStr = "+";
        int hasil = 0;

        switch (op)
        {
            case 0:
                hasil = a + b;
                opStr = "+";
                break;
            case 1:
                hasil = a - b;
                opStr = "-";
                break;
            case 2:
                hasil = a * b;
                opStr = "×";
                break;
            case 3:
                hasil = a;
                b = Random.Range(1, 10);
                hasil = a * b;
                soalText.text = $"{hasil} ÷ {b} = ?";
                jawabanBenar = hasil / b;
                goto SkipSet; // skip regular logic
        }

        soalText.text = $"{a} {opStr} {b} = ?";
        jawabanBenar = hasil;

    SkipSet:
        // random posisi jawaban
        int[] posisi = { 0, 1, 2 };
        Shuffle(posisi);

        TextMeshPro[] box = { jawabanBoxKiri, jawabanBoxTengah, jawabanBoxKanan };
        box[posisi[0]].text = jawabanBenar.ToString();
        box[posisi[1]].text = (jawabanBenar + Random.Range(1, 6)).ToString();
        box[posisi[2]].text = (jawabanBenar - Random.Range(1, 4)).ToString();
    }

    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(i, array.Length);
            int temp = array[rnd];
            array[rnd] = array[i];
            array[i] = temp;
        }
    }
}
