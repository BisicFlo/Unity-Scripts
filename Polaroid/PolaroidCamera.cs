using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PolaroidCamera is a realistic instant camera system for Unity.
/// It captures photos from a RenderTexture, prints physical photo objects with ejection animation. 
/// When taking photo of "Risk" objects, it "fixes" the risk and places photos into a world-space photo album
/// It has a limited film count feature
///  
/// 
/// 1) Attach this script to a camera GameObject (one with the Camera component).  
/// 2) Create a Render Texture in your Assets folder (Create -> Rendering -> Render Texture) for example "CameraRenderTexture.renderTexture".
/// 3) In the Camera component drag your Render Texture in the "Output" field.
/// 4) (Optional) Create a Child GameObject of your camera GameObject with the script "Laser.cs" if you want object detection.
/// 5) Create a Child GameObject of your camera GameObject with a "Canvas" ( In World Space).
/// 6) Create a empty Child of your Canvas with a "Raw Image" component .
/// 7) Set the "Texture" of your Raw Image to your Render Texture "CameraRenderTexture.renderTexture".
/// 8) Create a prefab of the Photo (Canvas + image)
/// 
/// 
/// /// Digital Camera  -> Camera Component -> "Output" : CameraRenderTexture.renderTexture (Create -> Rendering -> Render Texture)
///                 -> PhotoCapture.cs
///                 -> Child -> CameraView and Laser -> Pyramid Mesh "IsTrigger" -> "Laser.cs"
///                 -> Child -> Canvas (World Space) -> Raw Image "Texture" : CameraRenderTexture.renderTexture
/// 
/// Photo Prefab -> Canvas -> Image 
/// 
/// Photo Album -> Empty with Blank Photos
/// 
/// 
///</summary>

public class PolaroidCamera : MonoBehaviour {

    // --------------------------------------------------------------
    //   Inspector Fields
    // --------------------------------------------------------------    

    public RenderTexture myRenderTexture; // CameraRenderTexture.renderTexture -> in the project files

    [SerializeField] private GameObject PhotoPrefab; // Used to print photos : Empty -> Child Canvas -> Child Image

    [SerializeField] private Transform PhotoEjectionTransform; // Used to print the photo   

    [SerializeField] private RawImage OutputImageUI; // The LCD screen of the camera where is display in real time the image :
                                                     // RawImage component -> "Texture" : CameraRenderTexture.renderTexture
                                                     // Used to display during fiew seconds the picture taken

    [SerializeField] private Text numberPhotoText;  // Text UI used to display the number of photos remaining 

    [SerializeField] private int numberPhoto; // number of photos remaining 

    [SerializeField] private Laser Laser_Script; // You need "Laser.cs", used to know what the camera can see or can't see   

    // --------------------------------------------------------------
    //   Private 
    // --------------------------------------------------------------

    private bool isBusy = false; // Used to prevent taking photos if the camera is already printing one

    private Properties Properties_Script; // Properties.cs is on every Risk

    private WaitForSeconds tempo = new WaitForSeconds(0.1f);


    // --------------------------------------------------------------
    //   MonoBehaviour
    // --------------------------------------------------------------

    public void OnButtonPressed() {
        // Called to take Pictures with the camera
        if (!isBusy) {
            isBusy = true;
            StartCoroutine(FreezeImage()); // Display the photo taken on the screen for fiew seconds
            StartCoroutine(CaptureRenderTexture(myRenderTexture)); 
        } else {
            Debug.Log("is Busy");
        }
    }

    IEnumerator CaptureRenderTexture(RenderTexture renderTexture) {

        // Wait for the end of the frame to ensure the render texture is updated 
        yield return new WaitForEndOfFrame();       

        if (numberPhoto > 0) {

            // Save the current active Render Texture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the Render Texture as the active Render Texture
            RenderTexture.active = renderTexture;

            // Create a new Texture2D with the same dimensions as the Render Texture
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            // Restore the original active Render Texture
            RenderTexture.active = currentActiveRT;

            // Create a new Sprite from the Texture2D and assign it to the UI Image           
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            //photoDisplayArea.sprite = sprite;

            // Start Coroutine for effects and printing
            StartCoroutine(PrintPhoto(sprite));

            // Decrease number of photos and display updated value
            numberPhoto--;
            numberPhotoText.text = "" + numberPhoto;
           
        } else {           
            Debug.Log("no Photo Left ");
        }
    }

    IEnumerator PrintPhoto(Sprite mySprite) {

        bool isPhotoGood = false;
        GameObject RiskObject = null;

        //Optional 
        if (Laser_Script != null) {
            // Get the Object in the center from a field of view ("Laser.cs")  
             RiskObject = Laser_Script.GetCenterObject();

            // If the method can find a interesting object (By default object with a name containing "RISK")  it returns null
            if (RiskObject == null) isPhotoGood = false;

            // If the method return a value we perform a Raycast to know if the object is behind a obstacle, a wall for example
            else if (Laser_Script.TestRaycast(RiskObject)) {

                // The selected risk is therefore fixed with the coroutine
                StartCoroutine(FixCoroutine(RiskObject));

                // The photo is considered Good 
                isPhotoGood = true;
            } else {
                isPhotoGood = false;
                Debug.Log("Obstacle detected");
            }
        }

        // The photo is instantiate and the image component is updated 
        GameObject Photo = Instantiate(PhotoPrefab, PhotoEjectionTransform.position, PhotoEjectionTransform.rotation);
        Photo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Image newImage = Photo.transform.GetChild(0).GetChild(1).transform.GetComponent<Image>();
        newImage.overrideSprite = mySprite;

        //The photo is printing like a Polaroid
        Photo.transform.SetParent(PhotoEjectionTransform, true); // true : worldPositionStay
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 16; i++) {
            Photo.transform.localPosition += new Vector3(0, 0, -0.022f);
            yield return tempo;
        }

        // If the photo is good it's sent to the Album 
        if (isPhotoGood) StartCoroutine(SendPhotoAlbum(RiskObject, Photo)); // NEw
        //else Destroy(Photo, 8f);

        //The Photo becomes a indepedant object 
        Photo.transform.SetParent(null, true);
        ItemConstruction(Photo);
        isBusy = false;

    }

    private void FixRisk(GameObject Risk) {
        Properties_Script = Risk.GetComponent<Properties>();
        if (Properties_Script != null) {
            if (Properties_Script.Fixes.Count > 0) {
                foreach (GameObject obj in Properties_Script.Fixes) {
                    obj.SetActive(true);
                    obj.transform.position = Risk.transform.position; // test ?
                }
            }
        }

        Risk.SetActive(false);
        Laser_Script.RemoveFixedRisk(Risk); // remove from List

    }

    private void ItemConstruction(GameObject MyGameObject) {
        // Adding every components needed 
        Rigidbody rb = MyGameObject.AddComponent<Rigidbody>();
        rb.linearDamping = 2;
        rb.mass = 0.1f;
        BoxCollider bc = MyGameObject.AddComponent<BoxCollider>();
        bc.size = new Vector3(0.33f, 0.02f, 0.33f);      

    }

    private void ItemDeconstruction(GameObject MyGameObject) {
        // Remove components
        Rigidbody rb = MyGameObject.GetComponent<Rigidbody>();
        Destroy(rb);
    }


    private void CompleteAlbum(GameObject Risk, GameObject Photo) {      
 
        if (Risk.GetComponent<Properties>() == null) Debug.Log("Properties not found on : " + Risk.name);
        if (Risk.GetComponent<Properties>().PrePhotoAlbum == null) Debug.Log("PrePhotoAlbum not found on : " + Risk.name);

        GameObject BlankPhoto = Risk.GetComponent<Properties>().PrePhotoAlbum;

        ItemDeconstruction(Photo);

        Photo.transform.position = BlankPhoto.transform.position;
        Photo.transform.rotation = BlankPhoto.transform.rotation;
        Photo.transform.localScale = new Vector3(1, 1, 1);
        BlankPhoto.SetActive(false);
    }


    IEnumerator FixCoroutine(GameObject Risk) {
        yield return new WaitForSeconds(0.6f);
        FixRisk(Risk);       
    }

    IEnumerator SendPhotoAlbum(GameObject Risk, GameObject Photo) {

        yield return new WaitForSeconds(4f);        
        CompleteAlbum(Risk, Photo); // Put photo on the Album
    }


    private IEnumerator FreezeImage() {

        RenderTexture RenderTextureTemp = new RenderTexture(myRenderTexture.width, myRenderTexture.height, 16, RenderTextureFormat.ARGB32);
        Graphics.CopyTexture(myRenderTexture, RenderTextureTemp);

        RawImage RawImage = OutputImageUI.GetComponent<RawImage>();
        RawImage.texture = RenderTextureTemp;

        yield return new WaitForSeconds(2f); // freeze the image for 2 seconds 
        RawImage.texture = myRenderTexture;

    }
}
