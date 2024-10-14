using System;
using System.IO;
using UnityEngine;

public static class ShareUtils
{
	public static void OpenShareComposer(string title, string subject, string body)
	{
#if UNITY_IOS
		NativeShare nativeShare = new NativeShare();
		if (!string.IsNullOrEmpty(title))
		{
			nativeShare.SetTitle(title);
		}
		nativeShare.SetSubject(subject);
		nativeShare.SetText(body);
		nativeShare.Share();
#elif UNITY_ANDROID
		AndroidJavaClass intentClass = GetIntentClass();
		AndroidJavaObject intentObject = CreateSendIntent("text/plain");

		//put text and subject extra
		if (!string.IsNullOrEmpty(title))
		{
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), title);
		}
		if (!string.IsNullOrEmpty(subject))
		{
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
		}
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), body);

		StartChooserActivity(intentObject, title);
#else
		Debug.Log($"{title} ({subject}): {body}");
#endif
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="shareWindowTitle"></param>
	/// <param name="caption"></param>
	/// <param name="filePath"></param>
	/// <param name="iOSCallback">This callback needs to be a static function.</param>
	public static void OpenAttachmentShareComposer(string shareWindowTitle, string caption, string filePath, NativeShare.ShareResultCallback iOSCallback)
	{
#if UNITY_IOS && !UNITY_EDITOR
		NativeShare nativeShare = new NativeShare();
		nativeShare.AddFile(filePath);
		nativeShare.SetCallback(iOSCallback);
		nativeShare.Share();
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject currentActivity = AndroidUtils.GetCurrentActivity();
		AndroidJavaObject absoluteFileObject = new AndroidJavaObject("java.io.File", filePath);
		AndroidJavaClass fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider");
		string authority = string.Format("{0}.{1}", AndroidUtils.GetPackageName(), "fileprovider");
		AndroidJavaObject uriObject = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", currentActivity, authority, absoluteFileObject);

		AndroidJavaObject intentObject = CreateSendIntent(uriObject);
		if (!string.IsNullOrEmpty(caption))
		{
			intentObject.Call<AndroidJavaObject>("putExtra", intentObject.GetStatic<string>("EXTRA_TEXT"), caption);
			intentObject.Call<AndroidJavaObject>("putExtra", intentObject.GetStatic<string>("EXTRA_SUBJECT"), caption);
		}
		StartChooserActivity(intentObject, shareWindowTitle);
#endif
	}

#if UNITY_ANDROID
	private static AndroidJavaClass GetIntentClass()
	{
		return new AndroidJavaClass("android.content.Intent");
	}

	private static AndroidJavaObject CreateSendIntent(string type)
	{
		AndroidJavaClass intentClass = GetIntentClass();
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intentObject.Call<AndroidJavaObject>("setType", type);
		return intentObject;
	}

	private static AndroidJavaObject CreateSendIntent(AndroidJavaObject uriObject)
	{
		AndroidJavaObject currentActivity = AndroidUtils.GetCurrentActivity();
		AndroidJavaObject contentResolverObject = currentActivity.Call<AndroidJavaObject>("getContentResolver");
		string mimeType = contentResolverObject.Call<string>("getType", uriObject);
		AndroidJavaObject intentObject = CreateSendIntent(mimeType);
		intentObject.Call<AndroidJavaObject>("putExtra", intentObject.GetStatic<string>("EXTRA_STREAM"), uriObject);
		intentObject.Call<AndroidJavaObject>("addFlags", intentObject.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
		return intentObject;
	}

	private static void StartChooserActivity(AndroidJavaObject intentObject, string title)
	{
		AndroidJavaObject chooser = intentObject.CallStatic<AndroidJavaObject>("createChooser", intentObject, title);
		AndroidJavaObject currentActivity = AndroidUtils.GetCurrentActivity();
		currentActivity?.Call("startActivity", chooser);
	}
#endif
}