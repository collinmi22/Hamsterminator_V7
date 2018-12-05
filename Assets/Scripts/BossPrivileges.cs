using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPrivileges : MonoBehaviour {
    [HideInInspector]
    public GameObject Cam;
    [HideInInspector]
    public GameManager gm;

    public float speed;

    public int hits;
	// Use this for initialization
	void Start () {
        Cam = GameObject.FindGameObjectWithTag("GameController");
        gm = Cam.GetComponent<GameManager>();
        transform.LookAt(Cam.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.forward * speed;
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == Cam.GetComponent<Collision>())
        {
            if (gm.InCover == false)
            {
                gm.HitPoints--;
            }
            
        }
    }

    private void OnMouseDown()
    {
        hits--;
        if (hits <= 0)
        {
            Destroy(gameObject);
        }
    }
}
