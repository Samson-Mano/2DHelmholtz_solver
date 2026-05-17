#include "MinvKOp.h"

MinvKOp::MinvKOp(const Eigen::SparseMatrix<double>& K_,
    Eigen::SimplicialLLT<Eigen::SparseMatrix<double>>& chol_)
    : K(K_), chol(chol_) 
{


}

void MinvKOp::perform_op(const double* x_in, double* y_out) const
{
    Eigen::Map<const Eigen::VectorXd> x(x_in, K.cols());
    Eigen::Map<Eigen::VectorXd> y(y_out, K.rows());

    // y = K * x
    Eigen::VectorXd temp = K * x;

    // y = M^{-1} (Kx)
    y = chol.solve(temp);
}



