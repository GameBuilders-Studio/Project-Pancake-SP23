using UnityEngine;
using System;
using System.Collections.Generic;

namespace Autotiles3D
{
    public static class Autotiles3D_GridRaycast
    {

        /// <summary>
        /// Raycasts without using Colliders/Physics and return the first node it finds in the given layer of a Grid
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="internalHit"></param>
        /// <param name="hitNormal"></param>
        /// <returns></returns>

        public static bool GridRayCast(Autotiles3D_TileLayer layer, Vector3 start, Vector3 end, out Vector3Int internalHit, out Vector3Int hitNormal, out List<Vector3> visits)
        {
            internalHit = new Vector3Int();
            hitNormal = new Vector3Int();
            visits = new List<Vector3>();

            Transform transform = layer.Grid.transform;

            float unit = layer.Grid.Unit;


            start = transform.InverseTransformPoint(start); // convert world start points (can be float vector3s) into local space
            end = transform.InverseTransformPoint(end);

            start = start / unit;
            end = end / unit;


            start += Vector3.one * 0.5f; //add 0.5f to each point in order for .Floot to work correctly. important to add the 0.5 in the LOCAL space, after inverseTransformPoint
            end += Vector3.one * 0.5f;

            var gx0 = start.x;
            var gy0 = start.y;
            var gz0 = start.z;
            var gx1 = end.x;
            var gy1 = end.y;
            var gz1 = end.z;

            var gx0idx = Mathf.Floor(gx0);
            var gy0idx = Mathf.Floor(gy0);
            var gz0idx = Mathf.Floor(gz0);

            var gx1idx = Mathf.Floor(gx1);
            var gy1idx = Mathf.Floor(gy1);
            var gz1idx = Mathf.Floor(gz1);

            var sx = gx1idx > gx0idx ? 1 : gx1idx < gx0idx ? -1 : 0;
            var sy = gy1idx > gy0idx ? 1 : gy1idx < gy0idx ? -1 : 0;
            var sz = gz1idx > gz0idx ? 1 : gz1idx < gz0idx ? -1 : 0;

            var gx = gx0idx;
            var gy = gy0idx;
            var gz = gz0idx;

            //Planes for each axis that we will next cross
            var gxp = gx0idx + (gx1idx > gx0idx ? 1 : 0);
            var gyp = gy0idx + (gy1idx > gy0idx ? 1 : 0);
            var gzp = gz0idx + (gz1idx > gz0idx ? 1 : 0);

            //Only used for multiplying up the error margins
            var vx = gx1 == gx0 ? 1 : gx1 - gx0;
            var vy = gy1 == gy0 ? 1 : gy1 - gy0;
            var vz = gz1 == gz0 ? 1 : gz1 - gz0;

            //Error is normalized to vx * vy * vz so we only have to multiply up
            var vxvy = vx * vy;
            var vxvz = vx * vz;
            var vyvz = vy * vz;

            //Error from the next plane accumulators, scaled up by vx*vy*vz
            var errx = (gxp - gx0) * vyvz;
            var erry = (gyp - gy0) * vxvz;
            var errz = (gzp - gz0) * vxvy;

            var derrx = sx * vyvz;
            var derry = sy * vxvz;
            var derrz = sz * vxvy;

            var testEscape = 100;

            Vector3Int prevVistor = new Vector3Int(Mathf.RoundToInt(gx), Mathf.RoundToInt(gy), Mathf.RoundToInt(gz));

            do
            {
                Vector3Int visitor = new Vector3Int(Mathf.RoundToInt(gx), Mathf.RoundToInt(gy), Mathf.RoundToInt(gz));
                visits.Add(visitor);

                if (layer.ContainsKey(visitor))
                {
                    internalHit = visitor;
                    hitNormal = prevVistor - visitor;
                    return true;
                }

                if (gx == gx1idx && gy == gy1idx && gz == gz1idx) break;

                //Which plane do we cross first?
                var xr = Math.Abs(errx);
                var yr = Math.Abs(erry);
                var zr = Math.Abs(errz);

                if (sx != 0 && (sy == 0 || xr < yr) && (sz == 0 || xr < zr))
                {
                    gx += sx;
                    errx += derrx;
                }
                else if (sy != 0 && (sz == 0 || yr < zr))
                {
                    gy += sy;
                    erry += derry;
                }
                else if (sz != 0)
                {
                    gz += sz;
                    errz += derrz;
                }

                prevVistor = visitor;


            } while (testEscape-- > 0);

            return false;

        }
    }

}