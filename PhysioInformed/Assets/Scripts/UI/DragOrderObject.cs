using UnityEngine;
using UnityEngine.EventSystems;

public class DragOrderObject : MonoBehaviour, IPointerEnterHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    DragOrderContainer container = null;

    //To contain draggable object
    GameObject tempObject;
    float tempObjectOffset = 175f;

    void Start()
    {
        container = GetComponentInParent<DragOrderContainer>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        container.objectBeingDragged = this.gameObject;

        //Create an instance of gameobject to drag alone mouse
        tempObject = Instantiate(Resources.Load<GameObject>($"Preferences/{this.gameObject.name}"), GameObject.FindGameObjectWithTag("RankingPanel").transform, false);
        //Make dragged object invisible
        transform.localScale = Vector3.zero; 
    }
    public void OnDrag(PointerEventData data)
    {
        //Position just right of mouse to not detect OnPointerEnter
        tempObject.transform.position = Input.mousePosition + Vector3.right * tempObjectOffset;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (container.objectBeingDragged == this.gameObject) container.objectBeingDragged = null;
        
        //Destroy temp and make dragged object visible
        Destroy(tempObject);
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject objectBeingDragged = container.objectBeingDragged;

        if (objectBeingDragged != null && objectBeingDragged != this.gameObject)
        {
            //Change index in hierarchy
            objectBeingDragged.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
        }
    }
}
