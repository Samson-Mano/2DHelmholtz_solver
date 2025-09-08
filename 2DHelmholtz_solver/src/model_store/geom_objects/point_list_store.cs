using _2DHelmholtz_solver.opentk_control.shader_compiler;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_buffer;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class point_store
    {
        public int point_id { get; set; }
        public double x_coord { get; set; }
        public double y_coord { get; set; }
        public double z_coord { get; set; } 
        public int point_index { get; set; }
        public double normalized_defl_scale { get; set; } 

        public Vector3 pt_coord
        {
            get { return new Vector3((float)x_coord, (float)y_coord, (float)z_coord); }
        }


    }


    public class point_list_store
    {
        public Dictionary<int, point_store> pointMap = new Dictionary<int, point_store>();
        public int point_count = 0;

        private graphicBuffers point_buffer;
        private Shader point_shader;

        public point_list_store()
        {
            // (Re)Initialize the data
            pointMap = new Dictionary<int, point_store>();
            point_count = 0;

        }

        public void add_point(int point_id, double x_coord, double y_coord, double z_coord)
        {
            // Add the Point to the list
            point_store temp_point = new point_store
            {
                point_id = point_id,
                x_coord = x_coord,
                y_coord = y_coord,
                z_coord = z_coord,
                point_index = point_count,
                normalized_defl_scale = 0.0
            };

            pointMap[point_id] = temp_point;
            point_count++;

        }

        public void update_point(int point_id, double x_coord, double y_coord, double z_coord, double normalized_defl_scale)
        {
            // Update the point co-ordinates
            pointMap[point_id].x_coord = x_coord;
            pointMap[point_id].y_coord = y_coord;
            pointMap[point_id].z_coord = z_coord;
            pointMap[point_id].normalized_defl_scale = normalized_defl_scale;

        }

        public void set_buffer()
        {


        }

        public void update_buffer()
        {


        }
        
        public void set_point_color()
        {

        }


        public void paint_static_points()
        {

        }


        public void paint_dynamic_points()
        {


        }





    }
}
