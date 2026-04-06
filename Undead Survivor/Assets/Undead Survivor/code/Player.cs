using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem; // inputSystem 도구를 사용하는 것

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    //시작할 때 한번만 실행되는 생명주기 Awake
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
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
