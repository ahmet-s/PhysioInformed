using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MouseOverPopUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    TextMeshProUGUI popUpBox;

    [SerializeField] Vector2 size;
    [SerializeField] float posY;
    [SerializeField] string popUpMessage;

    Transform go;
    Vector2 goPos;

    private void Awake()
    {
        popUpBox = GameObject.FindGameObjectWithTag("PopUp").transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        go = this.gameObject.transform;        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        popUpBox.gameObject.SetActive(true);
        
        popUpBox.rectTransform.sizeDelta = size;

        goPos = go.position;
        goPos.y += posY; //add offset
        popUpBox.transform.position = goPos;

        popUpBox.text = popUpMessage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popUpBox.gameObject.SetActive(false);
    }
}
