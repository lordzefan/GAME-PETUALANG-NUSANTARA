using UnityEngine;

public class RegionButton : MonoBehaviour
{
    public CameraZoomController cameraZoom;
    public Transform regionTarget;
    public GameObject panelInformatif;

    public void OnClick()
    {
        Debug.Log("Tombol ditekan untuk: " + regionTarget.name);
        cameraZoom.ZoomToRegion(regionTarget);
        panelInformatif.SetActive(true);
    }

}
