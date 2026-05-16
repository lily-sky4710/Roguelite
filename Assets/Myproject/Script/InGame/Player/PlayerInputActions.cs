using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Player
{

    public class PlayerController : MonoBehaviour
    {
        //移動速度
        private const float moveSpeed = 5.0f;

        //物理演算コンポーネント
        [SerializeField] private Rigidbody rigidbody;

        private Vector2 moveInput = Vector2.zero;

        //移動方向のベクトル
        private Vector3 moveDireection = Vector3.zero;

        private PlayerInputActions inputActions;
        private Vector3 moveDirection;

        //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
        public Vector3 CurrentVelocity { get; private set; }

        private void Awake()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;
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

            //実際の移動速度計算
            Vector3 targetVelocity = new Vector3(moveInput.x, rigidbody.linearVelocity.y, moveInput.y);
            targetVelocity.Normalize();

            rigidbody.linearVelocity = targetVelocity * moveSpeed;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            Debug.Log("Fire");
        }
    }
}