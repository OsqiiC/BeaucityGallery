using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenImage : MonoBehaviour
{
    [field: SerializeField]
    public RawImage rawImage;

    public Vector2 textureResolution => new Vector2(rawImage.texture.width,rawImage.texture.height);
}
