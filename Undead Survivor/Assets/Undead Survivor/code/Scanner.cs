using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;
    public LayerMask targetLayer;
    public RaycastHit2D[] targets;
    public Transform nearestTarget;

    void FixedUpdate()
    {
        // 현재 위치에서 scanRange 반경의 원 안에 있는 targetLayer 오브젝트를 모두 감지
        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, targetLayer);
        nearestTarget = GetNearest();
    }

    Transform GetNearest()
    {
        Transform result = null;
        float diff = 100; // 비교 기준용 초기 거리 (충분히 큰 값)

        foreach (RaycastHit2D target in targets)
        {
            Vector3 myPos = transform.position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            // 현재까지 찾은 것보다 더 가까우면 교체
            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }

        // 가장 가까운 적의 Transform 반환 (없으면 null)
        return result;
    }
}
