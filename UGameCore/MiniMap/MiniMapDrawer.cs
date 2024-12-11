using System;
using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGameCore.MiniMap
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class MiniMapDrawer : MaskableGraphic, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        struct Line
        {
            public List<Vector2> Points;
            public readonly int NumPoints => Points?.Count ?? 0;
            public Color32 Color;
        }

        readonly List<Line> Lines = new();
        public Color32 DrawColorLeftMouse = Color.green;
        public Color32 DrawColorRightMouse = Color.red;
        public float Thickness = 4f;

        public bool EnableDrawing = true;

        Line CurrentLine;

        readonly List<UIVertex> VertexBuffer = new List<UIVertex>();
        readonly List<int> IndexBuffer = new List<int>();

        UIVertex[] TempVertexes = Array.Empty<UIVertex>();
        int[] TempIndexes = Array.Empty<int>();

        bool IsPointerDown = false;


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Drawer populate mesh");

            vh.Clear();
            VertexBuffer.Clear();
            IndexBuffer.Clear();

            foreach (Line line in Lines)
                AddLinePoints(VertexBuffer, IndexBuffer, line, Thickness);

            if (CurrentLine.NumPoints > 0)
                AddLinePoints(VertexBuffer, IndexBuffer, CurrentLine, Thickness);

            UnityEngine.Profiling.Profiler.BeginSample("Add mesh");
            vh.AddUIVertexStream(VertexBuffer, IndexBuffer);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.EndSample();
        }

        void AddLinePoints(
            List<UIVertex> vertices, List<int> indices, Line line, float thickness)
        {
            if (line.NumPoints <= 1)
                return;

            ReadOnlySpan<Vector2> linePoints = line.Points.ListAsSpan();
            Color32 col = line.Color;

            int vertexCount = linePoints.Length * 2;
            int indexCount = (linePoints.Length - 1) * 6;

            TempVertexes = TempVertexes.EnsureCountNextPowerOf2(vertexCount);
            TempIndexes = TempIndexes.EnsureCountNextPowerOf2(indexCount);

            Span<UIVertex> verts = TempVertexes.AsSpan(0, vertexCount);
            Span<int> inds = TempIndexes.AsSpan(0, indexCount);

            int startIndex = vertices.Count;
            float thicknessHalf = thickness * 0.5f;

            for (int i = 0; i < linePoints.Length; i++)
            {
                Vector2 point = linePoints[i];
                Vector2 nextPoint = i < linePoints.Length - 1 ? linePoints[i + 1] : (i > 0 ? linePoints[i - 1] : point);

                Vector2 dir = nextPoint - point;
                Vector2 perpendicular = Vector2.Perpendicular(dir).normalized.ZeroIfNotFinite();

                verts[i * 2].position = point + perpendicular * thicknessHalf;
                verts[i * 2].color = col;

                verts[i * 2 + 1].position = point - perpendicular * thicknessHalf;
                verts[i * 2 + 1].color = col;
            }

            for (int i = 0; i < linePoints.Length - 1; i++)
            {
                inds[i * 6 + 0] = startIndex + i * 2;
                inds[i * 6 + 1] = startIndex + i * 2 + 1;
                inds[i * 6 + 2] = startIndex + i * 2 + 3;
                inds[i * 6 + 3] = startIndex + i * 2;
                inds[i * 6 + 4] = startIndex + i * 2 + 3;
                inds[i * 6 + 5] = startIndex + i * 2 + 2;
            }

            vertices.AddSpan(verts);
            indices.AddSpan(inds);
        }

        public void RequestToRedraw()
        {
            SetVerticesDirty();
        }

        public void ClearDrawing()
        {
            if (Lines.Count == 0 && CurrentLine.NumPoints == 0)
                return;

            Lines.Clear();
            CurrentLine = default;
            SetVerticesDirty();
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (!IsPointerDown)
                return;

            AddPointToDraw(eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsPointerDown = false;
            EndCurrentLine(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!IsPointerDown)
                return;

            IsPointerDown = false;
            EndCurrentLine(eventData);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            IsPointerDown = true;

            AddPointToDraw(eventData);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            // handle this event just in case Unity's EventSystem gets bugged
            IsPointerDown = false;
            EndCurrentLine(eventData);
        }

        Vector2 GetPointsToDraw(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.enterEventCamera, out Vector2 localPoint);

            return localPoint;
        }

        Color32 GetColor(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Right ? DrawColorRightMouse : DrawColorLeftMouse;
        }

        void AddPointToDraw(PointerEventData eventData)
        {
            if (!EnableDrawing)
                return;

            if (CurrentLine.NumPoints == 0)
                CurrentLine.Color = GetColor(eventData);

            CurrentLine.Points ??= new List<Vector2>();
            CurrentLine.Points.Add(GetPointsToDraw(eventData));

            RequestToRedraw();
        }

        void EndCurrentLine(PointerEventData eventData)
        {
            if (!EnableDrawing)
                return;

            if (CurrentLine.NumPoints == 0)
                return;

            Lines.Add(CurrentLine);
            CurrentLine = default;

            RequestToRedraw();
        }
    }
}
