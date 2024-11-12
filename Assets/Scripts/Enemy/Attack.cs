using UnityEngine;
using Unity.Netcode;
using System.Collections;


public class Attack : NetworkBehaviour {
    [SerializeField] private string attackName = "Default Attack";
    [SerializeField] private float startLag = 0.0f;
    [SerializeField] private float range = 1.0f;
    public float Range { get { return range; } } // Not so sure about this, we'll see how it goes 
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private Vector2 damageRange = Vector2(10, 25);
    [SerializeField] private LayerMask attackable;
    private Animator anim;
    
    void Start() {
        anim = GetComponent<Animator>();
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

        Vector3 attackPosition = transform.position + transform.forward * range;

        float damage = Random.Range(damageRange.x, damageRange.y);

        // Perform the attack
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius, attackable);
        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log($"Hit {hitCollider.name} with {damage} damage using {attackName}.");
            Health hp_component = hitCollider.GetComponent<Health>();
            hp_component.AdjustHP(damage);
        }
    }

    // Activates animation in all clients 
    [ClientRpc]
    private void TriggerAttackAnimationClientRpc()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}