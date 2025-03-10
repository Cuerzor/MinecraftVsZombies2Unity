using UnityEngine;

namespace MVZ2.Level.UI
{
    public class MovingBlueprintList : MonoBehaviour
    {
        private void Awake()
        {
            movingBlueprintTemplate.gameObject.SetActive(false);
        }
        public MovingBlueprint CreateMovingBlueprint()
        {
            var item = Instantiate(movingBlueprintTemplate.gameObject, movingBlueprintRoot);
            item.SetActive(true);
            return item.GetComponent<MovingBlueprint>();
        }
        public void RemoveMovingBlueprint(MovingBlueprint blueprint)
        {
            if (!blueprint)
                return;
            Destroy(blueprint.gameObject);
        }
        [SerializeField]
        private MovingBlueprint movingBlueprintTemplate;
        [SerializeField]
        private Transform movingBlueprintRoot;
    }
}
