using UnityEngine;
using UnityEngine.UI;

public class ToolbarSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject highlight;

    public void SetItem(ItemData data)
    {
        if (data != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (highlight != null)
            highlight.SetActive(isActive);
    }
}
