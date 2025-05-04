using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    public float zoomSize = 5f;
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;

    private Vector3 targetPosition;
    private float targetZoom;
    private bool isZooming = false;

    private Camera cam;

    // Tambahan: simpan posisi & zoom awal
    private Vector3 originalPosition;
    private float originalZoom;

    void Start()
    {
        cam = Camera.main;
        targetZoom = cam.orthographicSize;
        targetPosition = cam.transform.position;
        

        // Simpan posisi dan zoom awal
        originalPosition = cam.transform.position;
        originalZoom = cam.orthographicSize;
    }

    void Update()
    {
        if (isZooming)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, Time.deltaTime * moveSpeed);

            if (Mathf.Abs(cam.orthographicSize - targetZoom) < 0.05f &&
                Vector3.Distance(cam.transform.position, targetPosition) < 0.05f)
            {
                isZooming = false;
                Debug.Log("Zoom selesai");
            }
        }
    }

    public void ZoomToRegion(Transform regionTransform)
    {
        Debug.Log("Zooming to: " + regionTransform.name);

        targetPosition = new Vector3(
            regionTransform.position.x,
            regionTransform.position.y,
            cam.transform.position.z
        );
        targetZoom = zoomSize;
        isZooming = true;
    }

    // Tambahan: Fungsi reset kamera ke posisi awal
    public void ResetCamera()
    {
        Debug.Log("Reset kamera ke posisi awal");

        targetPosition = originalPosition;
        targetZoom = originalZoom;
        isZooming = true;
       
    }
}
