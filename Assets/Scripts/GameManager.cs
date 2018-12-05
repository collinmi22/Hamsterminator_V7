/*
 * Script written by: Collin Ilonummi
 * Most Recent update: 10/5/18
 * Purpose of Script: Handle the player movement, HP, Enemy Spawning and other non specific functions.
 * Comments refer to the line below
 */ 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    #region Public Fields
    [Tooltip("Use empty game objects to find spots for an enemy to be spawned at")]
    public Transform[] spawnPoints;
    [Tooltip("Track our health")]
    public Image Hearts;
    [Tooltip("Starts at zero, will be our identifier on which spawn point to use. Each spawn we'll increment it.")]
    public int spawnAmount = 0;
    [Tooltip("A timer to tell us which point enemies should pop out at each spot. Player will never see this timer.")]
    public int timer;
    [Tooltip("This will track a player's score, which is determined by how fast they can shoot their enemies.")]
    public float points;
    // A list to keep track of how many living enemies are on screen, ensuring the player won't be able to leave before all are defeated.
    [HideInInspector]
    public List<GameObject> enemyCount = new List<GameObject>();
    [Tooltip("A reference to our Camera, so we can move it across the level.")]
    public GameObject Cam;
    [Tooltip("Keeps track of our current enemy wave")]
    public int waveCount = 1;
    [Tooltip("A track of our ammo load")]
    public int Ammo;
    [Tooltip("Player's health at any given moment")]
    public int HitPoints;
    [Tooltip("Our Ammo counter for the player to see")]
    public Text ammoText;
    // A reference to our Health animator
    [HideInInspector]
    public Animator HeartAnim;
    [Tooltip("An array to determine what timer each wave will start with")]
    public int[] waveTimes;
    [Tooltip("The time between each wave")]
    public float[] delayTimes;

    public float delayTimer;

    public int final;

    public Collider col; 
    
    // If we are in cover or not
    [HideInInspector]
    public bool InCover = false;
    #endregion Public Fields

    #region Private Fields
    // This bool will prevent spawning Multiple enemies at the same time.
    private bool SpawnOne = true;
    // a reference to the camera's animator, so we can trigger parameters on it.
    private Animator CamAnim;
    // First touch point
    private Vector3 fp;
    // End point
    private Vector3 lp;
    // How far our finger has swiped
    private float swipeDistance;
   
    // A timer for reloading our gun
    private bool Reloading = false;
    // Checking if we are transitioning or not
    private bool Inbetween = false;
    #endregion Private Fields

    // Use this for initialization
    void Start () {
        #region Starting Parameters
        // Grabbing our animator from the camera, so we can trigger it.
        CamAnim = Cam.GetComponent<Animator>();

        col = Cam.GetComponent<Collider>(); 
        // Resetting our point value to make sure it doesn't start with false numbers.
        points = 0;
        // Resetting our wave count
        waveCount = 1;
        // Set our swipe threshold to %15 of the screen
        swipeDistance = Screen.height * 15 / 100;

        timer = waveTimes[waveCount - 1];

        

        delayTimer = delayTimes[waveCount - 1];
        // Telling our timer to begin
        StartCoroutine(Timer());
        #endregion Starting Parameters
    }

    // Update is called once per frame
    void Update () {
        
        #region UI settings
        // Keep track of our ammo on screen for the player
        ammoText.text = "x " + Ammo;
        HeartAnim = Hearts.GetComponent<Animator>();
        HeartAnim.SetInteger("HitPoints", HitPoints);
        if (HitPoints <= 0)
        {
            SceneManager.LoadScene(1);
        }
        #endregion UI settings

        #region Timer checks
        CamAnim.SetInteger("Wave", waveCount);
        // Check if wave has been completed
        if (timer <= 0 && enemyCount.Count == 0)
        {
            if (waveTimes[waveCount - 1] == final)
        {
            SceneManager.LoadScene(2);
        }
            else
            // Log Check to see if it fires
            Debug.Log("Time Up!");
            /*
            // Trigger Camera move
            CamAnim.SetTrigger("StageClear");
            // A test second trigger to clear it out
            CamAnim.SetTrigger("StageClear");
            */
            
            // Set our timer to current wave

            Inbetween = true;
            // Add one to the wave count
            waveCount++;

            timer = waveTimes[waveCount - 1];
        }

        if (Inbetween)
        {
            delayTimer = delayTimer - (1 * Time.deltaTime);
            if (delayTimer <= 0)
            {
                Inbetween = false;
                delayTimer = delayTimes[waveCount - 1];
                StartCoroutine(Timer());
            }
        }
        #endregion Timer checks
        #region Accelerometer Controls
        // If our accelerometer picks up a value on any axis stronger than 2, reload
        if (Input.acceleration.x >= 2 || Input.acceleration.y >= 2 || Input.acceleration.z >= 2)
            {
            // Set bool to prevent from reloading several times
                Reloading = true;
            // Start our reload process
                StartCoroutine(Reload());
            // A log check to see if it fired
                Debug.Log("Reload!");
            }
        #endregion Accelerometer Controls

        #region Touch Controls
        // Check if we have touched the screen or not
        if (Input.touchCount == 1)
        {
            // Save the touch point
            Touch touch = Input.GetTouch(0);
            // When we first touch the screen
            if(touch.phase == TouchPhase.Began)
            {
                // Set the first position we touched
                fp = touch.position;
                // Set the last position we touched
                lp = touch.position;
            }
            // Check while we are moving our finger
            else if (touch.phase == TouchPhase.Moved)
            {
                // Set our last position to where our finger is
                lp = touch.position;
            }
            // Check when our finger leaves the screen
            else if (touch.phase == TouchPhase.Ended)
            {
                // Set our last position to where our finger left the screen
                lp = touch.position;
                // If our X distance or Y distance is greater than the swipe threshold, it's a swipe.
                if (Mathf.Abs(lp.x - fp.x) > swipeDistance || Mathf.Abs(lp.y - fp.y) > swipeDistance)
                {
                    // if our x distance is greater than our y distance, then it's a horizontal swipe
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {
                        // If our distance is to the positive X axis, its a right swipe
                        if (lp.x > fp.x)
                        {
                            Debug.Log("Right Swipe");
                            if (InCover == true)
                            {
                                // Trigger our animation for getting out of cover
                                CamAnim.SetBool("Cover", false);
                                // Tells enemies that we are out of cover
                                InCover = false;
                            }
                            else
                            {
                                CamAnim.SetBool("Cover", true);

                                InCover = true;
                            }
                        }
                        // If our distance is to the negative x axis, its a left swipe
                        else
                        {
                            Debug.Log("Left Swipe");
                            if (InCover == false)
                            {
                                // Trigger the animation for getting into cover
                                CamAnim.SetBool("Cover", true);
                                // Tells enemies that we are in cover
                                InCover = true;
                            }
                            else
                            {
                                // Trigger our animation for getting out of cover
                                CamAnim.SetBool("Cover", false);
                                // Tells enemies that we are out of cover
                                InCover = false;
                            }
                        }
                    }
                    // If our y distance was more than our x distance, then its a vertical swipe
                    else
                    {
                        // If our distance is towards the positive y axis, it's an upwards swipe
                        if (lp.y > fp.y)
                        {
                            // Log check for an Upwards Swipe
                            Debug.Log("Up Swipe");
                        }
                        // If our distance is to the negative y axis, its a downwards swipe
                        else
                        {
                            // Log check for a downwards swipe
                            Debug.Log("Down Swipe");
                        }
                    }
                }
                // If the finger movement does not break the swipe threshold, it's a tap
                else
                {
                    // Log check for a tap
                    Debug.Log("Tap");
                    // Minus our ammuntion by one with every shot
                    Ammo--;
                }

}
        }
        #endregion Touch Controls
        
    }

    #region Enumerators
    IEnumerator Timer()
    {
        // If we still have time left
        if (timer > 0)
        {
            // Decrement our timer
            timer--;
            // Wait for one second
            yield return new WaitForSeconds(1);
            // Rinse and repeat.
            if (!Inbetween)
            {
                StartCoroutine(Timer());
            }
        }
    }

    IEnumerator Spawn()
    {
        // Check if we just spawned
        if (SpawnOne == false)
        {
            // Wait one second
            yield return new WaitForSeconds(1);
            // Reset our ability to spawn
            SpawnOne = true;
        }
    }

    IEnumerator Reload()
    {
        if (Reloading == true)
        {
            // Set our ammo to max
            Ammo = 10;
            // Wait one second
            yield return new WaitForSeconds(1);
            // Reset our reload so we can reload again
            Reloading = false;
        }
    }
    #endregion Enumerators
}
