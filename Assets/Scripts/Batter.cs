using Oculus.Interaction.Body.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Batter : MonoBehaviour
{
    private Animator anim = null;
    private Vector3 defaltPos;
    private Quaternion defaltRot;

    private bool isSwing = false;

    [SerializeField] Transform rightHand;
    [SerializeField] GameObject batObj;
    private Rigidbody batRb;
    private Vector3 batDefaltPos;
    private Quaternion batDefaltRot;

    private void Start()
    {
        anim = GetComponent<Animator>();
        defaltPos = transform.position;
        defaltRot = transform.rotation;

        batDefaltPos = batObj.transform.localPosition;
        batDefaltRot = batObj.transform.localRotation;

        batRb = batObj.GetComponent<Rigidbody>();
    }


    private void Update()
    {
        if (isSwing && anim.GetCurrentAnimatorStateInfo(0).IsName("swing|Idle"))
        {
            transform.localPosition = defaltPos;
            transform.localRotation = defaltRot;

            Debug.Log("test1");

            batRb.useGravity = false;
            batRb.isKinematic = true;
            batObj.transform.parent = rightHand;
            batObj.transform.localPosition = batDefaltPos;
            batObj.transform.localRotation = batDefaltRot;

            isSwing = false;
            Debug.Log("デフォルト位置に戻る");
        }
        
        if(!isSwing && anim.GetCurrentAnimatorStateInfo(0).IsName("swing|Swing"))
        {
            StartCoroutine("DropBatt");
            isSwing = true;
            
        }

    }

    public void SwingStart(float _throwDuration)
    {
        anim.SetTrigger("Swing");
    }

    IEnumerator DropBatt()
    {
        Debug.Log("test1");
        yield return new WaitForSeconds(1.5f);

        batObj.transform.parent = null;
        batRb.isKinematic = false;
        batRb.useGravity = true;

    }

}
