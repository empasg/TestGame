using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using StarterAssets;


    public class Force : MonoBehaviour
    {
        
        [SerializeField] private Material forceObjectMaterial;
        public float ForcePower = 10f;
        [SerializeField] private float ChangeTime = 0.5f;
        [SerializeField] private float ChangeStrong = 90;
        [SerializeField] private float coolDown = 1;
        [SerializeField] private float maxNearDistance = 2;
        private VolumeProfile Vol;
        private float coolDownTime = 0;
        private bool inChange;
        private StarterAssetsInputs _input;
        private ColorAdjustments _ColorAdj;
        private ThirdPersonController _TPController;
        private SphereCollider _forceCollider;
        private MeshCollider _selectCollider;
        private CharacterController _charController;

        private GameObject _forceSelect;
        private ForceSelect _forceSelectComp;
        private GameObject _mainCamera;

        private GameObject _boneAttach;

        private float _ColorAdjContrast;
        public bool _forceModeActive = false;
        private float changeLerp;
        private float time;
        private Vector3 _lastMove;

        private List<MeshRenderer> _forceColliderMeshes = new List<MeshRenderer>();
        public List<MeshRenderer> _selectedMeshes = new List<MeshRenderer>();
        public MeshRenderer _selectedMesh;

        private void Start()
        {

            _TPController = GetComponent<ThirdPersonController>();
            _charController = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _forceCollider = GetComponent<SphereCollider>();

            _forceSelect = GameObject.FindGameObjectWithTag("ForceSelect");
            _forceSelectComp = _forceSelect.GetComponent<ForceSelect>();
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            _boneAttach = GameObject.FindGameObjectWithTag("RightHand");

            Vol = FindObjectOfType<Volume>().sharedProfile;

            if (Vol.TryGet<ColorAdjustments>(out _ColorAdj))
            {
                _ColorAdjContrast = _ColorAdj.contrast.value;
            }

            coolDown = coolDown <= ChangeTime ? coolDown : ChangeTime + 0.5f;
            
        }

        void OnApplicationQuit()
        {
            _ColorAdj.contrast.value = _ColorAdjContrast;
        }
        
        private void LateUpdate()
        {

            ForceMode();

            MeshOutline();

        }

        private void Update()
        {

            SelectMesh();
            AddCharControllerSelectedMesh();

        }

        private void FixedUpdate() 
        {

            MoveSelected();

        }

        
        private void OnTriggerEnter(Collider _collider)
        {
            
            var _colliderMesh = _collider.gameObject.GetComponentInChildren<MeshRenderer>();
            if (_colliderMesh != null)
            {

                EnableOutline(_colliderMesh);

                _forceColliderMeshes.Add(_colliderMesh);

            }
            
        }

        private void OnTriggerExit(Collider _collider)
        {
            
            var _colliderMesh = _collider.gameObject.GetComponentInChildren<MeshRenderer>();
            if (_colliderMesh != null)
            {
                
                DisableOutline(_colliderMesh);

                _forceColliderMeshes.Remove(_colliderMesh);
            
            }
            
        }

        private void SelectMesh()
        {

            _selectedMeshes.Sort( (firstMesh, secondMesh) => 

                Vector3.Distance( firstMesh.transform.root.position, transform.position ) > Vector3.Distance( secondMesh.transform.root.position, transform.position ) ? 1 : -1

            );
            
            if (_selectedMeshes.Count != 0)
            {
                _selectedMesh = _selectedMeshes[0];
            } 
            else
            {
                _selectedMesh = null;
            }
            
        }

        private void AddCharControllerSelectedMesh()
        {
            if (_selectedMesh == null) return;

            var _target = _selectedMesh.transform.root.gameObject;

            var checkCharController = _target.GetComponent<CharacterController>();
            if (checkCharController) return;

            var _capsuleCollider = _target.GetComponent<CapsuleCollider>();
            var _boxCollider = _target.GetComponent<BoxCollider>();
            var _sphereCollider = _target.GetComponent<SphereCollider>();

            float _height = 1;
            float _radius = 1;

            if ( _capsuleCollider != null )
            {

                _height = _capsuleCollider.height;
                _radius = _capsuleCollider.radius;

            }

            if (_boxCollider != null)
            {

                _height = _boxCollider.center.y;
                _radius = ( _boxCollider.center.x + _boxCollider.center.z ) / 2;

            }

            if (_sphereCollider != null)
            {

                _height = _sphereCollider.radius;
                _radius = _sphereCollider.radius;

            }

            var _newCharController = _target.AddComponent<CharacterController>();
            _newCharController.radius = _radius;
            _newCharController.height = _height;

        }

        private void ForceMode()
        {
            
            if ( coolDownTime - Time.time > -coolDown) return;
            
            if (_input.rightClick) inChange = true;

            if (inChange == false) return;
            
            if (_forceModeActive)
            {
                changeLerp = Mathf.Lerp( _ColorAdj.contrast.value, _ColorAdjContrast, time );
            }
            else
            {
                changeLerp = Mathf.Lerp( _ColorAdj.contrast.value, _ColorAdjContrast + ChangeStrong, time );
            }

            time += ChangeTime * Time.deltaTime;

            float ForceModeActive = _forceModeActive ? 0 : 1;

            _forceCollider.radius = ForcePower * time * 2 * ForceModeActive ;

            float ForceModeMultiplier = _forceModeActive ? 1 : 0.5f;

            _TPController.ChangeSpeed(Mathf.Clamp(time * 2, 0, 1) * ForceModeMultiplier * 1.5f, Mathf.Clamp(time * 2, 0, 1) * ForceModeMultiplier);

            _ColorAdj.contrast.value = changeLerp;
            
            if (_ColorAdj.contrast.value == _ColorAdjContrast + ChangeStrong & !_forceModeActive || _ColorAdj.contrast.value == _ColorAdjContrast & _forceModeActive )
            {

                inChange = false;
                coolDownTime = Time.time;
                _forceModeActive = !_forceModeActive;
                time = 0.0f;
                
            }

        }

        private void EnableOutline(MeshRenderer _mesh)
        {

            var _matList = new List<Material>();
            _matList.Add(forceObjectMaterial);

            foreach(Material _mat in _mesh.materials)
            {
                _matList.Add(_mat);
            }

            _mesh.materials = _matList.ToArray();

        }

        private void DisableOutline(MeshRenderer _mesh)
        {
            
            var _matList = new List<Material>();

            foreach (Material _mat in _mesh.materials)
            {
                if ( !_mat.name.Contains("MatForce") )
                {
                    _matList.Add(_mat);
                }
            }

            _mesh.materials = _matList.ToArray();

        }


        private void MeshOutline()
        {

            if (_selectedMeshes.Count != 0)
            {
                foreach (MeshRenderer _mesh in _selectedMeshes)
                {

                    foreach (Material _mat in _mesh.materials)
                    {
                        if ( !_mat.name.Contains("MatForce") ) continue;

                        float _multiplier = _mesh == _selectedMesh ? 4 : 1;

                        _mat.color = new Color( _mat.color.r, _mat.color.g, _mat.color.b, Mathf.Lerp(_mat.color.a, _multiplier, Time.deltaTime * 6) );
                        
                    }

                }
            }

            
            foreach(MeshRenderer _mesh in _forceColliderMeshes)
            {

                if ( _selectedMeshes.Contains(_mesh) ) continue;

                foreach( Material _mat in _mesh.materials )
                {

                    if (!_mat.name.Contains("MatForce")) continue;

                    _mat.color = new Color( _mat.color.r, _mat.color.g, _mat.color.b, Mathf.Lerp(_mat.color.a, 0.25f, Time.deltaTime * 6) );

                }                

            }

        }

        private void MoveSelected()
        {
            if (_selectedMesh == null) return;

            var _charController = _selectedMesh.transform.root.gameObject.GetComponent<CharacterController>();
            
            if (_charController == null) return;

            float _direction = _input.scroll;

            if (_lastMove == null) _lastMove = _mainCamera.transform.forward;

            Vector3 move;

            move = new Vector3 (_input.look.x, _input.look.y, _input.look.y);

            _charController.Move(move);

        }

    }


