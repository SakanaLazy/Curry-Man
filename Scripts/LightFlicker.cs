using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minIntensity = 6f;
    public float maxIntensity = 8f;
    public float flickerSpeed = 0.1f; // lower = faster flicker
    public float smoothness = 0.2f;   // higher = smoother transitions

    private Light lamp;
    private float targetIntensity;
    private float timer;

    void Start()
    {
        lamp = GetComponent<Light>();
        if (lamp == null)
        {
            Debug.LogWarning($"{name} has no Light component!");
            enabled = false;
            return;
        }

        targetIntensity = lamp.intensity;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flickerSpeed)
        {
            timer = 0f;
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }

        lamp.intensity = Mathf.Lerp(lamp.intensity, targetIntensity, Time.deltaTime / smoothness);
    }
}
