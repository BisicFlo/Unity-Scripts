using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour
{

    public List<GameObject> Locations = new List<GameObject>();

    public List<GameObject> Fixes = new List<GameObject>();

    public string RiskName; 

    public GameObject PrePhotoAlbum; // used for position / completion of the Album

    public List<GameObject> SimilarRisks = new List<GameObject>(); // 

    public bool CanBeChosen;
}
