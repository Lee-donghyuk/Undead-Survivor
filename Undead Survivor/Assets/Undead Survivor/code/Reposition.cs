using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    //모든 Collidar 의 부모
    Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger: " + collision.tag); // 추가
        if (!collision.CompareTag("Area"))
            return;

        //player의 위치
        Vector3 playerPos = GameManager.instance.player.transform.position;
        //유니티 월드맵의 위치
        Vector3 myPos = transform.position;

        switch (transform.tag)
        {
            case ("Ground"):
                float diffX = playerPos.x - myPos.x;
                float diffY = playerPos.y - myPos.y;
                //player가 움직이는 방향 (-1  ~   1) 사이
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY< 0 ? -1 : 1;
                diffX = Mathf.Abs(diffX);
                diffY = Mathf.Abs(diffY);

                if (diffX > diffY)
                    transform.Translate(Vector3.right * dirX * 40 );
                else if (diffX < diffY)
                    transform.Translate(Vector3.up * dirY * 40);
                break;

            case ("Enemy"):
                if (coll.enabled)
                {
                    Vector3 dist = playerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(-3,3),Random.Range(-3,3), 0);
                    //맵 하나의 크기만큼 이동 , 랜덤한 위치에서 등장하도록 벡터 더함
                    transform.Translate(ran + dist * 2);
                }
                break;
        }
    }
}
