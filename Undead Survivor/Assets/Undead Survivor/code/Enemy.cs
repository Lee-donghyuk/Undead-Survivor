using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    
    public Rigidbody2D target;


    bool isLive;
    //몬스터의 생사 판별


    //물리적 이동
    Rigidbody2D rigid;    
    Animator anim;
    SpriteRenderer spriter;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
    }

    //적 이동함수
    void FixedUpdate()
    {
        if (!isLive)
            return;

        //target 과의 위치를 따라가는 것    
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed *Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero; //0 ->물리 속도가 이동에 영향을 주지 않도록
    }

    void LateUpdate()
    {
        if(!isLive)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable() //스크립트가 활성화 될 때, 호출되는 함수
    {   
        //enemy에서 스스로 player를 찾아서 target으로 하면 좋음 
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        health = maxHealth;
    }


    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    // 투사체와 충돌했을 때 호출되는 함수
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Bullet 태그가 아니거나 이미 죽은 경우 무시
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        health -= collision.GetComponent<Bullet>().damege;

        if (health > 0)
        {
            // 살아있을 때 - 피격 반응 (추후 애니메이션 추가)
        }
        else
        {
            Dead();
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
