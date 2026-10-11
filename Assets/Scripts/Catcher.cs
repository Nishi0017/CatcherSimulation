using System.Collections;
using UnityEngine;

public class Catcher : MonoBehaviour
{
    [SerializeField] BallManager ballManager;
    [SerializeField] GameObject catchedBall;
    [SerializeField] float vibrationTime = 1f;
    AudioSource audioSource;
    Coroutine vibration;

    void Awake() { audioSource = GetComponent<AudioSource>(); }
    void Start() { if (catchedBall != null) catchedBall.SetActive(false); }

    void OnTriggerEnter(Collider other) { CatchContact(other); }
    void OnTriggerStay(Collider other) { CatchContact(other); }

    void CatchContact(Collider other)
    {
        GameObject candidate = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject : other.gameObject;
        if (candidate.CompareTag("Ball")) TryCatch(ballManager, candidate);
    }

    public bool TryCatch(BallManager manager, GameObject candidate)
    {
        if (!isActiveAndEnabled || manager == null || manager != ballManager
            || !manager.OwnsBall(candidate)) return false;
        // Resolve immediately so multiple contacts cannot count the same pitch twice.
        manager.Catch();
        if (audioSource != null) audioSource.Play();
        if (catchedBall != null) catchedBall.SetActive(true);
        if (vibration != null) StopCoroutine(vibration);
        vibration = StartCoroutine(ControllerVibration());
        return true;
    }

    IEnumerator ControllerVibration()
    {
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
        yield return new WaitForSeconds(vibrationTime);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
        vibration = null;
    }

    void OnDisable()
    {
        if (vibration != null) StopCoroutine(vibration);
        vibration = null;
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
    }
}
