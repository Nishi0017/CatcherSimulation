using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BallManager : MonoBehaviour
{
    [SerializeField] Pitcher pithcer;
    [SerializeField] Batter batter;
    [SerializeField] DisplaySpeed displaySpeed;

    [SerializeField] Transform pitcherHand;
    private Vector3 pitchPos;

    [SerializeField] GameObject ballPrefab;
    [SerializeField] GameObject ballPrefab2;

    [SerializeField] Transform targetArea_min;
    [SerializeField] Transform targetArea_max;
    private Vector3 targetPoint;

    [SerializeField] Slider throwSpeedSlider;
    [SerializeField] float throwSpeed = 80f;
    [SerializeField] int throwSpeed_min = 25;
    [SerializeField] int throwSpeed_max = 80;
    private float throwDuration;

    private GameObject ball;
    private float throwTime;

    private bool catchedBall = false;
    public bool CatchedBall {  get { return catchedBall; } set { catchedBall = value; } }

    private AudioSource audioSource;
    [SerializeField] AudioClip miss;
    [SerializeField] AudioClip success;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (UsePhysicalFlight) return;
        if(ball != null && !catchedBall)
        {
            MoveBall();
        }
    }

    public void Pitch(bool _max) => Pitch(_max, null);

    public void Pitch(bool _max, float? testSpeedKmh)
    {
        if (UsePhysicalFlight) { PitchPhysical(_max, testSpeedKmh); return; }
        //throwSpeed = throwSpeedSlider.value * 10;
        throwSpeed = _max ? 200f : testSpeedKmh ?? (Random.Range(throwSpeed_min, throwSpeed_max + 1) * 2);
        throwDuration = 57.6f / throwSpeed;
        batter.SwingStart(throwDuration);

        pitchPos = pitcherHand.position;

        float x = Random.Range(targetArea_min.position.x, targetArea_max.position.x);
        float y = Random.Range(targetArea_min.position.y, targetArea_max.position.y);
        float z = Random.Range(targetArea_min.position.z, targetArea_max.position.z);
        targetPoint = new Vector3(x, y, z);

        GameObject createBall = _max ? ballPrefab2 : ballPrefab;
        ball = Instantiate(createBall, pitchPos, pitcherHand.rotation);
        throwTime = Time.time;
    }

    void MoveBall()
    {
        // ボールが投げられてからの経過時間を計算
        float elapsedTime = Time.time - throwTime;
        float t = elapsedTime / throwDuration;

        // ボールが到着する位置に向かって線形補間で移動
        ball.transform.position = Vector3.Lerp(pitchPos, targetPoint, t);

        // ボールが到着地点に到達したらボールを削除
        if (t >= 1.0f && ball != null)
        {
            audioSource.PlayOneShot(miss);
            Destroy(ball);
            ball = null;
            pithcer.CanPitch = true;
            displaySpeed.UpdateSpeedText((int)throwSpeed);
        }
    }

    public void Catch()
    {
        if (ball == null) return;
        audioSource.PlayOneShot(success);
        Destroy(ball);
        ball = null;
        pithcer.CanPitch = true;
        displaySpeed.UpdateSpeedText((int)throwSpeed);
    }
}
