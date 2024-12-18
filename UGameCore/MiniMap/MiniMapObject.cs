using TMPro;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static UGameCore.MiniMap.MiniMap;
using static UGameCore.MiniMap.MiniMapObject;

namespace UGameCore.MiniMap
{
    [System.Serializable]
    public class MiniMapObjectUIElementInspectorParams<T>
        where T : Graphic
    {
        public bool Create = false;
        public UIElementProperties<T> UIElementProperties;
        public RectTransformData RectTransformData = RectTransformData.Default;
        public Color Color = Color.white;
    }

    [System.Serializable]
    public class MiniMapObjectInspectorParams
    {
        public MiniMapObjectUIElementInspectorParams<RawImage> TextureProperties = new();
        public MiniMapObjectUIElementInspectorParams<Image> SpriteProperties = new();
        public MiniMapObjectUIElementInspectorParams<TextMeshProUGUI> TextProperties = new();

        public Texture2D Texture;
        public Sprite Sprite;
        public string Text;

        public MapSortingLayer SortingLayer = MapSortingLayer.Regular;
        public float LifeDuration = 0f;
        public bool SelfDestroyWhenUnregistered = false;


        public void Register(MiniMap miniMap, MiniMapObject miniMapObject)
        {
            // has to be assigned before registration, because registration will override some fields
            miniMapObject.TextureProperties = TextureProperties.UIElementProperties;
            miniMapObject.SpriteProperties = SpriteProperties.UIElementProperties;
            miniMapObject.TextProperties = TextProperties.UIElementProperties;

            miniMap.RegisterObject(
                miniMapObject, TextureProperties.Create, SpriteProperties.Create, TextProperties.Create, SortingLayer);

            miniMapObject.LifeDuration = LifeDuration;
            miniMapObject.SelfDestroyWhenUnregistered = SelfDestroyWhenUnregistered;

            if (TextureProperties.Create)
            {
                miniMapObject.TextureImage.texture = Texture;
                SetupUIElement(ref miniMapObject.TextureProperties, TextureProperties);
            }

            if (SpriteProperties.Create)
            {
                miniMapObject.SpriteImage.sprite = Sprite;
                SetupUIElement(ref miniMapObject.SpriteProperties, SpriteProperties);
            }

            if (TextProperties.Create)
            {
                miniMapObject.TextComponent.text = Text;
                SetupUIElement(ref miniMapObject.TextProperties, TextProperties);
            }
        }

        void SetupUIElement<T>(
            ref UIElementProperties<T> uiElementProperties, MiniMapObjectUIElementInspectorParams<T> uiElementInspectorParams)
            where T : Graphic
        {
            if (uiElementInspectorParams.Create)
            {
                uiElementInspectorParams.RectTransformData.Apply(uiElementProperties.Graphic.rectTransform);
                uiElementProperties.Graphic.color = uiElementInspectorParams.Color;
            }
        }
    }

    public class MiniMapObjectComponent : MonoBehaviour
    {
        [SerializeField] MiniMap m_MiniMapToRegisterOnStart;
        public MiniMapObjectInspectorParams Params;
        public MiniMapObject MiniMapObject { get; private set; }
        public bool IsRegistered => MiniMapObject != null && MiniMapObject.IsRegistered;


        void Start()
        {
            if (m_MiniMapToRegisterOnStart != null && !IsRegistered)
            {
                MiniMapObject = m_MiniMapToRegisterOnStart.CreateWithoutRegistering(gameObject);
                Params.Register(m_MiniMapToRegisterOnStart, MiniMapObject);
            }
        }

        void OnDestroy()
        {
            // if MiniMap is disabled, it doesn't unregister destroyed objects, so we will unregister from here
            if (MiniMapObject != null)
                MiniMapObject.TryUnregister();
            MiniMapObject = null;
        }

        void OnValidate()
        {
            if (MiniMapObject != null)
                MiniMapObject.MarkDirty();
        }
    }

    public class MiniMapObject
    {
        public MiniMap MiniMap { get; internal set; }

        /// <summary>
        /// If specified, <see cref="MiniMapObject"/> will position itself according to this <see cref="Transform"/>,
        /// otherwise it will follow <see cref="WorldTransformation"/>.
        /// </summary>
        public ExistableUnityObject<Transform> FollowedTransform { get; internal set; }

        public bool IsRegistered { get; internal set; } = false;

        public bool IsDirty { get; internal set; } = true;

        public double TimeWhenRegistered { get; internal set; } = double.NegativeInfinity;
        public float LifeDuration = 0f;

        public bool SelfDestroyWhenUnregistered = false;

        internal bool HasLastMatrix = false;
        internal PositionAndRotation LastMatrix;

        public PositionAndRotation WorldTransformation { get; set; } = PositionAndRotation.Identity;

        public string Name;

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

                if (this.MiniMapObject != null)
                    this.MiniMapObject.MarkDirty();
            }

            public bool AlwaysRotateTowardsCamera;

            public MapVisibilityType[] AllowedMapVisibilityTypes;
            static readonly MapVisibilityType[] s_mapBigVisibility = new MapVisibilityType[] { MapVisibilityType.Big };
            public void SetOnlyVisibleOnBigMapNonAlloc() => this.AllowedMapVisibilityTypes = s_mapBigVisibility;
            public bool IsOnlyVisibleOnBigMap { set => this.AllowedMapVisibilityTypes = new MapVisibilityType[] { MapVisibilityType.Big }; }
            public bool IsOnlyVisibleOnSmallMap { set => this.AllowedMapVisibilityTypes = new MapVisibilityType[] { MapVisibilityType.Small }; }

            public Vector2 OffsetOnMiniMap;

            public bool HasWorldSpaceSize;
            public Vector2 WorldSpaceSize;

            public float ColorAlphaMaxDuration;
            public AnimationCurve ColorAlphaCurve;
            public float ColorAlphaMultiplier;


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

            public readonly void SetSiblingIndex(int index)
            {
                this.Graphic.rectTransform.SetSiblingIndex(index);
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

        internal ExistableUnityObject<Component> LifeOwnerExistable;



        public void MarkDirty()
        {
            this.IsDirty = true;
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
            LifeOwnerExistable = component;
        }

        public void SetTimeWhenRegistered(double time)
        {
            time.ThrowIfNan();
            if (time == this.TimeWhenRegistered)
                return;
            this.TimeWhenRegistered = time;
            this.MarkDirty();
        }

        public void SetUIElementsHidden(bool bHidden)
        {
            TextureProperties.SetHidden(bHidden);
            SpriteProperties.SetHidden(bHidden);
            TextProperties.SetHidden(bHidden);
        }

        public void DestroyFollowedGameObject()
        {
            if (FollowedTransform.Object != null)
                FollowedTransform.Object.gameObject.DestroyEvenInEditMode();

            //FollowedTransform = default; // don't reset, because then it won't be removed by Minimap
        }
    }
}
