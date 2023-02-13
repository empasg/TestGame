using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StarterAssets;

namespace InventoryAsset
{

    public class ItemSelect : MonoBehaviour
    {

        private GameObject _player;
        public GameObject _mainCamera;

        private Inventory _inventory;
        private CharacterController _charController;
        

        private void Start()
        {

            _player = transform.parent.gameObject;
            
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            _inventory = _player.GetComponent<Inventory>();
            
            _charController = _player.GetComponent<CharacterController>();

        }

        private void OnTriggerEnter(Collider collider)
        {
            
            var _pickUp = collider.transform.root.gameObject.GetComponent<PickUpable>();

            if (!_pickUp || collider.transform.root.gameObject == transform.root.gameObject) return;
            
            _inventory._nearItems.Add(collider.transform.root.gameObject);

        }

        private void OnTriggerExit(Collider collider)
        {

            var _pickUp = collider.transform.root.gameObject.GetComponent<PickUpable>();

            if (!_pickUp) return;

            _inventory._nearItems.Remove(collider.transform.root.gameObject);

        }

        private void Update()
        {

            MoveCollider();

        }        

        private void MoveCollider()
        {

            transform.position = transform.parent.position + Vector3.up * ( _charController.height / 2 ) + transform.forward * ( transform.localScale.z * Mathf.PI );
            transform.forward = _mainCamera.transform.forward;
            transform.localScale = new Vector3 ( 1, 1, _inventory._pickUpRange );

        }

    }

}