using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Hardware;

public class ShakeDetector : Java.Lang.Object, ISensorEventListener
{
    /*
     * The gForce that is necessary to register as shake.
     * Must be greater than 1G (one earth gravity unit).
     * You can install "G-Force", by Blake La Pierre
     * from the Google Play Store and run it to see how
     *  many G's it takes to register a shake
     */
    private const float DEBUG_RECORD_TIME = 5000;
    private const float SHAKE_THRESHOLD_GRAVITY = 2.7F;
    private const int SHAKE_SLOP_TIME_MS = 500;
    private const int SHAKE_COUNT_RESET_TIME_MS = 3000;

    private OnShakeListener mListener;
    private long mShakeTimestamp;
    private int mShakeCount;

    public ShakeDetector(OnShakeListener listener)
    {
        mListener = listener;
        mShakeTimestamp = Java.Lang.JavaSystem.CurrentTimeMillis();
    }

    public interface OnShakeListener
    {
        void OnShake(int count);
    }

    public void OnAccuracyChanged(Sensor sensor, int accuracy)
    {
        // ignore
    }

    public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
    {
        //Do nothing
    }

    public void OnSensorChanged(SensorEvent sensorEvent)
    {
        if (mListener != null)
        {
#if DEBUG
            long myTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            if (mShakeTimestamp + DEBUG_RECORD_TIME < myTime) {
                mShakeTimestamp = myTime;
                mListener.OnShake(1);
            }
                        
            return;
#endif
            float x = sensorEvent.Values[0];
            float y = sensorEvent.Values[1];
            float z = sensorEvent.Values[2];

            float gX = x / SensorManager.GravityEarth;
            float gY = y / SensorManager.GravityEarth;
            float gZ = z / SensorManager.GravityEarth;

            // gForce will be close to 1 when there is no movement.
            double gForce = Math.Sqrt(gX * gX + gY * gY + gZ * gZ);

            if (gForce > SHAKE_THRESHOLD_GRAVITY)
            {
                long now = Java.Lang.JavaSystem.CurrentTimeMillis();
                // ignore shake events too close to each other (500ms)
                if (mShakeTimestamp + SHAKE_SLOP_TIME_MS > now)
                {
                    return;
                }

                // reset the shake count after 3 seconds of no shakes
                if (mShakeTimestamp + SHAKE_COUNT_RESET_TIME_MS < now)
                {
                    mShakeCount = 0;
                }

                mShakeTimestamp = now;
                mShakeCount++;

                mListener.OnShake(mShakeCount);
            }
        }
    }
}