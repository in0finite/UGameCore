using System.Collections.Generic;
using TMPro;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.UI;
using static UGameCore.CommandManager;

namespace UGameCore.MiniMap
{
    [DefaultExecutionOrder(70)]
    public class MiniMap : MonoBehaviour
    {
        public CommandManager CommandManager;

        public RectTransform RootTransform;
        public RawImage MapImage;
        public Image BackgroundPanelImage;

        public GameObject ImagePrefab;
        public GameObject TextPrefab;

        Canvas Canvas;

        readonly List<MiniMapObject> m_MiniMapObjects = new();
        public IReadOnlyList<MiniMapObject> MiniMapObjects => m_MiniMapObjects;

        public Vector3 WorldCenter = Vector3.zero;
        public Vector3 WorldSize = Vector3.zero;

        public bool InvertXPos = false;
        public bool InvertYPos = false;

        public bool NegateXOffset = false;
        public bool NegateZOffset = false;

        public Vector3 RotationOffset = Vector3.zero;

        bool m_repositionAllObjects = false;
        Vector2 m_MapImageSize = Vector2.zero;
        //Vector2 m_WorldSizeInverted = Vector2.zero;
        Quaternion m_rotationOffsetQuaternion = Quaternion.identity;

        RectTransformData m_OriginalMapImageData;
        public RectTransformData BigModeRectTransformData = new RectTransformData().WithDefault();
        [Range(0f, 1f)] public float BigMapSizePerc = 0.975f;

        Color m_BackgroundPanelImageOriginalColor;
        public Color BackgroundPanelImageColorBig = Color.black.WithAlpha(0.75f);

        public enum MapVisibilityType
        {
            Small = 0,
            Big,
            None,
        }

        public readonly int NumMapVisibilityTypes = System.Enum.GetValues(typeof(MapVisibilityType)).Length;

        public MapVisibilityType VisibilityType { get; private set; } = MapVisibilityType.None;

        public MapVisibilityType DefaultMapVisibilityType = MapVisibilityType.Small;

        public bool IsVisible { get; private set; } = false;



        void Start()
        {
            this.EnsureSerializableReferencesAssigned();
            this.Canvas = this.RootTransform.GetComponentInParentOrThrow<Canvas>();
            m_OriginalMapImageData = new RectTransformData(this.RootTransform);
            m_BackgroundPanelImageOriginalColor = this.BackgroundPanelImage.color;
            this.SetMapVisibilityType(this.DefaultMapVisibilityType, bForce: true);
            this.SetVisible(false);
            this.CommandManager.RegisterCommandsFromTypeMethods(this);
        }

        public void SetVisible(bool visible)
        {
            this.IsVisible = visible;
            this.RootTransform.gameObject.SetActive(this.IsVisible && this.VisibilityType != MapVisibilityType.None);
            m_repositionAllObjects = true;
        }

        public void SetBackgroundTexture(Texture2D texture)
        {
            this.MapImage.texture = texture;
        }

        public MiniMapObject Create(GameObject go, bool needsTexture, bool needsText)
        {
            MiniMapObject miniMapObject = go.AddComponent<MiniMapObject>();
            this.RegisterObject(miniMapObject, needsTexture, needsText);
            return miniMapObject;
        }

        public MiniMapObject CreateWithTexture(GameObject go, Texture2D texture, RectTransformData rectTransformData)
        {
            MiniMapObject miniMapObject = this.Create(go, true, false);
            miniMapObject.TextureImage.texture = texture;
            rectTransformData.Apply(miniMapObject.TextureImage.rectTransform);
            miniMapObject.MarkDirty();
            return miniMapObject;
        }

        public MiniMapObject CreateWithText(GameObject go, string text, Color textColor, RectTransformData rectTransformData)
        {
            MiniMapObject miniMapObject = this.Create(go, false, true);
            miniMapObject.TextComponent.text = text;
            miniMapObject.TextComponent.color = textColor;
            rectTransformData.Apply(miniMapObject.TextComponent.rectTransform);
            miniMapObject.MarkDirty();
            return miniMapObject;
        }

        public void RegisterObject(MiniMapObject miniMapObject, bool needsTexture, bool needsText)
        {
            if (miniMapObject.IsRegistered)
                throw new System.InvalidOperationException("Already registered");

            m_MiniMapObjects.Add(miniMapObject);
            miniMapObject.MiniMap = this;
            miniMapObject.IsRegistered = true;
            this.RentUIComponents(miniMapObject, needsTexture, needsText);
        }

        public void UnregisterObject(MiniMapObject miniMapObject)
        {
            if (!miniMapObject.IsRegistered)
                throw new System.InvalidOperationException($"Specified object is not registered");

            int index = m_MiniMapObjects.IndexOf(miniMapObject);
            if (index < 0)
                throw new System.InvalidOperationException($"Specified object not found among registered objects");

            m_MiniMapObjects[index] = null;
            miniMapObject.IsRegistered = false;
            miniMapObject.MiniMap = null;
            this.ReleaseUIComponents(miniMapObject);
        }

        void Update()
        {
            m_MapImageSize = this.MapImage.rectTransform.rect.size;
            //m_WorldSizeInverted = (Vector2.one / this.WorldSize).ZeroIfNotFinite();
            m_rotationOffsetQuaternion = Quaternion.Euler(this.RotationOffset);

            bool hasDeadObjects = false;

            foreach (MiniMapObject miniMapObject in m_MiniMapObjects)
            {
                if (null == miniMapObject)
                {
                    hasDeadObjects = true;
                    this.ReleaseUIComponents(miniMapObject);
                    continue;
                }

                this.UpdateTransform(miniMapObject);
            }

            if (hasDeadObjects)
                m_MiniMapObjects.RemoveDeadObjects();

            m_repositionAllObjects = false;
        }

        void RentUIComponents(MiniMapObject miniMapObject, bool needsTexture, bool needsText)
        {
            if (needsTexture && miniMapObject.TextureImage == null)
            {
                miniMapObject.TextureImage = Instantiate(this.ImagePrefab, this.RootTransform).GetComponentOrThrow<RawImage>();
                miniMapObject.TextureImage.name = miniMapObject.name;
            }

            if (needsText && miniMapObject.TextComponent == null)
            {
                miniMapObject.TextComponent = Instantiate(this.TextPrefab, this.RootTransform).GetComponentOrThrow<TextMeshProUGUI>();
                miniMapObject.TextComponent.name = miniMapObject.name;
            }
        }

        void ReleaseUIComponents(MiniMapObject miniMapObject)
        {
            if (miniMapObject.TextureImage != null)
            {
                miniMapObject.TextureImage.gameObject.DestroyEvenInEditMode();
            }

            if (miniMapObject.TextComponent != null)
            {
                miniMapObject.TextComponent.gameObject.DestroyEvenInEditMode();
            }

            miniMapObject.TextureImage = null;
            miniMapObject.TextComponent = null;
        }

        void UpdateTransform(MiniMapObject miniMapObject)
        {
            PositionAndRotation matrix = miniMapObject.CachedTransform.GetPositionAndRotation();
            if (!m_repositionAllObjects
                && miniMapObject.LastPositionAndRotation.HasValue
                && matrix.Equals(miniMapObject.LastPositionAndRotation.Value))
                return;

            miniMapObject.LastPositionAndRotation = matrix;

            Vector3 miniMapPos = this.WorldToMiniMapPos(matrix.Position);
            Quaternion miniMapRot = miniMapObject.AlwaysRotateTowardsCamera
                ? Quaternion.identity
                : this.WorldToMiniMapRotation(matrix.Rotation);

            if (miniMapObject.TextureImage != null)
            {
                RectTransform tr = miniMapObject.TextureImage.rectTransform; // RectTransform is cached
                tr.anchoredPosition = miniMapPos;
                if (miniMapObject.AlwaysRotateTowardsCamera)
                    tr.rotation = miniMapRot;
                else
                    tr.localRotation = miniMapRot;
            }

            if (miniMapObject.TextComponent != null)
            {
                RectTransform tr = miniMapObject.TextComponent.rectTransform; // RectTransform is cached
                tr.anchoredPosition = miniMapPos;
                if (miniMapObject.AlwaysRotateTowardsCamera)
                    tr.rotation = miniMapRot;
                else
                    tr.localRotation = miniMapRot;
            }
        }

        Vector3 WorldToMiniMapPos(Vector3 worldPos)
        {
            Vector3 offsetWorld = worldPos - this.WorldCenter;

            if (this.NegateXOffset)
                offsetWorld.x = -offsetWorld.x;
            if (this.NegateZOffset)
                offsetWorld.z = -offsetWorld.z;

            //offsetWorld = m_rotationOffsetQuaternion.TransformDirection(offsetWorld.WithY(0f));

            Vector2 perc = offsetWorld.Divide(this.WorldSize).ToVec2WithXAndZ();

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

        Quaternion WorldToMiniMapRotation(Quaternion rotation)
        {
            // take the Yaw (Y axis) rotation only, and turn it into Roll (Z axis) rotation, because UI elements are rotated with Z component

            Vector3 eulerAngles = rotation.eulerAngles;

            return Quaternion.AngleAxis(-eulerAngles.y, Vector3.forward);
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

        void OnValidate()
        {
            m_repositionAllObjects = true;
        }

        public void SetMapVisibilityType(MapVisibilityType type, bool bForce = false)
        {
            if (type == this.VisibilityType && !bForce)
                return;

            this.VisibilityType = type;
            m_repositionAllObjects = true;

            switch (type)
            {
                case MapVisibilityType.Big:
                    this.BigModeRectTransformData.Apply(this.RootTransform);
                    this.RootTransform.sizeDelta = GUIUtils.ScreenRect.size.MinComponent() * this.BigMapSizePerc / this.Canvas.scaleFactor.OneIfZero() * Vector2.one;
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

            this.RootTransform.gameObject.SetActive(this.IsVisible && this.VisibilityType != MapVisibilityType.None);
        }

        public void ToggleMapVisibilityType()
        {
            this.SetMapVisibilityType((MapVisibilityType)((int)(this.VisibilityType + 1) % this.NumMapVisibilityTypes));
        }

        [CommandMethod("minimap_toggle_visibility_mode", "Toggle visibility mode of MiniMap", exactNumArguments = 0)]
        ProcessCommandResult ToggleMapVisibilityTypeCmd(ProcessCommandContext context)
        {
            this.ToggleMapVisibilityType();
            return ProcessCommandResult.SuccessResponse(this.VisibilityType.ToString());
        }
    }
}
