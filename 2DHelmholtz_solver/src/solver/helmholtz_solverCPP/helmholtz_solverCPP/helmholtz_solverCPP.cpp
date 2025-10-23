#include <iostream>
#include <fstream>
#include <vector>
#include <cmath>
#include <string>
#include <unordered_map>
#include <cstdint>
#include <Eigen/Dense>
#include "system_store/helmholtz_system_store.h"



// Helper to read C#-style strings (BinaryWriter.Write(string))
std::string ReadString(std::ifstream& in)
{
	int32_t length = 0;
	in.read(reinterpret_cast<char*>(&length), 4);
	std::string str(length, '\0');
	in.read(&str[0], length);
	return str;
}


// Function to solve the system setting from C# or Python
extern "C" __declspec(dllexport) void solve_helmholtzsolverCPP(const char* input_file, const char* output_file,
	bool* isAnalysisSuccess,
	void(*callback)(const char*))
{

	// Example placeholder
	std::ifstream infile(input_file, std::ios::binary);
	std::ofstream outfile(output_file, std::ios::binary);

	if (callback) callback("Initializing solver...");


	if (!infile.is_open())
	{
		if (callback)
		{
			std::string msg = "Error: Unable to open input file: " + std::string(input_file);
			callback(msg.c_str());

		}

		(*isAnalysisSuccess) = false;

		// std::cerr << "Error: Unable to open input file: " << input_file << std::endl;
		return;
	}
	if (!outfile.is_open())
	{
		if (callback)
		{
			std::string msg = "Error: Unable to open output file: " + std::string(output_file);
			callback(msg.c_str());

		}

		(*isAnalysisSuccess) = false;

		// std::cerr << "Error: Unable to open output file: " << output_file << std::endl;
		return;
	}


	helmholtz_system_store helmholtz_2dsystem;


	// ---------- Nodes ----------
	int32_t nodeCount;
	infile.read(reinterpret_cast<char*>(&nodeCount), 4);

	for (int i = 0; i < nodeCount; i++)
	{
		int32_t node_id = 0;double x_coord = 0.0, y_coord = 0.0, z_coord = 0.0;

		infile.read(reinterpret_cast<char*>(&node_id), 4);
		infile.read(reinterpret_cast<char*>(&x_coord), 8);
		infile.read(reinterpret_cast<char*>(&y_coord), 8);
		infile.read(reinterpret_cast<char*>(&z_coord), 8);

		// Add node to the helmholtz system store
		helmholtz_2dsystem.add_node(node_id, x_coord, y_coord);

	}

	// ---------- Edges ----------
	int32_t edgeCount;
	infile.read(reinterpret_cast<char*>(&edgeCount), 4);

	for (int i = 0; i < edgeCount; i++)
	{
		int32_t edge_id = 0, startnodeid = 0, endnodeid = 0;

		infile.read(reinterpret_cast<char*>(&edge_id), 4);
		infile.read(reinterpret_cast<char*>(&startnodeid), 4);
		infile.read(reinterpret_cast<char*>(&endnodeid), 4);

		// Add edge to the helmholtz system store
		helmholtz_2dsystem.add_edge(edge_id, startnodeid, endnodeid);

	}

	// ---------- Tri Elements ----------
	int32_t triCount;
	infile.read(reinterpret_cast<char*>(&triCount), 4);

	for (int i = 0; i < triCount; i++)
	{
		int32_t tri_id = 0, nodeid1 = 0, nodeid2 = 0, nodeid3 = 0, materialid = 0;

		infile.read(reinterpret_cast<char*>(&tri_id), 4);
		infile.read(reinterpret_cast<char*>(&nodeid1), 4);
		infile.read(reinterpret_cast<char*>(&nodeid2), 4);
		infile.read(reinterpret_cast<char*>(&nodeid3), 4);
		infile.read(reinterpret_cast<char*>(&materialid), 4);

		// Add tri element to the helmholtz system store
		helmholtz_2dsystem.add_trielement(tri_id, nodeid1, nodeid2, nodeid3, materialid);

	}

	// ---------- Quad Elements ----------
	int32_t quadCount;
	infile.read(reinterpret_cast<char*>(&quadCount), 4);

	for (int i = 0; i < quadCount; i++)
	{
		int32_t quad_id = 0, nodeid1 = 0, nodeid2 = 0, nodeid3 = 0, nodeid4 = 0, materialid = 0;

		infile.read(reinterpret_cast<char*>(&quad_id), 4);
		infile.read(reinterpret_cast<char*>(&nodeid1), 4);
		infile.read(reinterpret_cast<char*>(&nodeid2), 4);
		infile.read(reinterpret_cast<char*>(&nodeid3), 4);
		infile.read(reinterpret_cast<char*>(&nodeid4), 4);
		infile.read(reinterpret_cast<char*>(&materialid), 4);

		// Add quad element to the helmholtz system store
		helmholtz_2dsystem.add_quadelement(quad_id, nodeid1, nodeid2, nodeid3, nodeid4, materialid);

	}

	// ---------- Materials ----------
	int32_t matCount;
	infile.read(reinterpret_cast<char*>(&matCount), 4);

	for (int i = 0; i < matCount; i++)
	{
		int32_t materialid = 0, numelement = 0;
		double permittivity = 0.0, permeability = 0.0, conductivity = 0.0;


		infile.read(reinterpret_cast<char*>(&materialid), 4);
		std::string matname = ReadString(infile);
		infile.read(reinterpret_cast<char*>(&permittivity), 8);
		infile.read(reinterpret_cast<char*>(&permeability), 8);
		infile.read(reinterpret_cast<char*>(&conductivity), 8);
		infile.read(reinterpret_cast<char*>(&numelement), 4);

		// Add material to the helmholtz system store
		helmholtz_2dsystem.add_material(materialid, permittivity, permeability);

	}


	// ---------- Node Constraints ----------
	int32_t ndCnstCount;
	infile.read(reinterpret_cast<char*>(&ndCnstCount), 4);

	for (int i = 0; i < ndCnstCount; i++)
	{
		int32_t nodeConstraintsid = 0;
		double fieldvalue = 0.0, sourcevalue = 0.0;
		bool isfield = false;

		infile.read(reinterpret_cast<char*>(&nodeConstraintsid), 4);
		infile.read(reinterpret_cast<char*>(&fieldvalue), 8);
		infile.read(reinterpret_cast<char*>(&sourcevalue), 8);
		infile.read(reinterpret_cast<char*>(&isfield), 1);


		int32_t nidCount;
		infile.read(reinterpret_cast<char*>(&nidCount), 4);

		for (int j = 0; j < nidCount; j++)
		{
			int32_t node_id = 0;
			infile.read(reinterpret_cast<char*>(&node_id), 4);

			// Update the constraint of the node where constarints are applied
			helmholtz_2dsystem.add_nodeconstraint(node_id, fieldvalue, sourcevalue);

		}

	}


	// ---------- Edge Constraints ----------
	int32_t edgeCnstCount;
	infile.read(reinterpret_cast<char*>(&edgeCnstCount), 4);

	for (int i = 0; i < edgeCnstCount; i++)
	{
		int32_t edgeConstraintsid = 0;
		double fieldvalue = 0.0, normalderivfieldvalue = 0.0;
		bool isSommerfieldBC = false;


		infile.read(reinterpret_cast<char*>(&edgeConstraintsid), 4);
		infile.read(reinterpret_cast<char*>(&fieldvalue), 8);
		infile.read(reinterpret_cast<char*>(&normalderivfieldvalue), 8);
		infile.read(reinterpret_cast<char*>(&isSommerfieldBC), 1);

		int32_t eidCount;
		infile.read(reinterpret_cast<char*>(&eidCount), 4);

		for (int j = 0; j < eidCount; j++)
		{
			int32_t edge_id = 0, startnodeid = 0, endnodeid = 0;

			infile.read(reinterpret_cast<char*>(&edge_id), 4);
			infile.read(reinterpret_cast<char*>(&startnodeid), 4);
			infile.read(reinterpret_cast<char*>(&endnodeid), 4);

			// Update the constraint of the edge where constarints are applied
			helmholtz_2dsystem.add_edgeconstraint(edge_id, isSommerfieldBC, fieldvalue, normalderivfieldvalue);

		}

	}


}

