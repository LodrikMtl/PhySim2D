﻿using PhySim2D.Collision.Colliders;
using PhySim2D.Dynamics;
using PhySim2D.Tools;

namespace PhySim2D.Factories
{
    internal class BodyFactory
    {
        public static void CreateBody()
        {

        }

        public static Rigidbody CreateCircleBody(MassData massData, KTransform t)
        {
            return new Rigidbody(new Circle(KVector2.Zero), massData, t);
        }

        public static Rigidbody CreateSegmentBody(KVector2 start, KVector2 end, MassData massData, KTransform t)
        {
            return new Rigidbody(new Segment(start, end), massData, t);
        }

        public static Rigidbody CreateRectangleBody(KTransform tx, MassData massData, float hWidth, float hHeight)
        {
            KVertices rect = new KVertices
            {
                new KVector2(-hWidth,-hHeight),
                new KVector2(-hWidth,hHeight),
                new KVector2(hWidth,hHeight),
                new KVector2(hWidth,-hHeight)
            };

            return new Rigidbody(new Polygon(rect), massData, tx);
        }

        public static Rigidbody CreatePolygon(KTransform tx, MassData massData, KVertices vertices)
        {
            return new Rigidbody(new Polygon(vertices), massData, tx);
        }
    }
}
