using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delaunary
{

    /// <summary>
    /// 三角形外接圆相关类
    /// </summary>
    class CircumCircle
    {
        /// <summary>
        /// 三角形三顶点中的一个
        /// </summary>
        public int i { get; set; }

        /// <summary>
        /// 三角形三顶点中的一个
        /// </summary>
        public int j { get; set; }


        /// <summary>
        /// 三角形三顶点中的一个
        /// </summary>
        public int k { get; set; }

        /// <summary>
        /// 外接圆圆心X坐标
        /// </summary>
        public double x { get; set; }


        /// <summary>
        /// 外接圆圆心Y坐标
        /// </summary>
        public double y { get; set; }

        /// <summary>
        /// 外接圆半径的平方
        /// </summary>
        public double r { get; set; }

    }
}
