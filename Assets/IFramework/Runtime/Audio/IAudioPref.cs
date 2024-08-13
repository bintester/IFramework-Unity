/*********************************************************************************
 *Author:         anonymous
 *Version:        1.0
 *UnityVersion:   2021.3.33f1c1
 *Date:           2024-08-13
*********************************************************************************/
namespace IFramework.Audio
{
    public interface IAudioPref
    {
        void SetVolume(int channel, float volume);
        float GetVolume(int channel);
    }

}
