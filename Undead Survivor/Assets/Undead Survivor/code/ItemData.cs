using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//커스텀 메뉴 생성하는 속성
[CreateAssetMenu(fileName = "Item", menuName = "Scriptble Object/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType{Melee, Range, Glove, Shoe, Heal}

    [Header("# Main Info")]
    public ItemType itemType;
    public int itemId;
    public string itemName;
    [TextArea]
    public string itemDesc;
    public Sprite itemIcon;

    [Header("# Level Data")]
    public float baseDamage;
    public int baseCount;
    public float[] damages;
    public int[] counts;

    [Header("# Waepon")]
    public GameObject projectile;
    public Sprite hand;     // 무기 획득 시 손 UI에 표시할 스프라이트
}
