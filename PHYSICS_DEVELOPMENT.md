# DevelopScene の物理投球

## 実装

- `BallManager` はシーン名が `DevelopScene` のとき物理投球を使用する。MainScene / SampleScene は従来の直線補間を使用する。
- 1 Unity 単位を 1 m として、km/h を m/s に変換し、生成時だけ Rigidbody.velocity を設定する。
- 重力は Physics.gravity、空気抵抗は速度の二乗に比例する力。Rigidbody.drag は 0 に設定して二重適用を防ぐ。
- 現在の Fixed Timestep を使用した軌道予測で、目標位置へ届く低い側の投球角度を求める。目標が届かない場合は最大12回選び直し、失敗した投球は生成せず次の投球を許可する。
- 球速表示はリリース時の初速。捕球時の速さではない。スライダーには接続していない。
- Triggerに加え、移動区間のSphereCastとグローブ内の重なりで捕球を補助する。同じ投球の重複捕球を防ぐ。
- 捕球位置の後方通過、指定地面高さへの到達、時間切れで見逃しとする。
- 物理投球ではボールのSphereCollider半径を設定値へ合わせる。Prefabファイルと見た目のスケールは変更しない。

## Inspector の調整項目

BallManager の `DevelopScene physical pitch` を使用する。

| 項目 | 初期値 | 意味 |
| --- | --- | --- |
| Minimum / Maximum Speed Kmh | 60 / 160 | 通常投球の初速範囲 |
| Special Speed Kmh | 200 | 最大モードの初速 |
| Ball Mass Kg | 0.145 | 質量 |
| Ball Radius Metres | 0.0366 | 空気抵抗と捕球補助の半径 |
| Air Density | 1.225 | 空気密度 |
| Drag Coefficient | 0.35 | 抵抗係数の調整用初期値 |
| Target Tolerance | 0.03 | 軌道予測の目標高さの許容誤差 |
| Miss Distance Behind Target | 2 | 目標の何m後方で見逃しとするか |
| Flight Timeout | 5 | 投球の時間制限（秒） |
| Ground Height | 0 | 地面のワールドY座標 |

重力は垂直方向、風なしを前提とする。回転による揚力や変化球は未実装。
既存の投手・打者アニメーションは保持。Batter.SwingStartは引数の到達時間をまだ使用していない。

## 検証と残りの確認

- Unity 2022.3.62f3のDLLを参照し、Assets/ScriptsのC#コンパイルを確認。
- 実装の軌道計算を実行した96条件（5 / 12 / 16 / 18.44 m、60 / 100 / 160 / 200 km/h、3種類の高さ、空気抵抗あり / なし）で初速維持・目標高さの誤差・前方速度の減速を確認。到達不能な目標を拒否することも確認。
- 再実行: `powershell -File Tools/PhysicsValidation/Validate.ps1`。Unityと.NET SDKの場所は引数で変更可能。出力先はTempで、Unityを起動しない。
- 上記はUnity Editorのインポート・Play Mode・PhysXの実動作を検証するものではない。
- DevelopSceneのPlay ModeでConsole、通常 / 最大モード、捕球 / 見逃し、連続投球を確認する。
- 既存のワールドスケール、投球位置から目標までの距離、床のY座標を確認する。
- Questで高速球の捕球、グローブの移動中の判定、振動、見た目と判定球の大きさを確認する。ボールの通過区間は検査するが、グローブ自身の移動軌跡は検査していない。
- PhysXと軌道予測は衝突・浮動小数点計算等で差が出るため、実動作の目標誤差も確認する。
