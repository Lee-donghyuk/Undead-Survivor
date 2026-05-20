using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damege;
    public int count;
    public float speed;
    float timer;
    Player player;
    void Awake()
    {
        player = GameManager.instance.player;
    }
    void Update()
    {
            switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed *Time.deltaTime);
                break;
            default:
                timer +=Time.deltaTime;

                if(timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }

        //..Test Code..
        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(10, 1);
        }
    }

    public void LevelUp(float damege, int count)
    {
        this.damege = damege;
        this.count += count;

        if(id==0)
            Batch();

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }
    public void Init(ItemData data)
    {
        //Base let 
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero; //player를 기준으로 위치를 맞춰야함

        //Property Set
        id = data.itemId;
        damege = data.baseDamage;
        count = data.baseCount;

        for(int index = 0; index < GameManager.instance.pool.prefabs.Length; index++)
        {
            if(data.projectile == GameManager.instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }

        switch (id)
        {
            case 0:
                speed = 150;
                Batch();
                break;
            default:
                speed = 0.4f;
                break;
        }

        //Hand Set: itemType을 int로 캐스팅해 손 배열 인덱스로 사용 (Melee=0, Range=1)
        Hand hand = player.hands[(int)data.itemType];
        hand.spriter.sprite = data.hand;    // 무기 스프라이트 교체
        hand.gameObject.SetActive(true);    // 첫 획득 시 비활성 손 오브젝트 활성화

        //브로드 캐스트 메시지
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
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
            bullet.GetComponent<Bullet>().Init(damege, -1, Vector3.zero); // -1 is Infinity Per
        }
    }

    void Fire()
    {
        // 탐지된 적이 없으면 발사하지 않음
        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        // 현재 위치에서 적 방향으로의 단위 벡터 계산
        Vector3 dir = (targetPos - transform.position).normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        // Vector3.up(위쪽)을 기준으로 dir 방향으로 총알 회전
        // FromToRotation: 첫 번째 벡터에서 두 번째 벡터로 회전하는 Quaternion 반환
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damege, count, dir);
    }
}
