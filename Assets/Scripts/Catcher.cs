using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catcher : MonoBehaviour
{
    [SerializeField] BallManager ballManager;
    [SerializeField] GameObject catchedBall;

    private AudioSource audioSource;

    private bool canCatch = false;

    [SerializeField] float vibrationTime = 1f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
            canCatch = true;
            Debug.Log("enter");
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Ball")
        {
            canCatch = true;
            Debug.Log("stay");
        }

    }

    private void Start()
    {
        catchedBall.SetActive(false);

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(canCatch)
        {
            StartCoroutine("ControllerVibration");
            audioSource.Play();
            ballManager.Catch();
            catchedBall.SetActive(true);
            canCatch = false;
            Debug.Log("キャッチ");
        }
    }

    IEnumerator ControllerVibration()
    {
        //左のコントローラーを振動させる
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);

        //指定された時間待つ
        yield return new WaitForSeconds(vibrationTime);

        //コントローラーの振動を止める
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }
}
