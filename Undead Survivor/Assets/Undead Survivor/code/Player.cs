using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // inputSystem 도구를 사용하는 것

public class Player : MonoBehaviour
{
    
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;    // [0]: 왼손(Melee), [1]: 오른손(Range) — 계층 순서와 일치해야 함

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;


    //시작할 때 한번만 실행되는 생명주기 Awake
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        //직접 만든 스크립트도 컴포넌트와 동일하게 취급됨
        scanner = GetComponent<Scanner>();
        // true: 비활성 오브젝트 포함 — 처음에 손이 꺼져 있으므로 필수
        hands = GetComponentsInChildren<Hand>(true);
    }



    //void update()
    //{
    //    inputVec.x = Input.GetAxisRaw("Horizontal");
    //    inputVec.y = Input.GetAxisRaw("Vertical");
    //}
    //프레임이 종료 되기 전에 실행되는 생명주기함수

    void FixedUpdate()
    {
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        // 위치 이동
        rigid.MovePosition(rigid.position + nextVec);
    }

    //input system 사용해보기
    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    void LateUpdate()
    {
        // anim 의 float 파라미터 값을 Set 시킴 
        //파라미터 이름 Speed , inputVec의 크기 값으로 대입
        anim.SetFloat("Speed", inputVec.magnitude);
        
        if(inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }    
    }
}
