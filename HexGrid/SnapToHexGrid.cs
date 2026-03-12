using UnityEngine;

[ExecuteInEditMode]
public class SnapToHexGrid : MonoBehaviour {

    public float HexRadius = 1f;

    [SerializeField] private float scale = 1;

    [SerializeField] private float height = 0;

    [SerializeField] private bool SnapAllTiles = false;


    private void Update() {
        if (SnapAllTiles) {
            SnapAllTiles = false;

            GetTilesAndSnap();
        }
    }

    private void GetTilesAndSnap() {

        int nbChild = this.transform.childCount;

        for (int i = 0; i < nbChild; i++) {
            Transform child = transform.GetChild(i);
            Snap(child);
        }
    }


    private void Snap(Transform hexTile) {

        // _________Position_________
        Vector3 hexPosition = hexTile.position;
        Debug.Log("pos" + hexPosition);
        float offsetX = HexRadius * 2 * scale;
        float offsetZ = HexRadius * 1.73205080757f * scale; // root(3) = 1.73...

        if (offsetX == 0 || offsetZ == 0) return;

        // Calculate Y -> Z in World Space
        Debug.Log("coordinateZ not rounded " + hexPosition.z / offsetZ);

        float coordinateZ = Mathf.Round(hexPosition.z / offsetZ); //from world space position to coordinate in a 2D array
        float zPos = coordinateZ * offsetZ;
        Debug.Log("coordinateZ " + coordinateZ);

        // Calculate X
        float coordinateX = 0;
        float xPos = 0;
        if (coordinateZ % 2 == 1) {  // Offset every odd lignes
            coordinateX = Mathf.Round((hexPosition.x - HexRadius * scale) / offsetX); // - HexRadius * scale
            xPos = coordinateX * offsetX + HexRadius * scale;

        } else {
            coordinateX = Mathf.Round((hexPosition.x) / offsetX); // - HexRadius * scale
            xPos = coordinateX * offsetX;
        }

        Debug.Log("coordinateX " + coordinateX);

        // Calculate Z
        float yPos = height;

        Vector3 newPosition = new Vector3(xPos, yPos, zPos);

        hexTile.position = newPosition;

        // _________Rotation_________
        //Snaps the world Y rotation to the nearest 60° increment:

        Vector3 currentEuler = hexTile.eulerAngles;

        float yRotation = currentEuler.y;

        // Convert to range -180 to +180 
        if (yRotation > 180f) yRotation -= 360f;


        float steps = Mathf.Round(yRotation / 60f);
        float snappedY = steps * 60f;

        hexTile.eulerAngles = new Vector3(0, snappedY, 0);

    }
}



