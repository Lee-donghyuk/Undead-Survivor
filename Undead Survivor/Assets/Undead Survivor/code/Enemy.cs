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
    Collider2D coll;    
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait; 
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    //적 이동함수
    void FixedUpdate()
    {
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
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
        coll.enabled = true; //collider 컴포넌트 활성화
        rigid.simulated = true; // 리지드바디 물리적 활성화
        spriter.sortingOrder = 2;  //inspertor의 enemy 프리펩의 spriter 레이어를 2->1로 변경 
        anim.SetBool("Dead",false);
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
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            // 살아있을 때 - 피격 반응 (추후 애니메이션 추가)
            anim.SetTrigger("Hit");
        }
        else
        {
            isLive = false;
            coll.enabled = false; //collider 컴포넌트 비활성화
            rigid.simulated = false; // 리지드바디 물리적 비활성화
            spriter.sortingOrder = 1;  //inspertor의 enemy 프리펩의 spriter 레이어를 2->1로 변경 
            anim.SetBool("Dead",true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
        }
    }

    //코루틴(Corountine) : 생명 주기와 비동기처럼 실행되는 함수
    IEnumerator KnockBack()
    {
        yield return wait; // 하나의 물리 프레임 딜레이
        Vector3 playerPos = GameManager.instance.player.transform.position; // 플레이어의 위치
        Vector3 dirVec = transform.position - playerPos; // 플레이어의 반대 방향
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse); // 반대 방향으로 넉백(힘으로 처리하기 위해 normailized)
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
