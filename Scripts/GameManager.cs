using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game loop
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// A reference to the only instance of the GameManager
    /// </summary>
    public static GameManager instance = null;

    /// <summary>
    /// A reference to the play button
    /// </summary>
    [SerializeField]
    GameObject m_titleMenu;

    /// <summary>
    /// A reference to the continue button
    /// </summary>
    [SerializeField]
    GameObject m_pauseMenu;

    /// <summary>
    /// A reference to the exit button
    /// </summary>
    [SerializeField]
    GameObject m_victoryMenu;

    /// <summary>
    /// A reference to the exit button
    /// </summary>
    [SerializeField]
    GameObject m_instructionsMenu;

    /// <summary>
    /// A reference to the key icon's renderer to enable/disable it
    /// </summary>
    [SerializeField]
    Renderer m_keyIcon;

    /// <summary>
    /// How long to wait before respawning the player adter death 
    /// </summary>
    [SerializeField]
    float m_respawnDealy = .25f;

    /// <summary>
    /// Holds the position where to place the player on game init
    /// </summary>
    [SerializeField]
    Vector3 m_playerStartPosition = Vector3.zero;

    /// <summary>
    /// Stores the current spawn point for the player
    /// </summary>
    public Vector3 PlayerSpawnPoint { get; set; }

    /// <summary>
    /// A reference to the player component
    /// </summary>
    Player m_player;
    public Player PlayerScript
    {
        get {
            if(m_player == null)
            {
                m_player = FindObjectOfType<Player>();
            }
            return m_player;
        }
    }

    /// <summary>
    /// A reference to the player component
    /// </summary>
    RecoilCamera m_camera;
    public RecoilCamera GameCamera
    {
        get {
            if (m_camera == null)
            {
                m_camera = FindObjectOfType<RecoilCamera>();
            }
            return m_camera;
        }
    }

    /// <summary>
    /// True when this is a brand new game
    /// </summary>
    bool m_isNewGame = true;

    /// <summary>
    /// True when the game has transitioned from new game to gameplay
    /// </summary>
    bool m_isGameStarted = false;

    /// <summary>
    /// True when the gameplay is running
    /// </summary>
    public bool IsGamePlay { set; get; }

    /// <summary>
    /// True when the current play session is over regardless of reason
    /// </summary>
    bool m_isGameOver = false;

    /// <summary>
    /// True when waiting for a player to decide whether to click on a menu item
    /// </summary>
    bool m_isWaitingForPlayerResponse = false;

    /// <summary>
    /// True when the player wants to restart from last checkpoint
    /// </summary>
    bool m_isGameCompleted = false;

    /// <summary>
    /// Keeps track of all the music clips so that we can turn them on/off
    /// </summary>
    List<SoundClip> m_musicClips = new List<SoundClip>();

    /// <summary>
    /// How long to wait before going to main menu
    /// </summary>
    [SerializeField]
    float m_endSequenceDelay = 1f;

    /// <summary>
    /// Creates the GameManager instance
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Starts the game loop
    /// </summary>
    void Start()
    {        
        StartCoroutine(GameLoopRoutine());
    }

    /// <summary>
    /// Main Loop which handles initializing the level, loading each level, and triggering gameplay until application is closed
    /// </summary>
    /// <returns></returns>
    IEnumerator GameLoopRoutine()
    {
        InitGame();
        yield return StartCoroutine(ShowTitleScreen());
        
        // Show instructions
        yield return StartCoroutine(ShowTitleScreen());

        while (!m_isGameCompleted)
        {
            yield return StartCoroutine(LevelStartRoutine());
            yield return StartCoroutine(GamePlayRoutine());
            yield return StartCoroutine(GameOverRoutine());
        }

        StartCoroutine(VictoryRoutine());
    }

    /// <summary>
    /// Initializes the game, locks the cursor
    /// Plays the level music
    /// </summary>
    void InitGame()
    {
        PlayerSpawnPoint = m_playerStartPosition;
        PlayerScript.ControlsEnabled = false;

        // Keep the cursor within the game screen
        Cursor.lockState = CursorLockMode.Confined;

        // Ensure the icon is not displayed
        UpdateKeyIconStatus();

        // AudioManager.instance.PlayMusic(AudioName.LevelMusic);
        m_musicClips.Add(AudioManager.instance.PlaySound(AudioName.Level1Music));
        m_musicClips.Add(AudioManager.instance.PlaySound(AudioName.Level2Music));
        m_musicClips.Add(AudioManager.instance.PlaySound(AudioName.Level3Music));

        m_musicClips[1].Volume = 0f;
        m_musicClips[2].Volume = 0f;

        m_isNewGame = true;
        IsGamePlay = false;
        m_isGameCompleted = false;
        m_isGameOver = false;
        m_isWaitingForPlayerResponse = false;
    }

    /// <summary>
    /// Displays the instruction on how to play and waits for the player to click start
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTitleScreen()
    {
        m_isNewGame = false;
        m_titleMenu.SetActive(true);

        // Wait for the player to click start
        while (!m_isGameStarted)
        {
            yield return null;
        }

        m_titleMenu.SetActive(false);
    }

    /// <summary>
    /// Displays the instruction on how to play and waits for the player to click start
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowInstructionsRoutine()
    {
        m_instructionsMenu.SetActive(true);

        // Wait for the player to click start
        while (m_isWaitingForPlayerResponse)
        {
            yield return null;
        }

        m_instructionsMenu.SetActive(true);
    }

    /// <summary>
    /// Positions the player at the current spawn point
    /// Forces the camera to snap to the player
    /// Displays all menus
    /// </summary>
    /// <returns></returns>
    IEnumerator LevelStartRoutine()
    {      
        DisableMenus();
        // Player should always start facing to the right
        PlayerScript.transform.rotation = Quaternion.LookRotation(Vector3.right);

        // Ensure the player is inside the level
        PlayerScript.transform.position = new Vector3(
            PlayerScript.transform.position.x,
            PlayerScript.transform.position.y,
            0f
        );

        PlayerScript.ControlsEnabled = false;
        PlayerScript.transform.position = PlayerSpawnPoint;

        GameCamera.Target = PlayerScript.transform;

        // Player has respawned
        if (PlayerScript.IsDead)
        {
            PlayerScript.Respanwed();
        }

        // This could be a player respawn from being dead
        // So make sure we they are no logner dead
        PlayerScript.IsDead = false;

        GameCamera.Track(false);
        PlayerScript.ControlsEnabled = true;

        yield return null;
    }

    /// <summary>
    /// Triggers the enemies to start and allows the player to be controlled
    /// Yields until the game is over
    /// </summary>
    /// <returns></returns>
    IEnumerator GamePlayRoutine()
    {
        IsGamePlay = true;
        m_isGameOver = false;
        PlayerScript.ControlsEnabled = true;
        Time.timeScale = 1f;
        
        while (!m_isGameOver)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Checks the reason for game over and triggers the proper response
    /// For player victory/loss it shows the appropiate menu and yields until the player choses
    /// For a restart is simply allows everything to go through as normal
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOverRoutine()
    {
        IsGamePlay = false;
        m_player.ControlsEnabled = false;

        // Only for Victory doe we ned to do something
        // Death simply restarts the loop
        if (PlayerScript.IsDead)
        {
            // Delay showing the respawn
            yield return new WaitForSeconds(m_respawnDealy);

        // Game completed
        } else {
            m_isGameCompleted = true;
        }
    } 

    IEnumerator VictoryRoutine()
    {
        while (PlayerScript.IsFalling)
        {
            yield return null;
        }

        PlayerScript.PlayVictory();

        yield return new WaitForSeconds(m_endSequenceDelay);

        RestartLevel();
    }

    /// <summary>
    /// Disables the title screens and shows instructions
    /// </summary>
    public void StartGame()
    {
        m_isGameStarted = true;
    }

    /// <summary>
    /// Sets all menus to active/disabled
    /// </summary>
    void DisableMenus()
    {
        m_titleMenu.SetActive(false);
        m_instructionsMenu.SetActive(false);
        m_pauseMenu.SetActive(false);
        m_victoryMenu.SetActive(false);
    }
   
    /// <summary>
    /// Handles opening/closing pause menu while in game play
    /// </summary>
    void Update()
    {
        if (IsGamePlay && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    /// <summary>
    /// Toggles On/Off the pause menu
    /// </summary>
    public void TogglePauseMenu()
    {
        bool enable = !m_pauseMenu.activeSelf;
        m_pauseMenu.SetActive(enable);

        // Freeze/unfreeze time
        if (enable)
        {
            Time.timeScale = 0f;
            m_player.ControlsEnabled = false;
        } else {
            Time.timeScale = 1f;
            m_player.ControlsEnabled = true;
        }
    }

    /// <summary>
    /// Shows the win menu screen
    /// </summary>
    public void ShowVictoryMenu()
    {
        m_victoryMenu.SetActive(true);
    }

    /// <summary>
    /// Removes the waiting continue to replay the current level
    /// </summary>
    public void PlayGame()
    {
        m_isWaitingForPlayerResponse = false;
    }

    /// <summary>
    /// Triggers the player death sequence
    /// Camera stops tracking so that we can move the player arround without it following the player
    /// </summary>
    public void TriggerPlayerDeath()
    {
        // Force the player to look at the camera
        PlayerScript.transform.rotation = Quaternion.LookRotation(Vector3.back);
        PlayerScript.IsDead = true;

        // Move them forward enough to always be infront of everything so that it is visible they are dead
        // This is done after the IsDead as it stops the rigidbody from falling off the stage
        PlayerScript.transform.position = new Vector3(
            PlayerScript.transform.position.x,
            PlayerScript.transform.position.y,
            -8f
        );

        m_isGameOver = true;        
        GameCamera.Target = null;
    }

    public void NewLevelReached(int level)
    {
        if(level <= m_musicClips.Count)
        {
            m_musicClips[level].Volume = 1f;
        }
    }

    /// <summary>
    /// Triggered when a key is collected/removed to update the key icon to display or not
    /// </summary>
    public void UpdateKeyIconStatus()
    {
        m_keyIcon.enabled = PlayerScript.HasKeys;
    }

    /// <summary>
    /// Trigger victory
    /// </summary>
    public void TriggerPlayerVictory()
    {
        PlayerScript.ControlsEnabled = false;
        m_isGameOver = true;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Exits out of the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}