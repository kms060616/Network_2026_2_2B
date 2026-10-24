using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]
public class SlimeProjectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 12f;
    [SerializeField, Min(0.1f)] private float lifetime = 5f;

    private EnemyHealth target;
    private float damage;
    private float elapsed;
    private bool initialized;
    private bool finished;

    public void Initialize(
        EnemyHealth newTarget,
        float attackDamage,
        Color color)
    {
        target = newTarget;
        damage = attackDamage;
        elapsed = 0f;

        GetComponent<SpriteRenderer>().color = color;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || finished || Time.timeScale == 0f)
            return;

        if (target == null ||
            target.IsDead ||
            !target.isActiveAndEnabled)
        {
            Remove();
            return;
        }

        elapsed += Time.deltaTime;

        if (elapsed >= lifetime)
        {
            Remove();
            return;
        }

        Vector3 destination = target.transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        if ((transform.position - destination).sqrMagnitude <= 0.0001f)
        {
            EnemyHealth hitTarget = target;

            //중복 명중을 막은 뒤 피해를 적용합니다.
            Remove();

            if (hitTarget != null && !hitTarget.IsDead)
                hitTarget.TakeDamage(damage);
        }
    }

    private void Remove()
    {
        if (finished)
            return;

        finished = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
