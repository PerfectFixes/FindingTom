using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class PortalTouchGlow : MonoBehaviour
{
    [Header("Glow Effect")]
    public float glowDuration = 1f;
    public float maxRadius = 1f;
    public float glowWidth = 0.1f;
    public Color glowColor = Color.cyan;
    public float glowDistortionStrength = 0.06f;

    [Header("Base Ripple Settings")]
    public float baseDistortionStrength = 0.03f;
    public float baseRippleFrequency = 30f;
    public float baseRippleSpeed = 2f;

    [Header("Hit Ripple Boost (on Player touch)")]
    public float hitDistortionStrength = 0.1f;
    public float hitRippleFrequency = 10f;
    public float hitRippleSpeed = 6f;

    [Header("Base Color")]
    public Color baseColor = new Color(0.7f, 0.9f, 1f, 0.25f);
    public Color glowStartColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Ripple Reset Settings")]
    public float rippleResetDelay = 2f;
    public float rippleResetLerpTime = 1f;

    private Material mat;
    private float timer;
    private bool isGlowing;

    private bool shouldResetRipples = false;
    private float rippleResetTimer = 0f;
    private bool isLerpingRipples = false;
    private float rippleLerpElapsed = 0f;

    private float startDistortion;
    private float startFrequency;
    private float startSpeed;

    void Start()
    {
        mat = GetComponent<Renderer>().material;

        mat.SetFloat("_GlowRadius", 0f);
        mat.SetFloat("_GlowWidth", glowWidth);
        mat.SetColor("_GlowColor", glowColor);
        mat.SetFloat("_GlowDistortionStrength", glowDistortionStrength);

        mat.SetFloat("_DistortionStrength", baseDistortionStrength);
        mat.SetFloat("_RippleFrequency", baseRippleFrequency);
        mat.SetFloat("_RippleSpeed", baseRippleSpeed);

        mat.SetColor("_BaseColor", baseColor);
        mat.SetVector("_TouchPosition", new Vector2(10f, 10f));

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (isGlowing)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / glowDuration);

            float radius = (t < 0.5f) ? Mathf.Lerp(0f, maxRadius, t / 0.5f) : Mathf.Lerp(maxRadius, 0f, (t - 0.5f) / 0.5f);
            mat.SetFloat("_GlowRadius", radius);

            Color colorFade = (t < 0.5f)
                ? Color.Lerp(baseColor, glowStartColor, t / 0.5f)
                : Color.Lerp(glowStartColor, baseColor, (t - 0.5f) / 0.5f);
            mat.SetColor("_BaseColor", colorFade);

            if (t >= 1f)
            {
                isGlowing = false;
                timer = 0f;

                mat.SetVector("_TouchPosition", new Vector2(10f, 10f));
                mat.SetColor("_BaseColor", baseColor);
                mat.SetFloat("_GlowRadius", 0f);

                shouldResetRipples = true;
                rippleResetTimer = 0f;
            }
        }

        if (shouldResetRipples)
        {
            rippleResetTimer += Time.deltaTime;
            if (rippleResetTimer >= rippleResetDelay)
            {
                shouldResetRipples = false;
                isLerpingRipples = true;
                rippleLerpElapsed = 0f;

                // Store current state to lerp from
                startDistortion = mat.GetFloat("_DistortionStrength");
                startFrequency = mat.GetFloat("_RippleFrequency");
                startSpeed = mat.GetFloat("_RippleSpeed");
            }
        }

        if (isLerpingRipples)
        {
            rippleLerpElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(rippleLerpElapsed / rippleResetLerpTime);

            float lerpedDistortion = Mathf.Lerp(startDistortion, baseDistortionStrength, t);
            float lerpedFrequency = Mathf.Lerp(startFrequency, baseRippleFrequency, t);
            float lerpedSpeed = Mathf.Lerp(startSpeed, baseRippleSpeed, t);

            mat.SetFloat("_DistortionStrength", lerpedDistortion);
            mat.SetFloat("_RippleFrequency", lerpedFrequency);
            mat.SetFloat("_RippleSpeed", lerpedSpeed);

            if (t >= 1f)
            {
                isLerpingRipples = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mat.SetVector("_TouchPosition", new Vector2(0.5f, 0.5f));
            timer = 0f;
            isGlowing = true;

            mat.SetFloat("_DistortionStrength", hitDistortionStrength);
            mat.SetFloat("_RippleFrequency", hitRippleFrequency);
            mat.SetFloat("_RippleSpeed", hitRippleSpeed);
        }
    }
}
