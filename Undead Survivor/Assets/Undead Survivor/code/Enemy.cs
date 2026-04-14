using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;


    bool isLive = true; 
    //몬스터의 생사 판별


    //물리적 이동
    Rigidbody2D rigid;    
    SpriteRenderer sproter;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sproter = GetComponent<SpriteRenderer>();
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

        sproter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable() //스크립트가 활성화 될 때, 호출되는 함수
    {   
        //enemy에서 스스로 player를 찾아서 target으로 하면 좋음 
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
    }
}
