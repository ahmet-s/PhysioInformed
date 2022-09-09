using UnityEngine;
using System.Runtime.InteropServices;

public class Link : MonoBehaviour
{
	public void OpenLinkJSPlugin()
	{
#if !UNITY_EDITOR
		openWindow("https://forms.gle/Ekmhb4ZeDreo7ovE6");
#endif
	}

	[DllImport("__Internal")]
	private static extern void openWindow(string url);
}