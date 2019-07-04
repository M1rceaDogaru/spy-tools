using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace SpyTools
{
    public class ToolsFragment : Fragment
    {
        public const string APP_SETTINGS_NAME = "MirceaDogaru.SpyTools";
        public const string SETTING_VR = "IsVrOn";
        public const string SETTING_AR = "IsArOn";
        public const string SETTING_CR = "IsCrOn";

        ToggleButton _toggleVideoButton;
        ToggleButton _toggleAudioButton;
        ToggleButton _toggleCallButton;
        ISharedPreferences _preferences;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _preferences = Application.Context.GetSharedPreferences(APP_SETTINGS_NAME, FileCreationMode.Private);
            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();

            ConfigureToggleButtons();
            GetSettings();
            CheckFlagsAndToggleService();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.Tools, container, false);
        }

        void CheckFlagsAndToggleService()
        {
            if (!_toggleVideoButton.Checked && !_toggleCallButton.Checked && !_toggleAudioButton.Checked)
                Activity.StopService(new Intent(Activity, typeof(SpyToolService)));
            else if (!_preferences.GetBoolean(SpyToolService.SERVICE_STARTED, false))
                Activity.StartService(new Intent(Activity, typeof(SpyToolService)));
        }

        void SaveSetting(string settingName, bool value)
        {
            var prefEditor = _preferences.Edit();
            prefEditor.PutBoolean(settingName, value);
            prefEditor.Commit();
        }

        void GetSettings()
        {
            _toggleVideoButton.Checked = _preferences.GetBoolean(SETTING_VR, false);
            _toggleAudioButton.Checked = _preferences.GetBoolean(SETTING_AR, false);
            _toggleCallButton.Checked = _preferences.GetBoolean(SETTING_CR, false);
        }

        void ConfigureToggleButtons()
        {
            _toggleVideoButton = View.FindViewById<ToggleButton>(Resource.Id.toggleVr);
            _toggleVideoButton.Click += (obj, evt) =>
            {
                SaveSetting(SETTING_VR, _toggleVideoButton.Checked);
                CheckFlagsAndToggleService();
            };

            _toggleAudioButton = View.FindViewById<ToggleButton>(Resource.Id.toggleAr);
            _toggleAudioButton.Click += (obj, evt) =>
            {
                SaveSetting(SETTING_AR, _toggleAudioButton.Checked);
                CheckFlagsAndToggleService();
            };

            _toggleCallButton = View.FindViewById<ToggleButton>(Resource.Id.toggleCr);
            _toggleCallButton.Click += (obj, evt) =>
            {
                SaveSetting(SETTING_CR, _toggleCallButton.Checked);
                CheckFlagsAndToggleService();
            };
        }
    }
}