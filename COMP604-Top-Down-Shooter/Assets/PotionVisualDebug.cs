using UnityEngine;

public class PotionVisualDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"=== POTION DEBUG INFO ===");
        Debug.Log($"Name: {gameObject.name}");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Scale: {transform.localScale}");
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Debug.Log($"Has SpriteRenderer: YES");
            Debug.Log($"Sprite: {sr.sprite?.name ?? "NULL"}");
            Debug.Log($"Sprite Renderer Enabled: {sr.enabled}");
            Debug.Log($"Color: {sr.color}");
            Debug.Log($"Sorting Layer: {sr.sortingLayerName}");
            Debug.Log($"Order in Layer: {sr.sortingOrder}");
        }
        else
        {
            Debug.LogError($"NO SPRITE RENDERER FOUND!");
        }
        
        // Make sure we're visible - draw a debug sphere
        Debug.Log($"Potion should be visible at world position: {transform.position}");
    }
    
    void Update()
    {
        // Draw a visible debug sphere in Scene view
        Debug.DrawRay(transform.position, Vector3.up * 0.5f, Color.green);
        Debug.DrawRay(transform.position, Vector3.right * 0.5f, Color.red);
    }
}