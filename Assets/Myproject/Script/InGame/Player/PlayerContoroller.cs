using Core.Interface;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Core.MasterData;
using TPSRoguelite.InGame.Enum;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks.CompilerServices;

namespace TPSRoguelite.InGame.Player
{

    public class PlayerController : MonoBehaviour
    {
        //移動速度
        private const float MOVE_SPEED = 5.0f;

        //回転速度
        private const float ROTATE_SPEED = 10f;

        //レーザーポインターの描画距離
        private const float LASER_MAX_DISTANCE = 50f;

        //攻撃距離（射撃範囲）
        private const float ATACK_RANGE = 50f;

        private const float LEVEL_UP_EFFECT_DURATION = 2f;

        //物理演算コンポーネント
        [SerializeField] private Rigidbody rigidbody;

        //銃口のトランスフォーム
        [SerializeField] private Transform weponOrigin;

        //レーザーポインターの描画コンポーネント
        [SerializeField] private LineRenderer laserLineRenderer;

        //武器のID(デフォルトは1)
        [SerializeField] private ulong weaponId = 1;

        //マズルフラッシュのエフェクト
        [SerializeField] private ParticleSystem muzzleFlash;

        //武器の名前
        [SerializeField] private TextMeshProUGUI weaponName;

        //弾のテキスト
        [SerializeField] private TextMeshProUGUI ammoText;

        //リロード中のテキストと画像をまとめたオブジェクト
        [SerializeField] private GameObject reloadUI;

        //リロード中の時間がわかるサークル画像
        [SerializeField] private Image reloadCircleImage;

        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private ParticleSystem levelUpEffect;

        //武器のデータ
        private WeaponDataRecord currentWeapon;

        private Vector2 moveInput = Vector2.zero;

        //自動生成されたinput
        private PlayerInputActions inputActions;

        //カメラのトランスフォーム
        private Transform mainCameraTransform;

        //リロードしているか
        private bool isReloading;

        //射撃可能か
        private bool canShoot = true;

        //射撃のキャンセルトークン
        private CancellationTokenSource fireCts;

        //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
        public Vector3 CurrentVelocity { get; private set; }

        public int CurrentAmmo {  get; private set; }

        public int CurrentExp {  get; private set; }

        public int CurrentLevel {  get; private set; }

        private int RequiredExp => CurrentLevel * 5;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Setup()
        {
            currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(weaponId);

            if (currentWeapon != null)
            {
                CurrentAmmo = currentWeapon.MaxAmmo;
                UpdateWeaponUI();
            }
            else
            {
                Debug.LogError("WeaponDataがありません。");
            }

            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFire; //押し続けていると呼ばれる
            inputActions.Player.Fire.canceled += OnFire;
            inputActions.Player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("MainCameraが見つかりません");
            }

            if(reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            CurrentExp = 0;
            CurrentLevel = 1;

            if(levelUpText != null)
            {
                levelUpText.enabled = false;
            }

            UpdateExpUI();

            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            inputActions?.Enable();
        }
        private void OnDisable()
        {
            inputActions?.Disable();
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
            if (rigidbody == null || mainCameraTransform == null)
            {
                Debug.LogError("Rigidbodyが設定されていません");
                return;
            }

            Vector3 cameraForward = mainCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            if(cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATE_SPEED * Time.deltaTime);
            }

            //入力がない場合はピタッと止めておく
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0, rigidbody.linearVelocity.y, 0);
                CurrentVelocity = Vector3.zero;
                return;
            }

            //カメラの基準の計算に変更
            Vector3 cameraRight = mainCameraTransform.right;

            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            //外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;

        }

        private void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if(!canShoot ||  isReloading || currentWeapon == null)
                {
                    return;
                }

                fireCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(fireCts.Token,this.GetCancellationTokenOnDestroy());

                switch ((FireType)currentWeapon.WeapomFireType)
                {
                    case Enum.FireType.SemAuto:
                        ShootSemAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.Burst:
                        ShootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.FullAuto:
                        ShootFullAutoAsync(linkedCts.Token).Forget();
                        break;

                    default:
                        Debug.LogWarning($"割り当てていない射撃タイプがあります。{currentWeapon.WeapomFireType}");
                        break;

                }
            }

            if (context.canceled)
            {
                fireCts?.Cancel();
                fireCts?.Dispose();
                fireCts = null;
            }
        }

        private async UniTaskVoid ShootSemAutoAsync(CancellationToken token)
        {
            canShoot = false;

            if(CurrentAmmo == 0)
            {
                Reload();
                return;
            }

            canShoot = false;

            CurrentAmmo--;
            UpdateCurrentAmmoUI();
            Debug.Log($"セミオートで撃った!弾数:{CurrentAmmo}");
            Shoot();

            await UniTask.Delay(System.TimeSpan.FromSeconds(currentWeapon.FireRate),cancellationToken: token);

            canShoot = true;
        }

        //バーストの射撃処理
        private async UniTaskVoid ShootBurstAsync(CancellationToken token)
        {
            canShoot = false;

            for(int i = 0; i < 3; i++)
            {
                if(CurrentAmmo <= 0)
                {
                    Reload();
                    break;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
                Shoot();
                Debug.Log($"バースト!残弾数 : {CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInteval), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);
            canShoot = true;
        }

        private async UniTaskVoid ShootFullAutoAsync(CancellationToken token)
        {
            canShoot= false;

            while (!token.IsCancellationRequested)
            {
                if(CurrentAmmo <= 0)
                {
                    Reload();
                    break;
                }

                CurrentAmmo--;
                UpdateCurrentAmmoUI();
                Debug.Log($"フルオ－ト!残弾数 : {CurrentAmmo}");
                Shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInteval),cancellationToken: token).SuppressCancellationThrow();
                if( isCanceled)
                {
                    break;
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate),cancellationToken: this.GetCancellationTokenOnDestroy());

            canShoot = true;
        }

        //共通の射撃処理
        private void Shoot()
        {
            if(muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            //光線に何か当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中!");

                //当たった相手がIDamageableを持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                //ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null)
                {
                    target.TakeDamage(currentWeapon.AttackPower);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if(isReloading || CurrentAmmo == currentWeapon.MaxAmmo)
            {
                return;
            }

            Reload();
        }

        private void Reload()
        {
            isReloading = true;

            if(reloadUI != null)
            {
                reloadUI.SetActive(true);
            }

            if(reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = 0f;
            }

            DOVirtual.Float(0f, 1f, currentWeapon.ReloadTime, UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);
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

        private void UpdateWeaponUI()
        {
            if(weaponName != null)
            {
                weaponName.SetText(currentWeapon.WeaponName);

                //色で武器のタイプがわかる
                switch ((FireType)currentWeapon.WeapomFireType)
                {
                    case FireType.SemAuto:
                        weaponName.color = Color.white;
                        break;

                    case FireType.Burst:
                        weaponName.color = Color.yellow;
                        break;

                    case FireType.FullAuto:
                        weaponName.color = Color.red;
                        break;
                }
            }

            UpdateCurrentAmmoUI();
        }

        private void UpdateCurrentAmmoUI()
        {
            if(ammoText != null)
            {
                ammoText.SetText($"{CurrentAmmo}/{currentWeapon.MaxAmmo}");
            }
        }

        private void UpdateReloadUI(float value)
        {
            if(reloadCircleImage != null)
            {
                reloadCircleImage.fillAmount = value;
            }
        }

        private void FinishReload()
        {
            if(reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            CurrentAmmo = currentWeapon.MaxAmmo;
            UpdateCurrentAmmoUI();
            isReloading = false;
        }

        public void AddExp(int amount)
        {
            CurrentExp += amount;

            if(CurrentExp >= RequiredExp)
            {
                LevelUp();
            }

            UpdateExpUI();
        }

        private void UpdateExpUI()
        {
            if(expBar != null)
            {
                expBar.value = (float)CurrentExp / RequiredExp;
            }
        }

        private void LevelUp()
        {
            CurrentLevel++;

            CurrentExp -= RequiredExp;

            if(levelUpEffect != null)
            {
                levelUpEffect.Play();
            }

            ShowLebelUpTextAsync().Forget();
        }

        private async UniTaskVoid ShowLebelUpTextAsync()
        {
            if (levelUpText == null)
            {
                return;
            }

            levelUpText.enabled = true;
            levelUpText.SetText($"Level Up\n<size=50%>Lv.{CurrentLevel}</size>");

            await UniTask.Delay(TimeSpan.FromSeconds(LEVEL_UP_EFFECT_DURATION),cancellationToken: this.GetCancellationTokenOnDestroy());

            levelUpText.enabled = false;
        }
    }
}