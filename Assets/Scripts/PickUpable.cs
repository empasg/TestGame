using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType{
    weapon, head, chest, legs, accessory, stuff
}

namespace InventoryAsset
{

    public class PickUpable : MonoBehaviour
    {
        public ItemType type;
        public Vector3 offsetPosition;
        public Vector3 offsetEulerAngles;

        public string _name;
        public string description;

        public float healthBonus;
        public float resistanceBonus;

        public float damage;
        public float speed;        
        public Vector3 attackSize;
        public Vector3 attackCenter;

    }

}
