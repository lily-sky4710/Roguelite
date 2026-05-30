using Core.Interface;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

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

        //相手に与えるダメージ
        private const int ATACK_DAMAGE = 20;

        //攻撃距離（射撃範囲）
        private const float ATACK_RANGE = 50f;

        //最大弾数
        private const int MAX_AMMO = 30;

        //リロード時間
        private const float RELOAD_TIME = 1.5f;

        //物理演算コンポーネント
        [SerializeField] private Rigidbody rigidbody;

        //銃口のトランスフォーム
        [SerializeField] private Transform weponOrigin;

        //レーザーポインターの描画コンポーネント
        [SerializeField] private LineRenderer laserLineRenderer;

        private Vector2 moveInput = Vector2.zero;

        //自動生成されたinput
        private PlayerInputActions inputActions;

        //カメラのトランスフォーム
        private Transform mainCameraTransform;

        //リロードしているか
        private bool isReloading;

        //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
        public Vector3 CurrentVelocity { get; private set; }

        public int CurrentAmmo {  get; private set; }

        private void Awake()
        {
            CurrentAmmo = MAX_AMMO;

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire;
            inputActions.Player.Reload.performed += OnReload;

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
        }
        private void FixedUpdate()
        {
            Move();
        }
        private void LateUpdate()
        {
            DrawLaserPointer();
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
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            //光線に何か当たったか判定
            if(Physics.Raycast(ray, out RaycastHit hitInfo, ATACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中!");

                //当たった相手がIDamageableを持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                //ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if( target != null)
                {
                    target.TakeDamage(ATACK_DAMAGE);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if(isReloading && CurrentAmmo == MAX_AMMO)
            {
                return;
            }

            ReloadAsynk().Forget();
        }

        private async UniTask ReloadAsynk()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(RELOAD_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = MAX_AMMO;
            isReloading = false;
            Debug.Log("リロード完了");
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