using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThrowSpeedText : MonoBehaviour
{
    [SerializeField] Slider throwSpeedSlider; // スライダー
    private TextMeshProUGUI throwSpeedText; // 値を表示するテキスト

    void Start()
    {
        throwSpeedText = GetComponent<TextMeshProUGUI>();

        throwSpeedText.text = ((int)throwSpeedSlider.value).ToString() + "km/h";

        if (throwSpeedSlider != null)
        {
            // スライダーの初期値をテキストに設定
            UpdateValueText(throwSpeedSlider.value);

            // スライダーの値が変更されたときに呼び出されるリスナーを追加
            throwSpeedSlider.onValueChanged.AddListener(delegate { UpdateValueText(throwSpeedSlider.value); });
        }
    }

    // スライダーの値をテキストに更新するメソッド
    // スライダーの値をテキストに更新するメソッド
    void UpdateValueText(float value)
    {
        throwSpeedText.text = ((int)value).ToString() + "0 km/h"; // 整数として表示
    }
}
