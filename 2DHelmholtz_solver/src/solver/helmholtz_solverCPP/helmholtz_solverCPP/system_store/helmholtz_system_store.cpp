#include "helmholtz_system_store.h"


helmholtz_system_store::helmholtz_system_store()
{
	// Empty constructor
}

void helmholtz_system_store::add_node(const int& node_id, 
	const double& x_coord, const double& y_coord)
{
	// Node addition
	node_store temp_node;
	temp_node.node_id = node_id;
	temp_node.x_coord = x_coord;
	temp_node.y_coord = y_coord;
	temp_node.isboundarynode = false;
	temp_node.fieldvalue = 0.0; // Field value in the node
	temp_node.sourcevalue = 0.0; // Source value in the node

	// Insert to the node list
	node_list.insert({ node_id, temp_node });

}

void helmholtz_system_store::add_edge(const int& edge_id, 
	const int& startnodeid, const int& endnodeid)
{
	// Edge addition
	edge_store temp_edge;
	temp_edge.edge_id = edge_id;
	temp_edge.startnodeid = startnodeid;
	temp_edge.endnodeid = endnodeid;
	temp_edge.isboundaryedge = false;
	temp_edge.isSommerfieldBC = false;
	temp_edge.fieldvalue = 0.0;
	temp_edge.normalderivfieldvalue = 0.0;

	// Insert to the edge list
	edge_list.insert({ edge_id, temp_edge });

	// Add edge to node-to-edge map for both start and end nodes
	node_edge_map[startnodeid].push_back(edge_id);
	node_edge_map[endnodeid].push_back(edge_id);

}


void helmholtz_system_store::add_trielement(const int& tri_id, 
	const int& nodeid1, const int& nodeid2, const int& nodeid3, 
	const int& materialid)
{
	// Triangle element addition
	trielement_store temp_trielement;
	temp_trielement.tri_id = tri_id;
	temp_trielement.nodeid1 = nodeid1;
	temp_trielement.nodeid2 = nodeid2;
	temp_trielement.nodeid3 = nodeid3;
	temp_trielement.materialid = materialid;

	// Insert to the tri element list
	trielement_list.insert({ tri_id, temp_trielement });

}

void helmholtz_system_store::add_quadelement(const int& quad_id, 
	const int& nodeid1, const int& nodeid2, 
	const int& nodeid3, const int& nodeid4, const int& materialid)
{
	// Quadrilateral element addition
	quadelement_store temp_quadelement;
	temp_quadelement.quad_id = quad_id;
	temp_quadelement.nodeid1 = nodeid1;
	temp_quadelement.nodeid2 = nodeid2;
	temp_quadelement.nodeid3 = nodeid3;
	temp_quadelement.nodeid4 = nodeid4;
	temp_quadelement.materialid = materialid;

	// Insert to the quad element list
	quadelement_list.insert({ quad_id, temp_quadelement });
}

void helmholtz_system_store::add_material(const int& materialid, 
	const double& permittivity, const double& permeability,
	const double& wave_number)
{
	// Material addition
	material_store temp_material;
	temp_material.materialid = materialid;
	temp_material.permittivity = permittivity;
	temp_material.permeability = permeability;
	temp_material.wave_number = wave_number;

	// Insert to the material list
	material_list.insert({ materialid, temp_material });

}

void helmholtz_system_store::add_nodeconstraint(const int& node_id, 
	const bool& isFieldBC,
	const double& fieldvalue, const double& sourcevalue)
{
	// Node constraint addition
	node_list[node_id].isboundarynode = true;
	node_list[node_id].isFieldBC = isFieldBC;
	node_list[node_id].fieldvalue = fieldvalue;
	node_list[node_id].sourcevalue = sourcevalue;

}

void helmholtz_system_store::add_edgeconstraint(const int& edge_id, 
	const bool& isSommerfieldBC, const bool& isFieldBC,
	const bool& isDerivFieldBC, const double& fieldvalue,
	const double& normalderivfieldvalue)
{
	// Edge constraint addition
	edge_list[edge_id].isboundaryedge = true;
	edge_list[edge_id].isSommerfieldBC = isSommerfieldBC;
	edge_list[edge_id].isFieldBC = isFieldBC;
	edge_list[edge_id].isDerivFieldBC = isDerivFieldBC;
	edge_list[edge_id].fieldvalue = fieldvalue;
	edge_list[edge_id].normalderivfieldvalue = normalderivfieldvalue;

}











