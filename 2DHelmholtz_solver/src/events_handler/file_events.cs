using _2DHelmholtz_solver.src.model_store.fe_objects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.src.events_handler
{
    public static class file_events
    {
        private static double RoundToSixDigits(double value) => Math.Round(value, 6);


        public static void import_mesh(string fileContent,
                    ref node_list_store fe_nodes,
                    ref elementtri_list_store fe_tris,
                    ref elementquad_list_store fe_quads,
                    ref nodecnst_list_store fe_constraints,
                    ref nodeload_list_store fe_loads,
                    ref Dictionary<int, material_data> fe_materials,
                    ref List<Vector3> nodePtsList,
                    ref bool isModelLoadSuccess)
        {

            // Clear the data
            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_constraints = new nodecnst_list_store();
            fe_loads = new nodeload_list_store();


            var dataLines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int j = 0;

            // Node point list to capture the bounding geometry
            nodePtsList = new List<Vector3>();
            bool is_material_inpt_exists = false;

            while (j < dataLines.Length)
            {
                var line = dataLines[j].Trim();

                if (line == "*NODE")
                {

                    while (j < dataLines.Length)
                    {
                        var nodeLine = dataLines[j + 1].Trim();
                        var splitValues = nodeLine.Split(',');

                        if (splitValues.Length != 4)
                            break;

                        try
                        {
                            int nodeId = int.Parse(splitValues[0]);
                            double x = RoundToSixDigits(double.Parse(splitValues[1]));
                            double y = RoundToSixDigits(double.Parse(splitValues[2]));
                            double z = RoundToSixDigits(double.Parse(splitValues[3]));

                            var nodePt = new Vector3((float)x, (float)y, (float)z); 
                            nodePtsList.Add(nodePt);

                            // node added to the node list
                            fe_nodes.add_node(nodeId, x,y,z); 
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing node line: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }

                        j++;
                    }

                    // Console.WriteLine($"Nodes read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }



                if (line == "*ELEMENT,TYPE=S3")
                {
 
                    while (j < dataLines.Length)
                    {
                        var elementLine = dataLines[j + 1].Trim();
                        var splitValues = elementLine.Split(',');

                        if (splitValues.Length != 4)
                            break;

                        try
                        {
                            int triId = int.Parse(splitValues[0]);
                            int nd1 = int.Parse(splitValues[1]);
                            int nd2 = int.Parse(splitValues[2]);
                            int nd3 = int.Parse(splitValues[3]);

                            // Tirangle mesh added to the list
                            fe_tris.add_elementtriangle(triId, nd1 , nd2, nd3); 
                                
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing triangle element: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing triangle element: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                   // Console.WriteLine($"Triangle Elements read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }

                if (line == "*ELEMENT,TYPE=S4")
                {

                    while (j < dataLines.Length)
                    {
                        var elementLine = dataLines[j + 1].Trim();
                        var splitValues = elementLine.Split(',');

                        if (splitValues.Length != 5)
                            break;

                        try
                        {
                            int quadId = int.Parse(splitValues[0]);
                            int nd1 = int.Parse(splitValues[1]);
                            int nd2 = int.Parse(splitValues[2]);
                            int nd3 = int.Parse(splitValues[3]);
                            int nd4 = int.Parse(splitValues[4]);

                            // Quadrilateral mesh added to the list
                            fe_quads.add_elementquadrilateral(quadId, nd1 , nd2,nd3, nd4);
                            
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing quadrilateral element: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing quadrilateral element: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Console.WriteLine($"Quadrilateral Elements read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }


                if (line == "*MATERIAL_DATA")
                {
                    is_material_inpt_exists = true;
                    fe_materials.Clear(); // Clear existing materials

                    var triMaterialMap = new Dictionary<int, List<int>>();
                    var quadMaterialMap = new Dictionary<int, List<int>>();

                    while (j < dataLines.Length)
                    {
                        var materialLine = dataLines[j + 1].Trim();
                        var splitValues = materialLine.Split(',');

                        int numValues = splitValues.Length;

                        if (numValues == 7)
                        {
                            var tempMaterial = new material_data
                            {
                                material_id = int.Parse(splitValues[0]),
                                material_name = splitValues[1].Trim(),
                                material_youngsmodulus = double.Parse(splitValues[2]),
                                material_shearmodulus = double.Parse(splitValues[3]),
                                material_density = double.Parse(splitValues[4]),
                                shell_thickness = double.Parse(splitValues[5]),
                                poissons_ratio = double.Parse(splitValues[6])
                            };

                            // Add to material list
                            if (!fe_materials.ContainsKey(tempMaterial.material_id))
                            {
                                fe_materials[tempMaterial.material_id] = tempMaterial;
                            }
                        }
                        else if (numValues == 3)
                        {
                            int elmId = int.Parse(splitValues[0]); // Element ID
                            int elmType = int.Parse(splitValues[1]); // Element type 1 = Triangle, 2 = Quadrilateral
                            int materialId = int.Parse(splitValues[2]); // Material ID

                            if (elmType == 1)
                            {
                                // Add the Triangle element id to material ID map 
                                if (!triMaterialMap.ContainsKey(materialId))
                                    triMaterialMap[materialId] = new List<int>();

                                triMaterialMap[materialId].Add(elmId);
                            }
                            else if (elmType == 2)
                            {
                                // Add the Quadrilateral element id to material ID map 
                                if (!quadMaterialMap.ContainsKey(materialId))
                                    quadMaterialMap[materialId] = new List<int>();

                                quadMaterialMap[materialId].Add(elmId);
                            }
                        }
                        else
                        {
                            // Apply all materials at once
                            foreach (var kvp in triMaterialMap)
                            {
                                // value = list of tri element id, Key = material id
                                fe_tris.update_material(kvp.Value, kvp.Key);
                            }

                            foreach (var kvp in quadMaterialMap)
                            {
                                // value = list of quad element id, Key = material id
                                fe_quads.update_material(kvp.Value, kvp.Key);
                            }

                            // Console.WriteLine($"Material data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                            break;
                        }

                        j++;
                    }

                }


                if (line == "*CONSTRAINT_DATA")
                {

                    while (j < dataLines.Length)
                    {
                        var constraintLine = dataLines[j + 1].Trim();
                        var splitValues = constraintLine.Split(',');

                        if (splitValues.Length != 2)
                            break;

                        try
                        {
                            int nodeId = int.Parse(splitValues[0]);
                            int constraintType = int.Parse(splitValues[1]);

                            // Add the node constraint to list
                            fe_constraints.add_nodeconstraint(nodeId, constraintType);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing constraint data: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing constraint data: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Console.WriteLine($"Constraint data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }



                if (line == "*LOAD_DATA")
                {
                    Dictionary<int, nodeload_data> loadSetData = new Dictionary<int, nodeload_data>();

                    while (j < dataLines.Length)
                    {
                        var loadLine = dataLines[j + 1].Trim();
                        var splitValues = loadLine.Split(',');

                        if (splitValues.Length != 5)
                            break;

                        try
                        {
                            int loadSetId = int.Parse(splitValues[0]);
                            int nodeId = int.Parse(splitValues[1]);
                            double load_amplitude = double.Parse(splitValues[2]);
                            double load_frequency = double.Parse(splitValues[3]);
                            double load_phase = double.Parse(splitValues[4]);

                            if (!loadSetData.ContainsKey(loadSetId))
                                loadSetData[loadSetId] = new nodeload_data();

                            var loadEntry = loadSetData[loadSetId];
                            loadEntry.load_node_ids.Add(nodeId); // Add the multiple nodes where the particular load set is applied

                            // Add the load amplitude when the first node is added (all the nodes have same load values)
                            if (loadEntry.load_node_ids.Count == 1)
                            {
                                loadEntry.load_amplitude = load_amplitude;
                                loadEntry.load_frequency = load_frequency;
                                loadEntry.load_phase = load_phase;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error parsing load data: {ex.Message}", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // Console.WriteLine($"Error parsing load data: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Add to main load storage
                    foreach (var kvp in loadSetData)
                    {
                        var ld = kvp.Value;

                        // Add the node loads to the list
                        fe_loads.add_loads(ld.load_node_ids, ld.load_amplitude, ld.load_frequency, ld.load_phase);
                    }
                    // Console.WriteLine($"Load data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }


                // Iterate to next line

                j++;
            }


            // Check the model
            if(fe_nodes.node_count < 2 || (fe_tris.elementtri_count + fe_quads.elementquad_count) < 1)
            {
                isModelLoadSuccess = false;
                MessageBox.Show("Input error!! ", "Model Import Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }

            // No material is assigned in the model (Add a default material)
            if(is_material_inpt_exists == false || fe_materials.Count == 0)
            {
                var tempMaterial = new material_data
                {
                    material_id = 0, // material id
                    material_name = "Default material", // Default material name
                    material_youngsmodulus = 2.07 * Math.Pow(10, 5), //  MPa
                    material_shearmodulus = 0.80 * Math.Pow(10, 5), //  MPa
                    material_density = 7.83 * Math.Pow(10, -9), // tons/mm3
                    shell_thickness = 10.0, // mm
                    poissons_ratio = 0.3
                };

                // Add to the material list
                fe_materials.Clear();
                fe_materials[tempMaterial.material_id] = tempMaterial;


                // Add default material id to all the elements
                List<int> selected_tri_elm_ids = new List<int>();
                List<int> selected_quad_elm_ids = new List<int>();

                foreach(var tri in fe_tris.elementtriMap)
                {
                    selected_tri_elm_ids.Add(tri.Key);
                }

                // Add default material to triangle mesh
                fe_tris.update_material(selected_tri_elm_ids, tempMaterial.material_id);


                foreach (var quad in fe_quads.elementquadMap)
                {
                    selected_quad_elm_ids.Add(quad.Key);
                }

                // Add default material to quadrilateral mesh
                fe_quads.update_material(selected_quad_elm_ids, tempMaterial.material_id);

            }


            // Model load is successful
            isModelLoadSuccess = true;


        }


    }
}
