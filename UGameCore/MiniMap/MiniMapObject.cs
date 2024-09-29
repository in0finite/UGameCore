using TMPro;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static UGameCore.MiniMap.MiniMap;

namespace UGameCore.MiniMap
{
    public class MiniMapObject : MonoBehaviour
    {
        [SerializeField] MiniMap m_MiniMapToRegisterOnStart;
        public MiniMap MiniMap { get; internal set; }

        internal Transform CachedTransform;

        public bool IsRegistered { get; internal set; } = false;

        public bool IsDirty { get; internal set; } = true;

        internal bool HasLastMatrix = false;
        internal PositionAndRotation LastMatrix;

        [System.Serializable]
        public struct UIElementProperties<T>
            where T : Graphic
        {
            public T Graphic { readonly get; internal set; }
            public CachedUnityComponent<T> GraphicCached; // make it field so it's faster to access
            public readonly bool HasGraphic => this.GraphicCached.IsAliveCached;

            public bool IsHidden;
            public bool AlwaysRotateTowardsCamera;

            public MapVisibilityType[] AllowedMapVisibilityTypes;
            static readonly MapVisibilityType[] s_mapBigVisibility = new MapVisibilityType[] { MapVisibilityType.Big };
            public void SetOnlyVisibleOnBigMap() => this.AllowedMapVisibilityTypes = s_mapBigVisibility;

            public Vector2 OffsetOnMiniMap;

            public bool HasWorldSpaceSize;
            public Vector2 WorldSpaceSize;

            public readonly void BringToFrontInLayer()
            {
                this.Graphic.rectTransform.SetAsLastSibling();
            }

            public readonly void BringToBackInLayer()
            {
                this.Graphic.rectTransform.SetAsFirstSibling();
            }

            public readonly void SetSortingLayer(MapSortingLayer sortingLayer)
            {
                RectTransform parent = this.MiniMapObject.MiniMap.SortingLayerParents[(int)sortingLayer];
                this.Graphic.rectTransform.SetParent(parent, true);
                this.MiniMapObject.MarkDirty();
            }
        }

        public UIElementProperties<RawImage> TextureProperties;
        public UIElementProperties<Image> SpriteProperties;
        public UIElementProperties<TextMeshProUGUI> TextProperties;

        public RawImage TextureImage => this.TextureProperties.Graphic;
        public bool HasTextureImage => this.TextureProperties.HasGraphic;

        public Image SpriteImage => this.SpriteProperties.Graphic;
        public bool HasSpriteImage => this.SpriteProperties.HasGraphic;

        public TextMeshProUGUI TextComponent => this.TextProperties.Graphic;
        public bool HasTextComponent => this.TextProperties.HasGraphic;

        public Component LifeOwner;
        public bool HasLifeOwner = false;



        void Awake()
        {
            this.CachedTransform = this.transform;
        }

        void Start()
        {
            if (m_MiniMapToRegisterOnStart != null && !this.IsRegistered)
            {
                m_MiniMapToRegisterOnStart.RegisterObject(this, true, true, true);
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

        public bool TryUnregister()
        {
            if (!this.IsRegistered)
                return false;

            if (null == this.MiniMap)
                return false;

            return this.MiniMap.TryUnregisterObject(this);
        }
    }
}
