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
    }
}