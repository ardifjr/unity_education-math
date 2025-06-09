using UnityEngine;

public class SlowBarrel : MonoBehaviour
{
    [HideInInspector]
    public int barrelIndex; // 0=kiri, 1=tengah, 2=kanan
    
    [HideInInspector]
    public MathQuizManager quizManager;

    void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang nabrak adalah mobil (Car)
        Car car = other.GetComponent<Car>();
        if (car != null && quizManager != null)
        {
            Debug.Log($"Car hit barrel at index: {barrelIndex}");
            quizManager.OnBarrelHit(barrelIndex);
        }
    }
}