﻿/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Box2D.XNA
{
    public class CircleShape : Shape
    {
        public CircleShape()
        {
            ShapeType = ShapeType.Circle;
            _radius = 0.0f;
            _p = Vector2.Zero;
        }

        /// <summary>
        /// Implement Shape.
        /// </summary>
        /// <returns>Shape</returns>
        public override Shape Clone()
        {
            CircleShape shape = new CircleShape();
            shape.ShapeType = ShapeType;
            shape._radius = _radius;
            shape._p = _p;

            return shape;
        }

        /// <summary>
        /// @see Shape.TestPoint
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool TestPoint(ref Transform transform, Vector2 p)
        {
            Vector2 center = transform.Position + MathUtils.Multiply(ref transform.R, _p);
            Vector2 d = p - center;
            return Vector2.Dot(d, d) <= _radius * _radius;
        }

        /// <summary>
        /// Collision Detection in Interactive 3D Environments by Gino van den Bergen
        /// From Section 3.1.2
        /// x = s + a * r
        /// norm(x) = radius
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform)
        {
            output = new RayCastOutput();

            Vector2 position = transform.Position + MathUtils.Multiply(ref transform.R, _p);
            Vector2 s = input.p1 - position;
            float b = Vector2.Dot(s, s) - _radius * _radius;

            // Solve quadratic equation.
            Vector2 r = input.p2 - input.p1;
            float c = Vector2.Dot(s, r);
            float rr = Vector2.Dot(r, r);
            float sigma = c * c - rr * b;

            // Check for negative discriminant and short segment.
            if (sigma < 0.0f || rr < Settings.b2_epsilon)
            {
                return false;
            }

            // Find the point of intersection of the line with the circle.
            float a = -(c + (float)Math.Sqrt((double)sigma));

            // Is the intersection point on the segment?
            if (0.0f <= a && a <= input.maxFraction * rr)
            {
                a /= rr;
                output.fraction = a;
                Vector2 norm = (s + a * r);
                norm.Normalize();
                output.normal = norm;
                return true;
            }

            return false;
        }

        /// <summary>
        /// @see Shape.ComputeAABB
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="transform"></param>
        public override void ComputeAABB(out AABB aabb, ref Transform transform)
        {
            Vector2 p = transform.Position + MathUtils.Multiply(ref transform.R, _p);
            aabb.lowerBound = new Vector2(p.X - _radius, p.Y - _radius);
            aabb.upperBound = new Vector2(p.X + _radius, p.Y + _radius);
        }

        /// <summary>
        /// @see Shape.ComputeMass
        /// </summary>
        /// <param name="massData"></param>
        /// <param name="density"></param>
        public override void ComputeMass(out MassData massData, float density)
        {
            massData.mass = density * Settings.b2_pi * _radius * _radius;
            massData.center = _p;

            // inertia about the local origin
            massData.I = massData.mass * (0.5f * _radius * _radius + Vector2.Dot(_p, _p));
        }

        /// <summary>
        /// Get the supporting vertex index in the given direction.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public override int GetSupport(Vector2 d)
        {
            return 0;
        }

        /// <summary>
        /// Get the supporting vertex in the given direction.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public override Vector2 GetSupportVertex(Vector2 d)
        {
            return _p;
        }

        /// <summary>
        /// Get the vertex count.
        /// </summary>
        /// <returns>One</returns>
        public override int GetVertexCount() { return 1; }

        /// <summary>
        /// Get a vertex by index. Used by b2Distance.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override Vector2 GetVertex(int index)
        {
            Debug.Assert(index == 0);
            return _p;
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector2 _p;
    }
}
