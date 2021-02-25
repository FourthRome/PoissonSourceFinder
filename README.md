# PoissonSourceFinder
A program to find point sources inside a sphere.

## TODO
- [ ] Produce files with results of calculations - both human- and script-readable
- [ ] Make a pipeline for experiments (i.e. I want to be able to load several experiments in a row without my presence at the computer)
- [ ] Introduce logging with rewriting (so that logs do not take too much space)
- [ ] Optimize gradient descent with enlarging steps exponentially while it gives better result (an obvious improvement)
- [ ] (possibly?) Include gradient's norm into the stopping criteria
- [ ] Rewrite batch calculation with C++ and PInvoke, see if that helps
- [ ] Result visualization tool (at first for the results, then, maybe, for logs)
