using UnityEngine;
using TPSRoguelite.InGame.Player;

namespace TPSRoguelite.InGame.Item
{
    public class ExperienceOrb : MonoBehaviour
    {
        private const float MAGNET_RANGE = 5f;
        private const float MAGNET_SPEED = 15f;
        private const string PLAYER_TAG = "Player";

        private Transform playerTarget;
        private bool isFollowing = false;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if(playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Playerが見つかりませんでした。");
            }
        }

        private void Update()
        {
            if(playerTarget == null)
            {
                return;
            }

            if(isFollowing)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, MAGNET_SPEED * Time.deltaTime);
            }
            else
            {
                float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);
                if(distToPlayer <= MAGNET_RANGE)
                {
                    isFollowing = true;
                }
            }
        }

        //Playerに触れたときの処理(コライダーの「IsTrigger」がオンになっていないと動かない)
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PLAYER_TAG))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if(player != null)
                {
                    player.AddExp(1);
                }
                else
                {
                    Debug.LogWarning("PlayerContollerが見つかりませんでした。");
                }

                Destroy(gameObject);
            }
        }
    }
}
