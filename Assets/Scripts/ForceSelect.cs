using UnityEngine;

using StarterAssets;

    public class ForceSelect : MonoBehaviour
    {

        private GameObject _boneAttach;
        private Force _force;
        private MeshCollider _collider;
        private GameObject _mainCamera;
        private GameObject _player;
        private ThirdPersonController _TPController;
        private CharacterController _characterController;
        private Material _material;

        private float _lerpColor;
        private float _time;

        private void Start()
        {
            _boneAttach = GameObject.FindGameObjectWithTag("RightHand");
            _player = transform.parent.gameObject;

            _force = _player.GetComponent<Force>();
            _collider = GetComponent<MeshCollider>();
            _TPController = _player.GetComponent<ThirdPersonController>();
            _characterController = _player.GetComponent<CharacterController>();

            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            _material = GetComponent<SkinnedMeshRenderer>().material;

        }

        private void Update()
        {

            StateAndRemoveMeshes();

            MoveCollider();

            ChangeTransparent();   
        }



        private void OnTriggerEnter(Collider _collider)
        {
            if (_collider.gameObject.transform.root.gameObject.tag.Contains("Player")) return;

            var _colliderMesh = _collider.gameObject.GetComponentInChildren<MeshRenderer>();
            print(_collider.gameObject.transform.root.gameObject.tag);
            if (_colliderMesh != null)
            {

                _force._selectedMeshes.Add(_colliderMesh);
                
            }
                
        }

        private void OnTriggerExit(Collider _collider)
        {

            var _colliderMesh = _collider.gameObject.GetComponentInChildren<MeshRenderer>();
            if (_colliderMesh != null )
            {

                _force._selectedMeshes.Remove(_colliderMesh);

            }
                
        }

        public void StateAndRemoveMeshes()
        {
            _collider.enabled = _force._forceModeActive;

            if (_collider.enabled) return;

            foreach(MeshRenderer _mesh in _force._selectedMeshes.ToArray())
            {
                _force._selectedMeshes.Remove(_mesh);             
            }

        }

        private void MoveCollider()
        {

            Vector3 _eulerRotation = _mainCamera.transform.eulerAngles;
            _eulerRotation.x -= 180;

            transform.rotation = Quaternion.Euler( _eulerRotation );

            transform.position = _boneAttach.transform.position - transform.forward * _force.ForcePower/2;

            float Scale = Mathf.Clamp( _TPController._speed , 1, 10 );

            transform.localScale = new Vector3( Scale, Scale, _force.ForcePower/7.5f );
        }

        private void ChangeTransparent()
        {
            float _forceModeMultiplier = _force._forceModeActive ? 0.05f : 0;

            _lerpColor = Mathf.Lerp( _material.color.a, _forceModeMultiplier, Time.deltaTime * 10 );

            _material.color = new Color(_material.color.r,_material.color.g,_material.color.b, _lerpColor );          

        }

    }