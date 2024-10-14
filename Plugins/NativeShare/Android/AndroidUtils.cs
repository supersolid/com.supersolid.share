using UnityEngine;
using UnityEngine.Android;

public static class AndroidUtils
{
	private const string API_PREFIX = "API-";
	private const string API_DELIMITER = " ";

	private static string androidId = null;
	private static string advertisingId = null;

	private const int ANDROID_13_SDK_LEVEL = 33;

#if UNITY_ANDROID
	public static string GetAndroidId()
	{
		if (string.IsNullOrEmpty(androidId))
		{
			try
			{
				AndroidJavaObject currentActivity = GetCurrentActivity();
				AndroidJavaClass settingsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
				string ANDROID_ID = settingsSecure.GetStatic<string>("ANDROID_ID");
				AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
				androidId = settingsSecure.CallStatic<string>("getString", contentResolver, ANDROID_ID);
			}
			catch { }
		}

		return androidId;
	}

	public static string GetAdvertisingId()
	{
		if (string.IsNullOrEmpty(advertisingId))
		{
			try
			{
				AndroidJavaObject currentActivity = GetCurrentActivity();
				AndroidJavaClass clientClass = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
				AndroidJavaObject adInfo = clientClass.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

				bool limitTracking = adInfo.Call<bool>("isLimitAdTrackingEnabled");
				if (!limitTracking)
				{
					advertisingId = adInfo.Call<string>("getId").ToString();
				}
			}
			catch { }
		}

		return advertisingId;
	}

	public static AndroidJavaObject GetCurrentActivity()
	{
#if UNITY_EDITOR
		return null;
#else
		AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		return currentActivity;
#endif
	}

	private static AndroidJavaObject GetCurrentActivityIntent()
	{
		AndroidJavaObject currentActivity = GetCurrentActivity();
		if (currentActivity == null)
		{
			return null;
		}
		return currentActivity.Call<AndroidJavaObject>("getIntent");
	}

	public static string GetDeeplink()
	{
		AndroidJavaObject intent = GetCurrentActivityIntent();
		if (intent == null)
		{
			return null;
		}
		return intent.Call<string>("getDataString");
	}

	public static void ResetDeeplink()
	{
		AndroidJavaObject intent = GetCurrentActivityIntent();
		if (intent == null)
		{
			return;
		}
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "");
		intent.Call<AndroidJavaObject>("setData", uri);
	}

	public static string GetPackageName()
	{
		AndroidJavaObject currentActivity = GetCurrentActivity();
		return currentActivity?.Call<string>("getPackageName");
	}

	public static int GetSDKLevel()
	{
		using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
		{
			return version.GetStatic<int>("SDK_INT");
		}
	}

	public static bool CanOpenUrl(string uri)
	{
		AndroidJavaObject currentActivity = GetCurrentActivity();
		AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", uri);
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent", intentClass.GetStatic<string>("ACTION_VIEW"), uriObject);
		AndroidJavaObject resolveList = packageManager.Call<AndroidJavaObject>("queryIntentActivities", intentObject, 0);
		int count = resolveList.Call<int>("size");
		return count > 0;
	}

	public static void OpenAppSettings()
	{
		try
		{
			AndroidJavaObject currentActivity = GetCurrentActivity();
			string packageName = currentActivity.Call<string>("getPackageName");

			AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
			AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
			using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
			{
				intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
				intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
				currentActivity.Call("startActivity", intentObject);
			}
		}
		catch { }
	}

	public static void TryAskPostNotificationsPermission()
	{
		int sdkLevel = GetSDKLevel();
		if (sdkLevel >= ANDROID_13_SDK_LEVEL)
		{
			TryAskPermission("android.permission.POST_NOTIFICATIONS");
		}
	}

	private static void TryAskPermission(string permission)
	{
		if (Permission.HasUserAuthorizedPermission(permission))
		{
			return;
		}

		// We might still not see a prompt if we've already asked before
		Permission.RequestUserPermission(permission);
	}
#endif

	public static int GetSDKVersion()
	{
		return GetSDKVersion(SystemInfo.operatingSystem);
	}

	public static int GetSDKVersion(string operatingSystem)
	{
		int startPosition = operatingSystem.IndexOf(API_PREFIX) + API_PREFIX.Length;
		int length = operatingSystem.IndexOf(API_DELIMITER, startPosition) - startPosition;
		if (!int.TryParse(operatingSystem.Substring(startPosition, length), out int apiLevel))
		{
			return -1;
		}

		return apiLevel;
	}
}