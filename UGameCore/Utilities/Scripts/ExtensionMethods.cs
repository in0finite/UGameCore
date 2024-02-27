using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UGameCore.Net;
using UGameCore.Utilities;

namespace UGameCore
{

    public static	class ExtensionMethods {


		private	static	Vector3[]	m_fourCornersArray = new Vector3[4];



		public	static	int	RemoveAllDeadObjects<T>( this List<T> list )
			where T : UnityEngine.Object
		{
			return list.RemoveDeadObjects();
		}

		/// <summary>
		/// Finds the desired element in a collection by extracting value and comparing it.
		/// </summary>
		/// <param name="valueSelector">Extracts value from each element.</param>
		/// <param name="valueComparer">Compares 2 values. Should return true if first is selected, otherwise false.</param>
		public	static	T	SelectByValueComparison<T,R>( this IEnumerable<T> enumerable, System.Func<T,R> valueSelector,
			System.Func<R, R, bool> valueComparer ) {

			var en = enumerable.GetEnumerator ();

			if (!en.MoveNext ())
				return default (T);

			var selectedElement = en.Current;
			var selectedValue = valueSelector(selectedElement) ;

		//	System.Func<R, R, bool> valueComparer = (a, b) => a < b ;

			while (en.MoveNext ()) {
				var value = valueSelector (en.Current);
				if (valueComparer (value, selectedValue)) {
					selectedValue = value;
					selectedElement = en.Current;
				}
			}

			return selectedElement;
		}

		public	static	T	SelectMin<T,R>( this IEnumerable<T> enumerable, System.Func<T,R> valueSelector )
			where R : System.IComparable {

			System.Func<R, R, bool> valueComparer = (a, b) => a.CompareTo (b) < 0 ;
			return SelectByValueComparison (enumerable, valueSelector, valueComparer);

		}

		public	static	T	SelectMax<T,R>( this IEnumerable<T> enumerable, System.Func<T,R> valueSelector )
			where R : System.IComparable {

			System.Func<R, R, bool> valueComparer = (a, b) => a.CompareTo (b) > 0 ;
			return SelectByValueComparison (enumerable, valueSelector, valueComparer);

		}

		public	static	bool	AddIfDoesntExist<T>( this ICollection<T> collection, T obj ) {

			if (!collection.Contains (obj)) {
				collection.Add (obj);
				return true;
			}

			return false;
		}


		public	static	long	GetElapsedMicroSeconds( this System.Diagnostics.Stopwatch stopwatch ) {

			long freq = System.Diagnostics.Stopwatch.Frequency;

			if (freq > 0) {
				double elapsedSeconds = stopwatch.ElapsedTicks / (double)freq;
				return (long) (elapsedSeconds * 1000 * 1000);
			}

			// fallback to miliseconds
			return stopwatch.ElapsedMilliseconds * 1000;
		}


		public	static	bool	IsLocalPlayer(this MonoBehaviour monoBehaviour) {
			return monoBehaviour.GetComponent<NetworkIdentity> ().isLocalPlayer;
		}


		/// <summary>
		/// Instantiates the object, and if server is active, spawns him.
		/// </summary>
		public	static	GameObject	InstantiateWithNetwork(this GameObject go, Vector3 pos, Quaternion rot) {

			var clone = Object.Instantiate (go, pos, rot);
			if (NetworkStatus.IsServer) {
				NetManager.Spawn (clone);
			}
			return clone;
		}


		public	static	void	Invoke( this Component component, string methodName, params object[] args ) {

			var method = component.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			if(method != null) {
				method.Invoke( component, args );
			}

		}

		public	static	void	BroadcastMessageNoExceptions( this GameObject go, string msg, params object[] args ) {

			var components = go.GetComponentsInChildren<Component> ();
			foreach (var c in components) {
				try {

					if (c != null )
						c.Invoke( msg, args );

				} catch( System.Exception ex ) {
					Debug.LogException (ex);
				}
			}

		}


		public	static	T	AddComponentIfDoesntExist<T> (this GameObject gameObject) where T : Component {

			var component = gameObject.GetComponent<T> ();
			if (null == component)
				component = gameObject.AddComponent<T> ();

			return component;
		}


		public	static	Transform	FindChildOrLogError( this Transform transform, string childName ) {

			var child = transform.Find(childName);

			if (null == child) {
				Debug.LogError ("Failed to find child with name " + childName);
			}

			return child;
		}

		public	static	Transform	FindChildRecursivelyOrLogError( this Transform transform, string childName ) {

			var child = transform.GetComponentsInChildren<Transform>().FirstOrDefault(c => childName.Equals(c.name, System.StringComparison.Ordinal));

			if (null == child) {
				Debug.LogError ("Failed to find child with name " + childName);
			}

			return child;
		}


		public	static	GameObject	InstantiateAsUIElement( this GameObject prefab, Transform parent ) {

			var go = GameObject.Instantiate (prefab);
			go.transform.SetParent (parent, false);
			return go;
		}

		public	static	RectTransform	GetRectTransform( this Component component ) {

			return component.transform as RectTransform;

		}

		public	static	RectTransform	GetRectTransform( this GameObject go ) {

			return go.transform as RectTransform;

		}

		public static Vector2 GetScaleFactorFromCanvas(this RectTransform rectTransform)
		{
			var canvas = rectTransform.GetComponentInParent<Canvas>();
			return canvas != null ? canvas.transform.localScale : Vector2.one;
        }

		public	static	void	AnchorsToCorners( this RectTransform rectTransform ) {

			RectTransform t = rectTransform;
			RectTransform pt = rectTransform.parent as RectTransform;

			Rect parentRect = pt.rect;
            
            Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / parentRect.width,
				t.anchorMin.y + t.offsetMin.y / parentRect.height);
			Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / parentRect.width,
				t.anchorMax.y + t.offsetMax.y / parentRect.height);

			t.anchorMin = newAnchorsMin;
			t.anchorMax = newAnchorsMax;
			t.offsetMin = t.offsetMax = new Vector2(0, 0);

		}

		public	static	void	CornersToAnchors( this RectTransform rectTransform ) {
			
			rectTransform.offsetMin = rectTransform.offsetMax = new Vector2(0, 0);

		}

		public	static	void	SetRectAndAdjustAnchors( this RectTransform rectTransform, Rect rect ) {

			var parentRectTransform = rectTransform.parent as RectTransform;

			var normalizedRect = new Rect (rect.x / parentRectTransform.rect.width, rect.y / parentRectTransform.rect.height,
				rect.width / parentRectTransform.rect.width, rect.height / parentRectTransform.rect.height);

			rectTransform.SetNormalizedRectAndAdjustAnchors (normalizedRect);

		}

		public	static	void	SetNormalizedRectAndAdjustAnchors( this RectTransform rectTransform, Rect normalizedRect ) {

			// first set anchored position, anchors, and size
			rectTransform.anchorMin = normalizedRect.min;//new Vector2 (0, 1);
			rectTransform.anchorMax = normalizedRect.max; //new Vector2 (0, 1);
			//	Debug.Log ("anchored position before: " + rectTransform.anchoredPosition);
			//	rectTransform.anchoredPosition = new Vector2( rect.center.x, - rect.center.y) ;

			//	rectTransform.offsetMin = Vector2.zero;
			//	rectTransform.offsetMax = Vector2.one / 2;
			//	rectTransform.sizeDelta = rect.size;

			rectTransform.CornersToAnchors ();

			// move anchors to corners
			rectTransform.AnchorsToCorners ();

		//	Debug.Log ("anchored position: " + rectTransform.anchoredPosition + " anchor min: " + rectTransform.anchorMin + 
		//		" anchor max: " + rectTransform.anchorMax + " offset min: " + rectTransform.offsetMin + " offset max: " +
		//		rectTransform.offsetMax );
			
		}

		public	static	Rect	GetRect( this RectTransform rectTransform ) {

			Vector3[] localCorners = m_fourCornersArray;
			rectTransform.GetLocalCorners (localCorners);

			float xMin = float.PositiveInfinity, yMin = float.PositiveInfinity;
			float xMax = float.NegativeInfinity, yMax = float.NegativeInfinity;

			for (int i = 0; i < localCorners.Length; i++) {
				Vector3 corner = localCorners [i];

				if (corner.x < xMin)
					xMin = corner.x;
				else if (corner.x > xMax)
					xMax = corner.x;

				if (corner.y < yMin)
					yMin = corner.y;
				else if (corner.y > yMax)
					yMax = corner.y;
			}

			return new Rect (xMin, yMin, xMax - xMin, yMax - yMin);
		}

		public	static	Vector2	GetParentDimensions( this RectTransform rectTransform ) {

			if (null == rectTransform.parent)
				return new Vector2 (Screen.width, Screen.height);

		//	return ((RectTransform)rectTransform.parent).rect.size;
			return rectTransform.parent.GetRectTransform().GetRect().size;
		}

		public	static	Vector2	NormalizePositionRelativeToParent( this RectTransform rectTransform, Vector2 pos ) {

			Vector2 parentSize = rectTransform.GetParentDimensions ();

			Vector2 normalizedPos = Vector2.zero;

			if (parentSize.x != 0)
				normalizedPos.x = pos.x / parentSize.x;
			if (parentSize.y != 0)
				normalizedPos.y = pos.y / parentSize.y;

			return normalizedPos;
		}

		public	static	void	SetNormalColor( this UnityEngine.UI.Button button, Color normalColor ) {

			var colors = button.colors;
			colors.normalColor = normalColor;
			button.colors = colors;

		}


		/// <summary>
		/// Makes sure all color components are between 0 and 1. If colors are below 0, they are increased
		/// so that minimum value is 0. If colors are higher than 1, they are scaled so that maximum color
		/// is 1. Alpha value is just clamped between 0 and 1.
		/// </summary>
		public	static	void	NormalizeIfNeeded (this Color color) {

			color.a = Mathf.Clamp01 (color.a);


			float minColor = Mathf.Min (color.r, Mathf.Min (color.g, color.b));

			if (minColor < 0f) {
				// increase all colors so that minimum color is 0
				float increase = -minColor;
				color.r += increase;
				color.g += increase;
				color.b += increase;
			}

			float maxColor = Mathf.Max (color.r, Mathf.Max (color.g, color.b));

			if (maxColor > 1f) {
				// scale all values, so that the maximum value is 1
				float mul = 1.0f / maxColor;
				color.r *= mul;
				color.g *= mul;
				color.b *= mul;
			}

		}

	}

}
