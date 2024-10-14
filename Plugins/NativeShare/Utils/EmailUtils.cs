using UnityEngine;
using UnityEngine.Networking;

public static class EmailUtils
{
	public static void OpenEmailComposer(string subject, string body)
	{
		OpenEmailComposer("", subject, body);
	}

	public static void OpenEmailComposer(string toAddress, string subject, string body)
	{
		Application.OpenURL("mailto:" + toAddress + "?subject=" + EscapeUrl(subject) + "&body=" + EscapeUrl(body));
	}

	public static string EscapeUrl(string url)
	{
		// fixes the replaced space with the correctly escaped code
		return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
	}
}