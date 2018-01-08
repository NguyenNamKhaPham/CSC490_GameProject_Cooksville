using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Slot : MonoBehaviour, IPointerClickHandler, IDragHandler
{

    private Sprite slotEmpty;
    private Sprite slotHighLight;
    private Sprite slotDisabled;
	public Item thisSlotsItem;
    private bool empty = true;
    public InventoryBehavior parent;
    private bool transferON = true;
	
    // Use this for initialization
	void Start () {
        RectTransform slotRect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void ChangeSprite(Sprite neutral, Sprite highlight, Sprite disabled){
        GetComponent<Image>().sprite = neutral;
        SpriteState st = new SpriteState();
        st.highlightedSprite = highlight;
        st.pressedSprite = neutral;
        st.disabledSprite = disabled;
        Button butt = GetComponent<Button>();
        butt.spriteState = st;
        if (transferON == false)
        {
            butt.interactable = false;
        }
        else
        {
            butt.interactable = true;
        }
        

    }

    public void AddItem(Item thisItem){
        if (thisItem != null)
        {
            thisSlotsItem = thisItem;
            ChangeSprite(thisSlotsItem.spriteNeutral, thisSlotsItem.spriteHighlighted, thisSlotsItem.spriteDisabled);
            empty = false;
        }

    }

    public bool isEmpty()
    {
        return empty;
    }

    public Item RemoveItem()
    {
        Item i = thisSlotsItem;
        if (!empty && transferON)
        {
            thisSlotsItem = null;
            empty = true;
            ChangeSprite(slotEmpty, slotHighLight, slotDisabled);
            return i;

        }
        else
        {
            return null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log(thisSlotsItem.id);
        if (!empty && parent.GetComponent<InventoryBehavior>().clickable)
        {
            if (transferON && parent.other.emptySlots > 0)
            {
                parent.transfer(thisSlotsItem);
                RemoveItem();
            }
            //Debug.Log("delete");
        }

    }

    public Item getItem()
    {
        return thisSlotsItem;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("draged");
    }
    
    public void setTransferON(bool value)
    {
        transferON = value;
        GetComponent<Button>().interactable = value;
    }

}
