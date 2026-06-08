using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    [SerializeField] Pitcher pitcher;
    [SerializeField] BallManager ballManager;
    [SerializeField] Catcher catcher;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            pitcher.PitchStart(false);
        }
    }
}
