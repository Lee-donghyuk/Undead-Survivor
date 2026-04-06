using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger: " + collision.tag); // 추가
        if (!collision.CompareTag("Area"))
            return;

        //player의 위치
        Vector3 playerPos = GameManager.instance.player.transform.position;
        //유니티 월드맵의 위치
        Vector3 myPos = transform.position;

   
        float diffX = Mathf.Abs(playerPos.x - myPos.x);
        float diffY = Mathf.Abs(playerPos.y - myPos.y);


        //player가 움직이는 방향 (-1  ~   1) 사이
        Vector3 playerDir = GameManager.instance.player.inputVec;
        float dirX = playerDir.x < 0 ? -1 : 1;
        float dirY = playerDir.y < 0 ? -1 : 1;

        switch (transform.tag)
        {
            case ("Ground"):
                if (diffX > diffY)
                    transform.Translate(Vector3.right * dirX * 40 );
                else if (diffX < diffY)
                    transform.Translate(Vector3.up * dirY * 40);
                break;

            case ("Enemy"):

                break;
        }
    }
}
