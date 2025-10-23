#include "helmholtz_system_store.h"


helmholtz_system_store::helmholtz_system_store()
{
	// Empty constructor
}

void helmholtz_system_store::add_node(const int& node_id, 
	const double& x_coord, const double& y_coord)
{
	// Node addition


}

void helmholtz_system_store::add_edge(const int& edge_id, 
	const int& startnodeid, const int& endnodeid)
{
	// Edge addition


}

void helmholtz_system_store::add_trielement(const int& tri_id, 
	const int& nodeid1, const int& nodeid2, const int& nodeid3, 
	const int& materialid)
{
	// Triangle element addition


}

void helmholtz_system_store::add_quadelement(const int& quad_id, 
	const int& nodeid1, const int& nodeid2, 
	const int& nodeid3, const int& nodeid4, const int& materialid)
{
	// Quadrilateral element addition


}

void helmholtz_system_store::add_material(const int& materialid, 
	const double& permittivity, const double& permeability)
{
	// Material addition


}

void helmholtz_system_store::add_nodeconstraint(const int& node_id, 
	const double& fieldvalue, const double& sourcevalue)
{
	// Node constraint addition


}

void helmholtz_system_store::add_edgeconstraint(const int& edge_id, 
	const bool& isSommerfieldBC, const double& fieldvalue, 
	const double& normalderivfieldvalue)
{
	// Edge constraint addition


}




