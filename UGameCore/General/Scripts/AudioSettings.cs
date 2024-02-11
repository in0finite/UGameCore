using UGameCore.Utilities;
using UnityEngine;

namespace UGameCore
{
    public class AudioSettings : MonoBehaviour, IConfigVarRegistrator
    {
        public FloatConfigVar VolumeConfigVar;

        void IConfigVarRegistrator.Register(IConfigVarRegistrator.Context context)
        {
            this.VolumeConfigVar = new()
            {
                SerializationName = "audio_volume",
                Description = "Global audio volume",
                MinValue = 0f,
                MaxValue = 1f,
                DefaultValueFloat = AudioListener.volume,
                GetValueCallbackFloat = () => AudioListener.volume,
                SetValueCallbackFloat = (val) => AudioListener.volume = val,
            };
            
            context.ConfigVars.Add(this.VolumeConfigVar);
        }
    }
}
