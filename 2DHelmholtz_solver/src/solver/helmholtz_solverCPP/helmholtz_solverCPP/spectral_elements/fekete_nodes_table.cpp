#include "fekete_nodes_table.h"

const fekete_table& fekete_nodes_table::get_fekete_nodes(int spectral_order)
{
    switch (spectral_order)
    {
    case 1: return fekete_nodes_order1;
    case 2: return fekete_nodes_order2;
    case 3: return fekete_nodes_order3;
    case 4: return fekete_nodes_order4;
    case 5: return fekete_nodes_order5;
    default:
        // throw std::invalid_argument("Spectral order not supported for Fekete nodes.");
    }

	// TODO: insert return statement here
}


// Example Data Definition for Order 1 (3 nodes total)
// Coordinates are usually in the reference triangle: (0,0), (1,0), (0,1)
const fekete_table fekete_nodes_table::fekete_nodes_order1 =
{
    // 3 Corners
    { 
        {0.0, 0.0, 1.0 / 6.0},
        {0.0, 1.0, 1.0 / 6.0},
        {1.0, 0.0, 1.0 / 6.0} 
    },

     // 3 Edges (Order 1 has 0 node per edge midpoint)
     {
         {  }, // Edge 1
     },
    // 0 Internal nodes for Order 1
    { }
};



const fekete_table fekete_nodes_table::fekete_nodes_order2 = 
{
    // 3 Corners
    { {0.0, 0.0, 1.0 / 30.0}, 
     {0.0, 1.0, 1.0 / 30.0}, 
     {1.0, 0.0, 1.0 / 30.0} },

    // 3 Edges Order 2
    {
        { {0.276393, 0.723607, 1.0/6.0} }, 
        { {0.723607, 0.0, 1.0/6.0} }, 
        { {0.0, 0.276393, 1.0/6.0} }  ,
        { {0.0, 0.723607, 1.0 / 6.0} },
        { {0.276393, 0.0, 1.0 / 6.0} },
        { {0.723607, 0.276393, 1.0 / 6.0} }
    },
    // 0 Internal nodes for Order 2
    { {0.333333, 0.333333, 0.9} }
};



const fekete_table fekete_nodes_table::fekete_nodes_order5 =
{
    // 3 Corners
    { {0.0, 0.0, 0.000402},
     {0.0, 1.0, 0.000402},
     {.0, 0.0, 0.000402} },

     // 3 Edges Order 5
     {
         { {0.0, 0.084885, 0.019297} ,
          {0.0, 0.265565, 0.027234} ,
          {0.0, 0.5, 0.035894} ,
          {0.0, 0.734435, 0.027234} ,
          {0.0, 0.915115, 0.019297} }, // Edge 1
         { {0.084885, 0.915115, 0.019297 } ,
          {0.265565, 0.734435, 0.027234} ,
          {0.5, 0.5, 0.035894}   ,
          {0.734435, 0.265565, 0.027234} ,
          {0.915115, 0.084885, 0.019297} }, // Edge 2
         { {0.084885, 0.0, 0.019297} ,
          {0.265565, 0.0, 0.027234} ,
          {0.5, 0.0, 0.035894} ,
          {0.734435, 0.0, 0.027234} ,
          {0.915115, 0.0, 0.019297} }, // Edge 3

     },
    // 0 Internal nodes for Order 2
    { 
      {0.333333, 0.333333, 0.217856 },
      {0.106335, 0.787329, 0.110419 },
      {0.787329, 0.106335, 0.110419 },
      {0.106335, 0.106335, 0.110419 },
      {0.31627, 0.566549, 0.177135 },
      {0.566549, 0.117181, 0.177135 },
      {0.117181, 0.31627, 0.177135 },
      {0.117181, 0.566549, 0.177135 },
      {0.31627, 0.117181, 0.177135 },
      {0.566549, 0.31627, 0.177135 }}

};





//
//const std::vector<fekete_node> fekete_nodes_table::fekete_nodes_order1 =
//{
//	{0.0, 0.0, 1.0 / 6.0},
//	{1.0, 0.0, 1.0 / 6.0},
//	{0.0, 1.0, 1.0 / 6.0}
//};
//
//
//const std::vector<fekete_node> fekete_nodes_table::fekete_nodes_order2 =
//{
//    // corners
//    {0.0, 0.0, 1.0 / 60.0},
//    {1.0, 0.0, 1.0 / 60.0},
//    {0.0, 1.0, 1.0 / 60.0},
//
//    // edge midpoints
//    {0.5, 0.0, 1.0 / 15.0},
//    {0.5, 0.5, 1.0 / 15.0},
//    {0.0, 0.5, 1.0 / 15.0}
//};
//
//
//const std::vector<fekete_node> fekete_nodes_table::fekete_nodes_order3 =
//{
//    // corners
//    {0.0, 0.0, 0.025},
//    {1.0, 0.0, 0.025},
//    {0.0, 1.0, 0.025},
//
//    // edge nodes
//    {0.27639320225, 0.0, 0.0416666667},
//    {0.72360679775, 0.0, 0.0416666667},
//
//    {0.72360679775, 0.27639320225, 0.0416666667},
//    {0.27639320225, 0.72360679775, 0.0416666667},
//
//    {0.0, 0.72360679775, 0.0416666667},
//    {0.0, 0.27639320225, 0.0416666667},
//
//    // interior
//    {1.0 / 3.0, 1.0 / 3.0, 0.225}
//};
//
//
//const std::vector<fekete_node> fekete_nodes_table::fekete_nodes_order4 =
//{
//    {0.0, 0.0, 0.01},
//    {1.0, 0.0, 0.01},
//    {0.0, 1.0, 0.01},
//
//    {0.1726731646, 0.0, 0.03},
//    {0.5, 0.0, 0.03},
//    {0.8273268354, 0.0, 0.03},
//
//    {0.8273268354, 0.1726731646, 0.03},
//    {0.5, 0.5, 0.03},
//    {0.1726731646, 0.8273268354, 0.03},
//
//    {0.0, 0.8273268354, 0.03},
//    {0.0, 0.5, 0.03},
//    {0.0, 0.1726731646, 0.03},
//
//    {0.3333333333, 0.3333333333, 0.09}
//};
//
//
//
//
//
