using TMPro;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UGameCore.MiniMap
{
    public class MiniMapObject : MonoBehaviour
    {
        [SerializeField] MiniMap m_MiniMap;
        public MiniMap MiniMap { get => m_MiniMap; internal set => m_MiniMap = value; }

        internal Transform CachedTransform;

        public bool IsRegistered { get; internal set; } = false;

        public bool IsDirty { get; private set; } = true;
        internal PositionAndRotation? LastPositionAndRotation;

        //public Texture2D Texture;
        //public Vector2 TextureSize = Vector2.zero;
        //public Vector2 TexturePivot = Vector2.one * 0.5f;

        //public string Text = string.Empty;
        //public Color TextColor = Color.white;

        public RawImage TextureImage { get; internal set; }
        public TextMeshProUGUI TextComponent { get; internal set; }

        //public string UIName = string.Empty;

        public bool AlwaysRotateTowardsCamera = false;

        public Component LifeOwner;
        public bool HasLifeOwner = false;


        void Awake()
        {
            this.CachedTransform = this.transform;
        }

        void Start()
        {
            if (m_MiniMap != null && !this.IsRegistered)
            {
                m_MiniMap.RegisterObject(this, true, true);
            }
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
