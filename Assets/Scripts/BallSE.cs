using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSE : MonoBehaviour
{

    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball")
        {
            Debug.Log("oto");
            audioSource.Play();
        }
    }
}
