using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    //<summary>
    //移動速度
    //</summary>
    private const float MOVE_SPEED = 5.0f;

    //<summary>
    //物理演算コンポーネント
    //</summary>
    [SerializeField] private Rigidbody rigidbody;

    //<summary>
    //自動生成されたInputクラス
    //</summary>
    private PlayerInputActions inputActions;

    //<summary>
    //入力方向
    //</summary>
    private Vector2 moveInput = Vector2.zero;

    //<summary>
    //移動方向のベクトル
    //</summary>
    private Vector3 moveDirection = Vector3.zero;

    //<summary>
    //外部(アニメーションとかUIとか)に現在の速度を教えるために保持するVelocity
    //</summary>
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
    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    //<summary>
    //移動処理
    //</summary>
    private void Move()
    {
        if(rigidbody == null)
        {
            Debug.LogError("Rigidbodyが設定されていません。");
            return;
        }

        //入力がない場合は、ピタッと止めておく
        if(moveInput == Vector2.zero)
        {
            rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
            CurrentVelocity = Vector3.zero;
            return;
        }

        //実際の移動速度を計算
        Vector3 targetVelocity = new Vector3(moveInput.x, rigidbody.linearVelocity.y, moveInput.y);
        targetVelocity.Normalize();

        rigidbody.linearVelocity = targetVelocity * MOVE_SPEED;

        //外部(アニメーションやUIなど)に現在の速度を教えるためにプロパティを更新
        CurrentVelocity = rigidbody.linearVelocity;
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        Debug.Log("Fire");
    }
}
