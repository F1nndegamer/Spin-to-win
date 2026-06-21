using UnityEngine;

public class Inventory : MonoBehaviour
{
    void Awake()
    {
        GameObject Maincamera = GameObject.FindGameObjectWithTag("MainCamera");
        transform.SetParent(Maincamera.transform);
    }
}
