using UnityEngine;
namespace Mkey
{

    public class RayCaster : MonoBehaviour
    {

        public SlotSymbol GetSymbol()
        {
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(transform.position.x, transform.position.y));
            if (hit) { return hit.GetComponent<SlotSymbol>(); }
            else return null;
        }

    }
}