using UnityEngine;

public class AnswerBox : MonoBehaviour
{
    [HideInInspector]
    public int boxIndex; // 0=kiri, 1=tengah, 2=kanan
    [HideInInspector]
    public MathQuizManager quizManager;
    
    void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang menabrak adalah mobil (car)
        if (other.gameObject.GetComponent<Car>() != null)
        {
            Debug.Log($"Mobil menabrak box {GetBoxName()}!");
            
            // Panggil method di QuizManager
            if (quizManager != null)
            {
                quizManager.OnAnswerSelected(boxIndex);
            }
        }
    }
    
    string GetBoxName()
    {
        switch (boxIndex)
        {
            case 0: return "Kiri";
            case 1: return "Tengah";
            case 2: return "Kanan";
            default: return "Unknown";
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualisasi collider di Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
    }
}