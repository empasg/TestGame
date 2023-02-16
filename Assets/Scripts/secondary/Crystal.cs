using UnityEngine;
using Photon.Pun;

public class Crystal : MonoBehaviour
{

    public AudioClip[] CrystalHitClips;
    public float CrystalHitVolume = 0.25f;

    public float Damage;
    public GameObject Parent;
    private Rigidbody _rb;

    private bool _connected = false;

    private float time = 60;

    private void Start()
    {

        _rb = GetComponent<Rigidbody>();

        Physics.IgnoreCollision(Parent.GetComponent<CharacterController>(), transform.GetComponent<CapsuleCollider>(), true);

        foreach (Transform child in Parent.transform.GetComponentsInChildren<Transform>())
        {

            Collider[] collider = child.GetComponents<Collider>();

            foreach (Collider _collider in collider)
            {
                Physics.IgnoreCollision(_collider, transform.GetComponent<CapsuleCollider>(), true);
            }
        }

    }

    private void FixedUpdate()
    {

        DeleteTimer();

    }

    private void DeleteTimer()
    {
        
        time -= Time.deltaTime;

        if (time <= 0) PhotonNetwork.Destroy(gameObject);

    }

    private void OnCollisionEnter(Collision collision) 
    {

        if (!_connected)
        {

            var _crystalCollide = collision.gameObject.GetComponent<Crystal>();

            if (collision.transform.root.gameObject == Parent) return;
            if (_crystalCollide && _crystalCollide.Parent == Parent) return;

            var _combat = collision.transform.root.gameObject.GetComponent<CombatAsset.Combat>();

            if (_combat)
            {

                _combat.TakeDamage(Damage, CombatAsset.DamageType.sword);

                _rb.isKinematic = true;

            }

            transform.rotation = Quaternion.FromToRotation(Vector3.forward, collision.GetContact(0).normal);
            transform.position = collision.GetContact(0).point;

            transform.SetParent(collision.transform, true);

            _rb.constraints = RigidbodyConstraints.FreezeAll;

            _connected = true;

            foreach (Transform child in Parent.transform)
            {

                if (!child.tag.Contains("Skeleton")) continue;
                
                foreach (Transform skeletonChild in child.GetComponentsInChildren<Transform>())
                {
                    
                    Collider[] collider = skeletonChild.GetComponents<Collider>();

                    foreach (Collider _collider in collider)
                    {
                        Physics.IgnoreCollision(_collider, transform.GetComponent<CapsuleCollider>(), false);
                    }

                }
            }

            if (CrystalHitClips.Length > 0)
            {

                int index = Random.Range(0, CrystalHitClips.Length-1);

                AudioSource.PlayClipAtPoint(CrystalHitClips[index], transform.position, CrystalHitVolume);

            }

        }
        else
        {

            var _crystalCollide = collision.gameObject.GetComponent<Crystal>();
            if (_crystalCollide) return;

            var _combat = collision.transform.root.gameObject.GetComponent<CombatAsset.Combat>();
            
            if (!_combat) return;

            _combat.TakeDamage(Damage/10, CombatAsset.DamageType.sword);

            transform.parent = null;

            transform.rotation = Quaternion.FromToRotation(Vector3.forward, collision.GetContact(0).normal);
            transform.position = collision.GetContact(0).point;

            transform.SetParent(collision.transform, true);

            _rb.isKinematic = true;
        
        }


    }   

}
