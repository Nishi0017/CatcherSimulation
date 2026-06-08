using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
{
    // 振幅
    public float amplitude = 1.0f;
    // 周期
    public float period = 1.0f;
    // 開始時のオフセット角度（ラジアン）
    public float startAngle = 0.0f;

    // 初期位置
    private Vector3 startPosition;

    void Start()
    {
        // オブジェクトの初期位置を保存
        startPosition = transform.position;
    }

    void Update()
    {
        // 現在の時間
        float time = Time.time;
        // sin波を計算
        float sinValue = Mathf.Sin((time / period * 2 * Mathf.PI) + startAngle);
        // 新しい位置を計算
        Vector3 newPosition = startPosition;
        newPosition.y += sinValue * amplitude;
        // オブジェクトの位置を更新
        transform.position = newPosition;
    }
}
