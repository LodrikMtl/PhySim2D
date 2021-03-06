﻿using PhySim2D.Collision.Colliders;
using PhySim2D.Collision.Contacts;
using PhySim2D.Tools;
using PhySim2D.Sim;
using System;

namespace PhySim2D.Collision.Narrowphase
{
    class PolygonCollisionSAT
    {

        internal static bool PolygonVsPolygon(ref Contact contact)
        {
            Polygon a = (Polygon)contact.FixtureA.Collider;
            Polygon b = (Polygon)contact.FixtureB.Collider;


            CollisionDetection.SeparationSAT(a, b, out int indexA, out double separationA);

            if (separationA >= Config.EpsilonsFloat)
                 return false;

            CollisionDetection.SeparationSAT(b, a, out int indexB, out double separationB);

            if (separationB >= Config.EpsilonsFloat)
                return false;

            Polygon refPoly;
            Polygon incidentPoly;

            Face refFace;
            Face incidentFace;

            int index;
        
            bool flipNormal;

            if (separationB > separationA)
            {
                refPoly = b;
                refFace = b.ComputeFace(indexB);
                incidentPoly = a;
                index = indexA;
                flipNormal = false;
            }
            else
            {
                refPoly = a;
                refFace = a.ComputeFace(indexA);
                incidentPoly = b;
                index = indexB;
                flipNormal = true;
            }

            CollisionDetection.FindIncidentFace(incidentPoly, -refFace.WNormal, index, out incidentFace);

            KVector2 refDir = KVector2.Normalize(Face.Direction(refFace));
            int nbClipPoints;

            double offSet1 = refDir * refFace.WStart;
            nbClipPoints = CollisionDetection.ClipPointsToLine(incidentFace.WStart, incidentFace.WEnd, -refDir, -offSet1, out KVector2[] clipPoints);

            if (nbClipPoints < 2) return false;

            double offSet2 = refDir * refFace.WEnd;
            nbClipPoints = CollisionDetection.ClipPointsToLine(clipPoints[0], clipPoints[1], refDir, offSet2, out clipPoints);

            if (nbClipPoints < 2) return false;

            int count = 0;
            for (int i = 0; i < Math.Min(Config.MaxManifoldPoints, clipPoints.Length); i++)
            {
                //FinalClipping
                double separation = KVector2.Dot(clipPoints[i] - refFace.WStart, refFace.WNormal);

                if (separation < Config.EpsilonsFloat)
                {
                    ContactPoint cp = new ContactPoint
                    {
                        WPosition = clipPoints[i],
                        WPenetration = -separation,
                    };
                    contact.Manifold.ContactPoints[count] = cp;

                    count++;
                }
            }

            if (count == 0)
                return false;

            //Normal should always point from A to B.
            //it's due to the way that the ContactSolver was implemented.
            KVector2 normal;
            if (flipNormal)
                normal = refFace.WNormal;
            else
                normal = -refFace.WNormal;

            contact.Manifold.WNormal = normal;
            contact.Manifold.Count = count;

            return true;
        }

        internal static bool PolygonVsSegment(ref Contact contact)
        {
            Polygon a = (Polygon)contact.FixtureA.Collider;
            Segment b = (Segment)contact.FixtureB.Collider;


            CollisionDetection.SeparationSAT(a, b, out int indexA, out double separationA);

            if (separationA >= Config.EpsilonsFloat)
                return false;
            //TODO:not finish


            return true;
        }

        internal static bool PolygonVsCircle(ref Contact contact)
        {
            Polygon a = (Polygon)contact.FixtureA.Collider;
            Circle b = (Circle)contact.FixtureB.Collider;

            CollisionDetection.SeparationSAT(a, b, out int index, out double separation);

            //Simple but not accurate test
            if (separation >= Config.EpsilonsFloat)
                return false;

            int nextIndex = index + 1 < a.Vertices.Count ? index + 1 : 0;

            Face polFace = a.ComputeFace(index);

            KVector2 lDirN = b.Transform.TransformNormalWL(Face.Direction(polFace));
            KVector2 lStart = b.Transform.TransformPointWL(polFace.WStart);
            KVector2 lEnd = b.Transform.TransformPointWL(polFace.WEnd);

            //Closest point on segment to circle center
            double u = (b.LPosition - lStart) * lDirN;
            double v = (b.LPosition - lEnd) * -lDirN;

            KVector2 lPos;

            if (u < 0)
            {
                lPos = lStart;
            }
            else if (v < 0)
            {
                lPos = lEnd;
            }
            else
            {
                lPos = lStart + u * lDirN;
            }

            ContactPoint cp = new ContactPoint
            {
                WPosition = b.Transform.TransformPointLW(lPos),
                WPenetration = -separation,  
            };

            contact.Manifold.WNormal = polFace.WNormal;
            contact.Manifold.ContactPoints[0] = cp;
            contact.Manifold.Count = 1;

            return true;
        }

        internal static bool Collision(ref Contact contact)
        {
            switch (contact.FixtureB.Collider.Type)
            {
                case ColliderType.CIRCLE:
                    return PolygonVsCircle(ref contact);
                case ColliderType.SEGMENT:
                    return PolygonVsSegment(ref contact);
                case ColliderType.POLYGON:
                    return PolygonVsPolygon(ref contact);
                default:
                    throw new Exception("Collider type is not supported");
            }
        }
    }
}
