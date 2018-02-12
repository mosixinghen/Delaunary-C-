using Delaunary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DelaunaryTest.Controlers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            List<Point2D> vertices = new List<Point2D>();
            double x, y;
            Random ran = new Random();
            for (var i = 0; i < 5000; i++)
            {
                do
                {
                    x = ran.NextDouble() - 0.5;
                    y = ran.NextDouble() - 0.5;
                } while (x * x + y * y > 0.25);

                x = (x * 0.96875 + 0.5) * 1024;
                y = (y * 0.96875 + 0.5) * 1024;

                vertices.Add(new Point2D() { X = x, Y = y });
            }
            var t = Delaunary.Delaunay.triangulate(vertices);
            data d = new data() {
                vertices=vertices,
                triangles=t
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return View(null,null,json);
        }
    }
    public class data {
       public List<Point2D> vertices { get; set; }
       public List<int> triangles { get; set; }
    }
}
