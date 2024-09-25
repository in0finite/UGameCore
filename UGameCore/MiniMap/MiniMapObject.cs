using TMPro;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.MiniMap
{
    public class MiniMapObject : MonoBehaviour
    {
        [SerializeField] MiniMap m_MiniMap;
        public MiniMap MiniMap => m_MiniMap;

        internal Transform CachedTransform;

        public bool IsDirty { get; private set; } = true;
        internal PositionAndRotation? LastPositionAndRotation;

        public Texture2D Texture;
        public Vector2 TextureSize = Vector2.zero;
        public Vector2 TexturePivot = Vector2.one * 0.5f;

        public string Text = string.Empty;
        public Color TextColor = Color.white;

        public RawImage TextureImage { get; internal set; }
        public TextMeshProUGUI TextComponent { get; internal set; }

        public string UIName = string.Empty;


        void Start()
        {
            this.CachedTransform = this.transform;

            if (m_MiniMap != null)
                m_MiniMap.RegisterObject(this);
        }

        public static MiniMapObject Create(MiniMap miniMap, GameObject go)
        {
            MiniMapObject miniMapObject = go.AddComponent<MiniMapObject>();
            miniMapObject.m_MiniMap = miniMap;
            return miniMapObject;
        }

        public void MarkDirty()
        {
            this.IsDirty = true;
        }

        void OnValidate()
        {
            this.MarkDirty();
        }
    }
}
