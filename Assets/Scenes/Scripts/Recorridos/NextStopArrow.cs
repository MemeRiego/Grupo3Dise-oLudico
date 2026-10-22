using UnityEngine;

public class NextStopArrow : MonoBehaviour
{

  [SerializeField] private RouteManager route;
  [SerializeField] private SpriteRenderer arrowSprite;
  [SerializeField] private float hideDistance = 3f;

  
  private void LateUpdate()
  {
    BusStop next = route.NextStop;
    bool show = next != null && Vector2.Distance(transform.position, next.transform.position) > hideDistance;
    arrowSprite.enabled = show;
    if (!show) return;

    Vector2 direction = next.transform.position - transform.position;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // el sprite apunta hacia arriba
    transform.rotation = Quaternion.Euler(0f, 0f, angle);
  }
}
