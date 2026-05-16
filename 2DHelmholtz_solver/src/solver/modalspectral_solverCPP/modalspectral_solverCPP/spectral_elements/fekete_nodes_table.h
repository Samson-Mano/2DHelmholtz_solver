#pragma once
#include <vector>


struct fekete_node
{
	double xi;     // reference coordinate
	double eta;    // reference coordinate
	double weight; // quadrature weight
};


struct fekete_table
{

	std::vector<fekete_node> corner_nodes;
	std::vector<std::vector<fekete_node>> edge_nodes; // 3 edges
	std::vector<fekete_node> internal_nodes;
};



class fekete_nodes_table
{
public:

	static const fekete_table& get_fekete_nodes(int spectral_order);


private:
	// 2D Fekete nodes table for triangular elements
	static const fekete_table fekete_nodes_order1;
	static const fekete_table fekete_nodes_order2;
	static const fekete_table fekete_nodes_order3;
	static const fekete_table fekete_nodes_order4;
	static const fekete_table fekete_nodes_order5;
	static const fekete_table fekete_nodes_order8;



};