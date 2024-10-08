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

        public double TimeWhenRegistered { get; internal set; } = double.NegativeInfinity;
        public float LifeDuration = 0f;

        internal bool HasLastMatrix = false;
        internal PositionAndRotation LastMatrix;

        [System.Serializable]
        public struct UIElementProperties<T>
            where T : Graphic
        {
            public MiniMapObject MiniMapObject { get; internal set; }
            public T Graphic { get; internal set; }
            internal CachedGameObject CachedGameObject;
            public bool HasGraphic { get; internal set; }

            public bool IsHidden { get; internal set; }
            public void SetHidden(bool hidden)
            {
                if (hidden == this.IsHidden)
                    return;

                this.IsHidden = hidden;
                this.MiniMapObject.MarkDirty();
            }

            public bool AlwaysRotateTowardsCamera;

            public MapVisibilityType[] AllowedMapVisibilityTypes;
            static readonly MapVisibilityType[] s_mapBigVisibility = new MapVisibilityType[] { MapVisibilityType.Big };
            public void SetOnlyVisibleOnBigMap() => this.AllowedMapVisibilityTypes = s_mapBigVisibility;

            public Vector2 OffsetOnMiniMap;

            public bool HasWorldSpaceSize;
            public Vector2 WorldSpaceSize;


            public readonly void SetRectTransformData(RectTransformData rectTransformData)
            {
                rectTransformData.Apply(this.Graphic.rectTransform);
            }

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

        void OnDestroy()
        {
            // if MiniMap is disabled, it doesn't unregister destroyed objects, so we will unregister from here
            this.TryUnregister();
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

        public void SetLifeOwner(Component component)
        {
            this.LifeOwner = component;
            this.HasLifeOwner = component != null;
        }
    }
}
