// The contents of this file are sourced from https://github.com/d4rkc0d3r/d4rkAvatarOptimizer
/*
MIT License

Copyright (c) 2021 d4rkpl4y3r

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Prefabulous.Native.Shared.Editor
{
    internal static class Fromd4rk {
        public static void SetMeshIndexFormat(Mesh mesh, int newVertexCount)
        {
            // From d4rkAvatarOptimizer.cs :: CombineAndOptimizeMaterials
            mesh.indexFormat = newVertexCount >= 65536 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        }
        
        public static void SetUVs(Mesh mesh, Vector4[][] targetUv)
        {
            // From d4rkAvatarOptimizer.cs :: CombineAndOptimizeMaterials
            for (int i = 0; i < 8; i++)
            {
                var thatUV = targetUv[i];
                SetUV(mesh, i, thatUV, true);
            }
        }

        public static void SetUV(Mesh mesh, int channel, Vector4[] thatUV)
        {
            SetUV(mesh, channel, thatUV, false);
        }

        private static void SetUV(Mesh mesh, int channel, Vector4[] thatUV, bool doNothingIfThatUVIsOnlyZeroes)
        {
            // From d4rkAvatarOptimizer.cs :: CombineAndOptimizeMaterials
            if (thatUV.Any(uv => uv.w != 0))
            {
                mesh.SetUVs(channel, thatUV);
            }
            else if (thatUV.Any(uv => uv.z != 0))
            {
                mesh.SetUVs(channel, thatUV.Select(uv => new Vector3(uv.x, uv.y, uv.z)).ToArray());
            }
            else if (thatUV.Any(uv => uv.x != 0 || uv.y != 0))
            {
                mesh.SetUVs(channel, thatUV.Select(uv => new Vector2(uv.x, uv.y)).ToArray());
            }
            // Custom code (Haï)
            else if (!doNothingIfThatUVIsOnlyZeroes)
            {
                mesh.SetUVs(channel, Enumerable.Range(0, thatUV.Length).Select(i => Vector2.zero).ToArray());
            }
        }
    }
}