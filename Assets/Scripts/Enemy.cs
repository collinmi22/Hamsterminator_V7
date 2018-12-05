using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {
    #region public variables
    [Tooltip("Track how many hits to kill the enemy")]
    public int hitCount;
    [Tooltip("How many points the enemy should be worth")]
    public float pointCount;
    [HideInInspector]
    // Grab the object for script reference
    public GameObject ObjectWithScript;
    [HideInInspector]
    // Reference to the script
    public GameManager gm;
    [HideInInspector]
    // Check if our character is active in the scene or not
    public bool Active = false;
    [Tooltip("What point in the timer should the Enemy spawn")]
    public int spawnTime;
    [Tooltip("What wave should the enemy spawn")]
    public int waveTime;
    [Tooltip("How long until the enemy should shoot?")]
    public int shootTimer;
    [Tooltip("How long unti the enemy should shoot again after shooting")]
    public int timeAmount;
    [Tooltip("Whether the enemy should drop an item or not")]
    public bool itemDrop;

    public GameObject Powerup;

    public int Costume;
    #endregion Public variables

    #region Private variables
    // A reference to our animators
    private Animator anim;

    private bool Dead;
    #endregion Private variables

    private void Awake()
    {   // Get our animator
        anim = GetComponent<Animator>();
        anim.SetInteger("Costume", Costume);
    }

    // Use this for initialization
    void Start () {
        #region Starting variables
        // Find our Game Manager
        ObjectWithScript = GameObject.FindGameObjectWithTag("GameController");
        // Find the script on the empty game object
        gm = ObjectWithScript.GetComponent<GameManager>();
        
        // Set our animation triggers to make sure we start in the idle state
        anim.SetBool("Shoot", false);
        anim.SetBool("Shot", false);
        anim.SetBool("Dead", false);
        #endregion Starting variables
    }

    // Update is called once per frame
    void Update () {
        #region Spawning functions
        // If we are active
        if (Active == true)
        {
            // Minus our point count for as long as the enemy is alive
            pointCount = pointCount - 1;
        }
        // if we are in the correct wave, and hit our spawn time, and have not spawned already, Spawn
        if (spawnTime == gm.timer && waveTime == gm.waveCount && Active == false)
        {
            // Set us to our active state
            Active = true;
            // Set our position to the appropriate spawn location
            transform.position = new Vector3 (gm.spawnPoints[gm.spawnAmount].transform.position.x, gm.spawnPoints[gm.spawnAmount].transform.position.y, gm.spawnPoints[gm.spawnAmount].transform.position.z);
            // Add to our spawn variable so the next enemy will spawn in a different location
            gm.spawnAmount++;
            // Add the enemy to the active enemies list
            gm.enemyCount.Add(gameObject);
            // Start our shoot timer
            StartCoroutine(ShootTime());
        }
        #endregion Spawning functions

        #region Shooting Functions
        // If we are about to shoot, change to our warning animation
        if (shootTimer == 1)
        {
            // Set our booleans to go into warning mode
            anim.SetBool("Shoot", true);
            anim.SetBool("Shot", false);
        }
        // If our shoot timer hits zero, shoot the player
        if (shootTimer == 0)
        {
            if (gm.HitPoints > 0 && !Dead)
            {
                // Minus the player's health
                gm.HitPoints--;
            }
            // Log check to make sure we shot
            Debug.Log("BANG!");
            // Make the phone vibrate to indicate we've been shot
            Handheld.Vibrate();
            // Reset our timer to shoot again
            shootTimer = timeAmount;
            // Start the timer to reset our shoot animation
            StartCoroutine(Shootanim());
        }
        #endregion Shooting Functions
    }

    #region When Hit functions
    private void OnMouseDown() {
        // Check if the player has ammo
        if (gm.Ammo > 0)
        {
            // Minus our health by 1
            hitCount--;
            // Check if we still have health left
            if (hitCount == 0)
            {
                // Add the points to our point counter
                gm.points = gm.points + pointCount;
                // Trigger our death animation
                anim.SetBool("Dead", true);
                anim.SetBool("Shoot", false);
                anim.SetBool("Shot", false);
                Dead = true;
                // Start our death timer
                StartCoroutine(Deathanim());
            }   
        }
        
    }
    #endregion When Hit functions
    
    #region Enumerators
    // A timer for shooting at the player
    IEnumerator ShootTime()
    {
        // Check if we still have time
        if (shootTimer > 0)
        {
            // Minus our timer by one
            shootTimer--;
            // Wait one second
            yield return new WaitForSeconds(1);
            // Check our timer again
            StartCoroutine(ShootTime());
        }
    }

    IEnumerator Shootanim()
    {
        // Wait half a second
        yield return new WaitForSeconds(.5f);
        // Reset to our idle animation
        anim.SetBool("Shoot", false);
        anim.SetBool("Shot", true);
    }

    IEnumerator Deathanim()
    {
        // Wait half a second
        yield return new WaitForSeconds(.5f);
        // Remove from the active enemy list
        gm.enemyCount.RemoveAt(0);
        if (itemDrop)
        {
            Instantiate(Powerup, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity);
        }
        // Destroy the object
        Destroy(gameObject);
    }
    #endregion Enumerators
}
