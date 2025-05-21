using UnityEngine;

public class ErrorEffect : MonoBehaviour
{
    [Header("Error FX Settings")]
    [Range(1f, 45f)]
    public float coneAngle = 10f; // Spread control

    private ParticleSystem errorTemplate;

    void Start()
    {
        errorTemplate = CreateErrorFX();
        errorTemplate.gameObject.SetActive(false); // Hide the template
    }

    public void PlayError(Vector3 position)
    {
        GameObject newError = Instantiate(errorTemplate.gameObject, position, Quaternion.identity);
        newError.SetActive(true);

        ParticleSystem ps = newError.GetComponent<ParticleSystem>();
        ps.Play();

        Destroy(newError, 6f); // Let it linger and clean up
    }

    ParticleSystem CreateErrorFX()
    {
        GameObject errorGO = new GameObject("ErrorFX_Template");
        ParticleSystem errorFX = errorGO.AddComponent<ParticleSystem>();
        errorFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = errorFX.main;
        main.duration = 3f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.8f, 0f, 0f), new Color(0.2f, 0f, 0f));
        main.gravityModifier = 0.5f;
        main.maxParticles = 50;

        // Enable 3D rotation
        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 0.5f); // Slower rotation
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 0.5f); // Slower rotation
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 0.5f); // Slower rotation

        var emission = errorFX.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 10, 20)
        });

        var shape = errorFX.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle;
        shape.radius = 0.2f;
        shape.rotation = new Vector3(-90, 0, 0);

        var colorOverLifetime = errorFX.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.1f, 0.1f), 0f),
                new GradientColorKey(new Color(0.1f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = errorFX.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Slow down angular velocity (rotation over time)
        var rotationOverLifetime = errorFX.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.separateAxes = true;
        rotationOverLifetime.x = new ParticleSystem.MinMaxCurve(-10f, 10f); 
        rotationOverLifetime.y = new ParticleSystem.MinMaxCurve(-10f, 10f); 
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-10f, 10f); 

        var renderer = errorGO.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        return errorFX;
    }
}
