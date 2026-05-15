using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.solver
{
    public static class modalSolverInterop
    {

        // Declare the callback delegate
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackDelegate([MarshalAs(UnmanagedType.LPStr)] string message);



        // Import the DLL function (updated to accept callback)
        [DllImport("spectralmodalanalysis_solverCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void solve_spectralmodalanalysisCPP(
            [MarshalAs(UnmanagedType.LPStr)] string inputPath,
            [MarshalAs(UnmanagedType.LPStr)] string outputPath,
            double[] solver_settings,
            int solver_settings_count,
            ref bool isAnalysisSuccess,
            CallbackDelegate callback
        );



    }
}
