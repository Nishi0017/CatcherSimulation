using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplaySpeed : MonoBehaviour
{
#if UNITY_EDITOR
    public float? InspectorNormalSpeedKmh { get; set; }
#endif
    [SerializeField] Pitcher pitcher;

    [SerializeField] TextMeshProUGUI throwSpeedText;
    [SerializeField] TextMeshProUGUI stringText;

    [SerializeField] float NumberChangeSpeed = 0.05f;
    [SerializeField] float UpdateSpeedTextDelay = 2f;

    [SerializeField] GameObject speedTextObj;
    [SerializeField] GameObject stringTextObj;
    private bool changeSpeedText = false;

    [SerializeField] GameObject buttonObj;
    private bool buttonMove = false;
    private bool buttonHide = false;
    private Vector3 buttonDefaltPos;
    private Vector3 buttonTargetPos;
    [SerializeField] float buttonMoveSpeed = 0.5f;

    private void Start()
    {
        stringText.text = "Push...";
        speedTextObj.SetActive(false);
        stringTextObj.SetActive(true);

        buttonDefaltPos = buttonObj.transform.position;
        buttonTargetPos = buttonDefaltPos + new Vector3(0, -0.9f, 0);
    }

    private void Update()
    {
        if (buttonMove)
        {
            Vector3 targetPosition = buttonHide ? buttonDefaltPos : buttonTargetPos;
            Transform buttonTransform = buttonObj.transform;
            buttonTransform.position = Vector3.Lerp(buttonTransform.position, targetPosition, buttonMoveSpeed * Time.deltaTime); // –Ú“I‚ÌˆÊ’u‚ÉˆÚ“®

            if(Vector3.SqrMagnitude(buttonTransform.position - targetPosition) < 0.0001f)
            {

                buttonHide = !buttonHide;
                buttonMove = false;

                speedTextObj.SetActive(false);
                stringTextObj.SetActive(true);
            }
        }
    }

    public void ReadyText(bool _max)
    {
        if (buttonMove) return;

        stringText.text = "Ready...";
        speedTextObj.SetActive(false);
        stringTextObj.SetActive(true);
        float? testSpeedKmh = null;
#if UNITY_EDITOR
        if (!_max) testSpeedKmh = InspectorNormalSpeedKmh;
#endif
        StartCoroutine(ReadyTextCoroutine(_max, testSpeedKmh));
    }


    IEnumerator ReadyTextCoroutine(bool _max, float? testSpeedKmh)
    {
        yield return new WaitForSeconds(0.1f);
        buttonMove = true;

        yield return new WaitForSeconds(1f);

        pitcher.PitchStart(_max, testSpeedKmh);

        yield return new WaitForSeconds(1f);

        stringText.text = "GO!";

    }

    public void UpdateSpeedText(int _throwSpeed)
    {
        speedTextObj.SetActive(true);
        stringTextObj.SetActive(false);
        stringText.text = "Push...";
        changeSpeedText = true;
        StartCoroutine("ChangeText");
        StartCoroutine(UpdateSpeedTextCoroutine(_throwSpeed));

    }

    IEnumerator ChangeText()
    {
        while (changeSpeedText)
        {
            int n1 = Random.Range(0, 10);
            int n2 = Random.Range(0, 10);
            int n3 = Random.Range(0, 10);
            throwSpeedText.text = n1.ToString() + n2.ToString() + n3.ToString() + " K";
            yield return new WaitForSeconds(NumberChangeSpeed);
        }

    }

    IEnumerator UpdateSpeedTextCoroutine(int _throwSpeed)
    {
        yield return new WaitForSeconds(UpdateSpeedTextDelay);

        changeSpeedText = false;
        throwSpeedText.text = _throwSpeed.ToString("D3") + " K";

        buttonMove = true;

    }
}
