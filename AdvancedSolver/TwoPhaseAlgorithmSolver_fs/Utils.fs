module TwoPhaseAlgorithmSolver.Utils

let Factorial number = 
    let mutable faculty = 1

    for i = 1 to number do
        faculty <- faculty * i

    faculty

let BinomialCoefficient n k = 
    if n = 0 && (n - k) = -1  then
        0
    else
        (Factorial n) / ((Factorial k ) * (Factorial(n - k)))
