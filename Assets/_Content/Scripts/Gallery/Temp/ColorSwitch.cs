using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSwitch : MonoBehaviour
{
    [SerializeField]
    private List<Material> materials;
    [SerializeField]
    private List<MeshRenderer> meshRenderers;

    public void SwitchColors()
    {
        List<Material> tempMaterials = new List<Material>();
        foreach (var material in materials)
        {
            tempMaterials.Add(material);
        }
        foreach (var item in meshRenderers)
        {
            Material material = tempMaterials[Random.Range(0, tempMaterials.Count)];
            tempMaterials.Remove(material);
            item.material = material;
        }
    }
}
