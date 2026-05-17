#pragma once

#pragma warning(push)
#pragma warning (disable : 4244)
#pragma warning (disable : 26495)
#pragma warning (disable : 26451)
#pragma warning (disable : 6255)
#pragma warning (disable : 6294)
#pragma warning (disable : 26813)
#pragma warning (disable : 26454)


#include <Eigen/Dense>
#include <Eigen/Sparse>

#pragma warning(pop)

class MinvKOp
{

public:
    MinvKOp(const Eigen::SparseMatrix<double>& K_,
        Eigen::SimplicialLLT<Eigen::SparseMatrix<double>>& chol_);


    int rows() const { return K.rows(); }
    int cols() const { return K.cols(); }

    void perform_op(const double* x_in, double* y_out) const;

private:
    const Eigen::SparseMatrix<double>& K;
    Eigen::SimplicialLLT<Eigen::SparseMatrix<double>>& chol;

};


