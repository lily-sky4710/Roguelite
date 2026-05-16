using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Player
{

    public class PlayerController : MonoBehaviour
    {
        //移動速度
        private const float moveSpeed = 5.0f;

        //回転速度
        private const float ROTATE_SPEED = 10f;

        //レーザーポインターの描画距離
        private const float LASER_MAX_DISTANCE = 50f;

        //物理演算コンポーネント
        [SerializeField] private Rigidbody rigidbody;

        //銃口のトランスフォーム
        [SerializeField] private Transform weponOrigin;

        //レーザーポインターの描画コンポーネント
        [SerializeField] private LineRenderer laserLineRenderer;

        private Vector2 moveInput = Vector2.zero;

        private PlayerInputActions inputActions;

        //カメラのトランスフォーム
        private Transform mainCameraTransform;

        //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
        public Vector3 CurrentVelocity { get; private set; }

        private void Awake()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;

            if(UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("MainCameraが見つかりません");
            }
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
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            DrawLaserPointer();
        }
        private void FixedUpdate()
        {
            Move();
        }
        private void Move()//移動処理
        {
            if (rigidbody == null)
            {
                Debug.LogError("Rigidbodyが設定されていません");
                return;
            }

            //入力がない場合はピタッと止めておく
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0, rigidbody.linearVelocity.y, 0);
                CurrentVelocity = Vector3.zero;
                return;
            }

            //カメラの基準の計算に変更
            Vector3 cameraForwrad = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            cameraForwrad.y = 0f;
            cameraRight.y = 0f;
            cameraForwrad.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForwrad * moveInput.y + cameraRight * moveInput.x).normalized;

            //キャラクターを進行方向へ滑らかに振り向かせる
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATE_SPEED * Time.fixedDeltaTime);

            Vector3 targetVelocity = moveDirection * moveSpeed;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            //外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;

        }

        private void OnFire(InputAction.CallbackContext context)
        {
            Debug.Log("Fire");
        }

        //レーザーポインターの描画
        private void DrawLaserPointer()
        {
            if(laserLineRenderer == null || weponOrigin == null || mainCameraTransform == null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weponOrigin.position);

            Ray ray = new Ray(weponOrigin.position, mainCameraTransform.forward);
            if(Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}