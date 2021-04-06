using Assets.Scripts.Gameplay.Subsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Container for mapping SfxEvents to respective audio clips
    /// </summary>
    [System.Serializable]
    public struct SFXDataSet
    {
        public ESfxEvent SfxEvent;
        public AudioClip AudioClip;
    }

    /// <summary>
    /// Specifies getter for SfxClipsData
    /// </summary>
    public interface ISFXConfig
    {
        float GetDefaultVolume();
        AudioClip GetAudioClip(ESfxEvent sfxEvent);
    }
    
    [CreateAssetMenu(fileName = "New SFXData", menuName = "GameConfigs/SFXData")]
    public class SFXConfig : ScriptableObject, ISFXConfig
    {
        [SerializeField] private float _defaultVolume;
        [SerializeField] private List<SFXDataSet> _sfxDataSet;

        public float GetDefaultVolume()
        {
            return _defaultVolume;
        }

        public AudioClip GetAudioClip(ESfxEvent sfxEvent)
        {
            var requiredDataSet = _sfxDataSet.Find(dataSet => dataSet.SfxEvent == sfxEvent);
            if (requiredDataSet.SfxEvent != ESfxEvent.NONE)
            {
                return _sfxDataSet.Find(dataSet => dataSet.SfxEvent == sfxEvent).AudioClip;
            }
            Logger.Error("Requested clip is missing");
            return null;
        }
    }
}


