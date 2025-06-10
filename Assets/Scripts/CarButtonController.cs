using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarButtonController : MonoBehaviour
{
    [Header("Car Parent Reference")]
    public GameObject carParent; // Drag parent Car GameObject (yang kosong) ke sini
    
    [Header("Button References")]
    public Button leftButton;
    public Button rightButton;
    
    void Start()
    {
        // Setup button events
        SetupButton(leftButton, () => {
            Car activeCar = GetActiveCar();
            if (activeCar != null) activeCar.moveLeft = true;
        }, () => {
            Car activeCar = GetActiveCar();
            if (activeCar != null) activeCar.moveLeft = false;
        });
        
        SetupButton(rightButton, () => {
            Car activeCar = GetActiveCar();
            if (activeCar != null) activeCar.moveRight = true;
        }, () => {
            Car activeCar = GetActiveCar();
            if (activeCar != null) activeCar.moveRight = false;
        });
    }
    
    // Method untuk mencari mobil yang sedang aktif
    Car GetActiveCar()
    {
        if (carParent == null) return null;
        
        // Loop semua child objects
        foreach (Transform child in carParent.transform)
        {
            // Cek jika child aktif dan punya script Car
            if (child.gameObject.activeInHierarchy)
            {
                Car carScript = child.GetComponent<Car>();
                if (carScript != null)
                {
                    return carScript;
                }
            }
        }
        
        return null;
    }
    
    void SetupButton(Button button, System.Action onPress, System.Action onRelease)
    {
        if (button == null) return;
        
        // Add EventTrigger component jika belum ada
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Setup Pointer Down event (ketika button ditekan)
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { onPress?.Invoke(); });
        trigger.triggers.Add(pointerDownEntry);
        
        // Setup Pointer Up event (ketika button dilepas)
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { onRelease?.Invoke(); });
        trigger.triggers.Add(pointerUpEntry);
        
        // Setup Pointer Exit event (ketika cursor keluar dari button saat ditekan)
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { onRelease?.Invoke(); });
        trigger.triggers.Add(pointerExitEntry);
    }
    
    // Method alternatif untuk dipanggil dari button onClick jika diperlukan
    public void OnLeftButtonDown()
    {
        Car activeCar = GetActiveCar();
        if (activeCar != null) activeCar.moveLeft = true;
    }
    
    public void OnLeftButtonUp()
    {
        Car activeCar = GetActiveCar();
        if (activeCar != null) activeCar.moveLeft = false;
    }
    
    public void OnRightButtonDown()
    {
        Car activeCar = GetActiveCar();
        if (activeCar != null) activeCar.moveRight = true;
    }
    
    public void OnRightButtonUp()
    {
        Car activeCar = GetActiveCar();
        if (activeCar != null) activeCar.moveRight = false;
    }
}