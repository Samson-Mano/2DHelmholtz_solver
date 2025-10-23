#pragma once
#include <Eigen/Dense>
#include <unordered_map>


struct node_store
{
	int node_id = 0;
	double x_coord = 0.0;
	double y_coord = 0.0;

	bool isboundarynode = false;
	double fieldvalue = 0.0; // Field value in the node
	double sourcevalue = 0.0; // Source value in the node

};

struct edge_store
{
	int edge_id = 0;
	int startnodeid = 0;
	int endnodeid = 0;

	bool isboundaryedge = false;
	bool isSommerfieldBC = false;
	double fieldvalue = 0.0; 
	double normalderivfieldvalue = 0.0;

};

struct trielement_store
{
	int tri_id = 0;
	int nodeid1 = 0;
	int nodeid2 = 0;
	int nodeid3 = 0;
	int materialid = 0;

};

struct quadelement_store
{
	int quad_id = 0;
	int nodeid1 = 0;
	int nodeid2 = 0;
	int nodeid3 = 0;
	int nodeid4 = 0;
	int materialid = 0;

};


struct material_store
{
	int materialid = 0;
	double permittivity = 0.0;
	double permeability = 0.0;

};


class helmholtz_system_store
{
public:
	helmholtz_system_store();
	~helmholtz_system_store() = default;

	void add_node(const int& node_id,
		const double& x_coord,
		const double& y_coord);
	
	void add_edge(const int& edge_id,
		const int& startnodeid,
		const int& endnodeid);

	void add_trielement(const int& tri_id,
		const int& nodeid1,
		const int& nodeid2,
		const int& nodeid3,
		const int& materialid);

	void add_quadelement(const int& quad_id,
		const int& nodeid1,
		const int& nodeid2,
		const int& nodeid3,
		const int& nodeid4,
		const int& materialid);

	void add_material(const int& materialid,
		const double& permittivity,
		const double& permeability);

	void add_nodeconstraint(const int& node_id,
		const double& fieldvalue,
		const double& sourcevalue);

	void add_edgeconstraint(const int& edge_id,
		const bool& isSommerfieldBC,
		const double& fieldvalue,
		const double& normalderivfieldvalue);


private:
	std::unordered_map<int, node_store> node_list;
	std::unordered_map<int, edge_store> edge_list;
	std::unordered_map<int, trielement_store> trielement_list;
	std::unordered_map<int, quadelement_store> quadelement_list;
	std::unordered_map<int, material_store> material_list;


};

