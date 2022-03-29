using System.Linq;
using UnityEngine;

namespace Lemmings
{
    public static class RectTransformExtensions
    {
        public static Vector2 RandomPointInBounds(this RectTransform rect)
        {
            Vector3[] worldCorners = new Vector3[4];
            rect.GetWorldCorners(worldCorners);
            return new Vector2()
            {
                x = UnityEngine.Random.Range(worldCorners.Min(p => p.x), worldCorners.Max(p => p.x)),
                y = UnityEngine.Random.Range(worldCorners.Min(p => p.y), worldCorners.Max(p => p.y)),
            };
        }

        public static Vector2 TopRight(this RectTransform rect)
        {
            Vector3[] worldCorners = new Vector3[4];
            rect.GetWorldCorners(worldCorners);
            return new Vector2()
            {
                x = worldCorners.Max(p => p.x),
                y = worldCorners.Max(p => p.y)
            };
        }

        public static Vector2 BotLeft(this RectTransform rect)
        {
            Vector3[] worldCorners = new Vector3[4];
            rect.GetWorldCorners(worldCorners);
            return new Vector2()
            {
                x = worldCorners.Min(p => p.x),
                y = worldCorners.Min(p => p.y)
            };
        }

        public static Vector2 Center(this RectTransform rect)
        {
            Vector3[] worldCorners = new Vector3[4];
            rect.GetWorldCorners(worldCorners);
            Vector2 center = Vector2.zero;
            foreach (var corner in worldCorners)
            {
                center += (Vector2) corner;
            }

            center.Scale(new Vector2(1.0f / 4, 1.0f / 4));
            return center;
        }

        public static Vector2 Size(this RectTransform rect)
        {
            return rect.TopRight() - rect.BotLeft();
        }

        public static Bounds GetBounds(this RectTransform rect)
        {
            Vector2 center = rect.Center();
            Vector2 size = rect.Size();

            return new Bounds(new Vector3(center.x, center.y, 0), new Vector3(size.x, size.y, 1));
        }
    }
}