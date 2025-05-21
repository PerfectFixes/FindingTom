using UnityEngine;

public class ConfettiEffect : MonoBehaviour
{
    [Header("Confetti Settings")]
    [Range(1f, 45f)]
    public float coneAngle = 5f; // How wide the confetti spreads

    private ParticleSystem confettiTemplate;

    void Start()
    {
        confettiTemplate = CreateConfettiFX();
        confettiTemplate.gameObject.SetActive(false); // Hide the template
    }

    public void PlayConfetti(Vector3 position)
    {
        // Instantiate a new copy at runtime
        GameObject newConfetti = Instantiate(confettiTemplate.gameObject, position, Quaternion.identity);
        newConfetti.SetActive(true);

        ParticleSystem ps = newConfetti.GetComponent<ParticleSystem>();
        ps.Play();

        Destroy(newConfetti, 5f);
    }

    private ParticleSystem CreateConfettiFX()
    {
        GameObject confettiGO = new GameObject("ConfettiFX_Template");
        ParticleSystem confettiFX = confettiGO.AddComponent<ParticleSystem>();
        confettiFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = confettiFX.main;
        main.duration = 2f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 5.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0, 360);
        main.startRotationY = new ParticleSystem.MinMaxCurve(0, 360);
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0, 360);
        main.gravityModifier = 0.7f;
        main.maxParticles = 100;

        var emission = confettiFX.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 70)
        });

        var shape = confettiFX.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = coneAngle; // 🎯 Use the inspector-controlled value
        shape.radius = 0.2f;
        shape.rotation = new Vector3(-90, 0, 0);

        var colorOverLifetime = confettiFX.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                /*new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.yellow, 0.25f),*/
                new GradientColorKey(Color.green, 0.25f),
                new GradientColorKey(Color.blue, 0.5f),
                new GradientColorKey(Color.magenta, 0.75f),
                new GradientColorKey(Color.yellow, 1.25f),
                new GradientColorKey(Color.red, 1f),
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = confettiGO.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        return confettiFX;
    }
}
