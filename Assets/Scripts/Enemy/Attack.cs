using UnityEngine;
using Unity.Netcode;
using System.Collections;


public class Attack : NetworkBehaviour {
    [SerializeField] private string attackName = "Default Attack";
    [SerializeField] private float startLag = 0.5f;
    [SerializeField] private float endLag = 1.0f;
    private float range;
    [SerializeField] private Vector3 attackPos = new Vector3(0.0f, 1.0f, 1.0f);
    public float Range { get { return range; } } // Not so sure about this, we'll see how it goes 
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private Vector2 damageRange = new Vector2(10, 25);
    [SerializeField] private LayerMask attackable;
    private Animator anim1;
    private Animator anim2;
    
    void Initialize() {
        if (anim1 == null) anim1 = transform.GetChild(0).GetComponent<Animator>();
        if (anim2 == null) anim2 = transform.GetChild(1).GetComponent<Animator>();
        range = attackPos.z;
    }

    public override void OnNetworkSpawn() {
        Initialize();
    }
    void Start() {
        Initialize();
    }

    [ServerRpc]
    public void AttackServerRpc()
    {
        if (IsServer)
        {
            StartCoroutine(PerformAttackAfterDelay());
        }
    }

    // Private, accessible only through ServerRPC above
    private IEnumerator PerformAttackAfterDelay()
    {
        TriggerAttackAnimationClientRpc();
        yield return new WaitForSeconds(startLag);

        Vector3 attackPosition = transform.position + attackPos.y * transform.up + attackPos.z * transform.forward;

        int damage = -1 * (int) Random.Range(damageRange.x, damageRange.y);

        // Perform the attack
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius, attackable);
        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log($"Hit {hitCollider.name} with {damage} damage using {attackName}.");
            Health hp_component = hitCollider.GetComponent<Health>();
            hp_component.AdjustHP(damage);
        }
        yield return new WaitForSeconds(endLag);
    }

    // Activates animation in all clients 
    [ClientRpc]
    private void TriggerAttackAnimationClientRpc()
    {
        if (anim1 != null)
        {
            anim1.SetTrigger("Attack");
            anim2.SetTrigger("Attack");
        }
    }

    public bool TargetInRange(Transform p_target) {
        Vector3 attackPosition = transform.position + attackPos.y * transform.up + attackPos.z * transform.forward;

        // Debug.Log($"{range}, {Vector3.Distance(attackPosition, p_target.position)}");
        // Debug.Log(Vector3.Distance(attackPosition, p_target.position) < range);
        return Vector3.Distance(attackPosition, p_target.position) < range;

    }

    private void OnDrawGizmosSelected()
    {
        Vector3 attackPosition = transform.position + attackPos.y * transform.up + attackPos.z * transform.forward;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }
}