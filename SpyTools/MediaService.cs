using System;
using Android.Media;
using Java.IO;

namespace SpyTools
{
    public class MediaService
    {
        const string EXTENSION_VIDEO = "mp4";
        const string EXTENSION_AUDIO = "mp3";

        public event EventHandler OnServiceChanged;

        public enum RecordType
        {
            None,
            Audio,
            Video,
            Call
        }

        private MediaRecorder _recorder;
        private RecordType _type;
        private bool _isRecording;

        public void StartRecording(RecordType type)
        {
            _type = type;

            if (_type == RecordType.None)
                return;

            try
            { 
                _recorder = new MediaRecorder();
                _recorder.Error += (o, e) =>
                {
                    _isRecording = false;
                    OnServiceChanged.Invoke(this, EventArgs.Empty);
                };
                        
                _recorder.Reset();
            

                if (_type == RecordType.Video)
                {
                    _recorder.SetAudioSource(AudioSource.Camcorder);
                    _recorder.SetVideoSource(VideoSource.Camera);
                    _recorder.SetOutputFormat(OutputFormat.Mpeg4);
                    _recorder.SetVideoSize(640, 480);
                    _recorder.SetCaptureRate(30);
                    _recorder.SetVideoEncoder(VideoEncoder.Mpeg4Sp);
                    _recorder.SetAudioEncoder(AudioEncoder.Aac);
                }
                else if (_type == RecordType.Audio)
                {
                    _recorder.SetAudioSource(AudioSource.Mic);
                    _recorder.SetOutputFormat(OutputFormat.Default);
                    _recorder.SetAudioEncoder(AudioEncoder.Default);
                }            
            
                _recorder.SetOutputFile(GetFilePath());
                _recorder.Prepare();
                _recorder.Start();
                _isRecording = true;
                OnServiceChanged.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _isRecording = false;
                OnServiceChanged.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }

        public void StopRecording()
        {
            if (_recorder != null)
            {
                _recorder.Stop();
                _recorder.Release();
                _recorder.Dispose();
                _recorder = null;
                _isRecording = false;
            }
        }

        string GetFilePath()
        {
            var folder = GetToolsFolder();

            return string.Format("{0}{1}Rec-{2}.{3}", 
                folder,
                _type == RecordType.Video ? "V" : _type == RecordType.Audio ? "A" : "C", 
                DateTime.Now.ToString("yyyyMMddhhmmss"),
                _type == RecordType.Video ? EXTENSION_VIDEO : EXTENSION_AUDIO);
        }

        public bool IsRecording()
        {
            return _isRecording;
        }

        public static string GetToolsFolder()
        {
            // magic magic magic
            var folder = string.Format("{0}/SpyTools/", Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
            var directory = new File(folder);
            directory.Mkdirs();

            return folder;
        }
    }
}