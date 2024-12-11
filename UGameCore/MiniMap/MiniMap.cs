using System.Collections.Generic;
using TMPro;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static UGameCore.CommandManager;
using static UGameCore.MiniMap.MiniMapObject;

namespace UGameCore.MiniMap
{
    [DefaultExecutionOrder(70)]
    public class MiniMap : MonoBehaviour, IStatsCollectable
    {
        public GameManager GameManager;
        public CommandManager CommandManager;
        public IGameTimeProvider GameTimeProvider { get; private set; }

        public RectTransform RootTransform;
        public RawImage MapImage;
        public Image BackgroundPanelImage;
        public MiniMapDrawer MiniMapDrawer;

        public GameObject TexturePrefab;
        public GameObject SpritePrefab;
        public GameObject TextPrefab;

        Canvas Canvas;

        readonly ComponentPoolList<RawImage> m_PooledRawImages = new() { ActivateWhenRenting = false, SetParentWhenGettingFromPool = false };
        readonly ComponentPoolList<Image> m_PooledImages = new() { ActivateWhenRenting = false, SetParentWhenGettingFromPool = false };
        readonly ComponentPoolList<TextMeshProUGUI> m_PooledTexts = new() { ActivateWhenRenting = false, SetParentWhenGettingFromPool = false };

        public int NumPooledUIElements => m_PooledRawImages.NumPooledObjects + m_PooledImages.NumPooledObjects + m_PooledTexts.NumPooledObjects;

        readonly HashSetAndList<MiniMapObject> m_MiniMapObjects = new();
        public IReadOnlyCollection<MiniMapObject> MiniMapObjects => m_MiniMapObjects;
        public int NumMiniMapObjects => m_MiniMapObjects.Count;

        public long NumRegistrations { get; private set; } = 0;
        public long NumUnregistrations { get; private set; } = 0;
        public long NumCreatedObjects { get; private set; } = 0;

        public Vector3 WorldCenter = Vector3.zero;
        public Vector3 WorldSize = Vector3.zero;

        public bool InvertXPos = false;
        public bool InvertYPos = false;

        public bool NegateXOffset = false;
        public bool NegateZOffset = false;

        bool m_repositionAllObjects = false;
        Vector2 m_MapImageSize = Vector2.zero;
        Vector2 m_WorldSizeInverted2D = Vector2.zero;
        double m_timeNow = double.NegativeInfinity;

        RectTransformData m_OriginalMapImageData;
        public RectTransformData BigModeRectTransformData = new RectTransformData().WithDefault();
        [Range(0f, 1f)] public float BigMapSizePerc = 0.975f;

        Color m_BackgroundPanelImageOriginalColor;
        public Color BackgroundPanelImageColorBig = Color.black.WithAlpha(0.75f);

        Vector2 LastScreenSize = Vector2.zero;
        float LastCanvasScaleFactor = 1f;

        public enum MapVisibilityType
        {
            Small = 0,
            Big,
            None,
        }

        public static readonly int NumMapVisibilityTypes = System.Enum.GetValues(typeof(MapVisibilityType)).Length;

        public MapVisibilityType VisibilityType { get; private set; } = MapVisibilityType.None;

        public MapVisibilityType DefaultMapVisibilityType = MapVisibilityType.Small;
        public bool AllowTogglingToNoneVisibility = false;

        public bool IsVisible { get; private set; } = false;

        public bool HasVisibilityOwner { get; private set; } = false;
        public GameObject VisibilityOwner { get; private set; }

        public enum MapSortingLayer
        {
            First = 0,
            Second = 1,
            PreRegular1 = 4,
            Regular = 5,
            PostRegular1 = 6,
            Last = 9,
        }

        internal readonly RectTransform[] SortingLayerParents = new RectTransform[(int)MapSortingLayer.Last + 1];

        public GameObject SortingLayerParentPrefab;



        void Start()
        {
            this.EnsureSerializableReferencesAssigned();
            this.GameTimeProvider = this.GameManager.ServiceProvider.GetRequiredService<IGameTimeProvider>();
            this.Canvas = this.RootTransform.GetComponentInParentOrThrow<Canvas>();

            m_PooledRawImages.PrefabGameObject = this.TexturePrefab;
            m_PooledImages.PrefabGameObject = this.SpritePrefab;
            m_PooledTexts.PrefabGameObject = this.TextPrefab;

            m_OriginalMapImageData = new RectTransformData(this.RootTransform);
            m_BackgroundPanelImageOriginalColor = this.BackgroundPanelImage.color;

            this.SetMapVisibilityType(this.DefaultMapVisibilityType, bForce: true);
            this.SetVisible(false);
            
            for (int i = 0; i < this.SortingLayerParents.Length; i++)
                this.GetOrCreateSortingLayerParent((MapSortingLayer)i);

            this.CommandManager.RegisterCommandsFromTypeMethods(this);
        }

        public void SetVisible(bool visible)
        {
            bool bVisibilityChanged = visible != this.IsVisible;
            this.IsVisible = visible;
            this.UpdateGameObjectVisibility();
            m_repositionAllObjects = true;
            if (bVisibilityChanged && MiniMapDrawer != null)
                MiniMapDrawer.ClearDrawing();
        }

        public void SetVisibilityOwner(GameObject go)
        {
            this.HasVisibilityOwner = go != null;
            this.VisibilityOwner = go;
        }

        public void SetBackgroundTexture(Texture2D texture)
        {
            this.MapImage.texture = texture;
            this.MapImage.enabled = texture != null; // have to disable it, otherwise it will render white color
        }

        public MiniMapObject CreateWithoutRegistering(GameObject go)
        {
            this.NumCreatedObjects++;
            return go.AddComponent<MiniMapObject>();
        }

        public MiniMapObject Create(
            GameObject go, bool needsTexture, bool needsSprite, bool needsText, MapSortingLayer sortingLayer)
        {
            MiniMapObject miniMapObject = this.CreateWithoutRegistering(go);
            this.RegisterObject(miniMapObject, needsTexture, needsSprite, needsText, sortingLayer);
            return miniMapObject;
        }

        public MiniMapObject CreateWithTexture(
            GameObject go, Texture2D texture, RectTransformData rectTransformData, MapSortingLayer sortingLayer = MapSortingLayer.Regular)
        {
            MiniMapObject miniMapObject = this.Create(go, true, false, false, sortingLayer);
            miniMapObject.TextureImage.texture = texture;
            rectTransformData.Apply(miniMapObject.TextureImage.rectTransform);
            return miniMapObject;
        }

        public MiniMapObject CreateWithSprite(
            GameObject go, Sprite sprite, RectTransformData rectTransformData, MapSortingLayer sortingLayer = MapSortingLayer.Regular)
        {
            MiniMapObject miniMapObject = this.Create(go, false, true, false, sortingLayer);
            miniMapObject.SpriteImage.sprite = sprite;
            rectTransformData.Apply(miniMapObject.SpriteImage.rectTransform);
            return miniMapObject;
        }

        public MiniMapObject CreateWithText(
            GameObject go, string text, Color textColor, RectTransformData rectTransformData, MapSortingLayer sortingLayer = MapSortingLayer.Regular)
        {
            MiniMapObject miniMapObject = this.Create(go, false, false, true, sortingLayer);
            miniMapObject.TextComponent.text = text;
            miniMapObject.TextComponent.color = textColor;
            rectTransformData.Apply(miniMapObject.TextComponent.rectTransform);
            return miniMapObject;
        }

        public void RegisterObject(
            MiniMapObject miniMapObject, bool needsTexture, bool needsSprite, bool needsText, MapSortingLayer sortingLayer = MapSortingLayer.Regular)
        {
            if (null == miniMapObject)
                throw new System.ArgumentNullException();

            if (miniMapObject.IsRegistered)
                throw new System.InvalidOperationException("Already registered");

            m_MiniMapObjects.Add(miniMapObject);
            miniMapObject.MiniMap = this;
            miniMapObject.IsRegistered = true;
            miniMapObject.IsDirty = true;
            miniMapObject.HasLastMatrix = false;
            miniMapObject.TimeWhenRegistered = this.GameTimeProvider.Time;
            this.RentUIComponents(miniMapObject, needsTexture, needsSprite, needsText, sortingLayer);
            this.NumRegistrations++;
        }

        public void UnregisterObject(MiniMapObject miniMapObject)
        {
            if (null == miniMapObject)
                throw new System.ArgumentNullException();

            if (!miniMapObject.IsRegistered)
                throw new System.InvalidOperationException($"MiniMap object is not registered");

            if (miniMapObject.MiniMap != this)
                throw new System.InvalidOperationException($"MiniMap object does not belong to this MiniMap");

            // need to remove it here, otherwise duplicates could be added by calling RegisterObject() => UnregisterObject() => RegisterObject()
            bool bRemoved = m_MiniMapObjects.Remove(miniMapObject);
            if (!bRemoved)
                throw new ShouldNotHappenException("Failed to remove MiniMap object even though he is registered");

            this.UnregisterObjectInternal(miniMapObject);
        }

        void UnregisterObjectInternal(MiniMapObject miniMapObject)
        {
            // note: this function can be called while MiniMapObject is dead
            
            bool bTempShouldSelfDestroy = miniMapObject.SelfDestroyWhenUnregistered
                && miniMapObject != null; // here we need to check if he is alive

            // reset variables so that MiniMapObject can be re-used

            miniMapObject.IsRegistered = false;
            miniMapObject.MiniMap = null;
            miniMapObject.HasLastMatrix = false;
            miniMapObject.IsDirty = true;
            miniMapObject.TimeWhenRegistered = double.NegativeInfinity;
            miniMapObject.LifeDuration = 0f;
            miniMapObject.SelfDestroyWhenUnregistered = false;
            miniMapObject.HasLifeOwner = false;
            miniMapObject.LifeOwner = null;
            miniMapObject.HasSeparateWorldTransform = false;
            miniMapObject.SeparateWorldTransform = PositionAndRotation.Identity;

            this.ReleaseUIComponents(miniMapObject);

            if (bTempShouldSelfDestroy) 
                miniMapObject.gameObject.DestroyEvenInEditMode();

            this.NumUnregistrations++;
        }

        public bool TryUnregisterObject(MiniMapObject miniMapObject)
        {
            if (!miniMapObject.IsRegistered)
                return false;

            if (miniMapObject.MiniMap != this)
                return false;

            this.UnregisterObject(miniMapObject);

            return true;
        }

        void LateUpdate()
        {
            // use LateUpdate(), because MiniMap objects can be updated in Update(), and we want to pickup their
            // properties as late as possible
            this.UpdateInternal();
        }

        void UpdateInternal()
        {
            if (this.HasVisibilityOwner && this.VisibilityOwner == null)
            {
                this.SetVisible(false);
                return;
            }

            UpdateForScreenSize();

            m_timeNow = this.GameTimeProvider.Time;
            m_MapImageSize = this.MapImage.rectTransform.rect.size;
            m_WorldSizeInverted2D = (Vector2.one / this.WorldSize.ToVec2XZ()).ZeroIfNotFinite();

            bool hasObjectsToRemove = false;

            UnityEngine.Profiling.Profiler.BeginSample("Update objects");

            for (int i = 0; i < m_MiniMapObjects.ListCount; i++)
            {
                if (!m_MiniMapObjects.GetAtIndex(i, out MiniMapObject miniMapObject))
                {
                    hasObjectsToRemove = true;
                    continue;
                }

                if (this.ShouldRemoveMiniMapObject(miniMapObject))
                {
                    hasObjectsToRemove = true;
                    m_MiniMapObjects.Remove(miniMapObject);

                    // unregister even if object is dead, because he still holds UI elements, and also other stuff needs to be cleared
                    if (this.IsRegisteredWithMe(miniMapObject))
                    {
                        this.UnregisterObjectInternal(miniMapObject);
                    }

                    continue;
                }

                this.UpdateUIElements(miniMapObject);
            }

            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("Remove objects");

            if (hasObjectsToRemove)
                m_MiniMapObjects.ConsolidateList();

            UnityEngine.Profiling.Profiler.EndSample();

            m_repositionAllObjects = false;
        }

        void UpdateForScreenSize()
        {
            Vector2 screenSize = GUIUtils.ScreenRect.size;
            float canvasScaleFactor = this.Canvas.scaleFactor;
            if (!LastScreenSize.EqualsBitwise(screenSize) || LastCanvasScaleFactor != canvasScaleFactor)
            {
                m_repositionAllObjects = true;
                SetTransformBasedOnScreenSize();
            }
        }

        bool ShouldRemoveMiniMapObject(MiniMapObject miniMapObject)
        {
            return null == miniMapObject 
                || !miniMapObject.IsRegistered 
                || (miniMapObject.HasLifeOwner && null == miniMapObject.LifeOwner)
                || (miniMapObject.LifeDuration > 0f && !m_timeNow.BetweenInclusive(miniMapObject.TimeWhenRegistered, miniMapObject.TimeWhenRegistered + miniMapObject.LifeDuration));
        }

        bool IsRegisteredWithMe(MiniMapObject miniMapObject)
        {
            return miniMapObject.IsRegistered && miniMapObject.MiniMap == this;
        }

        void RentUIComponents(
            MiniMapObject miniMapObject, bool needsTexture, bool needsSprite, bool needsText, MapSortingLayer sortingLayer)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Rent UI");

            if (needsTexture)
            {
                RentUIComponent(ref miniMapObject.TextureProperties, m_PooledRawImages, miniMapObject, this.TexturePrefab, sortingLayer);
                RawImage rawImageOriginal = this.TexturePrefab.GetComponentOrThrow<RawImage>();
                RawImage rawImage = miniMapObject.TextureImage;
                rawImage.texture = rawImageOriginal.texture;
                rawImage.uvRect = rawImageOriginal.uvRect;
            }

            if (needsSprite)
            {
                RentUIComponent(ref miniMapObject.SpriteProperties, m_PooledImages, miniMapObject, this.SpritePrefab, sortingLayer);
                Image imageOriginal = this.SpritePrefab.GetComponentOrThrow<Image>();
                Image image = miniMapObject.SpriteImage;
                image.sprite = imageOriginal.sprite;
                image.material = imageOriginal.material;
                image.type = imageOriginal.type;
                image.fillMethod = imageOriginal.fillMethod;
                image.fillOrigin = imageOriginal.fillOrigin;
                image.fillAmount = imageOriginal.fillAmount;
                image.fillCenter = imageOriginal.fillCenter;
                image.fillClockwise = imageOriginal.fillClockwise;
            }

            if (needsText)
            {
                RentUIComponent(ref miniMapObject.TextProperties, m_PooledTexts, miniMapObject, this.TextPrefab, sortingLayer);
                TextMeshProUGUI textOriginal = this.TextPrefab.GetComponentOrThrow<TextMeshProUGUI>();
                TextMeshProUGUI text = miniMapObject.TextComponent;
                text.color = textOriginal.color;
                text.text = string.Empty;
                text.richText = textOriginal.richText;
                text.fontSize = textOriginal.fontSize;
                text.textWrappingMode = textOriginal.textWrappingMode;
                text.overflowMode = textOriginal.overflowMode;
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        void RentUIComponent<T>(
            ref UIElementProperties<T> elementProperties, ComponentPoolList<T> poolList, MiniMapObject miniMapObject, GameObject prefab, MapSortingLayer sortingLayer)
            where T : Graphic
        {
            RectTransform parent = this.SortingLayerParents[(int)sortingLayer];

            if (elementProperties.Graphic == null)
            {
                poolList.ParentTransform = parent;
                elementProperties.Graphic = poolList.GetOrCreate();
            }

            elementProperties.MiniMapObject = miniMapObject;

            elementProperties.HasGraphic = true;
            elementProperties.CachedGameObject = new CachedGameObject(elementProperties.Graphic);
            elementProperties.Graphic.name = miniMapObject.name;
            elementProperties.Graphic.rectTransform.SetParent(parent, true);
            RectTransformData.Default.Apply(elementProperties.Graphic.rectTransform);
            elementProperties.Graphic.rectTransform.SetAsLastSibling(); // bring to front

            // restore default Graphic properties
            T graphicOriginal = prefab.GetComponentOrThrow<T>();
            elementProperties.Graphic.color = graphicOriginal.color;
            elementProperties.Graphic.material = graphicOriginal.material;
            elementProperties.Graphic.raycastTarget = graphicOriginal.raycastTarget;
            elementProperties.Graphic.raycastPadding = graphicOriginal.raycastPadding;

            elementProperties.CachedGameObject.SetActiveCached(true);
        }

        void ReleaseUIComponents(MiniMapObject miniMapObject)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Release UI");

            ReleaseUIComponent(ref miniMapObject.TextureProperties, m_PooledRawImages);
            ReleaseUIComponent(ref miniMapObject.SpriteProperties, m_PooledImages);
            ReleaseUIComponent(ref miniMapObject.TextProperties, m_PooledTexts);

            UnityEngine.Profiling.Profiler.EndSample();
        }

        static void ReleaseUIComponent<T>(ref UIElementProperties<T> elementProperties, ComponentPoolList<T> poolList)
            where T : Graphic
        {
            if (elementProperties.Graphic != null)
            {
                elementProperties.Graphic.gameObject.SetActive(false);
                elementProperties.Graphic.name = "pooled"; // so we can see in Hierarchy
                poolList.ReturnToPool(elementProperties.Graphic);
            }

            elementProperties.MiniMapObject = null;
            elementProperties.CachedGameObject = default;
            elementProperties.HasGraphic = false;
            elementProperties.Graphic = null;
            elementProperties = default;
        }

        void UpdateUIElements(MiniMapObject miniMapObject)
        {
            PositionAndRotation matrix = miniMapObject.HasSeparateWorldTransform
                ? miniMapObject.SeparateWorldTransform
                : miniMapObject.CachedTransform.GetPositionAndRotation();

            bool transformChanged = m_repositionAllObjects
                || miniMapObject.IsDirty
                || !miniMapObject.HasLastMatrix
                || !matrix.Equals(miniMapObject.LastMatrix);

            PositionAndRotation miniMapMatrix;

            if (transformChanged)
            {
                miniMapObject.HasLastMatrix = true;
                miniMapObject.LastMatrix = matrix;

                miniMapMatrix.Position = this.WorldToMiniMapPos(matrix.Position);
                miniMapMatrix.Rotation = this.WorldToMiniMapRotation(matrix.Rotation);
            }
            else
            {
                miniMapMatrix = PositionAndRotation.Identity;
            }

            UpdateUIElement(ref miniMapObject.TextureProperties, transformChanged, ref miniMapMatrix);
            UpdateUIElement(ref miniMapObject.SpriteProperties, transformChanged, ref miniMapMatrix);
            UpdateUIElement(ref miniMapObject.TextProperties, transformChanged, ref miniMapMatrix);

            miniMapObject.IsDirty = false;
        }

        void UpdateUIElement<T>(
            ref UIElementProperties<T> elementProperties, bool transformChanged, ref PositionAndRotation miniMapMatrix)
            where T : Graphic
        {
            if (!elementProperties.HasGraphic)
                return;

            bool isActive = !elementProperties.IsHidden 
                && (elementProperties.AllowedMapVisibilityTypes.IsNullOrEmpty() || elementProperties.AllowedMapVisibilityTypes.ContainsNonAlloc(this.VisibilityType));
            
            elementProperties.CachedGameObject.SetActiveCached(isActive);

            if (!isActive)
                return;

            RectTransform tr = elementProperties.Graphic.rectTransform; // RectTransform is cached

            if (elementProperties.HasWorldSpaceSize)
            {
                tr.sizeDelta = this.WorldToMiniMapSize(elementProperties.WorldSpaceSize);
            }

            if (transformChanged)
            {
                Vector2 anchoredPosition = miniMapMatrix.Position.ToVec2XY() + elementProperties.OffsetOnMiniMap;

                if (elementProperties.AlwaysRotateTowardsCamera)
                {
                    tr.anchoredPosition = anchoredPosition;
                    tr.rotation = Quaternion.identity;
                }
                else
                {
                    // faster path, only 1 call to native code
                    tr.SetLocalPositionAndRotation(anchoredPosition, miniMapMatrix.Rotation);
                }
            }

            if (elementProperties.ColorAlphaMaxDuration > 0f && elementProperties.ColorAlphaCurve != null)
            {
                float elapsedTimeSinceRegistration = (float)(m_timeNow - elementProperties.MiniMapObject.TimeWhenRegistered);
                float elapsedPerc = elapsedTimeSinceRegistration / elementProperties.ColorAlphaMaxDuration;
                elapsedPerc = elapsedPerc.ZeroIfNotFinite().Clamp01();
                float curveValue = elementProperties.ColorAlphaCurve.Evaluate(elapsedPerc);
                curveValue = curveValue.Clamp01();
                float alpha = elementProperties.ColorAlphaMultiplier > 0f ? curveValue * elementProperties.ColorAlphaMultiplier : curveValue;
                elementProperties.Graphic.SetColorAlpha(alpha);
            }
        }

        public Vector3 WorldToMiniMapPos(Vector3 worldPos)
        {
            Vector3 offsetWorld = worldPos - this.WorldCenter;

            if (this.NegateXOffset)
                offsetWorld.x = -offsetWorld.x;
            if (this.NegateZOffset)
                offsetWorld.z = -offsetWorld.z;

            //offsetWorld = m_rotationOffsetQuaternion.TransformDirection(offsetWorld.WithY(0f));

            Vector2 perc = offsetWorld.ToVec2XZ() * m_WorldSizeInverted2D;

            if (this.InvertXPos)
                perc.x = -perc.x;
            if (this.InvertYPos)
                perc.y = -perc.y;

            //if (this.InvertXPos)
            //    perc.x = 1f - perc.x;
            //if (this.InvertYPos)
            //    perc.y = 1f - perc.y;

            //perc = perc.Clamp(-0.5f, 0.5f);
            //perc = perc.Clamp(0f, 1f);

            return m_MapImageSize * perc;
        }

        public Quaternion WorldToMiniMapRotation(Quaternion rotation)
        {
            // take the Yaw (Y axis) rotation only, and turn it into Roll (Z axis) rotation, because UI elements are rotated with Z component

            Vector3 eulerAngles = rotation.eulerAngles;
            return Quaternion.AngleAxis(-eulerAngles.y, Vector3.forward);

            // this approach seems to be faster, but not sure if it's fully accurate without normalizing vector
            //Vector3 rotationForward = rotation.GetForward();
            //Vector3 up = new Vector3(rotationForward.x, rotationForward.z, 0f);
            //return Quaternion.LookRotation(Vector3.forward, up);
        }

        public Vector2 WorldToMiniMapSize(Vector2 size)
        {
            Vector2 perc = size * m_WorldSizeInverted2D;
            return m_MapImageSize * perc;
        }

        public Vector3 UVToWorldPosition(Vector2 perc)
        {
            if (this.InvertXPos)
                perc.x = -perc.x;
            if (this.InvertYPos)
                perc.y = -perc.y;

            Vector3 offsetWorld = perc.ToVec3XZ().Mul(this.WorldSize);

            if (this.NegateXOffset)
                offsetWorld.x = -offsetWorld.x;
            if (this.NegateZOffset)
                offsetWorld.z = -offsetWorld.z;

            Vector3 bottomLeftPos = this.WorldCenter - this.WorldSize * 0.5f;
            Vector3 worldPos = bottomLeftPos + offsetWorld;
            return worldPos;
        }

        RectTransform GetOrCreateSortingLayerParent(MapSortingLayer sortingLayer)
        {
            RectTransform parent = this.SortingLayerParents[(int)sortingLayer];
            if (parent != null)
                return parent;

            GameObject go = Instantiate(this.SortingLayerParentPrefab, this.RootTransform);
            go.name = $"Sorting layer {sortingLayer}";
            parent = go.GetComponentOrThrow<RectTransform>();

            this.SortingLayerParents[(int)sortingLayer] = parent;

            return parent;
        }

        void OnValidate()
        {
            m_repositionAllObjects = true;
        }

        bool UpdateGameObjectVisibility()
        {
            bool visible = this.IsVisible && this.VisibilityType != MapVisibilityType.None;
            this.RootTransform.gameObject.SetActive(visible);
            return visible;
        }

        public void SetMapVisibilityType(MapVisibilityType type, bool bForce = false)
        {
            if (type == this.VisibilityType && !bForce)
                return;

            bool bVisibilityTypeChanged = type != this.VisibilityType;
            this.VisibilityType = type;
            m_repositionAllObjects = true;
            LastScreenSize = GUIUtils.ScreenRect.size;
            LastCanvasScaleFactor = this.Canvas.scaleFactor;

            switch (type)
            {
                case MapVisibilityType.Big:
                    this.SetTransformBasedOnScreenSize();
                    this.BackgroundPanelImage.color = this.BackgroundPanelImageColorBig;
                    break;
                case MapVisibilityType.Small:
                    m_OriginalMapImageData.Apply(this.RootTransform);
                    this.BackgroundPanelImage.color = m_BackgroundPanelImageOriginalColor;
                    break;
                case MapVisibilityType.None:
                    break;
                default:
                    throw new System.ArgumentException($"Unknown {nameof(MapVisibilityType)}");
            }

            bool newIsVisible = this.UpdateGameObjectVisibility();

            if (newIsVisible)
                this.RootTransform.SetAsLastSibling(); // bring to front

            // need to update all UI elements here, because for 1 frame, they are left in their previous positions
            this.UpdateInternal();

            if (bVisibilityTypeChanged && MiniMapDrawer != null)
                MiniMapDrawer.ClearDrawing();
        }

        public void ToggleMapVisibilityType()
        {
            MapVisibilityType newVisibilityType = this.VisibilityType;
            newVisibilityType = (MapVisibilityType)((int)(newVisibilityType + 1) % NumMapVisibilityTypes);
            
            if (!AllowTogglingToNoneVisibility && newVisibilityType == MapVisibilityType.None)
                newVisibilityType = (MapVisibilityType)((int)(newVisibilityType + 1) % NumMapVisibilityTypes);

            this.SetMapVisibilityType(newVisibilityType);
        }

        [CommandMethod("minimap_toggle_visibility_mode", "Toggle visibility mode of MiniMap", exactNumArguments = 0)]
        ProcessCommandResult ToggleMapVisibilityTypeCmd(ProcessCommandContext context)
        {
            this.ToggleMapVisibilityType();
            return ProcessCommandResult.SuccessResponse(this.VisibilityType.ToString());
        }

        void SetTransformBasedOnScreenSize()
        {
            Vector2 screenSize = GUIUtils.ScreenRect.size;
            float canvasScaleFactor = this.Canvas.scaleFactor;

            if (this.VisibilityType == MapVisibilityType.Big)
            {
                this.BigModeRectTransformData.Apply(this.RootTransform);
                this.RootTransform.sizeDelta = screenSize.MinComponent() * this.BigMapSizePerc / canvasScaleFactor.OneIfZero() * Vector2.one;
            }

            LastScreenSize = screenSize;
            LastCanvasScaleFactor = canvasScaleFactor;
        }
    }
}
