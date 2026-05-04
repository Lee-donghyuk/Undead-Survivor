using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damege;
    public int per;

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // damege: 데미지, per: 관통 횟수(-1은 무한), dir: 이동 방향(원거리 전용)
    public void Init(float damege, int per, Vector3 dir)
    {
        this.damege = damege;
        this.per = per;

        // per가 -1이면 근접 무기(회전형) → 이동 불필요
        if (per > -1)
        {
            //총알이 날라가는 속도
            rigid.velocity = dir*3;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy가 아니거나 근접 무기(per == -1)면 관통 처리 불필요
        if (!collision.CompareTag("Enemy") || per == -1)
            return;

        per--;

        // 관통 횟수 소진 시 풀로 반환 (Destroy 대신 SetActive)
        if (per == -1)
        {
            rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
