using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUps : MonoBehaviour {

    public bool heartMode;

    public Image[] Images;

    public Animator anim;

    public GameObject gmo;

    public GameObject MC;
    public GameManager gm;

    // Use this for initialization
    void Start () {
        MC = GameObject.FindGameObjectWithTag("MainCamera");
        gmo = GameObject.FindGameObjectWithTag("GameController");

        gm = gmo.GetComponent<GameManager>();

    }
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(MC.transform);
	}

    private void OnMouseDown()
    {
        if (gm.HitPoints == 1 || gm.HitPoints == 2)
        {
            gm.HitPoints++;
            Debug.Log("Healed!");
            Destroy(gameObject);
        }
    }
}
