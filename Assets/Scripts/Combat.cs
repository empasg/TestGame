using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Animations;
using UnityEngine.VFX;

using Photon.Pun;

using StarterAssets;
using InventoryAsset;

namespace CombatAsset
{

    public enum DamageType
    {
        sword, bleeding
    }

    public enum DashType
    {
        visible, invisible
    }

    public enum Mode
    {
        Base, GetsugaTenshou
    }

    public class Skill
    {
        public string Name;
        public float cd;
        public float cdTime;

        public Skill(string _name, float _cd, float _cdTime = 0)
        {

            Name = _name;
            cd = _cd;
            cdTime = _cdTime;

        }

        public bool IsReady()
        {
            return cdTime == 0;
        }

    }

    public class Combat : MonoBehaviour
    {
        public float Health = 100;
        public float AttackSpeed = 1;
        public float Damage = 1;
        public DashType dashType;
        public float dashSpeed = 1;
        public float dashRange = 1;
        public float BlockCD = 1;
        public float CDSkillMultiplier = 1;
        public float ModeActivationMultiplier = 1;
        public float ModeDurationMultiplier = 1;
        public Vector3 AttackSize;
        private Vector3 _attackSizeBuff;
        public Vector3 AttackCenter;
        private Vector3 _attackCenterBuff;
        public float BonusAttackCD = 1;
        public float GripChangeTime = 0.5f;
        public bool CanBreakAttack = true;
        

        public AudioClip[] SwordSlashClips;
        public AudioClip[] SwordSlashHitClips;
        public AudioClip[] SwordSlashBadHitClips;
        public float SwordSlashAudioVolume = 0.75f;

        public AudioClip[] DashInvisibleClips;
        public float DashInvisibleAudioVolume = 0.5f;

        public bool OffsetSetting = false;

        [SerializeField] private Material _invisibleMaterial;

        private float _busyTime;
        private float _stunTime;
        private bool _stun;

        private GameObject _mainCamera;

        private SkinnedMeshRenderer _mesh;
        private CharacterController _controller;
        private ThirdPersonController _TPController;
        private StarterAssetsInputs _input;
        private Animator _animator;
        private Inventory _inventory;
        private PhotonView _photonView;

        private GameObject _boneAttach;

        private GameObject _redCrystal;

        private GameObject _activeWeapon;
        private float _activateWeaponCD = -1;
        private float _attackWeaponCD = -1;
        private float _dashCD = -1;
        private float _blockCD = -1;
        private bool _invisibleDash;
        private float _invisibleDashLerpTime;
        private Vector3 _invisibleDashStartPos;
        private Vector3 _invisibleDashEndPos;
        public bool _block;
        private float _blockResetTime;
        private float _weightLerpTime;

        public string _currentGrip = "Base";
        public string _newGrip = "Base";

        private int _swordComboAttacks = 0;
        private int _swordMaxComboAttacks = 3;
        private float _attackResetTime;
        private List<Material> _meshMaterials = new List<Material>();

        private List<ParticleSystem> _swordTrailEffect = new List<ParticleSystem>();
        private List<ParticleSystem> _swordHitEffect = new List<ParticleSystem>();
        private List<ParticleSystem> _swordHitBlockEffect = new List<ParticleSystem>();
        private List<ParticleSystem> _invisibleDashEffect = new List<ParticleSystem>();
        private List<ParticleSystem> _getsugaTenshouActivateModeEffect = new List<ParticleSystem>();
        private List<ParticleSystem> _getsugaTenshouModeEffect = new List<ParticleSystem>();
        private List<VisualEffect> _getsugaTenshouSlashEffect = new List<VisualEffect>();

        private List<Skill> _skills = new List<Skill>();
        private Skill _currentSkill;

        private TextMeshProUGUI _skillNameUI;
        private bool _skillUIChange;
        private float _skillUITime;
        private float _skillUIGoal = -1;

        private TextMeshProUGUI _skillCdUI; 

        private Mode _mode;
        private float _startModeActivationTime;
        private float _modeActivationTime;
        private float _modeActivationTimeInverse;
        private bool _modeActivated = true;
        private float _modeDuration;

        private float _getsugaTenshouEffectNextBurst = -1;

        private void Start()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            _boneAttach = GameObject.FindGameObjectWithTag("RightHand");

            _mesh = GameObject.FindGameObjectWithTag("PlayerArmatureMesh").GetComponent<SkinnedMeshRenderer>();
            _meshMaterials.AddRange(_mesh.materials);

            _controller = GetComponent<CharacterController>();
            _TPController = GetComponent<ThirdPersonController>();
            _input = GetComponent<StarterAssetsInputs>();
            _animator = GetComponent<Animator>();
            _inventory = GetComponent<Inventory>();
            _photonView = GetComponent<PhotonView>();

            _redCrystal = Resources.Load("RedCrystal") as GameObject;

            foreach (Transform child in GameObject.FindGameObjectWithTag("VFX").transform)
            {
                if (!_photonView.IsMine) return;

                if (child.name.Contains("invisibleDash"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<ParticleSystem>();
                        if (!_ps) continue;

                        _invisibleDashEffect.Add(_ps);

                        _ps.Stop();
                    }
                }

                if (child.name.Contains("swordHit") && !child.name.Contains("swordHitBlock"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<ParticleSystem>();
                        if (!_ps) continue;

                        _swordHitEffect.Add(_ps);

                        _ps.Stop();
                    }
                }

                if (child.name.Contains("swordHitBlock"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<ParticleSystem>();
                        if (!_ps) continue;

                        _swordHitBlockEffect.Add(_ps);

                        _ps.Stop();
                    }
                }   
                if (child.name.Contains("getsugaTenshouActivateMode"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<ParticleSystem>();
                        if (!_ps) continue;

                        _getsugaTenshouActivateModeEffect.Add(_ps);

                        _ps.Stop();
                    }
                }
                if (child.name.Contains("getsugaTenshouMode"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<ParticleSystem>();
                        if (!_ps) continue;

                        _getsugaTenshouModeEffect.Add(_ps);

                        _ps.Stop();
                    }
                }      
                if (child.name.Contains("getsugaTenshouSlash") && !child.name.Contains("getsugaTenshouSlashRef"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<VisualEffect>();
                        if (!_ps) continue;

                        _getsugaTenshouSlashEffect.Add(_ps);

                        _ps.Stop();
                    }

                    child.parent = null;
                }  
                if (child.name.Contains("getsugaTenshouSlashRef"))
                {
                    foreach (Transform _child in child)
                    {
                        var _ps = _child.GetComponent<VisualEffect>();
                        if (!_ps) continue;

                        _ps.Stop();
                    }
                } 

            }

            if (_skills.Count < 1)
            {
                _skills.Add( new Skill("Getsuga Tenshou", 60) );

                _skills.Add( new Skill("Disappointed", 10) );

                _skills.Add( new Skill("Ridiculous", 1.5f) );

                _skills.Sort((x,y) =>
                
                    x.cd.CompareTo(y.cd)
                );
            }

            _skillNameUI = GameObject.FindGameObjectWithTag("SkillName").GetComponent<TextMeshProUGUI>();
            _skillCdUI = GameObject.FindGameObjectWithTag("SkillCd").GetComponent<TextMeshProUGUI>();

        }

        private void Update()
        {
            if (!_photonView.IsMine) return;

            offsetSetting();

            ChangeSkillUI();

            ChangeBusy();
            ChangeStun();

            ChangeGrip();
            ChangeAttackReset();

            ChangeDash();

            ChangeBlock();

            SkillSelect();
            SkillCoolDown();

            ChangeModeActivation();
            ChangeModeDuration();
            ChangeMode();
            ModeEffect();

            if (_busyTime != 0 || _stunTime != 0) return;

            OutStun();

            ActivateWeapon();

            Attack();
            AttackReset();

            Dash();

            Block();

            SkillUse();

        }

        private void offsetSetting()
        {

            if(!_activeWeapon || !OffsetSetting) return;

            _activeWeapon.transform.localPosition = _activeWeapon.GetComponent<PickUpable>().offsetPosition;
            _activeWeapon.transform.localEulerAngles = _activeWeapon.GetComponent<PickUpable>().offsetEulerAngles;

        }

        private void AddBusy(float _time)
        {

            if (_busyTime <= _time)
            {
                _busyTime = _time;
            }
            else
            {
                _busyTime = _time + _busyTime;
            }

        }

        private void ChangeBusy()
        {
            
            _busyTime = Mathf.Clamp( _busyTime - Time.deltaTime, 0, float.MaxValue );

        }

        private void AddStun(float _time)
        {

            _stun = true;

            if (_stunTime <= _time)
            {
                _stunTime = _time;
            }
            else
            {
                _stunTime = _time + _stunTime;
            }

        }

        private void ChangeStun()
        {
            
            _stunTime = Mathf.Clamp( _stunTime - Time.deltaTime, 0, float.MaxValue );

        }

        private void OutStun()
        {

            if (_stunTime == 0 && _stun)
            {
                _TPController.ChangeSpeed(1,1);

                _animator.SetInteger("SwordHitted", 0);
                _animator.speed = 1;

                _stun = false;
                
            }

        }

        private void ActivateWeapon()
        {
            if (_inventory.GetItem(ItemType.weapon) != _activeWeapon) _activeWeapon = null;

            if (_input.r && _activateWeaponCD <= Time.time)
            {

                var _weapon = _inventory.GetItem(ItemType.weapon);

                if (_weapon)
                {     

                    _activeWeapon = _activeWeapon ? null : _weapon;

                    _weapon.SetActive(!_weapon.activeSelf);
                    
                    var rb = _weapon.GetComponent<Rigidbody>();
                    rb.isKinematic = !rb.isKinematic;

                    _weapon.layer = _activeWeapon ? LayerMask.NameToLayer("Item") : LayerMask.NameToLayer("Default");

                    Transform _attach = _activeWeapon ? _boneAttach.transform : null;

                    _weapon.transform.parent = _attach;

                    if (_attach)
                    {
                        _weapon.transform.localPosition = _weapon.GetComponent<PickUpable>().offsetPosition;
                        _weapon.transform.localEulerAngles = _weapon.GetComponent<PickUpable>().offsetEulerAngles;
                    }

                    if (_weapon.name.Contains("sword"))
                    {

                        _newGrip = _activeWeapon ? "Axe" : "Base";

                    }

                    foreach(Transform child in _weapon.transform.GetComponentsInChildren<Transform>())
                    {

                        if (child.name.Contains("swordTrail"))
                        {

                            foreach (Transform _child in child)
                            {

                                if (_activeWeapon)
                                {

                                    _swordTrailEffect.Add( _child.GetComponent<ParticleSystem>() );
                                    _child.GetComponent<ParticleSystem>().Stop();

                                }
                                else
                                {

                                    _swordTrailEffect.Remove( _child.GetComponent<ParticleSystem>() );
                                    _child.GetComponent<ParticleSystem>().Stop();

                                }

                            }

                        }

                    }

                    AttackSpeed = _activeWeapon ? AttackSpeed - _weapon.GetComponent<PickUpable>().speed : AttackSpeed + _weapon.GetComponent<PickUpable>().speed;
                    Damage = _activeWeapon ? Damage + _weapon.GetComponent<PickUpable>().damage : Damage - _weapon.GetComponent<PickUpable>().damage;
    
                    AttackSize = _activeWeapon ? AttackSize + _weapon.GetComponent<PickUpable>().attackSize + _attackSizeBuff : AttackSize - _weapon.GetComponent<PickUpable>().attackSize - _attackSizeBuff;
                    AttackCenter = _activeWeapon ? AttackCenter + _weapon.GetComponent<PickUpable>().attackCenter + _attackCenterBuff : AttackCenter - _weapon.GetComponent<PickUpable>().attackCenter - _attackCenterBuff;
                    
                    float _speed = _activeWeapon ? 0.75f : 1;
                    _TPController.ChangeLongSpeed(_speed, _speed);

                    _activateWeaponCD = Time.time + 1f;

                    AddBusy(1);
                }

            }

            _input.r = false;

        }

        private void ChangeGrip()
        {
            if (_newGrip == _currentGrip) return;

            float _weightLerp = Mathf.Lerp(0, 1, _weightLerpTime / GripChangeTime );
            _weightLerpTime += Time.deltaTime;
            
            _animator.SetLayerWeight(_animator.GetLayerIndex(_newGrip), _weightLerp);
            _animator.SetLayerWeight(_animator.GetLayerIndex(_currentGrip), 1 - _weightLerp);

            _TPController.ChangeSpeed(_weightLerp,_weightLerp);
            
            if (_weightLerp == 1)
            {
                _currentGrip = _newGrip;
                _weightLerpTime = 0;
            }  

        }

        private void Attack()
        {
            
            if (_activeWeapon)
            {
                
                if (_input.leftClick && _attackWeaponCD <= Time.time)
                {
                    
                    if (_activeWeapon.name.Contains("sword"))
                    {
                        
                        if (_swordComboAttacks < _swordMaxComboAttacks)
                        {
                            
                            foreach (ParticleSystem _ps in _swordTrailEffect)
                            {
                                _ps.Play();
                            }

                            _swordComboAttacks++;

                            _animator.SetInteger("Attack", _swordComboAttacks);

                            _animator.speed = ( AttackSpeed * 2 ) / ( AttackSpeed * AttackSpeed );
                            
                            _attackWeaponCD = _swordComboAttacks == _swordMaxComboAttacks ? Time.time + AttackSpeed + BonusAttackCD : Time.time + AttackSpeed * 0.75f;

                            _attackResetTime = AttackSpeed;  

                            _TPController.ChangeSpeed(0,0);                      

                            AddBusy(_attackResetTime);

                        }

                    }

                }

            }

            _input.leftClick = false;
        }

        private void DoAttack(AnimationEvent animationEvent)
        {

            if (!_activeWeapon) return;

            if (_activeWeapon.name.Contains("sword"))
            {

                if (_mode == Mode.GetsugaTenshou)
                {
                    foreach (VisualEffect _vfx in _getsugaTenshouSlashEffect)
                    {
                        _vfx.transform.parent.position = transform.TransformPoint(Vector3.zero);
                        _vfx.transform.parent.forward = transform.forward;

                        _vfx.SetVector3("DistancePower", new Vector3 ( AttackSize.z, AttackSize.y, AttackSize.x ) + new Vector3 ( _attackSizeBuff.z, _attackSizeBuff.y, _attackSizeBuff.x ) * 10 );
                        
                        var pos = AttackCenter + _attackCenterBuff;
                        Quaternion[] rot = new Quaternion[] { Quaternion.Euler( new Vector3( 0, -90, 0) ), Quaternion.Euler( new Vector3( 75, -90, 0) ), Quaternion.Euler( new Vector3( 160, -90, 0) ) };

                        var index = Mathf.Clamp(_swordComboAttacks - 1, 0, rot.Length - 1);

                        _vfx.transform.SetLocalPositionAndRotation(pos, rot[index]);

                        _vfx.SetFloat(Shader.PropertyToID("LifeTime"), AttackSpeed );

                        _vfx.Play();
                    }                                
                }

                bool hitted = false;
                Collider[] characterColliders = Physics.OverlapBox(transform.TransformPoint(AttackCenter),AttackSize + _attackSizeBuff, Quaternion.Euler(0,transform.eulerAngles.y,0), LayerMask.NameToLayer("Ground"));
                Collider[] defaultColliders = Physics.OverlapBox(transform.TransformPoint(AttackCenter),AttackSize + _attackCenterBuff, Quaternion.Euler(0,transform.eulerAngles.y,0), LayerMask.NameToLayer("Ground"));

                foreach(Collider _collider in characterColliders)
                {
                    
                    var _combat = _collider.transform.root.gameObject.GetComponent<Combat>();
                    if (!_combat || _collider.transform.root.gameObject == gameObject) continue;

                    if (SwordSlashHitClips.Length != 0)
                    {
                        var index = Random.Range(0,SwordSlashHitClips.Length);
                        
                        AudioSource.PlayClipAtPoint(SwordSlashHitClips[index], _collider.transform.root.gameObject.GetComponent<CharacterController>().center, SwordSlashAudioVolume);
                        
                    }  
                    hitted = true;

                    var animID = _combat._swordComboAttacks;

                    _combat.TakeDamage(Damage, DamageType.sword, animID, _combat.AttackSpeed);
                    
                }

                if (!hitted)
                {

                    foreach(Collider _collider in defaultColliders)
                    {

                        if (_collider.transform.root.gameObject == gameObject) continue;
                        
                        if (SwordSlashBadHitClips.Length != 0)
                        {
                            var index = Random.Range(0,SwordSlashBadHitClips.Length);
                            
                            AudioSource.PlayClipAtPoint(SwordSlashBadHitClips[index], _collider.ClosestPoint(transform.position), SwordSlashAudioVolume);

                        } 
                        if (CanBreakAttack) StartCoroutine(BreakAttack());

                        hitted = true;

                        break;
                    }

                }


                if (!hitted)
                {
                    var index = Random.Range(0,SwordSlashClips.Length);
                            
                    AudioSource.PlayClipAtPoint(SwordSlashClips[index], transform.TransformPoint(_controller.center) + transform.forward, SwordSlashAudioVolume);
                }

            }

        }
        
        private IEnumerator BreakAttack()
        {

            _animator.SetBool("BreakAttack", true);

            AddBusy(1f);

            _attackResetTime = 0;
            _attackWeaponCD = Time.time + AttackSpeed + BonusAttackCD;

            yield return new WaitForSeconds(0.1f);

            _animator.SetBool("BreakAttack", false);

        }

        private void AttackReset()
        {
            if (_attackResetTime == 0 && _swordComboAttacks != 0)
            {
                _animator.speed = 1;
                _swordComboAttacks = 0;
                _animator.SetInteger("Attack", _swordComboAttacks);

                _TPController.ChangeSpeed(1,1);

                foreach (ParticleSystem _ps in _swordTrailEffect)
                {
                    _ps.Stop();
                }

            }

        }

        private void ChangeAttackReset()
        {
            _attackResetTime = Mathf.Clamp(_attackResetTime - Time.deltaTime, 0, float.MaxValue);
        }

        private void OnDrawGizmos() 
        {
            Gizmos.DrawCube(transform.TransformPoint(AttackCenter + _attackCenterBuff),AttackSize + _attackSizeBuff);
        }

        public void TakeDamage(float amount, DamageType dmgType, int animationID = 0, float attackSpeed = 1)
        {

            Health = Mathf.Clamp( Health - amount * (_block ? 0 : 1), 0, float.MaxValue );

            if (dmgType == DamageType.sword)
            {

                if (Health > 0)
                {

                    _animator.SetInteger("SwordHitted", animationID);

                    if (_block)
                    {

                        _blockResetTime = attackSpeed;

                    }
                    
                    _animator.speed = ( attackSpeed * 2 ) / ( attackSpeed * attackSpeed );

                    AddStun( attackSpeed * (_block ? 0 : 1) );

                    _TPController.ChangeSpeed(0,0);

                    if (!_block)
                    {

                        foreach (ParticleSystem _ps in _swordHitEffect)
                        {
                            _ps.Play();
                        }

                    }
                    else
                    {

                        foreach (ParticleSystem _ps in _swordHitBlockEffect)
                        {
                            _ps.Play();
                        }                        

                    }

                    if (_modeActivationTime != 0)
                    {
                        BreakModeActivating();
                    }
                }
                else
                {
                    _animator.SetInteger("SwordHitted", 0);
                    _animator.SetInteger("SwordDead", animationID);

                    AddStun(float.MaxValue);

                    _TPController.ChangeLongSpeed(0,0);

                }

            }

        }

        private void Dash()
        {

            if (_input.q && _dashCD < Time.time)
            {

                if (dashType == DashType.invisible)
                {

                    RaycastHit hit;

                    int layerMask = LayerMask.GetMask("Default", "Ground", "Character");

                    if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.TransformDirection(Vector3.forward), out hit, dashRange, layerMask))
                    {

                        _invisibleDashStartPos = transform.position;
                        _invisibleDashEndPos = hit.point;

                        _invisibleDash = true;

                        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
                        {

                            var _skinnedMesh = child.GetComponent<SkinnedMeshRenderer>();
                            if (_skinnedMesh)
                            {

                                _skinnedMesh.enabled = false;

                            }

                            var _mesh = child.GetComponent<MeshRenderer>();
                            if (_mesh)
                            {
                                _mesh.enabled = false;
                            }

                        }

                        foreach(ParticleSystem _ps in _invisibleDashEffect)
                        {
                            _ps.Play();
                        }                                       

                    }

                }

                AddBusy( ( dashSpeed * 2 ) / ( dashSpeed * dashSpeed ) );

                _dashCD = Time.time + ( dashSpeed * 2 ) / ( dashSpeed * dashSpeed ) + 0.05f;
            }

            _input.q = false;

        }

        private void ChangeDash()
        {

            if (_invisibleDash)
            {

                Vector3 _invisibleDashLerp = Vector3.Lerp(_invisibleDashStartPos, _invisibleDashEndPos, _invisibleDashLerpTime / ( ( dashSpeed * 2 ) / ( dashSpeed * dashSpeed ) ) );
                _invisibleDashLerpTime += Time.deltaTime;

                transform.position = _invisibleDashLerp;

                if (transform.position == _invisibleDashEndPos)
                {

                    _invisibleDashLerpTime = 0;
                    _invisibleDash = false;

                    foreach (Transform child in transform.GetComponentsInChildren<Transform>())
                    {

                        var _skinnedMesh = child.GetComponent<SkinnedMeshRenderer>();
                        if (_skinnedMesh)
                        {

                            _skinnedMesh.enabled = true;

                        }

                        var _mesh = child.GetComponent<MeshRenderer>();
                        if (_mesh)
                        {
                            _mesh.enabled = true;
                        }

                    }

                    foreach(ParticleSystem _ps in _invisibleDashEffect)
                    {
                        _ps.Play();
                    }

                    if (DashInvisibleClips.Length != 0)
                    {
                        var index = Random.Range(0,DashInvisibleClips.Length);
                            
                        AudioSource.PlayClipAtPoint(DashInvisibleClips[index], transform.TransformPoint(_controller.center), DashInvisibleAudioVolume);
                            
                    } 

                }

            }

        }  

        private void Block()
        {
            
            if (_input.f && !_block && _blockCD <= Time.time && _activeWeapon)
            {

                _block = true;

                _animator.SetBool("Block", _block);

                AddBusy(float.MaxValue);

                _TPController.ChangeSpeed(0,0);

            }

            _input.f = false;
        }  

        private void ChangeBlock()
        {

            _blockResetTime = Mathf.Clamp(_blockResetTime * (_block ? 1 : 0) - Time.deltaTime, 0, float.MaxValue);

            if (_blockResetTime == 0 && _animator.GetBool("Block"))
            {

                _animator.SetInteger("SwordHitted",0);
                _animator.speed = 1;

            }

            if (_input.f && _block && _activeWeapon)
            {

                _block = false;

                _animator.SetBool("Block", _block);

                _busyTime = 0;

                _TPController.ChangeSpeed(1,1);

                _animator.SetInteger("SwordHitted", 0);
                _animator.speed = 1;

                _blockCD = Time.time + BlockCD;

            }

        }


        private void ChangeSkillUI()
        {
            if (_currentSkill == null) return;

            if (_currentSkill.Name != _skillNameUI.text && !_skillUIChange)
            {
                _skillUIChange = true;
                _skillUIGoal = 0;
            }

            if (_skillUIChange)
            {

                float _skillUiLerp = Mathf.Lerp(_skillUIGoal == 255 ? 0 : 255, _skillUIGoal, _skillUITime);
                _skillUITime += Time.deltaTime * 20;

                if (_skillUiLerp == _skillUIGoal)
                {

                    _skillUITime = 0;

                    _skillUIGoal = _skillUIGoal == 0 ? 255 : 0;

                    _skillUIChange = _skillUIGoal == 0 ? false : true;

                    _skillNameUI.text = _currentSkill.Name;

                }

                _skillNameUI.color = new Color32((byte)_skillNameUI.color.r, (byte)_skillNameUI.color.g, (byte)_skillNameUI.color.b, (byte)_skillUiLerp);
                _skillCdUI.color = new Color32((byte)_skillCdUI.color.r, (byte)_skillCdUI.color.g, (byte)_skillCdUI.color.b, (byte)_skillUiLerp);
            }

            if (_currentSkill.cdTime.ToString() != _skillCdUI.text)
            {
                _skillCdUI.text = _currentSkill.cdTime == 0 ? _currentSkill.cd.ToString("F2") : _currentSkill.cdTime.ToString("F2");
            }

        }

        private void SkillSelect()
        {

            if (_skills.Count < 1) return;
            
            int num = -1;

            if (_input._1) num = 0;
            _input._1 = false;

            if (_input._2) num = 1;
            _input._2 = false;

            if (_input._3) num = 2;
            _input._3 = false;

            if (_input._4) num = 3;
            _input._4 = false;

            if (_input._5) num = 4;
            _input._5 = false;                                    

            if ( num >= 0 && _currentSkill != _skills[num] ) _currentSkill = _skills[num];
            
        }

        private void SkillUse()
        {

            if (_input.rightClick && _currentSkill != null && _currentSkill.IsReady())
            {

                if (_currentSkill.Name == "Getsuga Tenshou")
                {

                    ModeActivate(Mode.GetsugaTenshou);

                }

                if (_currentSkill.Name == "Disappointed")
                {

                    StartCoroutine(SkillDisappointed());

                }

                if (_currentSkill.Name == "Ridiculous")
                {

                    StartCoroutine(SkillRidiculous());

                }

                _currentSkill.cdTime = _currentSkill.cd * CDSkillMultiplier;
            }

            _input.rightClick = false;

        }

        #region Skills

        private IEnumerator SkillDisappointed()
        {

            for(int i = 0; i < Mathf.RoundToInt( dashSpeed ); i++)
            {

                float cornerAngle = 2f * Mathf.PI / (float)Mathf.RoundToInt( dashSpeed ) * i;
                float radius = 2;

                var pos = transform.TransformPoint( new Vector3( Mathf.Cos(cornerAngle * 0.5f) * radius, 0, Mathf.Sin(cornerAngle * 0.5f) * radius ) );

                var ilusion = PhotonNetwork.Instantiate(gameObject.name.Replace("(Clone)", ""), pos, Quaternion.identity);
                var _ilusionComp = ilusion.AddComponent<DisappointedIlusion>();

                _ilusionComp.Parent = gameObject;
                _ilusionComp.AliveTime = Health/20;
                _ilusionComp.Damage = Damage;
                _ilusionComp.Speed = _TPController.SprintSpeed * 1.5f;

                foreach (Transform child in ilusion.transform.GetComponentsInChildren<Transform>())
                {
                    
                    var childVfx = child.GetComponent<VisualEffect>(); 
                    if (childVfx)
                    {

                        foreach (Transform _child in transform.GetComponentsInChildren<Transform>())
                        {

                            var _childVfx = _child.GetComponent<VisualEffect>();
                            if (!_childVfx || _childVfx.transform.parent.name != childVfx.transform.parent.name) continue;

                            var list = new List<string>();
                            _childVfx.GetSpawnSystemNames(list);
                            var vinfo = _childVfx.GetSpawnSystemInfo(list[0]);

                            childVfx.Stop();

                        }

                    }

                    var childPs = child.GetComponent<ParticleSystem>(); 
                    if (childPs)
                    {

                        foreach (Transform _child in transform.GetComponentsInChildren<Transform>())
                        {

                            var _childPs = _child.GetComponent<ParticleSystem>();
                            if (!_childPs || _childPs.transform.parent.name != childPs.transform.parent.name) continue;

                            if (!_childPs.isPlaying) childPs.Stop();

                        }

                    }

                }

                yield return new WaitForSeconds( ( dashSpeed * 2 ) / ( dashSpeed * dashSpeed ) );
            }

        }

        private IEnumerator SkillRidiculous()
        {

            _TPController.ChangeSpeed(0,0);

            AddBusy(( Damage * 2 ) / Mathf.Pow(Damage, 3) * Mathf.RoundToInt(Damage));

            for(int i=0; i < Mathf.RoundToInt(Damage); i++)
            {

                Vector3 pos = transform.TransformPoint(_controller.center + Vector3.up * Random.Range(-0.5f,0.5f) + Vector3.right * Random.Range(-0.5f,0.5f) );

                var _newRedCrystal = PhotonNetwork.Instantiate(_redCrystal.name, pos, _mainCamera.transform.rotation);
                var _redCrystalRB = _newRedCrystal.GetComponent<Rigidbody>();
                var _crystal = _newRedCrystal.GetComponent<Crystal>();

                _crystal.Parent = gameObject;
                _crystal.Damage = Damage;

                _redCrystalRB.AddForce(_mainCamera.transform.forward * ( AttackSpeed ) * 50 / ( AttackSpeed * AttackSpeed ), ForceMode.Impulse);

                yield return new WaitForSeconds( ( Damage * 2 ) / ( Mathf.Pow(Damage, 3) ) );

            }

            _TPController.ChangeSpeed(1,1);

        }

        #endregion

        private void SkillCoolDown()
        {

            foreach (Skill _skill in _skills)
            {

                _skill.cdTime = Mathf.Clamp(_skill.cdTime - Time.deltaTime, 0, float.MaxValue);

            }

        }

        private void ModeActivate(Mode mode)
        {

            if (mode == Mode.GetsugaTenshou)
            {

                _startModeActivationTime = 2 * ModeActivationMultiplier;

                _modeActivationTime = _startModeActivationTime;

                _TPController.ChangeSpeed(0,0);

                _modeDuration = 30 * ModeDurationMultiplier;

                AddBusy(_modeActivationTime);

            }

            _mode = mode;
            _modeActivated = false;

        }

        private void ChangeModeActivation()
        {

            _modeActivationTimeInverse = Mathf.Clamp(_modeActivationTimeInverse + Time.deltaTime, 0, _modeActivationTime);

            if (_modeActivationTime != 0)
            {

                if (_mode == Mode.GetsugaTenshou)
                {
                    
                    float _getsugaTenshouActivateLerp = Mathf.Lerp(0,1, _modeActivationTimeInverse / _startModeActivationTime);
                    
                    foreach(ParticleSystem _ps in _getsugaTenshouActivateModeEffect)
                    {

                        if ( _getsugaTenshouEffectNextBurst <= Time.time )
                        {

                            if (_ps.name == "Particles")
                            {

                                float burstInterval = ( 1 - _getsugaTenshouActivateLerp ) / 10 ;

                                int count = (int)Mathf.Ceil(_getsugaTenshouActivateLerp * 100);

                                _ps.Emit(count);
                                
                                _getsugaTenshouEffectNextBurst = Time.time + burstInterval;
                                
                            }

                        }

                    }

                }

            }

            _modeActivationTime = Mathf.Clamp(_modeActivationTime - Time.deltaTime, 0, float.MaxValue);

        }

        private void BreakModeActivating()
        {

            _mode = Mode.Base;
            _modeActivationTime = 0;
            _modeActivationTimeInverse = 0;

            AddStun(1);

        }

        private void ChangeMode()
        {

            if (!_modeActivated && _modeActivationTime <= 0)
            {
                
                if (_mode == Mode.GetsugaTenshou)
                {

                    _TPController.ChangeLongSpeed(2,2);
                    _attackSizeBuff = new Vector3(2,1.5f,5); 
                    _attackCenterBuff = new Vector3(0,0,2.5f); 
                    CanBreakAttack = false;
                
                }

                _TPController.ChangeSpeed(1,1); 

                _modeActivated = true;

            }

        }

        private void ChangeModeDuration()
        {
            if (_modeActivationTime != 0) return;

            _modeDuration = Mathf.Clamp(_modeDuration - Time.deltaTime, 0, float.MaxValue);

            if (_modeDuration <= 0 && _mode != Mode.Base)
            {

                _mode = Mode.Base;

                _TPController.ChangeLongSpeed(1,1);

                _attackSizeBuff = Vector3.zero; 
                _attackCenterBuff = Vector3.zero;

                CanBreakAttack = true;

                _modeActivated = false;

            }

        }

        private void ModeEffect()
        {

            if (_mode == Mode.GetsugaTenshou && _modeActivationTime == 0)
            {

                foreach (ParticleSystem _ps in _getsugaTenshouModeEffect)
                {

                    if (!_invisibleDash)
                    {
                        _ps.Play();
                    }
                    else
                    {
                        _ps.Clear();
                    } 

                }

            }
            else
            {

                foreach (ParticleSystem _ps in _getsugaTenshouModeEffect)
                {

                    _ps.Stop();

                }  

            }

        }

    }

}
