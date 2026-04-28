using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damege;
    public int per;

    public void Init(float damege, int per)
    {
        this.damege = damege;
        this.per = per;
    }
}
