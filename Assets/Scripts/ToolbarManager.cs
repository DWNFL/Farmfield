using UnityEngine;
using System.Collections.Generic;

public class ToolbarManager : MonoBehaviour
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    [SerializeField] private List<ToolbarSlot> slots = new List<ToolbarSlot>();

    private int selectedIndex = 0;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // Переключение слотов по клавишам 1-9
        for (int i = 0; i < slots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    void SelectSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            selectedIndex = index;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
                slots[i].SetItem(items[i]);
            else
                slots[i].SetItem(null);

            slots[i].SetHighlight(i == selectedIndex);
        }
    }
}
