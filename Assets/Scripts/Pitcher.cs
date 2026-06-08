using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{
    private Animator anim = null;
    private Vector3 defaltPos;
    private Quaternion defaltRot;

    [SerializeField] BallManager ballScript;
    [SerializeField] float pitchTime = 1.5f;

    [SerializeField] GameObject catchedBall;

    private bool isPitching = false;

    private bool canPitch = true;
    public bool CanPitch
    {
        get { return canPitch; }
        set { canPitch = value; }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        defaltPos = transform.position;
        defaltRot = transform.rotation;
    }

    public void PitchStart(bool _max)
    {
        if (!canPitch || isPitching) return;

        catchedBall.SetActive(false);
        isPitching = true;
        canPitch = false;
        //displaySpeed.RandomNumberDisplay();
        anim.SetTrigger("Pitch");
        StartCoroutine(PitchCoroutine(_max));
    }

    IEnumerator PitchCoroutine(bool _max)
    {
        yield return new WaitForSeconds(pitchTime);

        ballScript.Pitch(_max);
    }

    private void Update()
    {
        if (isPitching && anim.GetCurrentAnimatorStateInfo(0).IsName("Pitcher|Idle"))
        {
            Debug.Log("idle‚Ö‘JˆÚ");
            transform.localPosition = defaltPos;
            transform.localRotation = defaltRot;
            isPitching = false;
        }
    }
}