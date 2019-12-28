using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryBar : MonoBehaviour
{
    public string Resource;
    TextMeshProUGUI Text;
    Image Bar;

    private int TargetVal;
    private float val = 0;

    private float TargetSize;
    private float size = 0;

    private float ChangeSpeed = 1f;

    void Start()
    {
        Text = GetComponentInChildren<TextMeshProUGUI>();
        Bar = GetComponentInChildren<Image>();
    }
    void Update()
    {
        if (PlayerBase.M.Inventory.ContainsKey(Resource))
        {
            TextUpdate();
            BarUpdate();
        }
        else
        {
            Text.text = 0 + "/" + PlayerBase.M.StorageCapacity;
            Bar.rectTransform.sizeDelta = new Vector2(0, Bar.rectTransform.sizeDelta.y);
        }
    }

    void TextUpdate()
    {
        TargetVal = PlayerBase.M.Inventory[Resource];

        if (val != TargetVal)
        {
            ValueLerp(ref val, TargetVal, ChangeSpeed * 7.5f);
            Text.text = (int)val + "/" + PlayerBase.M.StorageCapacity;
        }
    }

    void BarUpdate()
    {
        TargetSize = 750 * ((float)PlayerBase.M.Inventory[Resource] / (float)PlayerBase.M.StorageCapacity);

        if (size != TargetSize)
        {
            ValueLerp(ref size, TargetSize, ChangeSpeed);
            Bar.rectTransform.sizeDelta = new Vector2(size, Bar.rectTransform.sizeDelta.y);
        }
    }

    void ValueLerp(ref float value, float target, float speed)
    {
        if (value != target)
        {
            if (value < target)
            {
                if (value + speed > target)
                    value = target;
                else
                    value += speed;
            }
            else if (value > target)
            {
                if (value - speed < target)
                    value = target;
                else
                    value -= speed;
            }
        }
    }  
}
