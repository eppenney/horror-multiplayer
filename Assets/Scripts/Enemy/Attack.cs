using UnityEngine;

public class Attack : MonoBehaviour {
    [SerializeField] private float startLag;
    [SerializeField] private float range;
    [SerializeField] private float attackRadius;
    [SerializeField] private int damage;
    [SerializeField] private LayerMask attackable;
    private Animator anim;
    
    void Start() {
        anim = GetComponent<Animator>();
    }

    public void Attack() {
        StartCoroutine(PerformAttackAfterDelay());
    }

    private IEnumerator PerformAttackAfterDelay()
    {
        // Wait for the specified start lag time
        yield return new WaitForSeconds(startLag);

        Vector3 attackPosition = transform.position + transform.forward * range;

        // Perform the attack
        Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackRadius, attackable);
        foreach (Collider hitCollider in hitColliders)
        {
            Debug.Log($"Hit {hitCollider.name} with {damage} damage.");
            // Example: hitCollider.GetComponent<Health>().TakeDamage(damage);
        }

        // Trigger attack animation if applicable
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