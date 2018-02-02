using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayScript : MonoBehaviour {

    public MovieTexture tex;
	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().material.mainTexture = tex;
        tex.Play();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
