using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damege;
    public int count;
    public float speed;

    void Start()
    {
        Init();
    }

        // Update is called once per frame
    void Update()
    {
             switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed *Time.deltaTime);
                break;
            default:
                break;
        }

        //..Test Code..
        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(20, 5);
        }
    }

    public void LevelUp(float damege, int count)
    {
        this.damege = damege;
        this.count += count;   

        if(id==0)
            Batch();
    }
    public void Init()
    {
        switch (id)
        {
            case 0:
                speed = -150;
                Batch();
                break;
            default:
                break;
            
        }
    }

    void Batch()
    {
        for (int index = 0; index < count; index++)
        {
            Transform bullet;

            if(index < transform.childCount){
                bullet = transform.GetChild(index);
            }
            else{
                bullet= GameManager.instance.pool.Get(prefabId).transform;   
                bullet.parent = transform;
            }
            // prefabId로 풀에서 총알 가져오기
           
            
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * index/count;
            bullet.Rotate(rotVec);
            //움직이는 건 스페이스 월드 기준이다 
            bullet.Translate(bullet.up * 1.5f, Space.World);
            // 균등 각도로 초기 위치 오프셋 설정 (반지름 1.5)
            bullet.GetComponent<Bullet>().Init(damege, -1); // -1 is Infinity Per
        }
    }
}
