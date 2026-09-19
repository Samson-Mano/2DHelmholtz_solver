#include "MinvKOp.h"

MinvKOp::MinvKOp(const Eigen::SparseMatrix<Scalar>& K_,
    const Eigen::SparseMatrix<Scalar>& chol_)
	: m_K(K_), m_chol(chol_), m_cholT(chol_.transpose())
{
	m_cholT.makeCompressed();

    //// Verify the solver is initialized
    //if (m_chol.info() != Eigen::Success) 
    //{
    //    // Handle error - maybe throw an exception
    //}
}

void MinvKOp::perform_op(const Scalar* x_in, Scalar* y_out) const
{
    // y = L^{-1} * K * L^{-T} * x

    // Map input and output to Eigen vectors
    Eigen::Map<const Eigen::VectorXd> x(x_in, m_K.cols());
    Eigen::Map<Eigen::VectorXd> y(y_out, m_K.rows());


    // 1) w = L^{-T} x   =>  solve L^T w = x   (L^T is upper triangular)
    Eigen::VectorXd w = m_cholT.triangularView<Eigen::Upper>().solve(x);

    // 2) v = K w
    Eigen::VectorXd v = m_K * w;

    // 3) y = L^{-1} v   =>  solve L y = v    (L is lower triangular)
    y = m_chol.triangularView<Eigen::Lower>().solve(v);


}







