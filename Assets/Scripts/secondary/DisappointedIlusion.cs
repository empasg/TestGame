using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

using StarterAssets;
using InventoryAsset;
using CombatAsset;

using Photon.Pun;

public class DisappointedIlusion : MonoBehaviour
{

    private StarterAssetsInputs _originalInput;
    private StarterAssetsInputs _input;

    public GameObject Parent;
    private Inventory _inventory;
    private Animator _animator;
    private Combat _combat; 
    private ThirdPersonController _TPController;
    private CharacterController _controller;
    private PhotonView _photonView;

    public float AliveTime;
    public float Damage;
    public float Speed;
    
    private List<ParticleSystem> _psCreate = new List<ParticleSystem>();
    public List<VisualEffect> _vfxDestroy = new List<VisualEffect>();

    private void Start()
    {

        _originalInput = Parent.GetComponent<StarterAssetsInputs>();
        _input = GetComponent<StarterAssetsInputs>();
        _TPController = GetComponent<ThirdPersonController>();
        _controller = GetComponent<CharacterController>();
        _inventory = GetComponent<Inventory>();
        _animator = GetComponent<Animator>();
        _combat = GetComponent<Combat>();
        _photonView = GetComponent<PhotonView>();

        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {        

            if (child.name.Contains("createEffect"))
            {
                foreach (Transform _child in child)
                {
                    var _ps = _child.GetComponent<ParticleSystem>();
                    if (!_ps) continue;

                    _psCreate.Add(_ps);

                    _photonView.RPC("_ps.Play()", RpcTarget.All);

                }
            }

            if (child.name.Contains("getsugaTenshouSlashRef"))
            {
                foreach (Transform _child in child)
                {
                    var _vfx = _child.GetComponent<VisualEffect>();
                    if (!_vfx) continue;

                    _vfxDestroy.Add(_vfx);

                }
            }

        }

        _input.move = new Vector2(0,1); 

        EquipmentSync();

        _input.sprint = true;
        
        _TPController.ChangeLongSpeed(Speed/_TPController.SprintSpeed, Speed/_TPController.SprintSpeed);

    }

    private void Update()
    {

        InputSync();

        RemoveTimer();

    }

    private void InputSync()
    {

        _input.look = _originalInput.look;

        _input.rightClick = _originalInput.rightClick;
        _input.q = _originalInput.q;

    }

    private void RemoveTimer()
    {

        AliveTime -= Time.deltaTime;

        if (AliveTime <= 0)
        {

            foreach (ParticleSystem _ps in _psCreate)
            {

                _ps.Play();

            }

            PhotonNetwork.Destroy(gameObject);

        }

    }

    private void EquipmentSync()
    {

        _inventory._equipment = Parent.GetComponent<Inventory>()._equipment; 
        _combat._currentGrip = "Base";
        _combat._newGrip = Parent.GetComponent<Combat>()._currentGrip;   

    }

    private void OnControllerColliderHit(ControllerColliderHit collider) 
    {
        
        if (collider.transform.root.gameObject == Parent ) return;

        var _dissapointedIlusion = collider.transform.root.gameObject.GetComponent<DisappointedIlusion>();
        if (_dissapointedIlusion && _dissapointedIlusion.Parent == Parent) return;

        var _combatCollider = collider.transform.root.gameObject.GetComponent<Combat>();

        if (!_combatCollider && collider.gameObject.layer == LayerMask.NameToLayer("Default"))
        {   
            
            PhotonNetwork.Destroy(gameObject);

        }

        if (_combatCollider)
        {

            _combatCollider.TakeDamage(Damage, DamageType.sword);

            PhotonNetwork.Destroy(gameObject);

        }

    }

    private void OnDestroy() 
    {
        
        foreach (VisualEffect _vfx in _vfxDestroy)
        {

            _vfx.SetVector3( "DistancePower", new Vector3(_controller.radius*50f, Damage*2.5f, _controller.radius*50f) );

            _vfx.transform.parent = null;
            _vfx.transform.position = transform.TransformPoint(_controller.center);
            
            _vfx.Play();

            Destroy(_vfx.gameObject, _vfx.GetFloat(Shader.PropertyToID("LifeTime")));

        }

    }

}
