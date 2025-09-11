using _2DHelmholtz_solver.src.model_store.geom_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using OpenTK;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{
    public class fedata_store
    {
        public node_list_store fe_nodes {get; }
        public elementtri_list_store fe_tris { get; }
        public elementquad_list_store fe_quads { get; }

        public nodecnst_list_store fe_constraints { get; }
        public nodeload_list_store fe_loads { get; }

        public meshdata_store meshdata { get; }

        private double RoundToSixDigits(double value) => Math.Round(value, 6);

        public fedata_store()
        {
            // (Re)Initialize the data
            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_constraints = new nodecnst_list_store();
            fe_loads = new nodeload_list_store();

            meshdata = new meshdata_store();

        }

        public void importMesh(string fileContent)
        {


            var dataLines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int j = 0;


            while (j < dataLines.Length)
            {
                var line = dataLines[j].Trim();
                if (line == "*NODE")
                {
                    j++; // Move to the next line after *NODE

                    while (j < dataLines.Length)
                    {
                        var nodeLine = dataLines[j].Trim();
                        var splitValues = nodeLine.Split(',');

                        if (splitValues.Length != 4)
                            break;

                        try
                        {
                            int nodeId = int.Parse(splitValues[0]);
                            double x = RoundToSixDigits(double.Parse(splitValues[1]));
                            double y = RoundToSixDigits(double.Parse(splitValues[2]));
                            double z = RoundToSixDigits(double.Parse(splitValues[3]));

                            var nodePt = new Vector3((float)x, (float)y, (float)z); // Assuming you're using System.Numerics.Vector3
                            nodePtsList.Add(nodePt);

                            modelNodes.AddNode(nodeId, nodePt); // Assuming modelNodes is a class with AddNode method
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing node line: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    Console.WriteLine($"Nodes read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }



                if (line == "*ELEMENT,TYPE=S3")
                {
                    j++; // Move to the next line after *ELEMENT,TYPE=S3

                    while (j < dataLines.Length)
                    {
                        var elementLine = dataLines[j].Trim();
                        var splitValues = elementLine.Split(',');

                        if (splitValues.Length != 4)
                            break;

                        try
                        {
                            int triId = int.Parse(splitValues[0]);
                            int nd1 = int.Parse(splitValues[1]);
                            int nd2 = int.Parse(splitValues[2]);
                            int nd3 = int.Parse(splitValues[3]);

                            modelTriElements.AddElementTriangle(
                                triId,
                                modelNodes.NodeMap[nd1],
                                modelNodes.NodeMap[nd2],
                                modelNodes.NodeMap[nd3]
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing triangle element: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    Console.WriteLine($"Triangle Elements read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }

                if (line == "*ELEMENT,TYPE=S4")
                {
                    j++; // Move to the next line after *ELEMENT,TYPE=S4

                    while (j < dataLines.Length)
                    {
                        var elementLine = dataLines[j].Trim();
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

                            modelQuadElements.AddElementQuadrilateral(
                                quadId,
                                modelNodes.NodeMap[nd1],
                                modelNodes.NodeMap[nd2],
                                modelNodes.NodeMap[nd3],
                                modelNodes.NodeMap[nd4]
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing quadrilateral element: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    Console.WriteLine($"Quadrilateral Elements read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }


                if (line == "*MATERIAL_DATA")
                {
                    isMaterialExists = true;
                    matWindow.MaterialList.Clear(); // Clear existing materials

                    var triMaterialMap = new Dictionary<int, List<int>>();
                    var quadMaterialMap = new Dictionary<int, List<int>>();

                    j++; // Move to the next line after *MATERIAL_DATA

                    while (j < dataLines.Length)
                    {
                        var materialLine = dataLines[j].Trim();
                        var splitValues = materialLine.Split(',');

                        int numValues = splitValues.Length;

                        if (numValues == 7)
                        {
                            var tempMaterial = new MaterialData
                            {
                                MaterialId = int.Parse(splitValues[0]),
                                MaterialName = splitValues[1].Trim(),
                                YoungsModulus = double.Parse(splitValues[2]),
                                ShearModulus = double.Parse(splitValues[3]),
                                Density = double.Parse(splitValues[4]),
                                ShellThickness = double.Parse(splitValues[5]),
                                PoissonsRatio = double.Parse(splitValues[6])
                            };

                            // Add to material list
                            if (!matWindow.MaterialList.ContainsKey(tempMaterial.MaterialId))
                            {
                                matWindow.MaterialList[tempMaterial.MaterialId] = tempMaterial;
                            }
                        }
                        else if (numValues == 3)
                        {
                            int elmId = int.Parse(splitValues[0]);
                            int elmType = int.Parse(splitValues[1]);
                            int materialId = int.Parse(splitValues[2]);

                            if (elmType == 1)
                            {
                                if (!triMaterialMap.ContainsKey(materialId))
                                    triMaterialMap[materialId] = new List<int>();

                                triMaterialMap[materialId].Add(elmId);
                            }
                            else if (elmType == 2)
                            {
                                if (!quadMaterialMap.ContainsKey(materialId))
                                    quadMaterialMap[materialId] = new List<int>();

                                quadMaterialMap[materialId].Add(elmId);
                            }
                        }
                        else
                        {
                            foreach (var kvp in triMaterialMap)
                            {
                                modelTriElements.UpdateMaterial(kvp.Value, kvp.Key);
                            }

                            foreach (var kvp in quadMaterialMap)
                            {
                                modelQuadElements.UpdateMaterial(kvp.Value, kvp.Key);
                            }

                            Console.WriteLine($"Material data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                            break;
                        }

                        j++;
                    }

                }


                if (line == "*CONSTRAINT_DATA")
                {
                    j++; // Move to the next line after *CONSTRAINT_DATA

                    while (j < dataLines.Length)
                    {
                        var constraintLine = dataLines[j].Trim();
                        var splitValues = constraintLine.Split(',');

                        if (splitValues.Length != 8)
                            break;

                        try
                        {
                            int nodeId = int.Parse(splitValues[0]);

                            var location = new Vector3(
                                (float)double.Parse(splitValues[1]),
                                (float)double.Parse(splitValues[2]),
                                (float)double.Parse(splitValues[3])
                            );

                            var normal = new Vector3(
                                (float)double.Parse(splitValues[4]),
                                (float)double.Parse(splitValues[5]),
                                (float)double.Parse(splitValues[6])
                            );

                            int constraintType = int.Parse(splitValues[7]);

                            nodeConstraints.AddNodeConstraint(nodeId, location, normal, constraintType);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing constraint data: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    Console.WriteLine($"Constraint data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }



                if (line == "*LOAD_DATA")
                {
                    var loadSetData = new Dictionary<int, LoadData>();
                    j++; // Move to the next line after *LOAD_DATA

                    while (j < dataLines.Length)
                    {
                        var loadLine = dataLines[j].Trim();
                        var splitValues = loadLine.Split(',');

                        if (splitValues.Length != 11)
                            break;

                        try
                        {
                            int loadSetId = int.Parse(splitValues[0]);
                            int nodeId = int.Parse(splitValues[1]);

                            var location = new Vector3(
                                (float)double.Parse(splitValues[2]),
                                (float)double.Parse(splitValues[3]),
                                (float)double.Parse(splitValues[4])
                            );

                            var normal = new Vector3(
                                (float)double.Parse(splitValues[5]),
                                (float)double.Parse(splitValues[6]),
                                (float)double.Parse(splitValues[7])
                            );

                            double value = double.Parse(splitValues[8]);
                            double startTime = double.Parse(splitValues[9]);
                            double endTime = double.Parse(splitValues[10]);

                            if (!loadSetData.ContainsKey(loadSetId))
                                loadSetData[loadSetId] = new LoadData();

                            var loadEntry = loadSetData[loadSetId];
                            loadEntry.NodeIds.Add(nodeId);
                            loadEntry.LoadLocations.Add(location);
                            loadEntry.LoadNormals.Add(normal);

                            if (loadEntry.NodeIds.Count == 1)
                            {
                                loadEntry.LoadValue = value;
                                loadEntry.LoadStartTime = startTime;
                                loadEntry.LoadEndTime = endTime;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing load data: {ex.Message}");
                            break;
                        }

                        j++;
                    }

                    // Add to main load storage
                    foreach (var kvp in loadSetData)
                    {
                        var ld = kvp.Value;
                        nodeLoads.AddLoads(ld.NodeIds, ld.LoadLocations, ld.LoadNormals, ld.LoadStartTime, ld.LoadEndTime, ld.LoadValue);
                    }

                    Console.WriteLine($"Load data read completed at {stopwatch.Elapsed.TotalSeconds:F2} secs");
                }


                // Iterate to next line

                j++;
            }





        }


    }
}
