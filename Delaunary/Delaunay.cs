using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Delaunary
{
    public class Delaunay
    {
        const double EPSILON = 1.0 / 1048576.0;
        private static CircumCircle circumcircle(List<Point2D> vertices, int i, int j, int k)
        {
            double x1 = vertices[i].X,
                y1 = vertices[i].Y,
                x2 = vertices[j].X,
                y2 = vertices[j].Y,
                x3 = vertices[k].X,
                y3 = vertices[k].Y,
                fabsy1y2 = Math.Abs(y1 - y2),
                fabsy2y3 = Math.Abs(y2 - y3),
                xc, yc, m1, m2, mx1, mx2, my1, my2, dx, dy;

            if (fabsy1y2 < EPSILON && fabsy2y3 < EPSILON)
                return null;
            if (fabsy1y2 < EPSILON)
            {
                m2 = -((x3 - x2) / (y3 - y2));
                mx2 = (x2 + x3) / 2.0;
                my2 = (y2 + y3) / 2.0;
                xc = (x2 + x1) / 2.0;
                yc = m2 * (xc - mx2) + my2;
            }

            else if (fabsy2y3 < EPSILON)
            {
                m1 = -((x2 - x1) / (y2 - y1));
                mx1 = (x1 + x2) / 2.0;
                my1 = (y1 + y2) / 2.0;
                xc = (x3 + x2) / 2.0;
                yc = m1 * (xc - mx1) + my1;
            }

            else
            {
                m1 = -((x2 - x1) / (y2 - y1));
                m2 = -((x3 - x2) / (y3 - y2));
                mx1 = (x1 + x2) / 2.0;
                mx2 = (x2 + x3) / 2.0;
                my1 = (y1 + y2) / 2.0;
                my2 = (y2 + y3) / 2.0;
                xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                yc = (fabsy1y2 > fabsy2y3) ?
                  m1 * (xc - mx1) + my1 :
                  m2 * (xc - mx2) + my2;
            }

            dx = x2 - xc;
            dy = y2 - yc;
            return new CircumCircle()
            {
                i = i,
                j = j,
                k = k,
                x = xc,
                y = yc,
                r = dx * dx + dy * dy,
            };
        }

        private static void dedup(List<int> edges)
        {
            int a, b, m, n;
            int j = edges.Count;
            while (j > 1)
            {
                j = edges.Count < j ? edges.Count : j;
                b = edges[--j];
                a = edges[--j];
                var i = j;
                while (i > 1)
                {
                    i = edges.Count < i ? edges.Count : i;
                    n = edges[--i];
                    m = edges[--i];

                    if ((a == m && b == n) || (a == n && b == m))
                    {
                        edges.RemoveRange(j, 2);
                        edges.RemoveRange(i, 2);
                        break;
                    }
                }
            }
        }

        public static List<int> triangulate(List<Point2D> vertices)
        {
            var n = vertices.Count;
            int[] indices = new int[n];
            List<int> triangles = new List<int>();
            Triangle st;
            List<CircumCircle> open = new List<Delaunary.CircumCircle>();
            List<CircumCircle> closed = new List<Delaunary.CircumCircle>();
            List<int> edges = new List<int>();
            double dx, dy;
            int a, b, c;
            if (n < 3)
                return null;

            List<Tuple<int, Point2D>> relations = new List<Tuple<int, Point2D>>();
            for (var i = 0; i < vertices.Count; i++)
            {
                relations.Add(new Tuple<int, Point2D>(i, vertices[i]));
            }

            relations.Sort(new Comparison<Tuple<int, Point2D>>((r1, r2) =>
            {
                var diff = r1.Item2.X - r2.Item2.X;
                return (int)diff;
            }));
            relations.Reverse();
            for (var i = 0; i < indices.Length; i++)
            {
                indices[i] = relations[i].Item1;
            }

            st = supertriangle(vertices);
            vertices.Add(st.P1);
            vertices.Add(st.P2);
            vertices.Add(st.P3);

            open.Add(circumcircle(vertices, n + 0, n + 1, n + 2));

            for (var i = indices.Length - 1; i >= 0; i--)
            {
                edges.Clear();
                c = indices[i];
                for (var j = open.Count - 1; j >= 0; j--)
                {
                    dx = vertices[c].X - open[j].x;
                    if (dx > 0.0 && dx * dx > open[j].r)
                    {
                        closed.Add(open[j]);
                        open.RemoveRange(j, 1);
                        continue;
                    }

                    dy = vertices[c].Y - open[j].y;
                    if (dx * dx + dy * dy - open[j].r > EPSILON)
                        continue;

                    edges.Add(open[j].i);
                    edges.Add(open[j].j);
                    edges.Add(open[j].j);
                    edges.Add(open[j].k);
                    edges.Add(open[j].k);
                    edges.Add(open[j].i);

                    open.RemoveRange(j, 1);
                }
                dedup(edges);
                var m = edges.Count;
                while (m > 0)
                {
                    b = edges[--m];
                    a = edges[--m];
                    var circum = circumcircle(vertices, a, b, c);
                    if (circum != null)
                    {
                        open.Add(circum);
                    }
                }
            }
            for (var i = open.Count - 1; i >= 0; i--)
                closed.Add(open[i]);

            for (var i = closed.Count - 1; i >= 0; i--)
                if (closed[i].i < n && closed[i].j < n && closed[i].k < n)
                {
                    triangles.Add(closed[i].i);
                    triangles.Add(closed[i].j);
                    triangles.Add(closed[i].k);
                }

            return triangles;
        }

        private static Triangle supertriangle(List<Point2D> vertices)
        {
            double xmin = 0, ymin = 0, xmax = 0, ymax = 0, dx = 0, dy = 0, dmax = 0, xmid = 0, ymid = 0;

            for (var i = vertices.Count - 1; i >= 0; i--)
            {
                if (vertices[i].X < xmin) xmin = vertices[i].X;
                if (vertices[i].X > xmax) xmax = vertices[i].X;
                if (vertices[i].Y < ymin) ymin = vertices[i].Y;
                if (vertices[i].Y > ymax) ymax = vertices[i].Y;
            }

            dx = xmax - xmin;
            dy = ymax - ymin;
            dmax = Math.Max(dx, dy);
            xmid = xmin + dx * 0.5;
            ymid = ymin + dy * 0.5;

            return new Triangle()
            {
                P1 = new Point2D()
                {
                    X = xmid - 20 * dmax,
                    Y = ymid - dmax
                },
                P2 = new Point2D()
                {
                    X = xmid,
                    Y = ymid + 20 * dmax
                },
                P3 = new Point2D()
                {
                    X = xmid + 20 * dmax,
                    Y = ymid - dmax
                }
            };
        }

    }
}