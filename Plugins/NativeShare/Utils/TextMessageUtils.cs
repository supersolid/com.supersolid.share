using UnityEngine;

public static class TextMessageUtils
{
	public static void OpenTextMessageComposer(string body)
	{
#if UNITY_IOS
		Application.OpenURL("sms:?&body=" + EmailUtils.EscapeUrl(body));
#elif UNITY_ANDROID
		ShareUtils.OpenShareComposer(null, null, body);
#endif
	}
}