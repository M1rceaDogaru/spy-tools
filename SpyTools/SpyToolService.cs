using Android.App;
using Android.Content;
using Android.OS;
using SpyTools;
using System;
using System.Collections.Generic;

[Service]
[IntentFilter(new String[] { "com.MirceaDogaru.SpyToolService" })]
public class SpyToolService : Service, ShakeDetector.OnShakeListener
{    
    public const string SERVICE_STARTED = "ServiceStarted";

    private IBinder _binder;
    private ShakeDetector _shakeDetector;

    private Notification _ongoingNotification;
    private PendingIntent _pendingIntent;

    private bool _isServiceStarted;
    private MediaService _mediaService;
    private ISharedPreferences _preferences;

    public override void OnCreate()
    {
        base.OnCreate();

        _mediaService = new MediaService();
        _mediaService.OnServiceChanged += mediaService_OnServiceChanged;

        // --- Register this as a listener with the underlying service.
        var sensorManager = GetSystemService(SensorService) as Android.Hardware.SensorManager;
        var sensor = sensorManager.GetDefaultSensor(Android.Hardware.SensorType.Accelerometer);

        _shakeDetector = new ShakeDetector(this);
        SetServiceState(true);

        sensorManager.RegisterListener(_shakeDetector, sensor, Android.Hardware.SensorDelay.Normal);
        _preferences = Application.Context.GetSharedPreferences(ToolsFragment.APP_SETTINGS_NAME, FileCreationMode.Private);
    }

    private void mediaService_OnServiceChanged(object sender, EventArgs e)
    {
        var args = e as UnhandledExceptionEventArgs;

        if (e != null)
        {
            // Not nice. Need to make my own event args to pass exception message or whatever
            UpdateRecordingService(((Exception)args.ExceptionObject).Message);
        }
        else
        {
            UpdateRecordingService();
        }
    }

    public override void OnDestroy()
    {
        var sensorManager = GetSystemService(SensorService) as Android.Hardware.SensorManager;
        var sensor = sensorManager.GetDefaultSensor(Android.Hardware.SensorType.Accelerometer);

        sensorManager.UnregisterListener(_shakeDetector);
        SetServiceState(false);

        base.OnDestroy();
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        UpdateRecordingService();
        SetServiceState(true);
        return StartCommandResult.Sticky;
    }

    public override IBinder OnBind(Intent intent)
    {
        _binder = new SpyToolServiceBinder(this);
        return _binder;
    }

    public void OnShake(int count)
    {
        var recordingType = GetRecordingTypeSetting();

        if (_mediaService.IsRecording())
            _mediaService.StopRecording();
        else
            _mediaService.StartRecording(recordingType);

        UpdateRecordingService();
    }

    void UpdateRecordingService(string message = null)
    {
        _ongoingNotification = new Notification(Resource.Drawable.Icon, GetString(Resource.String.MediaRecordingService));
        _pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0);

        _ongoingNotification.SetLatestEventInfo(this, 
            GetString(Resource.String.MediaRecordingService), 
            message ?? (_mediaService.IsRecording() ? GetString(Resource.String.RecordingInProgress) : GetString(Resource.String.RecordingStopped)), 
            _pendingIntent);

        StartForeground((int)NotificationFlags.ForegroundService, _ongoingNotification);        
    }

    void SetServiceState(bool value)
    {
        var prefs = Application.Context.GetSharedPreferences(ToolsFragment.APP_SETTINGS_NAME, FileCreationMode.Private);
        var prefEditor = prefs.Edit();
        prefEditor.PutBoolean(SERVICE_STARTED, value);
        _isServiceStarted = value;
    }

    MediaService.RecordType GetRecordingTypeSetting()
    {
        var isVrOn = _preferences.GetBoolean(ToolsFragment.SETTING_VR, false);
        var isArOn = _preferences.GetBoolean(ToolsFragment.SETTING_AR, false);
        var isCrOn = _preferences.GetBoolean(ToolsFragment.SETTING_CR, false);

        return isVrOn ? MediaService.RecordType.Video : isArOn ? MediaService.RecordType.Audio : isCrOn ? MediaService.RecordType.Call : MediaService.RecordType.None;
    }
}