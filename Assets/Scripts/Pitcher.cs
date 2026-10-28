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

    public void PitchStart(bool _max) => PitchStart(_max, null);

    public void PitchStart(bool _max, float? testSpeedKmh)
    {
        if (!canPitch || isPitching) return;

        catchedBall.SetActive(false);
        isPitching = true;
        canPitch = false;
        //displaySpeed.RandomNumberDisplay();
        anim.SetTrigger("Pitch");
        StartCoroutine(PitchCoroutine(_max, testSpeedKmh));
    }

    IEnumerator PitchCoroutine(bool _max, float? testSpeedKmh)
    {
        yield return new WaitForSeconds(pitchTime);

        ballScript.Pitch(_max, testSpeedKmh);
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