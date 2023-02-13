using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StarterAssets;

namespace InventoryAsset
{

    public class Slot
    {

        public ItemType type;
        public GameObject item;

    }

    public class Inventory : MonoBehaviour
    {

        public List<GameObject> _inventory = new List<GameObject>();

        public List<Slot> _equipment = new List<Slot>();

        public int _maxSlots = 3;

        private CharacterController _charController;
        private StarterAssetsInputs _input;

        private GameObject _mainCamera;
        private GameObject _forceSelect;

        public float _pickUpRange = 4.0f;

        public List<GameObject> _nearItems = new List<GameObject>();
        private GameObject _selectedItem;

        private void Start()
        {

            _charController = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            _forceSelect = GameObject.FindGameObjectWithTag("ForceSelect");

            CreateItemSlots();

        }

        private void CreateItemSlots()
        {

            foreach ( ItemType _itemType in ItemType.GetValues(typeof(ItemType)) )
            {
                Slot _slot = new Slot();
                _slot.type = _itemType;
                _slot.item = null;

                _equipment.Add(_slot);

            }

        }

        private void Update()
        {

            SearchItem();
            PickUp();

        }

        private void SearchItem()
        {

            if (_inventory.Count >= _maxSlots) return;

            if (_nearItems.Count != 0)
            {

                _nearItems.Sort( (firstItem, secondItem) =>

                    Vector3.Distance( firstItem.transform.root.position, transform.position + Vector3.up * _charController.height / 2 ) > 
                     Vector3.Distance( secondItem.transform.root.position, transform.position + Vector3.up * _charController.height / 2 ) ? 1 : -1

                );

                _selectedItem = _nearItems[0];
                
            }
            else
            {

                _selectedItem = null;
                
            }

        }

        private void PickUp()
        {
            if (_selectedItem && _input.e && _selectedItem.transform.root != transform.root)
            {
                _nearItems.Remove(_selectedItem);

                if ( !TryAddEquipment(_selectedItem) ) _inventory.Add(_selectedItem);

                _selectedItem.SetActive(false);
                
                foreach(Slot _slot in _equipment)
                {
                    if (_slot.item) print(_slot.type.ToString() + " " + _slot.item.name);
                }
            }

            _input.e = false;

        }

        private bool TryAddEquipment(GameObject _item)
        {

            var _itemType = _item.GetComponent<PickUpable>().type;
            if (_itemType == ItemType.stuff) return false;

            foreach (Slot _slot in _equipment)
            {

                if (_slot.type != _itemType || _slot.item != null) continue;

                _slot.item = _item;
                return true;

            }  

            return false;          

        }

        public GameObject GetItem(ItemType _type)
        {

            foreach (Slot _slot in _equipment)
            {

                if (_slot.type == _type)
                {

                    return _slot.item;

                }

            }

            return null;

        }                

    }

}
