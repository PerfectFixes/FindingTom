/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuLogicDep : MonoBehaviour
{
    [System.Serializable]
    public class MenuButtonData
    {
        public string buttonName;
        public Button buttonReference;
        public MenuAction actionType;
        public string targetMenuGroup = "";
        public string targetSceneName = "";
        
        public enum MenuAction
        {
            OpenMenu,
            LoadScene,
            ExitGame,
            Custom
        }
    }

    [Header("Menu Navigation")]
    [SerializeField] private MenuNavigationController menuNavController;
    
    [Header("Scene Loading")]
    [SerializeField] private string mainGameSceneName = "Main Game Scene";
    [SerializeField] private Object mainGameScene; // Optional: Scene asset reference
    [SerializeField] private bool preloadMainScene = true;
    
    [Header("Scene Transition")]
    [SerializeField] private bool useTransition = false;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1.0f;
    [SerializeField] private string transitionTriggerName = "StartTransition";
    
    [Header("Menu Configuration")]
    [SerializeField] private string initialMenuGroup = "MainMenu";
    [SerializeField] private List<MenuButtonData> menuButtons = new List<MenuButtonData>();
    
    [Header("Menu Panels")]
    [SerializeField] private List<MenuNavigationController.MenuGroup> menuGroups = new List<MenuNavigationController.MenuGroup>();
    
    // Dictionary for quick lookup of menu groups by name
    private Dictionary<string, MenuNavigationController.MenuGroup> menuGroupDict = new Dictionary<string, MenuNavigationController.MenuGroup>();
    
    // Scene loading operation
    private AsyncOperation sceneLoadOperation;
    
    // Delegate for custom button actions
    public delegate void CustomButtonAction(string buttonName);
    public CustomButtonAction OnCustomButtonPressed;
    
    private void Awake()
    {
        // Check if we have a valid scene asset reference
        if (mainGameScene != null)
        {
            #if UNITY_EDITOR
            mainGameSceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GetAssetPath(mainGameScene));
            Debug.Log($"Using scene from asset: {mainGameSceneName}");
            #endif
        }
        
        // Build the menu group dictionary for quick lookup
        BuildMenuGroupDictionary();
    }
    
    private void Start()
    {
        // Initialize the menu navigation controller
        if (menuNavController == null)
        {
            menuNavController = GetComponent<MenuNavigationController>();
            if (menuNavController == null)
            {
                Debug.LogError("MenuNavigationController not found! Please assign it in the inspector.");
                return;
            }
        }
        
        // Configure the menu navigation controller with our menu groups
        ConfigureMenuNavigation();
        
        // Set up all buttons
        SetupButtons();
        
        // Start preloading the main game scene if enabled
        if (preloadMainScene)
        {
            StartCoroutine(PreloadMainScene());
        }
        
        // Show the initial menu group
        menuNavController.ShowMenuGroup(initialMenuGroup);
    }
    
    private void BuildMenuGroupDictionary()
    {
        menuGroupDict.Clear();
        foreach (var group in menuGroups)
        {
            if (!string.IsNullOrEmpty(group.groupName))
            {
                menuGroupDict[group.groupName] = group;
            }
        }
    }
    
    private void ConfigureMenuNavigation()
    {
        // Pass our menu groups to the navigation controller
        List<MenuNavigationController.MenuGroup> navGroups = new List<MenuNavigationController.MenuGroup>();
        
        foreach (var group in menuGroups)
        {
            navGroups.Add(group);
        }
        
        // If the navigation controller has a method to set menu groups, call it
        menuNavController.SetMenuGroups(navGroups);
    }
    
    private void SetupButtons()
    {
        foreach (var buttonData in menuButtons)
        {
            if (buttonData.buttonReference != null)
            {
                // Clear any existing listeners
                buttonData.buttonReference.onClick.RemoveAllListeners();
                
                // Add the appropriate action based on the button type
                switch (buttonData.actionType)
                {
                    case MenuButtonData.MenuAction.OpenMenu:
                        string targetMenu = buttonData.targetMenuGroup;
                        buttonData.buttonReference.onClick.AddListener(() => OpenMenuGroup(targetMenu));
                        break;
                        
                    case MenuButtonData.MenuAction.LoadScene:
                        string targetScene = !string.IsNullOrEmpty(buttonData.targetSceneName) ? 
                                             buttonData.targetSceneName : mainGameSceneName;
                        buttonData.buttonReference.onClick.AddListener(() => LoadScene(targetScene));
                        break;
                        
                    case MenuButtonData.MenuAction.ExitGame:
                        buttonData.buttonReference.onClick.AddListener(ExitGame);
                        break;
                        
                    case MenuButtonData.MenuAction.Custom:
                        string buttonName = buttonData.buttonName;
                        buttonData.buttonReference.onClick.AddListener(() => TriggerCustomAction(buttonName));
                        break;
                }
            }
        }
    }
    
    // Open a specific menu group
    public void OpenMenuGroup(string menuGroupName)
    {
        if (menuNavController != null)
        {
            menuNavController.ShowMenuGroup(menuGroupName);
        }
    }
    
    // Trigger a custom action for a button
    private void TriggerCustomAction(string buttonName)
    {
        Debug.Log($"Custom button action: {buttonName}");
        OnCustomButtonPressed?.Invoke(buttonName);
    }
    
    // Preload the main game scene in the background
    private IEnumerator PreloadMainScene()
    {
        Debug.Log($"Preloading scene: {mainGameSceneName}");
        
        try
        {
            // Begin loading the scene in the background
            sceneLoadOperation = SceneManager.LoadSceneAsync(mainGameSceneName);
            
            // Don't allow the scene to activate yet
            sceneLoadOperation.allowSceneActivation = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error preloading scene: {e.Message}");
        }
        
        yield return null;
    }
    
    // Load a scene (either the main game scene or another specified scene)
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        
        // If using transition and it's the main game scene
        if (useTransition && transitionAnimator != null && sceneName == mainGameSceneName)
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
        else
        {
            // If it's the preloaded main scene, activate it
            if (sceneName == mainGameSceneName && preloadMainScene && sceneLoadOperation != null)
            {
                Debug.Log($"Activating preloaded scene: {sceneName}");
                sceneLoadOperation.allowSceneActivation = true;
            }
            // Otherwise load the scene directly
            else
            {
                Debug.Log($"Loading scene directly: {sceneName}");
                try
                {
                    SceneManager.LoadScene(sceneName);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading scene '{sceneName}': {e.Message}");
                }
            }
        }
    }
    
    // Load a scene with transition animation
    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        // Trigger the transition animation
        transitionAnimator.SetTrigger(transitionTriggerName);
        
        // Wait for the transition to complete
        yield return new WaitForSecondsRealtime(transitionTime);
        
        // Load the scene
        if (sceneName == mainGameSceneName && preloadMainScene && sceneLoadOperation != null)
        {
            sceneLoadOperation.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    // Shortcut method to start the main game
    public void StartGame()
    {
        LoadScene(mainGameSceneName);
    }
    
    // Exit the game
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        
        #if UNITY_EDITOR
        // In the editor, stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // In a build, quit the application
        Application.Quit();
        #endif
    }
    
    // Method to add a button at runtime
    public void AddButton(MenuButtonData buttonData)
    {
        if (buttonData.buttonReference != null)
        {
            menuButtons.Add(buttonData);
            
            // Set up the button's action
            buttonData.buttonReference.onClick.RemoveAllListeners();
            
            switch (buttonData.actionType)
            {
                case MenuButtonData.MenuAction.OpenMenu:
                    string targetMenu = buttonData.targetMenuGroup;
                    buttonData.buttonReference.onClick.AddListener(() => OpenMenuGroup(targetMenu));
                    break;
                    
                case MenuButtonData.MenuAction.LoadScene:
                    string targetScene = !string.IsNullOrEmpty(buttonData.targetSceneName) ? 
                                        buttonData.targetSceneName : mainGameSceneName;
                    buttonData.buttonReference.onClick.AddListener(() => LoadScene(targetScene));
                    break;
                    
                case MenuButtonData.MenuAction.ExitGame:
                    buttonData.buttonReference.onClick.AddListener(ExitGame);
                    break;
                    
                case MenuButtonData.MenuAction.Custom:
                    string buttonName = buttonData.buttonName;
                    buttonData.buttonReference.onClick.AddListener(() => TriggerCustomAction(buttonName));
                    break;
            }
        }
    }
    
    // Method to add a menu group at runtime
    public void AddMenuGroup(MenuNavigationController.MenuGroup menuGroup)
    {
        menuGroups.Add(menuGroup);
        if (!string.IsNullOrEmpty(menuGroup.groupName))
        {
            menuGroupDict[menuGroup.groupName] = menuGroup;
        }
        
        // Update the navigation controller
        ConfigureMenuNavigation();
    }
}*/