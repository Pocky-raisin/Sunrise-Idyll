using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using Sunrise;


namespace Sunrise
{
    public static class TrespasserGraphics
    {
        public static void ApplyHooks()
        {

        }





        private static void ApplyMeshTexture(TriangleMesh triMesh) //Code adapted from SlimeCubed's CustomTails
        {
            if (triMesh.verticeColors == null || triMesh.verticeColors.Length != triMesh.vertices.Length)
            {
                triMesh.verticeColors = new Color[triMesh.vertices.Length];
            }
            triMesh.customColor = true;

            for (var j = triMesh.verticeColors.Length - 1; j >= 0; j--)
            {
                var num = (j / 2f) / (triMesh.verticeColors.Length / 2f);
                triMesh.verticeColors[j] = triMesh.color;
                Vector2 vector;
                if (j % 2 == 0)
                {
                    vector = new Vector2(num, 0f);
                }
                else if (j < triMesh.verticeColors.Length - 1)
                {
                    vector = new Vector2(num, 1f);
                }
                else
                {
                    vector = new Vector2(1f, 0f);
                }
                vector.x = Mathf.Lerp(triMesh.element.uvBottomLeft.x, triMesh.element.uvTopRight.x, vector.x);
                vector.y = Mathf.Lerp(triMesh.element.uvBottomLeft.y, triMesh.element.uvTopRight.y, vector.y);
                triMesh.UVvertices[j] = vector;
            }
            triMesh.Refresh();
        }

    }
}