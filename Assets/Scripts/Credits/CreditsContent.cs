using UnityEngine;
using TMPro;

public class CreditsContent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;
    
    [TextArea(10, 30)]
    [SerializeField] private string gameTitle = "YOUR GAME TITLE";
    
    [TextArea(10, 30)]
    [SerializeField] private string teamMembers = 
        "Lead Developer: Your Name\n" +
        "Artist: Artist Name\n" +
        "Sound Designer: Sound Designer Name";
    
    [TextArea(10, 30)]
    [SerializeField] private string specialThanks = 
        "Special Thanks To:\n" +
        "Family & Friends\n" +
        "Unity Technologies\n" +
        "Your Supporters";
    
    [TextArea(10, 30)]
    [SerializeField] private string thirdPartyAssets = 
        "Third-Party Assets:\n" +
        "Asset Name 1 - Creator\n" +
        "Asset Name 2 - Creator";
    
    [TextArea(10, 30)]
    [SerializeField] private string copyright = "© 2025 Your Studio Name\nAll Rights Reserved";
    
    private void Start()
    {
        if (creditsText != null)
        {
            // Combine all sections with spacing Add \n\n\n{thirdPartyAssets} between special thanks to copyright if needed
            creditsText.text = $"{gameTitle}\n\n\n{teamMembers}\n\n\n{specialThanks}\n\n\n{copyright}";
        }
    }
}