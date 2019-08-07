using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour {

    World world;

    public Player player;

    public RectTransform highLight;

    public ItemSlot[] ItemSlots;

    int SlotIndex = 0;

    public void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();



        foreach(ItemSlot slot in ItemSlots)
        {
            Debug.Log(world.BlockTypes[slot.ItemId].BlockName);

            slot.Icon.sprite = world.BlockTypes[slot.ItemId].Icon;
            slot.Icon.enabled = true;
        }
    }

    public void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {

            if (scroll > 0)
                SlotIndex--;
            else
                SlotIndex++;

            var length = ItemSlots.Length - 1;

            if (SlotIndex > length)
                SlotIndex = 0;

            if (SlotIndex < 0)
                SlotIndex = length;
        }

        highLight.position = ItemSlots[SlotIndex].Icon.transform.position;

        player.SelectedBlockIndex = ItemSlots[SlotIndex].ItemId;
    }


}

[System.Serializable]
public class ItemSlot
{
    public byte ItemId;
    public Image Icon;
}
