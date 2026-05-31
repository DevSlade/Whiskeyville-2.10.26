using UnityEngine;

public class TileInteractionHandler : MonoBehaviour
{
    public string tileType = "junk";

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Tapped 3D Tile: " + tileType);
                    PlayMakerFSM.BroadcastEvent("JUNK_TAPPED");
                }
            }
        }
    }
}
