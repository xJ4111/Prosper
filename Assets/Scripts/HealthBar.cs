using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image Bar;

    public void UpdateBar(float Current, float Max)
    {
        if (Current != Max)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            Quaternion rot = Quaternion.LookRotation(CameraMovement.M.Cam.gameObject.transform.position - transform.position);
            transform.rotation = rot * Quaternion.Euler(0, 180, 0);
            Bar.rectTransform.sizeDelta = new Vector2(150 * (Current / Max), Bar.rectTransform.sizeDelta.y);

            Bar.color = new Color(2 * (1 - (Current / Max)), (Current / Max), 0);
        }
        else
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }

}
