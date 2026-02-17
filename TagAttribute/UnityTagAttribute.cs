using UnityEngine;

/// <summary>
/// Need to be outside the "Editor" folder
/// Prevent build errors because the attribute class would no longer be visible to MonoBehaviour scripts during build.
/// </summary>
public class UnityTagAttribute : PropertyAttribute { }