using UnityEngine;
using Unity.Netcode;
using System.Collections;


public class Attack : NetworkBehaviour {
    [SerializeField] private string attackName = "Default Attack";
    [SerializeField] private float startLag;
    [SerializeField] private float range;
    public float Range { get { return range; } } // Not so sure about this, we'll see how it goes 
    [SerializeField] private float attackRadius;
    [SerializeField] private int damage;
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