using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;         // true: 근접무기(왼손), false: 원거리무기(오른손)
    public SpriteRenderer spriter;

    SpriteRenderer player;

    // 오른손(원거리) 정방향/반전 위치
    Vector3 rightPos = new Vector3(0.35f, -0.15f, 0);
    Vector3 rightPosReverse = new Vector3(-0.15f, -0.15f, 0);
    // 왼손(근접) 정방향/반전 회전각
    Quaternion leftRot = Quaternion.Euler(0, 0, -35);
    Quaternion leftRotReverse = Quaternion.Euler(0, 0, -135);

    void Awake()
    {
        // [0]은 Hand 자신, [1]이 부모인 플레이어의 SpriteRenderer
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    void LateUpdate()
    {
        // Player.LateUpdate에서 flipX가 갱신된 후 읽기 위해 LateUpdate 사용
        bool isReverse = player.flipX;

        if (isLeft)
        {   //근접무기: 회전으로 방향 표현, 반전 시 플레이어 뒤로(sortingOrder 낮춤)
            transform.localRotation = isReverse ? leftRotReverse : leftRot;
            spriter.flipY = isReverse;
            spriter.sortingOrder = isReverse ? 4 : 6;
        }
        else
        {   //원거리무기: 위치 이동으로 방향 표현, 반전 시 플레이어 앞으로(sortingOrder 높임)
            transform.localPosition = isReverse ? rightPosReverse : rightPos;
            spriter.flipX = isReverse;
            spriter.sortingOrder = isReverse ? 6 : 4;
        }
    }
}
