using System.Linq;
using UnityEngine;


namespace TPSRoguelite.InGame.Camera
{
    public class CameraController : MonoBehaviour
    {
        //<summary>
        //マウス感度
        //<summary>
        private float LOOK_SENSITIVITY = 0.2f;

        //<summary>
        //プレイヤーからの距離
        //<summary>
        private float DISTANCE = 5.0f;

        //<summary>
        //プレイヤーからの高さ
        //<summary>
        private float HEIGHT_OFFSET = 1.5f;

        //<summary>
        //縦の最小角度
        //<summary>
        private float MIN_PITCH = -10f;

        //<summary>
        //縦の最大角度
        //<summary>
        private float MAX_PITCH = 60f;

        //<summary>
        //追従するターゲット
        //<summary>
        [SerializeField] private Transform target;

        //<summary>
        //自動生成されたクラス
        //<summary>
        private PlayerInputActions inputActions;

        //<summary>
        //マウスの移動量
        //<summary>
        private Vector2 lookInput = Vector2.zero;

        //<summary>
        //横の回転角度(Y軸回転)
        //<summary>
        private float currentYaw = 0f;

        //<summary>
        //縦の回転角度(X軸回転)
        //<summary>
        private float currentPitch = 20f;

        private void Awake()
        {
            inputActions = new PlayerInputActions ();

            //マウスカーソルを画面中央にロックして非表示にする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        void Update()
        {
            //マウスの移動量を取得
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();

            //感度を掛けて現在の角度に足し引きする
            currentYaw += lookInput.x * LOOK_SENSITIVITY;
            currentPitch -= lookInput.y * LOOK_SENSITIVITY;

            currentPitch = Mathf.Clamp(currentPitch, MIN_PITCH, MAX_PITCH);
        }

        private void LateUpdate()
        {
            //カメラの移動は、プレイヤーの移動が終わった後に行う

            //ターゲットが設定されていない場合はエラー回避
            if(target == null)
            {
                return;
            }

            //注視点の計算（プレイヤーの腰当たり）
            Vector3 targetPosition = target.position + Vector3.up * HEIGHT_OFFSET;

            //角度をQuaternionに変換
            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw, 0f);

            //注視点から、計算した角度から後ろ方向へ距離分だけ離した位置を計算
            Vector3 cameraPosition = target.position - (rotate * Vector3.forward * DISTANCE);

            //
            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}