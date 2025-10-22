using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.src.global_variables
{
    public static class helmholtzSolverInterop
    {

        [DllImport("helmholtz_solverCPP.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void solve_helmholtzsolverCPP(string input_file, string output_file);

    }
}
